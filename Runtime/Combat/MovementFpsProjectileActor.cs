using Deucarian.Common;
using Deucarian.Combat;
using Deucarian.TemplateGameMovementFps.Actors;
using UnityEngine;

namespace Deucarian.TemplateGameMovementFps.Combat
{
    public sealed class MovementFpsProjectileActor : MonoBehaviour
    {
        private MovementFpsPlayerController _owner;
        private DamageTypeId _damageType;
        private Vector3 _velocity;
        private double _damage;
        private float _collisionRadius;
        private float _remainingLifetime;

        public float RemainingLifetime => Mathf.Max(0f, _remainingLifetime);

        public void Initialize(
            MovementFpsPlayerController owner,
            DamageTypeId damageType,
            double damage,
            Vector3 velocity,
            float lifetimeSeconds,
            float collisionRadius)
        {
            _owner = owner;
            _damageType = damageType.IsEmpty ? BasicMovementFpsGame.KineticDamageType : damageType;
            _damage = System.Math.Max(0d, damage);
            _velocity = velocity;
            _remainingLifetime = Mathf.Max(0.05f, lifetimeSeconds);
            _collisionRadius = Mathf.Max(0.02f, collisionRadius);
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            _remainingLifetime -= deltaTime;
            if (_remainingLifetime <= 0f)
            {
                UnityObjectUtility.DestroySafely(gameObject);
                return;
            }

            Vector3 displacement = _velocity * deltaTime;
            if (TryFindImpact(displacement, out RaycastHit hit))
            {
                MovementFpsEnemyActor enemy = hit.collider.GetComponentInParent<MovementFpsEnemyActor>();
                if (enemy != null)
                {
                    enemy.ApplyDamage(_damage, new CombatantId("combatant.player"), _damageType);
                }

                UnityObjectUtility.DestroySafely(gameObject);
                return;
            }

            transform.position += displacement;
        }

        private bool TryFindImpact(Vector3 displacement, out RaycastHit nearestHit)
        {
            nearestHit = default;
            if (displacement.sqrMagnitude <= 0.000001f)
            {
                return false;
            }

            RaycastHit[] hits = Physics.SphereCastAll(transform.position, _collisionRadius, displacement.normalized, displacement.magnitude, ~0, QueryTriggerInteraction.Ignore);
            float nearestDistance = float.MaxValue;
            bool found = false;
            for (int index = 0; index < hits.Length; index++)
            {
                RaycastHit hit = hits[index];
                if (hit.collider == null ||
                    hit.collider.transform.IsChildOf(transform) ||
                    (_owner != null && hit.collider.transform.IsChildOf(_owner.transform)))
                {
                    continue;
                }

                if (hit.distance < nearestDistance)
                {
                    nearestDistance = hit.distance;
                    nearestHit = hit;
                    found = true;
                }
            }

            return found;
        }
    }
}
