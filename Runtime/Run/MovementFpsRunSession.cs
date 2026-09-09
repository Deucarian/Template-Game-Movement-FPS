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
    internal sealed class MovementFpsRunSession
    {
        private readonly MovementFpsDefaultContent _content;
        private readonly IMovementFpsRunWorld _world;
        private readonly IMovementFpsRunFeedback _feedback;
        private readonly Func<int> _nextSeed;
        private readonly MovementFpsRunSummaryTracker _runSummaryTracker = new MovementFpsRunSummaryTracker();
        private readonly MovementFpsRunProgression _progression = new MovementFpsRunProgression(BasicMovementFpsGame.CreateUpgradeCatalog());
        private MovementFpsWaveDirector _waveDirector;
        private float _elapsedSeconds;
        private bool _draftOpen;
        private bool _defeat;
        private bool _victory;
        private bool _miniBossDefeated;
        private MovementFpsRunState _runState;

        internal MovementFpsRunSession(MovementFpsDefaultContent content, IMovementFpsRunWorld world,
            IMovementFpsRunFeedback feedback, Func<int> nextSeed)
        {
            _content = content ?? throw new ArgumentNullException(nameof(content));
            _world = world ?? throw new ArgumentNullException(nameof(world));
            _feedback = feedback ?? throw new ArgumentNullException(nameof(feedback));
            _nextSeed = nextSeed ?? throw new ArgumentNullException(nameof(nextSeed));
        }

        internal MovementFpsRunProgression Progression => _progression;
        internal bool IsGameplayPaused => _draftOpen || _defeat || _victory;
        internal bool DraftOpen => _draftOpen;
        internal bool Defeated => _defeat;
        internal bool Victory => _victory;
        internal bool MiniBossSpawned => _waveDirector != null && _waveDirector.MiniBossSpawned;
        internal bool MiniBossDefeated => _miniBossDefeated;
        internal MovementFpsRunState RunState => _runState;
        internal float RunElapsedSeconds => _elapsedSeconds;
        internal MovementFpsWaveSpawnSnapshot CurrentSpawnSnapshot => _waveDirector == null ? default : _waveDirector.CurrentSnapshot;
        internal MovementFpsRunSummary CurrentRunSummary => _runSummaryTracker.CreateSummary(_elapsedSeconds);

        internal void Start(MovementFpsWaveSettings settings)
        {
            _world.ClearCombatObjects();
            _progression.Reset();
            _draftOpen = false;
            _defeat = false;
            _victory = false;
            _miniBossDefeated = false;
            _runState = MovementFpsRunState.Running;
            _elapsedSeconds = 0f;
            _runSummaryTracker.Reset();
            _waveDirector = new MovementFpsWaveDirector(_content.Wave, _nextSeed());
            _waveDirector.SpawnIntervalMultiplier = settings.Interval;
            _waveDirector.SpawnBatchMultiplier = settings.Batch;
            _waveDirector.EscalationMultiplier = settings.Escalation;
            _waveDirector.MaxAliveOverride = settings.MaxAlive;
            _waveDirector.MiniBossSpawningEnabled = settings.MiniBoss;
            _world.ResetPlayer();
            _world.SpawnInitialEnemies();
        }

        internal void Tick(float deltaTime, MovementFpsRunInput input, bool spawningEnabled, MovementFpsWaveSettings settings)
        {
            if (_defeat || _victory)
            {
                if (input.Restart) Start(settings);
                return;
            }
            if (_draftOpen)
            {
                if (input.First) ChooseDraft(0);
                else if (input.Second) ChooseDraft(1);
                else if (input.Third) ChooseDraft(2);
                return;
            }
            Advance(deltaTime, deltaTime, spawningEnabled);
        }

        internal void TickForTest(float deltaTime, bool spawningEnabled)
        {
            if (IsGameplayPaused) return;
            Advance(Mathf.Max(0f, deltaTime), deltaTime, spawningEnabled);
        }

        private void Advance(float elapsedDelta, float waveDelta, bool spawningEnabled)
        {
            _elapsedSeconds += elapsedDelta;
            if (spawningEnabled && _waveDirector != null)
                _waveDirector.Tick(waveDelta, _elapsedSeconds, _world.EnemyCount, _world.SpawnWaveEnemy);
            TryCompleteSurvivalVictory();
        }

        internal void RecordEnemyKilled(bool miniBoss, Vector3 position, int experience)
        {
            _runSummaryTracker.RecordKill(miniBoss);
            _world.SpawnExperiencePickup(position + Vector3.up * 0.4f, experience);
            _feedback.EnemyKilled(position, miniBoss);
            if (miniBoss)
            {
                _miniBossDefeated = true;
                _runSummaryTracker.RecordReward(new MovementFpsRunReward(BasicMovementFpsGame.ChoirOgreRewardId, "Choir Ogre Banished", 1));
                CompleteVictory();
            }
        }

        internal void HandlePlayerDefeated()
        {
            if (_victory)
            {
                return;
            }

            _defeat = true;
            _draftOpen = false;
            _runState = MovementFpsRunState.Defeated;
            _runSummaryTracker.Complete(MovementFpsRunOutcome.Defeat);
            _feedback.Defeated();
        }

        internal void CollectExperience(int amount)
        {
            if (_defeat || _victory)
            {
                return;
            }

            _runSummaryTracker.RecordExperience(amount);
            _progression.GainExperience(amount);
            _draftOpen = _progression.HasDraft;
            _runState = _draftOpen ? MovementFpsRunState.Draft : MovementFpsRunState.Running;
            _feedback.ExperienceCollected(_draftOpen);
        }

        internal RunUpgradeSelectionResult ChooseDraft(int index)
        {
            RunUpgradeSelectionResult result = _progression.ChooseDraft(index);
            if (result.Succeeded)
            {
                _world.ApplyUpgradeContent(result.Id);
                _runSummaryTracker.RecordUpgrade(result.Id.Value);
                _feedback.UpgradeChosen();
            }

            _draftOpen = _progression.HasDraft;
            _runState = _draftOpen ? MovementFpsRunState.Draft : MovementFpsRunState.Running;
            return result;
        }

        private void TryCompleteSurvivalVictory()
        {
            if (_victory || _defeat || _content.Wave == null || _content.Wave.HasMiniBoss)
            {
                return;
            }

            if (_elapsedSeconds >= _content.Wave.VictoryTimeSeconds)
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
            _feedback.Victory();
        }

        internal RunUpgradeSelectionResult ApplyUpgradeByIdForTest(RunUpgradeId id)
        {
            RunUpgradeSelectionResult result = _progression.ApplyUpgradeById(id);
            if (result.Succeeded)
            {
                _world.ApplyUpgradeContent(result.Id);
            }

            return result;
        }

    }
}
