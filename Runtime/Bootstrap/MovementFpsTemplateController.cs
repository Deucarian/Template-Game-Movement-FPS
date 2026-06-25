using System.Collections.Generic;
using Deucarian.Combat;
using Deucarian.RunUpgrades;
using Deucarian.TemplateGameMovementFps.Actors;
using Deucarian.TemplateGameMovementFps.Combat;
using Deucarian.TemplateGameMovementFps.Movement;
using Deucarian.TemplateGameMovementFps.Progression;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Deucarian.TemplateGameMovementFps
{
    public sealed class MovementFpsTemplateController : MonoBehaviour
    {
        [SerializeField]
        private bool buildSampleArenaOnAwake = true;

        [SerializeField, Min(1f)]
        private float enemySpawnIntervalSeconds = 4f;

        [SerializeField, Min(1)]
        private int maximumEnemiesAlive = 6;

        private readonly List<MovementFpsEnemyActor> _enemies = new List<MovementFpsEnemyActor>();
        private Transform _runtimeRoot;
        private MovementFpsPlayerController _player;
        private CombatCatalog _combatCatalog;
        private MovementFpsRunProgression _progression;
        private MovementFpsPlayerDefinition _playerDefinition;
        private MovementFpsEnemyDefinition _enemyDefinition;
        private MovementFpsGunDefinition _carbineDefinition;
        private MovementFpsGunDefinition _riftLauncherDefinition;
        private MovementFpsAutoPowerDefinition _orbitPulseDefinition;
        private MovementFpsAutoPowerDefinition _chainBoltDefinition;
        private MovementFpsAutoPowerDefinition _groundRiftDefinition;
        private float _spawnTimer;
        private bool _draftOpen;
        private bool _defeat;

        public MovementFpsPlayerController Player => _player;
        public CombatCatalog CombatCatalog => _combatCatalog;
        public MovementFpsRunProgression Progression => _progression;
        public bool IsGameplayPaused => _draftOpen || _defeat;
        public bool DraftOpen => _draftOpen;
        public bool Defeated => _defeat;
        public int EnemyCount => _enemies.Count;
        public float PlayerPickupRadius => _playerDefinition.PickupRadius + (float)(_progression == null ? 0d : _progression.PickupRadiusBonus);

        private void Awake()
        {
            EnsureBootstrapped();
        }

        private void Update()
        {
            if (_defeat)
            {
                if (WasPressed(Key.R))
                {
                    RestartRun();
                }

                return;
            }

            if (_draftOpen)
            {
                if (WasPressed(Key.Digit1))
                {
                    ChooseDraft(0);
                }
                else if (WasPressed(Key.Digit2))
                {
                    ChooseDraft(1);
                }
                else if (WasPressed(Key.Digit3))
                {
                    ChooseDraft(2);
                }

                return;
            }

            _spawnTimer -= Time.deltaTime;
            if (_spawnTimer <= 0f && _enemies.Count < maximumEnemiesAlive)
            {
                _spawnTimer = enemySpawnIntervalSeconds;
                SpawnEnemy();
            }
        }

        public void EnsureBootstrapped()
        {
            if (_combatCatalog != null)
            {
                return;
            }

            _combatCatalog = BasicMovementFpsGame.CreateCombatCatalog();
            _progression = new MovementFpsRunProgression(BasicMovementFpsGame.CreateUpgradeCatalog());
            _playerDefinition = BasicMovementFpsGame.CreatePlayerDefinition();
            _enemyDefinition = BasicMovementFpsGame.CreateEnemyDefinition();
            _carbineDefinition = BasicMovementFpsGame.CreateCarbineDefinition();
            _riftLauncherDefinition = BasicMovementFpsGame.CreateRiftLauncherDefinition();
            _orbitPulseDefinition = BasicMovementFpsGame.CreateOrbitPulseDefinition();
            _chainBoltDefinition = BasicMovementFpsGame.CreateChainBoltDefinition();
            _groundRiftDefinition = BasicMovementFpsGame.CreateGroundRiftDefinition();

            GameObject root = new GameObject("Movement FPS Runtime");
            root.transform.SetParent(transform, false);
            _runtimeRoot = root.transform;

            if (buildSampleArenaOnAwake)
            {
                BuildSampleArena();
            }

            CreatePlayer();
            StartRun();
        }

        public void StartRun()
        {
            EnsureBootstrapped();
            ClearRuntimeCombatObjects();
            _progression.Reset();
            _draftOpen = false;
            _defeat = false;
            _spawnTimer = 0.25f;
            _player.ResetPlayer(new Vector3(0f, 1.2f, -8f), Quaternion.identity);
            SpawnEnemy(new Vector3(0f, 1f, 8f));
        }

        public void RestartRun()
        {
            StartRun();
        }

        public MovementFpsEnemyActor SpawnEnemy()
        {
            Vector2 circle = Random.insideUnitCircle.normalized;
            if (circle.sqrMagnitude <= 0.01f)
            {
                circle = Vector2.up;
            }

            Vector3 origin = _player == null ? Vector3.zero : _player.transform.position;
            Vector3 position = origin + new Vector3(circle.x, 0f, circle.y) * 13f;
            position.y = 1f;
            return SpawnEnemy(position);
        }

        public MovementFpsEnemyActor SpawnEnemy(Vector3 position)
        {
            EnsureBootstrapped();
            GameObject enemyObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            enemyObject.name = "Movement FPS Enemy";
            enemyObject.transform.SetParent(_runtimeRoot, false);
            enemyObject.transform.position = position;
            enemyObject.transform.localScale = new Vector3(1f, 1.15f, 1f);
            MovementFpsEnemyActor enemy = enemyObject.AddComponent<MovementFpsEnemyActor>();
            enemy.Initialize(_enemyDefinition, this);
            _enemies.Add(enemy);
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
            EnsureBootstrapped();
            Vector3 resolvedDirection = direction.sqrMagnitude <= 0.0001f ? Vector3.forward : direction.normalized;
            GameObject projectileObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projectileObject.name = "Movement FPS Projectile";
            projectileObject.transform.SetParent(_runtimeRoot, false);
            projectileObject.transform.SetPositionAndRotation(origin, Quaternion.LookRotation(resolvedDirection));
            projectileObject.transform.localScale = Vector3.one * (gun.ProjectileCollisionRadius * 2.4f);
            Renderer renderer = projectileObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = new Color(0.75f, 0.45f, 1f, 1f);
            }

            MovementFpsProjectileActor projectile = projectileObject.AddComponent<MovementFpsProjectileActor>();
            projectile.Initialize(
                owner,
                gun.DamageType,
                damage,
                resolvedDirection * Mathf.Max(1f, resolvedSpeed),
                gun.ProjectileLifetimeSeconds,
                gun.ProjectileCollisionRadius);
            return projectile;
        }

        public void HandleEnemyKilled(MovementFpsEnemyActor enemy)
        {
            if (enemy == null)
            {
                return;
            }

            _enemies.Remove(enemy);
            SpawnExperiencePickup(enemy.transform.position + Vector3.up * 0.4f, enemy.ExperienceDrop);
            Destroy(enemy.gameObject);
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

        public void ApplyPlayerDamage(double amount)
        {
            if (_player != null)
            {
                _player.ApplyDamage(amount);
            }
        }

        public void HandlePlayerDefeated()
        {
            _defeat = true;
            _draftOpen = false;
        }

        public void CollectExperience(int amount)
        {
            _progression.GainExperience(amount);
            _draftOpen = _progression.HasDraft;
        }

        public RunUpgradeSelectionResult ChooseDraft(int index)
        {
            RunUpgradeSelectionResult result = _progression.ChooseDraft(index);
            if (result.Succeeded)
            {
                ApplyRuntimeContentForUpgrade(result.Id);
            }

            _draftOpen = _progression.HasDraft;
            return result;
        }

        public MovementFpsEnemyActor SpawnEnemyForTest(Vector3 position)
        {
            return SpawnEnemy(position);
        }

        public Deucarian.Combat.DamageResult FireAtEnemyForTest(MovementFpsEnemyActor enemy)
        {
            return _player.FireAt(enemy);
        }

        public MovementFpsProjectileActor FireProjectileAtEnemyForTest(MovementFpsEnemyActor enemy)
        {
            _player.AddGun(_riftLauncherDefinition);
            return _player.FireProjectileAtForTest(enemy);
        }

        public void AddPowerForTest(MovementFpsAutoPowerDefinition definition)
        {
            _player.AddAutoPower(definition);
        }

        public void TickPlayerPowersForTest(float deltaTime)
        {
            _player.TickAutoPowersForTest(deltaTime);
        }

        public RunUpgradeSelectionResult ApplyUpgradeByIdForTest(RunUpgradeId id)
        {
            RunUpgradeSelectionResult result = _progression.ApplyUpgradeById(id);
            if (result.Succeeded)
            {
                ApplyRuntimeContentForUpgrade(result.Id);
            }

            return result;
        }

        public void CollectExperienceForTest(int amount)
        {
            CollectExperience(amount);
        }

        public RunUpgradeSelectionResult ChooseDraftForTest(int index)
        {
            return ChooseDraft(index);
        }

        public void RestartRunForTest()
        {
            RestartRun();
        }

        private void CreatePlayer()
        {
            GameObject playerObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            playerObject.name = "Movement FPS Player";
            playerObject.transform.SetParent(_runtimeRoot, false);
            playerObject.transform.position = new Vector3(0f, 1.2f, -8f);

            WallrunnerMotor motor = playerObject.AddComponent<WallrunnerMotor>();
            motor.MaxVelocityEnabled = true;
            motor.MaxVelocityMetersPerSecond = 36f;
            motor.BunnyHopEnabled = true;

            GameObject cameraObject = new GameObject("Player Camera");
            cameraObject.transform.SetParent(playerObject.transform, false);
            cameraObject.transform.localPosition = new Vector3(0f, 0.62f, 0f);
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.nearClipPlane = 0.03f;
            camera.fieldOfView = 76f;

            _player = playerObject.AddComponent<MovementFpsPlayerController>();
            _player.Initialize(
                this,
                _playerDefinition,
                new[] { _carbineDefinition },
                new[] { _orbitPulseDefinition });
        }

        private void ApplyRuntimeContentForUpgrade(RunUpgradeId id)
        {
            if (id.Equals(BasicMovementFpsGame.RiftLauncherUnlockUpgradeId))
            {
                _player.AddGun(_riftLauncherDefinition);
            }
            else if (id.Equals(BasicMovementFpsGame.ChainBoltUnlockUpgradeId))
            {
                _player.AddAutoPower(_chainBoltDefinition);
            }
            else if (id.Equals(BasicMovementFpsGame.GroundRiftUnlockUpgradeId))
            {
                _player.AddAutoPower(_groundRiftDefinition);
            }
        }

        private void BuildSampleArena()
        {
            CreateSolid("Arena Floor", new Vector3(0f, -0.55f, 0f), new Vector3(42f, 1f, 42f), new Color(0.24f, 0.25f, 0.27f, 1f));
            CreateSolid("Left Wallrun Wall", new Vector3(-7f, 2f, 0f), new Vector3(0.45f, 4f, 22f), new Color(0.16f, 0.22f, 0.31f, 1f));
            CreateSolid("Right Wallrun Wall", new Vector3(7f, 2f, 0f), new Vector3(0.45f, 4f, 22f), new Color(0.16f, 0.22f, 0.31f, 1f));
            CreateSolid("Low Flow Vault", new Vector3(0f, 0.55f, -1f), new Vector3(2.2f, 1.1f, 0.7f), new Color(0.32f, 0.28f, 0.18f, 1f));
            CreateSolid("Tall Safety Mantle", new Vector3(3.8f, 1.05f, 3.5f), new Vector3(2.2f, 2.1f, 0.7f), new Color(0.28f, 0.24f, 0.34f, 1f));

            GameObject lightObject = new GameObject("Arena Directional Light");
            lightObject.transform.SetParent(_runtimeRoot, false);
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.15f;
        }

        private void CreateSolid(string name, Vector3 position, Vector3 scale, Color color)
        {
            GameObject solid = GameObject.CreatePrimitive(PrimitiveType.Cube);
            solid.name = name;
            solid.transform.SetParent(_runtimeRoot, false);
            solid.transform.position = position;
            solid.transform.localScale = scale;
            Renderer renderer = solid.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = color;
            }
        }

        private void SpawnExperiencePickup(Vector3 position, int amount)
        {
            GameObject pickupObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            pickupObject.name = "Movement FPS XP Pickup";
            pickupObject.transform.SetParent(_runtimeRoot, false);
            pickupObject.transform.position = position;
            pickupObject.transform.localScale = Vector3.one * 0.38f;
            MovementFpsPickupActor pickup = pickupObject.AddComponent<MovementFpsPickupActor>();
            pickup.Initialize(this, amount);
        }

        private void ClearRuntimeCombatObjects()
        {
            for (int index = _runtimeRoot.childCount - 1; index >= 0; index--)
            {
                Transform child = _runtimeRoot.GetChild(index);
                if (child == null || child.GetComponent<MovementFpsPlayerController>() != null)
                {
                    continue;
                }

                if (child.GetComponent<MovementFpsEnemyActor>() != null ||
                    child.GetComponent<MovementFpsPickupActor>() != null ||
                    child.GetComponent<MovementFpsProjectileActor>() != null)
                {
                    Destroy(child.gameObject);
                }
            }

            _enemies.Clear();
        }

        private static bool WasPressed(Key key)
        {
            return Keyboard.current != null && Keyboard.current[key].wasPressedThisFrame;
        }

        private void OnGUI()
        {
            if (_player == null || _progression == null)
            {
                return;
            }

            GUILayout.BeginArea(new Rect(16f, 16f, 520f, 280f));
            GUILayout.Label("Movement FPS Template");
            GUILayout.Label($"HP {_player.CurrentHealth:0}/{_player.MaximumHealth:0}  Level {_progression.Level}  XP {_progression.CurrentExperience}/{_progression.RequiredExperience}");
            GUILayout.Label($"State {_player.Motor.State}  Speed {Vector3.ProjectOnPlane(_player.Motor.Velocity, Vector3.up).magnitude:0.0}");
            if (_player.CurrentGun != null)
            {
                MovementFpsGunRuntimeState gun = _player.CurrentGun;
                GUILayout.Label($"{gun.Definition.DisplayName}  Ammo {gun.Ammo}/{gun.Definition.MagazineSize}{(gun.Reloading ? "  Reloading" : string.Empty)}");
            }

            if (_draftOpen)
            {
                GUILayout.Label("Choose upgrade: 1 / 2 / 3");
                for (int index = 0; index < _progression.CurrentDraft.Count; index++)
                {
                    GUILayout.Label($"{index + 1}. {_progression.CurrentDraft[index].Id.Value}");
                }
            }
            else if (_defeat)
            {
                GUILayout.Label("Defeated - press R to restart");
            }
            else
            {
                GUILayout.Label("WASD move, Shift sprint, Ctrl/C slide, Space jump, mouse fire/look, Q swaps guns");
            }

            GUILayout.EndArea();
        }
    }
}
