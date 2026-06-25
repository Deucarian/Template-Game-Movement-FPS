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
                bodyRenderer.material.color = new Color(0.78f, 0.18f, 0.18f, 1f);
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
                _contactTimer = 0.75f;
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
    }
}
