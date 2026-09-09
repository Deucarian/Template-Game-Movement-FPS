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

        private MovementFpsDefaultContent _content;
        private MovementFpsUnityWorld _world;
        private MovementFpsRunSession _run;
        private MovementFpsFeedbackPresenter _feedback;
        private readonly MovementFpsHudPresenter _hud = new MovementFpsHudPresenter();

        public MovementFpsPlayerController Player => _world == null ? null : _world.Player;
        public CombatCatalog CombatCatalog => _content == null ? null : _content.CombatCatalog;
        public MovementFpsRunProgression Progression => _run == null ? null : _run.Progression;
        public bool IsGameplayPaused => _run != null && _run.IsGameplayPaused;
        public bool DraftOpen => _run != null && _run.DraftOpen;
        public bool Defeated => _run != null && _run.Defeated;
        public bool Victory => _run != null && _run.Victory;
        public bool MiniBossSpawned => _run != null && _run.MiniBossSpawned;
        public bool MiniBossDefeated => _run != null && _run.MiniBossDefeated;
        public MovementFpsRunState RunState => _run == null ? default : _run.RunState;
        public float RunElapsedSeconds => _run == null ? 0f : _run.RunElapsedSeconds;
        public MovementFpsWaveSpawnSnapshot CurrentSpawnSnapshot => _run == null ? default : _run.CurrentSpawnSnapshot;
        public MovementFpsRunSummary CurrentRunSummary => _run == null ? new MovementFpsRunSummaryTracker().CreateSummary(0f) : _run.CurrentRunSummary;
        public IReadOnlyList<MovementFpsEnemyActor> Enemies => _world == null ? Array.Empty<MovementFpsEnemyActor>() : _world.Enemies;
        public int EnemyCount => _world == null ? 0 : _world.EnemyCount;
        public float PlayerPickupRadius => (_content == null ? 0f : _content.Player.PickupRadius) + (float)(Progression == null ? 0d : Progression.PickupRadiusBonus);

        private void Awake() => EnsureBootstrapped();

        private void Update()
        {
            _run?.Tick(Time.deltaTime, MovementFpsRunInput.ReadKeyboard(), enemySpawningEnabled, CaptureWaveSettings());
        }

        public void EnsureBootstrapped()
        {
            if (_content != null) return;
            _content = new MovementFpsDefaultContent();
            try
            {
                _world = new MovementFpsUnityWorld(transform, this, _content);
                _feedback = new MovementFpsFeedbackPresenter(_world.Root, () => Player == null ? Vector3.zero : Player.transform.position);
                _world.SetFeedback(_feedback);
                _run = new MovementFpsRunSession(_content, _world, _feedback, () => Random.Range(int.MinValue, int.MaxValue));
                if (buildSampleArenaOnAwake) new MovementFpsArenaBuilder(_world.Root).Build();
                _feedback.Build();
                _world.CreatePlayer();
                StartRun();
            }
            catch
            {
                ReleaseRuntime();
                throw;
            }
        }

        private MovementFpsWaveSettings CaptureWaveSettings()
        {
            return new MovementFpsWaveSettings(spawnIntervalMultiplier, spawnBatchMultiplier,
                escalationMultiplier, maxAliveOverride, miniBossSpawningEnabled);
        }

        private void OnDestroy() => ReleaseRuntime();

        private void ReleaseRuntime()
        {
            _feedback?.Dispose();
            _world?.Dispose();
            _feedback = null;
            _world = null;
            _run = null;
            _content = null;
        }

        public void StartRun()
        {
            EnsureBootstrapped();
            _run.Start(CaptureWaveSettings());
        }

        public void RestartRun() => StartRun();

        public MovementFpsEnemyActor SpawnEnemy()
        {
            EnsureBootstrapped();
            return _world.SpawnEnemy();
        }

        public MovementFpsEnemyActor SpawnEnemy(Vector3 position)
        {
            EnsureBootstrapped();
            return _world.SpawnEnemy(position);
        }

        public MovementFpsEnemyActor SpawnEnemy(MovementFpsEnemyDefinition definition, Vector3 position)
        {
            EnsureBootstrapped();
            return _world.SpawnEnemy(definition, position);
        }

        public MovementFpsProjectileActor SpawnProjectile(MovementFpsPlayerController owner, Vector3 origin,
            Vector3 direction, MovementFpsGunDefinition gun, double damage, float resolvedSpeed)
        {
            EnsureBootstrapped();
            return _world.SpawnProjectile(owner, origin, direction, gun, damage, resolvedSpeed);
        }

        public void HandleEnemyKilled(MovementFpsEnemyActor enemy)
        {
            if (enemy == null) return;
            _world.RemoveEnemy(enemy);
            _run.RecordEnemyKilled(enemy.IsMiniBoss, enemy.transform.position, enemy.ExperienceDrop);
            _world.DestroyEnemy(enemy);
        }

        public void DamageEnemiesInRadius(Vector3 center, float radius, double damage, DamageTypeId damageType)
        {
            _world.DamageEnemiesInRadius(center, radius, damage, damageType);
        }

        public IReadOnlyList<MovementFpsEnemyActor> GetNearestEnemies(Vector3 origin, float range, int targetCount)
        {
            return _world.GetNearestEnemies(origin, range, targetCount);
        }

        public void ApplyPlayerDamage(double amount)
        {
            if (Player != null) Player.ApplyDamage(amount);
        }

        public void HandlePlayerDefeated() => _run.HandlePlayerDefeated();
        public void CollectExperience(int amount) => _run.CollectExperience(amount);
        public RunUpgradeSelectionResult ChooseDraft(int index) => _run.ChooseDraft(index);
        public MovementFpsEnemyDefinition GetEnemyDefinitionForTest(string id) => _content.FindEnemy(id);
        public void TickRunForTest(float deltaTime) => _run.TickForTest(deltaTime, enemySpawningEnabled);
        public RunUpgradeSelectionResult ApplyUpgradeByIdForTest(RunUpgradeId id) => _run.ApplyUpgradeByIdForTest(id);

        internal void PlayWeaponFeedback(Vector3 origin, Vector3 direction, MovementFpsGunKind kind)
        {
            _feedback.PlayWeaponFeedback(origin, direction, kind);
        }

        internal void PlayPowerFeedback(Vector3 position, MovementFpsAutoPowerKind kind)
        {
            _feedback.PlayPowerFeedback(position, kind);
        }

        private void OnGUI()
        {
            if (Player == null || Progression == null) return;
            _hud.Draw(new MovementFpsHudSnapshot(_run, Player, EnemyCount, _content.Wave.VictoryTimeSeconds));
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
            return SpawnEnemy(_content.ChoirOgre, position);
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
            return Player.FireAt(enemy);
        }

        public MovementFpsProjectileActor FireProjectileAtEnemyForTest(MovementFpsEnemyActor enemy)
        {
            Player.AddGun(_content.RiftLauncher);
            return Player.FireProjectileAtForTest(enemy);
        }

        public void AddPowerForTest(MovementFpsAutoPowerDefinition definition)
        {
            Player.AddAutoPower(definition);
        }

        public void TickPlayerPowersForTest(float deltaTime)
        {
            Player.TickAutoPowersForTest(deltaTime);
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
    }
}
