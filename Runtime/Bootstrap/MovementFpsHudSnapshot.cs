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
    internal readonly struct MovementFpsHudSnapshot
    {
        // Capture is the Unity read adapter. The presenter receives only values,
        // copied draft IDs and the existing immutable summary, never live state.
        internal MovementFpsHudSnapshot(MovementFpsRunSession run, MovementFpsPlayerController player, int enemyCount, float victoryTime)
        {
            CurrentHealth = player.CurrentHealth;
            MaximumHealth = player.MaximumHealth;
            CurrentExperience = run.Progression.CurrentExperience;
            RequiredExperience = run.Progression.RequiredExperience;
            Level = run.Progression.Level;
            Elapsed = run.RunElapsedSeconds;
            VictoryTime = victoryTime;
            EnemyCount = enemyCount;
            Spawn = run.CurrentSpawnSnapshot;
            Summary = run.CurrentRunSummary;
            RunState = run.RunState;
            MovementState = player.Motor.State;
            Speed = Vector3.ProjectOnPlane(player.Motor.Velocity, Vector3.up).magnitude;
            GunCount = player.Guns.Count;
            PowerCount = player.AutoPowers.Count;
            HasGun = player.CurrentGun != null;
            GunName = player.CurrentGun == null ? null : player.CurrentGun.Definition.DisplayName;
            Ammo = player.CurrentGun == null ? 0 : player.CurrentGun.Ammo;
            MagazineSize = player.CurrentGun == null ? 0 : player.CurrentGun.Definition.MagazineSize;
            Reloading = player.CurrentGun != null && player.CurrentGun.Reloading;
            DraftOpen = run.DraftOpen;
            Defeated = run.Defeated;
            Victory = run.Victory;
            var choices = new string[run.Progression.CurrentDraft.Count];
            for (int index = 0; index < choices.Length; index++) choices[index] = run.Progression.CurrentDraft[index].Id.Value;
            DraftChoices = Array.AsReadOnly(choices);
        }

        internal double CurrentHealth { get; }
        internal double MaximumHealth { get; }
        internal int CurrentExperience { get; }
        internal int RequiredExperience { get; }
        internal int Level { get; }
        internal float Elapsed { get; }
        internal float VictoryTime { get; }
        internal int EnemyCount { get; }
        internal MovementFpsWaveSpawnSnapshot Spawn { get; }
        internal MovementFpsRunSummary Summary { get; }
        internal MovementFpsRunState RunState { get; }
        internal WallrunnerMovementState MovementState { get; }
        internal float Speed { get; }
        internal int GunCount { get; }
        internal int PowerCount { get; }
        internal bool HasGun { get; }
        internal string GunName { get; }
        internal int Ammo { get; }
        internal int MagazineSize { get; }
        internal bool Reloading { get; }
        internal bool DraftOpen { get; }
        internal bool Defeated { get; }
        internal bool Victory { get; }
        internal IReadOnlyList<string> DraftChoices { get; }
    }
}
