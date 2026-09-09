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
    internal interface IMovementFpsRunWorld
    {
        int EnemyCount { get; }
        void ClearCombatObjects();
        void ResetPlayer();
        void SpawnInitialEnemies();
        bool SpawnWaveEnemy(MovementFpsEnemyDefinition definition);
        void ApplyUpgradeContent(RunUpgradeId id);
        void SpawnExperiencePickup(Vector3 position, int amount);
    }

    internal interface IMovementFpsRunFeedback
    {
        void Defeated();
        void ExperienceCollected(bool draftOpened);
        void UpgradeChosen();
        void Victory();
        void EnemyKilled(Vector3 position, bool miniBoss);
    }

    internal readonly struct MovementFpsWaveSettings
    {
        internal MovementFpsWaveSettings(float interval, float batch, float escalation, int maxAlive, bool miniBoss)
        {
            Interval = interval;
            Batch = batch;
            Escalation = escalation;
            MaxAlive = maxAlive;
            MiniBoss = miniBoss;
        }
        internal float Interval { get; }
        internal float Batch { get; }
        internal float Escalation { get; }
        internal int MaxAlive { get; }
        internal bool MiniBoss { get; }
    }
}
