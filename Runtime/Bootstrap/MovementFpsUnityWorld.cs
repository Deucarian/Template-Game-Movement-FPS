using System;
using System.Collections.Generic;
using Deucarian.Common;
using Deucarian.Combat;
using Deucarian.RunUpgrades;
using Deucarian.TemplateGameMovementFps.Actors;
using Deucarian.TemplateGameMovementFps.Combat;
using Deucarian.TemplateGameMovementFps.Movement;
using Deucarian.TemplateGameMovementFps.Progression;
using Deucarian.TemplateGameMovementFps.Run;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Deucarian.TemplateGameMovementFps
{
    internal sealed class MovementFpsUnityWorld : IMovementFpsRunWorld, IDisposable
    {
        private readonly MovementFpsTemplateController _owner;
        private readonly MovementFpsDefaultContent _content;
        private readonly List<MovementFpsEnemyActor> _enemies = new List<MovementFpsEnemyActor>();
        private MovementFpsFeedbackPresenter _feedback;

        internal MovementFpsUnityWorld(Transform parent, MovementFpsTemplateController owner, MovementFpsDefaultContent content)
        {
            _owner = owner;
            _content = content;
            GameObject root = new GameObject("Movement FPS Runtime");
            root.transform.SetParent(parent, false);
            Root = root.transform;
        }

        internal Transform Root { get; }
        internal MovementFpsPlayerController Player { get; private set; }
        internal IReadOnlyList<MovementFpsEnemyActor> Enemies => _enemies;
        public int EnemyCount => _enemies.Count;
        internal void SetFeedback(MovementFpsFeedbackPresenter feedback) { _feedback = feedback; }
        internal void RemoveEnemy(MovementFpsEnemyActor enemy) => _enemies.Remove(enemy);
        internal void DestroyEnemy(MovementFpsEnemyActor enemy) => UnityObjectUtility.DestroySafely(enemy.gameObject);

        public void ResetPlayer() => Player.ResetPlayer(new Vector3(0f, 1.2f, -8f), Quaternion.identity);
        public void SpawnInitialEnemies()
        {
            SpawnEnemy(new Vector3(0f, 1f, 8f));
            SpawnEnemy(_content.LeapingRunner, new Vector3(5f, 1f, 7f));
            SpawnEnemy(_content.BoneBulwark, new Vector3(-5.5f, 1f, 9f));
        }

        public void Dispose()
        {
            _enemies.Clear();
            if (Root != null) UnityObjectUtility.DestroySafely(Root.gameObject);
        }

        public MovementFpsEnemyActor SpawnEnemy()
        {
            if (!TryPickSpawnPosition(out Vector3 position))
            {
                position = Player == null ? Vector3.forward * 13f : Player.transform.position + Player.transform.forward * 13f;
                position.y = 1f;
            }

            return SpawnEnemy(position);
        }

        public MovementFpsEnemyActor SpawnEnemy(Vector3 position)
        {
            return SpawnEnemy(_content.Enemy, position);
        }

        public MovementFpsEnemyActor SpawnEnemy(MovementFpsEnemyDefinition definition, Vector3 position)
        {
            GameObject enemyObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            enemyObject.name = string.IsNullOrWhiteSpace(definition.DisplayName) ? "Movement FPS Enemy" : definition.DisplayName;
            enemyObject.transform.SetParent(Root, false);
            enemyObject.transform.position = position;
            enemyObject.transform.localScale = new Vector3(1f, 1.15f, 1f) * definition.VisualScale;
            MovementFpsEnemyActor enemy = enemyObject.AddComponent<MovementFpsEnemyActor>();
            enemy.Initialize(definition, _owner);
            MovementFpsMaterialOwner.Own(enemyObject.GetComponent<Renderer>());
            _enemies.Add(enemy);
            _feedback.EnemySpawned(position, definition.IsMiniBoss);
            return enemy;
        }

        public MovementFpsProjectileActor SpawnProjectile(
            MovementFpsPlayerController owner,
            Vector3 origin,
            Vector3 direction,
            MovementFpsGunDefinition gun,
            double damage,
            float resolvedSpeed)
        {
            Vector3 resolvedDirection = direction.sqrMagnitude <= 0.0001f ? Vector3.forward : direction.normalized;
            GameObject projectileObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projectileObject.name = "Movement FPS Projectile";
            projectileObject.transform.SetParent(Root, false);
            projectileObject.transform.SetPositionAndRotation(origin, Quaternion.LookRotation(resolvedDirection));
            projectileObject.transform.localScale = Vector3.one * (gun.ProjectileCollisionRadius * 2.4f);
            Renderer renderer = projectileObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = new Color(0.75f, 0.45f, 1f, 1f);
                MovementFpsMaterialOwner.Own(renderer);
            }

            var trail = projectileObject.AddComponent<TrailRenderer>();
            trail.time = 0.22f;
            trail.startWidth = 0.28f;
            trail.endWidth = 0.02f;
            trail.material = new Material(Shader.Find("Sprites/Default"));
            MovementFpsMaterialOwner.Own(trail);
            trail.startColor = new Color(0.78f, 0.48f, 1f, 0.95f);
            trail.endColor = new Color(0.18f, 0.9f, 1f, 0f);

            MovementFpsProjectileActor projectile = projectileObject.AddComponent<MovementFpsProjectileActor>();
            projectile.Initialize(
                owner,
                gun.DamageType,
                damage,
                resolvedDirection * Mathf.Max(1f, resolvedSpeed),
                gun.ProjectileLifetimeSeconds,
                gun.ProjectileCollisionRadius);
            _feedback.ProjectileSpawned(origin, resolvedDirection);
            return projectile;
        }

        public void DamageEnemiesInRadius(Vector3 center, float radius, double damage, DamageTypeId damageType)
        {
            List<MovementFpsEnemyActor> targets = new List<MovementFpsEnemyActor>();
            float sqrRadius = radius * radius;
            for (int index = 0; index < _enemies.Count; index++)
            {
                MovementFpsEnemyActor enemy = _enemies[index];
                if (enemy == null || !enemy.IsAlive)
                {
                    continue;
                }

                if ((enemy.transform.position - center).sqrMagnitude <= sqrRadius)
                {
                    targets.Add(enemy);
                }
            }

            for (int index = 0; index < targets.Count; index++)
            {
                targets[index].ApplyDamage(damage, new CombatantId("combatant.player"), damageType);
            }
        }

        public IReadOnlyList<MovementFpsEnemyActor> GetNearestEnemies(Vector3 origin, float range, int targetCount)
        {
            List<MovementFpsEnemyActor> targets = new List<MovementFpsEnemyActor>();
            float sqrRange = range * range;
            for (int index = 0; index < _enemies.Count; index++)
            {
                MovementFpsEnemyActor enemy = _enemies[index];
                if (enemy == null || !enemy.IsAlive)
                {
                    continue;
                }

                if ((enemy.transform.position - origin).sqrMagnitude <= sqrRange)
                {
                    targets.Add(enemy);
                }
            }

            targets.Sort((left, right) =>
                (left.transform.position - origin).sqrMagnitude.CompareTo((right.transform.position - origin).sqrMagnitude));

            int count = Mathf.Clamp(targetCount, 0, targets.Count);
            if (count == targets.Count)
            {
                return targets;
            }

            return targets.GetRange(0, count);
        }

        public bool SpawnWaveEnemy(MovementFpsEnemyDefinition definition)
        {
            if (TryPickSpawnPosition(out Vector3 position))
            {
                SpawnEnemy(definition, position);
                return true;
            }

            return false;
        }

        private bool TryPickSpawnPosition(out Vector3 position)
        {
            Vector3 origin = Player == null ? Vector3.zero : Player.transform.position;
            for (int attempt = 0; attempt < 10; attempt++)
            {
                Vector2 circle = Random.insideUnitCircle;
                if (circle.sqrMagnitude <= 0.01f)
                {
                    circle = Vector2.up;
                }

                circle.Normalize();
                float radius = Random.Range(13f, 18f);
                position = origin + new Vector3(circle.x, 0f, circle.y) * radius;
                position.y = 1f;
                if (Player == null || !Physics.Linecast(origin + Vector3.up * 1.4f, position + Vector3.up * 1.2f))
                {
                    return true;
                }
            }

            position = origin + Vector3.forward * 15f;
            position.y = 1f;
            return true;
        }

        internal void CreatePlayer()
        {
            GameObject playerObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            playerObject.name = "Movement FPS Player";
            playerObject.transform.SetParent(Root, false);
            playerObject.transform.position = new Vector3(0f, 1.2f, -8f);

            WallrunnerMotor motor = playerObject.AddComponent<WallrunnerMotor>();
            motor.MaxVelocityEnabled = true;
            motor.MaxVelocityMetersPerSecond = 36f;
            motor.BunnyHopEnabled = true;

            GameObject cameraObject = new GameObject("Player Camera");
            cameraObject.transform.SetParent(playerObject.transform, false);
            cameraObject.transform.localPosition = new Vector3(0f, 0.62f, 0f);
            Camera camera = cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();
            camera.nearClipPlane = 0.03f;
            camera.fieldOfView = 76f;

            Player = playerObject.AddComponent<MovementFpsPlayerController>();
            Player.Initialize(
                _owner,
                _content.Player,
                new[] { _content.Carbine, _content.RiftLauncher },
                new[] { _content.OrbitPulse, _content.ChainBolt, _content.GroundRift });
        }

        public void ApplyUpgradeContent(RunUpgradeId id)
        {
            if (id.Equals(BasicMovementFpsGame.RiftLauncherUnlockUpgradeId))
            {
                Player.AddGun(_content.RiftLauncher);
            }
            else if (id.Equals(BasicMovementFpsGame.ChainBoltUnlockUpgradeId))
            {
                Player.AddAutoPower(_content.ChainBolt);
            }
            else if (id.Equals(BasicMovementFpsGame.GroundRiftUnlockUpgradeId))
            {
                Player.AddAutoPower(_content.GroundRift);
            }
        }

        public void SpawnExperiencePickup(Vector3 position, int amount)
        {
            GameObject pickupObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            pickupObject.name = "Movement FPS XP Pickup";
            pickupObject.transform.SetParent(Root, false);
            pickupObject.transform.position = position;
            pickupObject.transform.localScale = Vector3.one * 0.38f;
            MovementFpsPickupActor pickup = pickupObject.AddComponent<MovementFpsPickupActor>();
            pickup.Initialize(_owner, amount);
            MovementFpsMaterialOwner.Own(pickupObject.GetComponent<Renderer>());
            _feedback.PickupSpawned(position);
        }

        public void ClearCombatObjects()
        {
            for (int index = Root.childCount - 1; index >= 0; index--)
            {
                Transform child = Root.GetChild(index);
                if (child == null || child.GetComponent<MovementFpsPlayerController>() != null)
                {
                    continue;
                }

                if (child.GetComponent<MovementFpsEnemyActor>() != null ||
                    child.GetComponent<MovementFpsPickupActor>() != null ||
                    child.GetComponent<MovementFpsProjectileActor>() != null)
                {
                    UnityObjectUtility.DestroySafely(child.gameObject);
                }
            }

            _enemies.Clear();
        }

    }
}
