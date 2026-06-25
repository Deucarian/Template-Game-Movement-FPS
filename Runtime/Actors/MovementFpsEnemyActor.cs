using Deucarian.Combat;
using UnityEngine;

namespace Deucarian.TemplateGameMovementFps.Actors
{
    [RequireComponent(typeof(Collider))]
    public sealed class MovementFpsEnemyActor : MonoBehaviour
    {
        [SerializeField]
        private Renderer bodyRenderer;

        private MovementFpsEnemyDefinition _definition;
        private MovementFpsTemplateController _session;
        private HealthState _health;
        private float _contactTimer;

        public bool IsAlive => _health != null && _health.IsAlive;
        public string DefinitionId => _definition.Id;
        public string DisplayName => _definition.DisplayName;
        public bool IsMiniBoss => _definition.IsMiniBoss;
        public double CurrentHealth => _health == null ? 0d : _health.CurrentHealth;
        public int ExperienceDrop => _definition.ExperienceDrop;

        public void Initialize(MovementFpsEnemyDefinition definition, MovementFpsTemplateController session)
        {
            _definition = definition;
            _session = session;
            _health = new HealthState(new CombatantId(definition.Id + "." + GetInstanceID()), definition.MaximumHealth, definition.MaximumHealth);
            _contactTimer = 0f;

            if (bodyRenderer == null)
            {
                bodyRenderer = GetComponentInChildren<Renderer>();
            }

            if (bodyRenderer != null)
            {
                bodyRenderer.material.color = ResolveColor(definition);
            }
        }

        private void Update()
        {
            if (_session == null || _session.IsGameplayPaused || !IsAlive || _session.Player == null)
            {
                return;
            }

            float deltaTime = Time.deltaTime;
            _contactTimer -= deltaTime;
            Vector3 playerPosition = _session.Player.transform.position;
            Vector3 toPlayer = playerPosition - transform.position;
            toPlayer.y = 0f;
            float distance = toPlayer.magnitude;
            Vector3 direction = distance <= 0.001f ? Vector3.zero : toPlayer / distance;

            if (distance > 1.25f)
            {
                transform.position += direction * (_definition.MoveSpeed * deltaTime);
                if (direction.sqrMagnitude > 0.01f)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), 10f * deltaTime);
                }
            }
            else if (_contactTimer <= 0f)
            {
                _contactTimer = _definition.ContactIntervalSeconds;
                _session.ApplyPlayerDamage(_definition.ContactDamage);
            }
        }

        public Deucarian.Combat.DamageResult ApplyDamage(double amount, CombatantId sourceId)
        {
            return ApplyDamage(amount, sourceId, BasicMovementFpsGame.KineticDamageType);
        }

        public Deucarian.Combat.DamageResult ApplyDamage(double amount, CombatantId sourceId, DamageTypeId damageType)
        {
            if (!IsAlive)
            {
                return null;
            }

            DamageRequest request = new DamageRequest(
                _health.Id,
                new[] { new DamageComponent(damageType.IsEmpty ? BasicMovementFpsGame.KineticDamageType : damageType, amount) },
                sourceId: sourceId);
            Deucarian.Combat.DamageResult result = DamageResolver.Apply(_session.CombatCatalog, _health, null, request);

            if (!IsAlive)
            {
                _session.HandleEnemyKilled(this);
            }

            return result;
        }

        private static Color ResolveColor(MovementFpsEnemyDefinition definition)
        {
            if (definition.IsMiniBoss)
            {
                return new Color(0.55f, 0.08f, 0.9f, 1f);
            }

            switch (definition.Id)
            {
                case "enemy.leaping-runner":
                    return new Color(1f, 0.55f, 0.25f, 1f);
                case "enemy.bone-bulwark":
                    return new Color(0.85f, 0.82f, 0.68f, 1f);
                default:
                    return new Color(0.8f, 0.25f, 0.35f, 1f);
            }
        }
    }
}
