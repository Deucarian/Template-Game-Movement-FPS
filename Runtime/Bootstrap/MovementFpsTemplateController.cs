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
using UnityEngine.InputSystem;

namespace Deucarian.TemplateGameMovementFps
{
    public sealed class MovementFpsTemplateController : MonoBehaviour
    {
        private const string PresentationRootName = "Movement FPS Presentation";
        private const string WeaponPulseName = "Movement FPS Weapon Pulse";
        private const string PowerPulseName = "Movement FPS Power Pulse";
        private const string EnemyPulseName = "Movement FPS Enemy Pulse";
        private const string PickupPulseName = "Movement FPS Pickup Pulse";
        private const string RunPulseName = "Movement FPS Run State Pulse";
        private const string PresentationAudioName = "Movement FPS Feedback Audio";

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
        private readonly MovementFpsRunSummaryTracker _runSummaryTracker = new MovementFpsRunSummaryTracker();
        private MovementFpsGunDefinition _carbineDefinition;
        private MovementFpsGunDefinition _riftLauncherDefinition;
        private MovementFpsAutoPowerDefinition _orbitPulseDefinition;
        private MovementFpsAutoPowerDefinition _chainBoltDefinition;
        private MovementFpsAutoPowerDefinition _groundRiftDefinition;
        private Transform _presentationRoot;
        private ParticleSystem _weaponPulse;
        private ParticleSystem _powerPulse;
        private ParticleSystem _enemyPulse;
        private ParticleSystem _pickupPulse;
        private ParticleSystem _runPulse;
        private AudioSource _feedbackAudio;
        private AudioClip _weaponClip;
        private AudioClip _projectileClip;
        private AudioClip _powerClip;
        private AudioClip _enemyClip;
        private AudioClip _pickupClip;
        private AudioClip _runClip;
        private GUIStyle _hudTitleStyle;
        private GUIStyle _hudLabelStyle;
        private GUIStyle _hudSmallStyle;
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
        public MovementFpsRunSummary CurrentRunSummary => _runSummaryTracker.CreateSummary(_elapsedSeconds);
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

            BuildPresentation();
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
            _runSummaryTracker.Reset();
            _waveDirector = new MovementFpsWaveDirector(_waveDefinition, Random.Range(int.MinValue, int.MaxValue));
            ApplyWaveTuning();
            _player.ResetPlayer(new Vector3(0f, 1.2f, -8f), Quaternion.identity);
            SpawnEnemy(new Vector3(0f, 1f, 8f));
            SpawnEnemy(_leapingRunnerDefinition, new Vector3(5f, 1f, 7f));
            SpawnEnemy(_boneBulwarkDefinition, new Vector3(-5.5f, 1f, 9f));
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
            PlayFeedback(_enemyPulse, position, definition.IsMiniBoss ? 42 : 14, definition.IsMiniBoss ? _runClip : _enemyClip);
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

            var trail = projectileObject.AddComponent<TrailRenderer>();
            trail.time = 0.22f;
            trail.startWidth = 0.28f;
            trail.endWidth = 0.02f;
            trail.material = new Material(Shader.Find("Sprites/Default"));
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
            PlayFeedback(_weaponPulse, origin + resolvedDirection * 2f, 14, _projectileClip);
            return projectile;
        }

        public void HandleEnemyKilled(MovementFpsEnemyActor enemy)
        {
            if (enemy == null)
            {
                return;
            }

            _enemies.Remove(enemy);
            bool miniBoss = enemy.IsMiniBoss;
            _runSummaryTracker.RecordKill(miniBoss);
            SpawnExperiencePickup(enemy.transform.position + Vector3.up * 0.4f, enemy.ExperienceDrop);
            PlayFeedback(_enemyPulse, enemy.transform.position, miniBoss ? 54 : 22, _enemyClip);
            if (miniBoss)
            {
                _miniBossDefeated = true;
                _runSummaryTracker.RecordReward(new MovementFpsRunReward(BasicMovementFpsGame.ChoirOgreRewardId, "Choir Ogre Banished", 1));
                CompleteVictory();
            }

            UnityObjectUtility.DestroySafely(enemy.gameObject);
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
            _runSummaryTracker.Complete(MovementFpsRunOutcome.Defeat);
            PlayFeedback(_runPulse, _player == null ? Vector3.zero : _player.transform.position, 38, _runClip);
        }

        public void CollectExperience(int amount)
        {
            if (_defeat || _victory)
            {
                return;
            }

            _runSummaryTracker.RecordExperience(amount);
            _progression.GainExperience(amount);
            _draftOpen = _progression.HasDraft;
            _runState = _draftOpen ? MovementFpsRunState.Draft : MovementFpsRunState.Running;
            PlayFeedback(_pickupPulse, _player == null ? Vector3.zero : _player.transform.position, _draftOpen ? 34 : 12, _draftOpen ? _runClip : _pickupClip);
        }

        public RunUpgradeSelectionResult ChooseDraft(int index)
        {
            RunUpgradeSelectionResult result = _progression.ChooseDraft(index);
            if (result.Succeeded)
            {
                ApplyRuntimeContentForUpgrade(result.Id);
                _runSummaryTracker.RecordUpgrade(result.Id.Value);
                PlayFeedback(_runPulse, _player == null ? Vector3.zero : _player.transform.position, 32, _runClip);
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
            _runSummaryTracker.Complete(MovementFpsRunOutcome.Victory);
            PlayFeedback(_runPulse, _player == null ? Vector3.zero : _player.transform.position, 58, _runClip);
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
            cameraObject.AddComponent<AudioListener>();
            camera.nearClipPlane = 0.03f;
            camera.fieldOfView = 76f;

            _player = playerObject.AddComponent<MovementFpsPlayerController>();
            _player.Initialize(
                this,
                _playerDefinition,
                new[] { _carbineDefinition, _riftLauncherDefinition },
                new[] { _orbitPulseDefinition, _chainBoltDefinition, _groundRiftDefinition });
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

        internal void PlayWeaponFeedback(Vector3 origin, Vector3 direction, MovementFpsGunKind kind)
        {
            Vector3 resolvedDirection = direction.sqrMagnitude <= 0.0001f ? Vector3.forward : direction.normalized;
            PlayFeedback(_weaponPulse, origin + resolvedDirection * (kind == MovementFpsGunKind.Projectile ? 2f : 5f), kind == MovementFpsGunKind.Projectile ? 14 : 8, kind == MovementFpsGunKind.Projectile ? _projectileClip : _weaponClip);
        }

        internal void PlayPowerFeedback(Vector3 position, MovementFpsAutoPowerKind kind)
        {
            int count = kind == MovementFpsAutoPowerKind.ChainBolt ? 24 : kind == MovementFpsAutoPowerKind.GroundRift ? 34 : 18;
            PlayFeedback(_powerPulse, position, count, _powerClip);
        }

        private void BuildPresentation()
        {
            GameObject root = new GameObject(PresentationRootName);
            root.transform.SetParent(_runtimeRoot, false);
            _presentationRoot = root.transform;
            _weaponPulse = CreatePulse(WeaponPulseName, new Color(0.78f, 0.48f, 1f), 0.16f, 3.2f, 0.32f);
            _powerPulse = CreatePulse(PowerPulseName, new Color(0.2f, 0.86f, 1f), 0.22f, 2.8f, 0.45f);
            _enemyPulse = CreatePulse(EnemyPulseName, new Color(1f, 0.25f, 0.28f), 0.24f, 3.0f, 0.48f);
            _pickupPulse = CreatePulse(PickupPulseName, new Color(0.4f, 1f, 0.55f), 0.14f, 2.2f, 0.35f);
            _runPulse = CreatePulse(RunPulseName, new Color(1f, 0.82f, 0.24f), 0.32f, 3.6f, 0.7f);

            GameObject audioObject = new GameObject(PresentationAudioName);
            audioObject.transform.SetParent(_presentationRoot, false);
            _feedbackAudio = audioObject.AddComponent<AudioSource>();
            _feedbackAudio.playOnAwake = false;
            _feedbackAudio.spatialBlend = 0f;
            _feedbackAudio.volume = 0.28f;
            _weaponClip = CreateTone("movement-fps-carbine", 620f, 0.055f, 0.16f);
            _projectileClip = CreateTone("movement-fps-launcher", 210f, 0.13f, 0.22f);
            _powerClip = CreateTone("movement-fps-power", 860f, 0.12f, 0.18f);
            _enemyClip = CreateTone("movement-fps-enemy", 330f, 0.08f, 0.16f);
            _pickupClip = CreateTone("movement-fps-pickup", 1040f, 0.07f, 0.15f);
            _runClip = CreateTone("movement-fps-run-state", 120f, 0.24f, 0.22f);
        }

        private ParticleSystem CreatePulse(string name, Color color, float startSize, float startSpeed, float lifetime)
        {
            GameObject instance = new GameObject(name);
            instance.transform.SetParent(_presentationRoot, false);
            ParticleSystem particles = instance.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = particles.main;
            main.loop = false;
            main.playOnAwake = false;
            main.startLifetime = lifetime;
            main.startSpeed = startSpeed;
            main.startSize = startSize;
            main.startColor = color;
            main.maxParticles = 120;
            ParticleSystem.EmissionModule emission = particles.emission;
            emission.enabled = false;
            ParticleSystem.ShapeModule shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.35f;
            return particles;
        }

        private void PlayFeedback(ParticleSystem particles, Vector3 position, int count, AudioClip clip)
        {
            if (particles != null)
            {
                particles.transform.position = position;
                particles.Emit(Mathf.Max(1, count));
            }

            if (_feedbackAudio != null && clip != null)
            {
                _feedbackAudio.PlayOneShot(clip);
            }
        }

        private static AudioClip CreateTone(string name, float frequency, float durationSeconds, float volume)
        {
            const int sampleRate = 22050;
            int sampleCount = Mathf.Max(1, Mathf.CeilToInt(sampleRate * durationSeconds));
            float[] samples = new float[sampleCount];
            for (int i = 0; i < sampleCount; i++)
            {
                float t = i / (float)sampleRate;
                float fade = Mathf.Clamp01(1f - i / (float)sampleCount);
                samples[i] = Mathf.Sin(Mathf.PI * 2f * frequency * t) * volume * fade;
            }

            AudioClip clip = AudioClip.Create(name, sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private void BuildSampleArena()
        {
            CreateSolid("Arena Floor", new Vector3(0f, -0.55f, 0f), new Vector3(42f, 1f, 42f), new Color(0.24f, 0.25f, 0.27f, 1f));
            CreateSolid("Left Wallrun Wall", new Vector3(-7f, 2f, 0f), new Vector3(0.45f, 4f, 22f), new Color(0.16f, 0.22f, 0.31f, 1f));
            CreateSolid("Right Wallrun Wall", new Vector3(7f, 2f, 0f), new Vector3(0.45f, 4f, 22f), new Color(0.16f, 0.22f, 0.31f, 1f));
            CreateSolid("Forward Wallrun Gate Left", new Vector3(-3.2f, 2.4f, 13f), new Vector3(0.42f, 4.8f, 8f), new Color(0.18f, 0.2f, 0.34f, 1f));
            CreateSolid("Forward Wallrun Gate Right", new Vector3(3.2f, 2.4f, 13f), new Vector3(0.42f, 4.8f, 8f), new Color(0.18f, 0.2f, 0.34f, 1f));
            CreateSolid("Back Slide Ramp", new Vector3(0f, 0.35f, -14f), new Vector3(6.5f, 0.7f, 5.4f), Quaternion.Euler(-12f, 0f, 0f), new Color(0.3f, 0.28f, 0.2f, 1f));
            CreateSolid("Side Transfer Ramp Left", new Vector3(-10.5f, 0.45f, 4f), new Vector3(5.6f, 0.75f, 3.2f), Quaternion.Euler(0f, 0f, -10f), new Color(0.28f, 0.25f, 0.18f, 1f));
            CreateSolid("Side Transfer Ramp Right", new Vector3(10.5f, 0.45f, 4f), new Vector3(5.6f, 0.75f, 3.2f), Quaternion.Euler(0f, 0f, 10f), new Color(0.28f, 0.25f, 0.18f, 1f));
            CreateSolid("Low Flow Vault", new Vector3(0f, 0.55f, -1f), new Vector3(2.2f, 1.1f, 0.7f), new Color(0.32f, 0.28f, 0.18f, 1f));
            CreateSolid("Tall Safety Mantle", new Vector3(3.8f, 1.05f, 3.5f), new Vector3(2.2f, 2.1f, 0.7f), new Color(0.28f, 0.24f, 0.34f, 1f));
            CreateSolid("Enemy Spawn Read North", new Vector3(0f, 0.05f, 17.5f), new Vector3(8f, 0.1f, 0.35f), new Color(0.68f, 0.16f, 0.2f, 1f));
            CreateSolid("Enemy Spawn Read East", new Vector3(17.5f, 0.05f, 0f), new Vector3(0.35f, 0.1f, 8f), new Color(0.68f, 0.16f, 0.2f, 1f));
            CreateSolid("Enemy Spawn Read West", new Vector3(-17.5f, 0.05f, 0f), new Vector3(0.35f, 0.1f, 8f), new Color(0.68f, 0.16f, 0.2f, 1f));

            GameObject lightObject = new GameObject("Arena Directional Light");
            lightObject.transform.SetParent(_runtimeRoot, false);
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.15f;
        }

        private void CreateSolid(string name, Vector3 position, Vector3 scale, Color color)
        {
            CreateSolid(name, position, scale, Quaternion.identity, color);
        }

        private void CreateSolid(string name, Vector3 position, Vector3 scale, Quaternion rotation, Color color)
        {
            GameObject solid = GameObject.CreatePrimitive(PrimitiveType.Cube);
            solid.name = name;
            solid.transform.SetParent(_runtimeRoot, false);
            solid.transform.SetPositionAndRotation(position, rotation);
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
            PlayFeedback(_pickupPulse, position, 12, _pickupClip);
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
                    UnityObjectUtility.DestroySafely(child.gameObject);
                }
            }

            _enemies.Clear();
        }

        private static bool WasPressed(Key key)
        {
            return Keyboard.current != null && Keyboard.current[key].wasPressedThisFrame;
        }

        private void EnsureHudStyles()
        {
            if (_hudTitleStyle != null)
            {
                return;
            }

            _hudTitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 17,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.84f, 0.94f, 1f) }
            };
            _hudLabelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                normal = { textColor = Color.white }
            };
            _hudSmallStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                wordWrap = true,
                normal = { textColor = new Color(0.78f, 0.88f, 0.95f) }
            };
        }

        private void DrawHudBar(string label, float value, Color fill)
        {
            Rect rect = GUILayoutUtility.GetRect(392f, 18f);
            GUI.Box(rect, GUIContent.none);
            Rect fillRect = new Rect(rect.x + 2f, rect.y + 2f, Mathf.Max(0f, rect.width - 4f) * Mathf.Clamp01(value), rect.height - 4f);
            Color oldColor = GUI.color;
            GUI.color = fill;
            GUI.DrawTexture(fillRect, Texture2D.whiteTexture);
            GUI.color = oldColor;
            GUI.Label(rect, label + " " + Mathf.RoundToInt(Mathf.Clamp01(value) * 100f).ToString() + "%", _hudSmallStyle);
        }

        private string ResolveLoadoutLabel()
        {
            if (_player == null)
            {
                return "none";
            }

            return _player.Guns.Count.ToString() + " guns / " + _player.AutoPowers.Count.ToString() + " powers";
        }

        private static string FormatTime(float seconds)
        {
            int total = Mathf.Max(0, Mathf.FloorToInt(seconds));
            return (total / 60).ToString("00") + ":" + (total % 60).ToString("00");
        }

        private void OnGUI()
        {
            if (_player == null || _progression == null)
            {
                return;
            }

            EnsureHudStyles();
            GUILayout.BeginArea(new Rect(16f, 16f, 430f, 276f), GUI.skin.box);
            GUILayout.Label("Movement FPS Template", _hudTitleStyle);
            DrawHudBar("Health", _player.MaximumHealth <= 0d ? 0f : (float)(_player.CurrentHealth / _player.MaximumHealth), new Color(0.9f, 0.24f, 0.22f));
            DrawHudBar("XP", _progression.CurrentExperience / (float)_progression.RequiredExperience, new Color(0.24f, 0.82f, 1f));
            DrawHudBar("Run", Mathf.Clamp01(_elapsedSeconds / Mathf.Max(1f, _waveDefinition.VictoryTimeSeconds)), new Color(0.78f, 0.48f, 1f));
            GUILayout.Label($"Level {_progression.Level}   Time {FormatTime(_elapsedSeconds)}   State {_runState}", _hudLabelStyle);
            GUILayout.Label($"Enemies {_enemies.Count}/{CurrentSpawnSnapshot.MaxAlive}   Wave batch {CurrentSpawnSnapshot.BatchSize}", _hudLabelStyle);
            MovementFpsRunSummary summary = CurrentRunSummary;
            GUILayout.Label($"Kills {summary.KillCount}   XP gained {summary.ExperienceGained}   Upgrades {summary.UpgradesChosen.Count}", _hudLabelStyle);
            GUILayout.Label($"Move {_player.Motor.State}   Speed {Vector3.ProjectOnPlane(_player.Motor.Velocity, Vector3.up).magnitude:0.0}", _hudLabelStyle);
            GUILayout.Label("Loadout: " + ResolveLoadoutLabel(), _hudSmallStyle);
            if (_player.CurrentGun != null)
            {
                MovementFpsGunRuntimeState gun = _player.CurrentGun;
                GUILayout.Label($"{gun.Definition.DisplayName}  Ammo {gun.Ammo}/{gun.Definition.MagazineSize}{(gun.Reloading ? "  Reloading" : string.Empty)}", _hudLabelStyle);
            }

            if (_draftOpen)
            {
                GUILayout.Label("Choose upgrade: 1 / 2 / 3", _hudLabelStyle);
                for (int index = 0; index < _progression.CurrentDraft.Count; index++)
                {
                    GUILayout.Label($"{index + 1}. {_progression.CurrentDraft[index].Id.Value}", _hudSmallStyle);
                }
            }
            else if (_defeat)
            {
                GUILayout.Label("Defeated - press R to restart", _hudLabelStyle);
                GUILayout.Label($"Summary: survived {summary.ElapsedSeconds:0}s, kills {summary.KillCount}, XP {summary.ExperienceGained}", _hudSmallStyle);
            }
            else if (_victory)
            {
                GUILayout.Label("Victory - press R to restart", _hudLabelStyle);
                GUILayout.Label($"Summary: cleared in {summary.ElapsedSeconds:0}s, kills {summary.KillCount}, rewards {summary.Rewards.Count}", _hudSmallStyle);
            }
            else
            {
                GUILayout.Label("WASD move, Shift sprint, Ctrl/C slide, Space jump, mouse fire/look, Q swap guns", _hudSmallStyle);
            }

            GUILayout.EndArea();
        }
    }
}
