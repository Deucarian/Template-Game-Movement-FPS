using System;
using System.Collections.Generic;
using Deucarian.Combat;
using Deucarian.TemplateGameMovementFps.Combat;
using Deucarian.TemplateGameMovementFps.Movement;
using Deucarian.TemplateGameMovementFps.Progression;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Deucarian.TemplateGameMovementFps.Actors
{
    internal sealed class MovementFpsPlayerCombat : IMovementFpsLoadoutEffects
    {
        private readonly MovementFpsPlayerController _owner;
        private readonly MovementFpsTemplateController _session;
        private readonly MovementFpsPlayerLoadout _loadout;
        private readonly MovementFpsCameraPresenter _camera;
        private readonly Transform _pose;
        private readonly Func<Camera> _view;
        private readonly Func<Transform> _muzzle;
        private Camera ViewCamera => _view();
        private Transform Muzzle => _muzzle();
        private MovementFpsGunRuntimeState CurrentGun => _loadout.CurrentGun;
        internal double GunDamage => CurrentGun == null ? 0d : ResolveGunDamage(CurrentGun.Definition);
        internal float GunCadenceMultiplier => MovementFpsPlayerStats.GunCadence(_session == null ? null : _session.Progression);

        internal MovementFpsPlayerCombat(MovementFpsPlayerController owner, MovementFpsTemplateController session,
            MovementFpsPlayerLoadout loadout, MovementFpsCameraPresenter camera, Func<Camera> view, Func<Transform> muzzle)
        {
            _owner = owner;
            _session = session;
            _loadout = loadout;
            _camera = camera;
            _pose = owner.transform;
            _view = view;
            _muzzle = muzzle;
        }

        public void ApplyRecoil(float degrees) => _camera.ApplyRecoil(degrees);
        private double ResolveGunDamage(MovementFpsGunDefinition definition) => MovementFpsPlayerStats.GunDamage(definition, _session == null ? null : _session.Progression);
        private float ResolveProjectileSpeed(MovementFpsGunDefinition definition) => MovementFpsPlayerStats.ProjectileSpeed(definition, _session == null ? null : _session.Progression);

        public Deucarian.Combat.DamageResult FireAt(MovementFpsEnemyActor enemy)
        {
            if (enemy == null)
            {
                return null;
            }

            MovementFpsGunRuntimeState gun = CurrentGun;
            DamageTypeId damageType = gun == null ? BasicMovementFpsGame.KineticDamageType : gun.Definition.DamageType;
            return enemy.ApplyDamage(GunDamage, new CombatantId("combatant.player"), damageType);
        }

        public MovementFpsProjectileActor FireProjectileAtForTest(MovementFpsEnemyActor enemy)
        {
            if (enemy == null)
            {
                return null;
            }

            MovementFpsGunRuntimeState projectileGun = _loadout.FindGun(MovementFpsGunKind.Projectile);
            if (projectileGun == null)
            {
                return null;
            }

            Vector3 origin = Muzzle == null ? _pose.position + Vector3.up * 1.45f : Muzzle.position;
            Vector3 target = enemy.transform.position + Vector3.up * 0.4f;
            Vector3 direction = target - origin;
            if (direction.sqrMagnitude <= 0.0001f)
            {
                direction = _pose.forward;
            }

            return FireProjectile(projectileGun.Definition, origin, direction.normalized);
        }

        public void FireGun(MovementFpsGunDefinition definition)
        {
            if (ViewCamera == null)
            {
                return;
            }

            Vector3 origin = Muzzle == null ? ViewCamera.transform.position : Muzzle.position;
            Vector3 direction = ResolveShotDirection(definition);
            if (definition.Kind == MovementFpsGunKind.Projectile)
            {
                FireProjectile(definition, origin, direction);
                _session.PlayWeaponFeedback(origin, direction, definition.Kind);
                return;
            }

            FireHitscan(definition, origin, direction);
            _session.PlayWeaponFeedback(origin, direction, definition.Kind);
        }

        private void FireHitscan(MovementFpsGunDefinition definition, Vector3 origin, Vector3 direction)
        {
            RaycastHit[] hits = Physics.RaycastAll(origin, direction, definition.Range, ~0, QueryTriggerInteraction.Ignore);
            float nearestDistance = float.MaxValue;
            MovementFpsEnemyActor target = null;
            for (int index = 0; index < hits.Length; index++)
            {
                RaycastHit hit = hits[index];
                if (hit.collider == null || hit.collider.transform.IsChildOf(_pose))
                {
                    continue;
                }

                MovementFpsEnemyActor enemy = hit.collider.GetComponentInParent<MovementFpsEnemyActor>();
                if (enemy != null && hit.distance < nearestDistance)
                {
                    nearestDistance = hit.distance;
                    target = enemy;
                }
            }

            if (target != null)
            {
                target.ApplyDamage(ResolveGunDamage(definition), new CombatantId("combatant.player"), definition.DamageType);
            }
        }

        private MovementFpsProjectileActor FireProjectile(MovementFpsGunDefinition definition, Vector3 origin, Vector3 direction)
        {
            return _session.SpawnProjectile(
                _owner,
                origin,
                direction,
                definition,
                ResolveGunDamage(definition),
                ResolveProjectileSpeed(definition));
        }

        private Vector3 ResolveShotDirection(MovementFpsGunDefinition definition)
        {
            Vector3 direction = ViewCamera == null ? _pose.forward : ViewCamera.transform.forward;
            if (definition.SpreadDegrees > 0f)
            {
                Vector2 spread = Random.insideUnitCircle * definition.SpreadDegrees;
                direction = Quaternion.Euler(-spread.y, spread.x, 0f) * direction;
            }

            return direction.normalized;
        }

        public void CastAutoPower(MovementFpsAutoPowerDefinition definition)
        {
            double damage = MovementFpsPlayerStats.PowerDamage(definition, _session == null ? null : _session.Progression);
            float radius = definition.Radius;
            switch (definition.Kind)
            {
                case MovementFpsAutoPowerKind.OrbitPulse:
                    _session.DamageEnemiesInRadius(_pose.position, radius, damage, definition.DamageType);
                    _session.PlayPowerFeedback(_pose.position, definition.Kind);
                    break;
                case MovementFpsAutoPowerKind.ChainBolt:
                    IReadOnlyList<MovementFpsEnemyActor> targets = _session.GetNearestEnemies(_pose.position, definition.Range, definition.TargetCount);
                    for (int index = 0; index < targets.Count; index++)
                    {
                        targets[index].ApplyDamage(damage, new CombatantId("combatant.player"), definition.DamageType);
                    }

                    _session.PlayPowerFeedback(_pose.position + _pose.forward * 3f, definition.Kind);
                    break;
                case MovementFpsAutoPowerKind.GroundRift:
                    IReadOnlyList<MovementFpsEnemyActor> riftTargets = _session.GetNearestEnemies(_pose.position, definition.Range, 1);
                    Vector3 center = riftTargets.Count > 0 ? riftTargets[0].transform.position : _pose.position + _pose.forward * 8f;
                    _session.DamageEnemiesInRadius(center, radius, damage, definition.DamageType);
                    _session.PlayPowerFeedback(center, definition.Kind);
                    break;
            }
        }

    }
}
