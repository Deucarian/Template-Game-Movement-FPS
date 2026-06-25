using System.Collections.Generic;
using Deucarian.Combat;
using Deucarian.RunUpgrades;
using Deucarian.TemplateGameMovementFps.Actors;
using Deucarian.TemplateGameMovementFps.Combat;
using Deucarian.TemplateGameMovementFps.Movement;
using Deucarian.TemplateGameMovementFps.Progression;
using Deucarian.TemplateGameMovementFps.Run;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Deucarian.TemplateGameMovementFps
{
    public sealed class MovementFpsTemplateController : MonoBehaviour
    {
        [SerializeField]
        private bool buildSampleArenaOnAwake = true;

        [SerializeField]
        private bool enemySpawningEnabled = true;

        [SerializeField, Min(0.05f)]
        private float spawnIntervalMultiplier = 1f;

        [SerializeField, Min(0.1f)]
        private float spawnBatchMultiplier = 1f;

        [SerializeField, Min(0f)]
        private float escalationMultiplier = 1f;

        [SerializeField, Min(0)]
        private int maxAliveOverride;

        [SerializeField]
        private bool miniBossSpawningEnabled = true;

        private readonly List<MovementFpsEnemyActor> _enemies = new List<MovementFpsEnemyActor>();
        private Transform _runtimeRoot;
        private MovementFpsPlayerController _player;
        private CombatCatalog _combatCatalog;
        private MovementFpsRunProgression _progression;
        private MovementFpsPlayerDefinition _playerDefinition;
        private MovementFpsEnemyDefinition _enemyDefinition;
        private MovementFpsEnemyDefinition _leapingRunnerDefinition;
        private MovementFpsEnemyDefinition _boneBulwarkDefinition;
        private MovementFpsEnemyDefinition _choirOgreDefinition;
        private MovementFpsWaveDefinition _waveDefinition;
        private MovementFpsWaveDirector _waveDirector;
        private MovementFpsGunDefinition _carbineDefinition;
        private MovementFpsGunDefinition _riftLauncherDefinition;
        private MovementFpsAutoPowerDefinition _orbitPulseDefinition;
        private MovementFpsAutoPowerDefinition _chainBoltDefinition;
        private MovementFpsAutoPowerDefinition _groundRiftDefinition;
        private float _elapsedSeconds;
        private bool _draftOpen;
        private bool _defeat;
        private bool _victory;
        private bool _miniBossDefeated;
        private MovementFpsRunState _runState;

        public MovementFpsPlayerController Player => _player;
        public CombatCatalog CombatCatalog => _combatCatalog;
        public MovementFpsRunProgression Progression => _progression;
        public bool IsGameplayPaused => _draftOpen || _defeat || _victory;
        public bool DraftOpen => _draftOpen;
        public bool Defeated => _defeat;
        public bool Victory => _victory;
        public bool MiniBossSpawned => _waveDirector != null && _waveDirector.MiniBossSpawned;
        public bool MiniBossDefeated => _miniBossDefeated;
        public MovementFpsRunState RunState => _runState;
        public float RunElapsedSeconds => _elapsedSeconds;
        public MovementFpsWaveSpawnSnapshot CurrentSpawnSnapshot => _waveDirector == null ? default : _waveDirector.CurrentSnapshot;
        public IReadOnlyList<MovementFpsEnemyActor> Enemies => _enemies;
        public int EnemyCount => _enemies.Count;
        public float PlayerPickupRadius => _playerDefinition.PickupRadius + (float)(_progression == null ? 0d : _progression.PickupRadiusBonus);

        private void Awake()
        {
            EnsureBootstrapped();
        }

        private void Update()
        {
            if (_defeat || _victory)
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

            float deltaTime = Time.deltaTime;
            _elapsedSeconds += deltaTime;
            if (enemySpawningEnabled)
            {
                TickWaveDirector(deltaTime);
            }

            TryCompleteSurvivalVictory();
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
            _leapingRunnerDefinition = BasicMovementFpsGame.CreateLeapingRunnerDefinition();
            _boneBulwarkDefinition = BasicMovementFpsGame.CreateBoneBulwarkDefinition();
            _choirOgreDefinition = BasicMovementFpsGame.CreateChoirOgreDefinition();
            _waveDefinition = BasicMovementFpsGame.CreatePrototypeWaveDefinition();
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
            _victory = false;
            _miniBossDefeated = false;
            _runState = MovementFpsRunState.Running;
            _elapsedSeconds = 0f;
            _waveDirector = new MovementFpsWaveDirector(_waveDefinition, Random.Range(int.MinValue, int.MaxValue));
            ApplyWaveTuning();
            _player.ResetPlayer(new Vector3(0f, 1.2f, -8f), Quaternion.identity);
            SpawnEnemy(new Vector3(0f, 1f, 8f));
        }

        public void RestartRun()
        {
            StartRun();
        }

        public MovementFpsEnemyActor SpawnEnemy()
        {
            if (!TryPickSpawnPosition(out Vector3 position))
            {
                position = _player == null ? Vector3.forward * 13f : _player.transform.position + _player.transform.forward * 13f;
                position.y = 1f;
            }

            return SpawnEnemy(position);
        }

        public MovementFpsEnemyActor SpawnEnemy(Vector3 position)
        {
            return SpawnEnemy(_enemyDefinition, position);
        }

        public MovementFpsEnemyActor SpawnEnemy(MovementFpsEnemyDefinition definition, Vector3 position)
        {
            EnsureBootstrapped();
            GameObject enemyObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            enemyObject.name = string.IsNullOrWhiteSpace(definition.DisplayName) ? "Movement FPS Enemy" : definition.DisplayName;
            enemyObject.transform.SetParent(_runtimeRoot, false);
            enemyObject.transform.position = position;
            enemyObject.transform.localScale = new Vector3(1f, 1.15f, 1f) * definition.VisualScale;
            MovementFpsEnemyActor enemy = enemyObject.AddComponent<MovementFpsEnemyActor>();
            enemy.Initialize(definition, this);
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
            if (enemy.IsMiniBoss)
            {
                _miniBossDefeated = true;
                CompleteVictory();
            }

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
            if (_victory)
            {
                return;
            }

            _defeat = true;
            _draftOpen = false;
            _runState = MovementFpsRunState.Defeated;
        }

        public void CollectExperience(int amount)
        {
            if (_defeat || _victory)
            {
                return;
            }

            _progression.GainExperience(amount);
            _draftOpen = _progression.HasDraft;
            _runState = _draftOpen ? MovementFpsRunState.Draft : MovementFpsRunState.Running;
        }

        public RunUpgradeSelectionResult ChooseDraft(int index)
        {
            RunUpgradeSelectionResult result = _progression.ChooseDraft(index);
            if (result.Succeeded)
            {
                ApplyRuntimeContentForUpgrade(result.Id);
            }

            _draftOpen = _progression.HasDraft;
            _runState = _draftOpen ? MovementFpsRunState.Draft : MovementFpsRunState.Running;
            return result;
        }

        public MovementFpsEnemyActor SpawnEnemyForTest(Vector3 position)
        {
            return SpawnEnemy(position);
        }

        public MovementFpsEnemyActor SpawnEnemyForTest(MovementFpsEnemyDefinition definition, Vector3 position)
        {
            return SpawnEnemy(definition, position);
        }

        public MovementFpsEnemyActor SpawnMiniBossForTest(Vector3 position)
        {
            return SpawnEnemy(_choirOgreDefinition, position);
        }

        public MovementFpsEnemyDefinition GetEnemyDefinitionForTest(string id)
        {
            if (_enemyDefinition.Id == id)
            {
                return _enemyDefinition;
            }

            if (_leapingRunnerDefinition.Id == id)
            {
                return _leapingRunnerDefinition;
            }

            if (_boneBulwarkDefinition.Id == id)
            {
                return _boneBulwarkDefinition;
            }

            if (_choirOgreDefinition.Id == id)
            {
                return _choirOgreDefinition;
            }

            return default;
        }

        public void TickRunForTest(float deltaTime)
        {
            if (_defeat || _victory || _draftOpen)
            {
                return;
            }

            _elapsedSeconds += Mathf.Max(0f, deltaTime);
            if (enemySpawningEnabled)
            {
                TickWaveDirector(deltaTime);
            }

            TryCompleteSurvivalVictory();
        }

        public void KillEnemyForTest(MovementFpsEnemyActor enemy)
        {
            if (enemy != null)
            {
                enemy.ApplyDamage(99999d, new CombatantId("combatant.test"), BasicMovementFpsGame.KineticDamageType);
            }
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

        private void ApplyWaveTuning()
        {
            if (_waveDirector == null)
            {
                return;
            }

            _waveDirector.SpawnIntervalMultiplier = spawnIntervalMultiplier;
            _waveDirector.SpawnBatchMultiplier = spawnBatchMultiplier;
            _waveDirector.EscalationMultiplier = escalationMultiplier;
            _waveDirector.MaxAliveOverride = maxAliveOverride;
            _waveDirector.MiniBossSpawningEnabled = miniBossSpawningEnabled;
        }

        private void TickWaveDirector(float deltaTime)
        {
            if (_waveDirector == null)
            {
                return;
            }

            _waveDirector.Tick(deltaTime, _elapsedSeconds, _enemies.Count, SpawnEnemyFromWave);
        }

        private bool SpawnEnemyFromWave(MovementFpsEnemyDefinition definition)
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
            Vector3 origin = _player == null ? Vector3.zero : _player.transform.position;
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
                if (_player == null || !Physics.Linecast(origin + Vector3.up * 1.4f, position + Vector3.up * 1.2f))
                {
                    return true;
                }
            }

            position = origin + Vector3.forward * 15f;
            position.y = 1f;
            return true;
        }

        private void TryCompleteSurvivalVictory()
        {
            if (_victory || _defeat || _waveDefinition == null || _waveDefinition.HasMiniBoss)
            {
                return;
            }

            if (_elapsedSeconds >= _waveDefinition.VictoryTimeSeconds)
            {
                CompleteVictory();
            }
        }

        private void CompleteVictory()
        {
            _victory = true;
            _defeat = false;
            _draftOpen = false;
            _runState = MovementFpsRunState.Victory;
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
            GUILayout.Label($"Run {_elapsedSeconds:0}s  State {_runState}  Enemies {_enemies.Count}/{CurrentSpawnSnapshot.MaxAlive}");
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
            else if (_victory)
            {
                GUILayout.Label("Victory - press R to restart");
            }
            else
            {
                GUILayout.Label("WASD move, Shift sprint, Ctrl/C slide, Space jump, mouse fire/look, Q swaps guns");
            }

            GUILayout.EndArea();
        }
    }
}
