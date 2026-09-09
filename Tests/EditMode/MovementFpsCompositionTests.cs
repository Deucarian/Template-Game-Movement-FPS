using System;
using System.Collections.Generic;
using Deucarian.RunUpgrades;
using Deucarian.TemplateGameMovementFps.Actors;
using Deucarian.TemplateGameMovementFps.Run;
using NUnit.Framework;
using UnityEngine;

namespace Deucarian.TemplateGameMovementFps.Tests
{
    public sealed class MovementFpsCompositionTests
    {
        private static readonly MovementFpsWaveSettings Settings = new MovementFpsWaveSettings(1f, 1f, 1f, 0, true);

        [Test]
        public void RunRestartClearsProgressionSummaryAndEncounterStateWithoutSceneObjects()
        {
            var host = new RunHost();
            int seeds = 0;
            var run = new MovementFpsRunSession(new MovementFpsDefaultContent(), host, host, () => ++seeds);
            run.Start(Settings);
            run.TickForTest(76f, true);
            Assert.That(host.EnemyCount, Is.GreaterThan(3));
            run.RecordEnemyKilled(false, Vector3.forward, 2);
            run.CollectExperience(6);
            run.HandlePlayerDefeated();
            float elapsed = run.RunElapsedSeconds;
            run.Tick(100f, default, true, Settings);
            Assert.That(run.RunElapsedSeconds, Is.EqualTo(elapsed));
            Assert.That(run.CurrentRunSummary.KillCount, Is.EqualTo(1));
            Assert.That(run.CurrentRunSummary.ExperienceGained, Is.EqualTo(6));

            host.Events.Clear();
            run.Tick(0.5f, new MovementFpsRunInput(true, true, true, true), true, Settings);
            CollectionAssert.AreEqual(new[] { "clear", "reset-player", "initial-enemies" }, host.Events);
            Assert.That(run.RunState, Is.EqualTo(MovementFpsRunState.Running));
            Assert.That(run.RunElapsedSeconds, Is.Zero);
            Assert.That(run.CurrentRunSummary.KillCount, Is.Zero);
            Assert.That(run.CurrentRunSummary.ExperienceGained, Is.Zero);
            Assert.That(run.Progression.Level, Is.EqualTo(1));
            Assert.That(host.EnemyCount, Is.EqualTo(3));
            Assert.That(seeds, Is.EqualTo(2));
        }

        [Test]
        public void DraftInputUsesFirstChoiceAndPausesClockForTheSelectionFrame()
        {
            var host = new RunHost();
            var run = new MovementFpsRunSession(new MovementFpsDefaultContent(), host, host, () => 17);
            run.Start(Settings);
            run.CollectExperience(run.Progression.RequiredExperience);
            string first = run.Progression.CurrentDraft[0].Id.Value;
            host.Events.Clear();
            run.Tick(1f, new MovementFpsRunInput(false, true, true, true), true, Settings);
            CollectionAssert.AreEqual(new[] { "upgrade:" + first, "upgrade-feedback" }, host.Events);
            Assert.That(run.CurrentRunSummary.UpgradesChosen, Does.Contain(first));
            Assert.That(run.RunElapsedSeconds, Is.Zero);
            Assert.That(run.RunState, Is.EqualTo(MovementFpsRunState.Running));
            run.Tick(0.25f, default, false, Settings);
            Assert.That(run.RunElapsedSeconds, Is.EqualTo(0.25f));
        }

        [Test]
        public void MiniBossDeathOrdersPickupAndFeedbackBeforeVictoryAndRejectsLaterDefeat()
        {
            var host = new RunHost();
            var run = new MovementFpsRunSession(new MovementFpsDefaultContent(), host, host, () => 17);
            run.Start(Settings);
            host.Events.Clear();
            run.RecordEnemyKilled(true, new Vector3(1f, 2f, 3f), 9);
            CollectionAssert.AreEqual(new[] { "pickup:9", "enemy-feedback:True", "victory-feedback" }, host.Events);
            Assert.That(host.LastPickup, Is.EqualTo(new Vector3(1f, 2.4f, 3f)));
            Assert.That(run.MiniBossDefeated, Is.True);
            Assert.That(run.CurrentRunSummary.MiniBossKills, Is.EqualTo(1));
            Assert.That(run.CurrentRunSummary.Rewards.Count, Is.EqualTo(1));
            run.HandlePlayerDefeated();
            run.CollectExperience(8);
            Assert.That(run.RunState, Is.EqualTo(MovementFpsRunState.Victory));
            Assert.That(run.Defeated, Is.False);
            Assert.That(run.CurrentRunSummary.ExperienceGained, Is.Zero);
        }

        [Test]
        public void DisabledSpawningAndNegativeTestDeltaDoNotCreatePressureOrReverseElapsedTime()
        {
            var host = new RunHost();
            var run = new MovementFpsRunSession(new MovementFpsDefaultContent(), host, host, () => 17);
            run.Start(Settings);
            run.TickForTest(76f, false);
            run.TickForTest(-3f, false);
            Assert.That(run.RunElapsedSeconds, Is.EqualTo(76f));
            Assert.That(host.EnemyCount, Is.EqualTo(3));
        }

        [Test]
        public void LoadoutCopiesStartingDefinitionsDeduplicatesAndResetsUnlockedContent()
        {
            MovementFpsGunDefinition carbine = BasicMovementFpsGame.CreateCarbineDefinition();
            var starting = new List<MovementFpsGunDefinition> { carbine, carbine };
            var loadout = new MovementFpsPlayerLoadout();
            loadout.Configure(starting, new[] { BasicMovementFpsGame.CreateOrbitPulseDefinition() });
            starting.Clear();
            loadout.Reset();
            var guns = loadout.Guns;
            Assert.That(guns.Count, Is.EqualTo(1));
            Assert.That(loadout.AddGun(BasicMovementFpsGame.CreateRiftLauncherDefinition()), Is.True);
            Assert.That(loadout.AddGun(carbine), Is.False);
            var effects = new LoadoutEffects();
            loadout.TickGun(true, false, false, 0f, 1f, effects);
            Assert.That(loadout.CurrentGun.Ammo, Is.EqualTo(carbine.MagazineSize - 1));
            loadout.Reset();
            Assert.That(loadout.Guns, Is.SameAs(guns));
            Assert.That(loadout.Guns.Count, Is.EqualTo(1));
            Assert.That(loadout.CurrentGun.Ammo, Is.EqualTo(carbine.MagazineSize));
            Assert.That(loadout.AutoPowers.Count, Is.EqualTo(1));
        }

        [Test]
        public void GunSelectionPrecedesFireAndReloadPreventsFireAndRecoil()
        {
            MovementFpsGunDefinition launcher = BasicMovementFpsGame.CreateRiftLauncherDefinition();
            var loadout = new MovementFpsPlayerLoadout();
            loadout.Configure(new[] { BasicMovementFpsGame.CreateCarbineDefinition(), launcher }, null);
            loadout.Reset();
            var effects = new LoadoutEffects();
            loadout.TickGun(true, false, true, 0f, 1f, effects);
            CollectionAssert.AreEqual(new[] { "fire:" + launcher.Id, "recoil" }, effects.Events);
            Assert.That(loadout.CurrentGun.Definition.Id, Is.EqualTo(launcher.Id));
            effects.Events.Clear();
            loadout.TickGun(true, true, false, 0f, 1f, effects);
            Assert.That(loadout.CurrentGun.Reloading, Is.True);
            Assert.That(effects.Events, Is.Empty);
            loadout.TickGun(false, false, false, launcher.ReloadSeconds, 1f, effects);
            Assert.That(loadout.CurrentGun.Reloading, Is.False);
            Assert.That(loadout.CurrentGun.Ammo, Is.EqualTo(launcher.MagazineSize));
        }

        [Test]
        public void PowersTickInLoadoutOrderAndResetCooldownOnlyAfterDispatch()
        {
            var loadout = new MovementFpsPlayerLoadout();
            MovementFpsAutoPowerDefinition orbit = BasicMovementFpsGame.CreateOrbitPulseDefinition();
            MovementFpsAutoPowerDefinition chain = BasicMovementFpsGame.CreateChainBoltDefinition();
            loadout.Configure(null, new[] { orbit, chain });
            loadout.Reset();
            var effects = new LoadoutEffects();
            loadout.TickAutoPowers(Mathf.Max(orbit.CooldownSeconds, chain.CooldownSeconds), effects);
            CollectionAssert.AreEqual(new[] { "power:" + orbit.Id, "power:" + chain.Id }, effects.Events);
            Assert.That(loadout.AutoPowers[0].CooldownRemaining, Is.EqualTo(orbit.CooldownSeconds));
            Assert.That(loadout.AutoPowers[1].CooldownRemaining, Is.EqualTo(chain.CooldownSeconds));
            effects.Events.Clear();
            loadout.TickAutoPowers(0f, effects);
            Assert.That(effects.Events, Is.Empty);
        }

        [Test]
        public void HealthReportsOnlyNewDeathAndResetRestoresTheConfiguredMaximum()
        {
            var health = new MovementFpsPlayerHealth();
            health.Configure(BasicMovementFpsGame.CreateCombatCatalog(), 100d);
            health.Reset();
            Assert.That(health.ApplyDamage(25d), Is.False);
            Assert.That(health.CurrentHealth, Is.EqualTo(75d));
            Assert.That(health.ApplyDamage(999d), Is.True);
            Assert.That(health.ApplyDamage(999d), Is.False);
            health.Reset();
            Assert.That(health.IsAlive, Is.True);
            Assert.That(health.CurrentHealth, Is.EqualTo(100d));
        }

        private sealed class LoadoutEffects : IMovementFpsLoadoutEffects
        {
            internal readonly List<string> Events = new List<string>();
            public void FireGun(MovementFpsGunDefinition definition) => Events.Add("fire:" + definition.Id);
            public void ApplyRecoil(float degrees) => Events.Add("recoil");
            public void CastAutoPower(MovementFpsAutoPowerDefinition definition) => Events.Add("power:" + definition.Id);
        }

        private sealed class RunHost : IMovementFpsRunWorld, IMovementFpsRunFeedback
        {
            internal readonly List<string> Events = new List<string>();
            internal Vector3 LastPickup;
            public int EnemyCount { get; private set; }
            public void ClearCombatObjects() { Events.Add("clear"); EnemyCount = 0; }
            public void ResetPlayer() => Events.Add("reset-player");
            public void SpawnInitialEnemies() { Events.Add("initial-enemies"); EnemyCount = 3; }
            public bool SpawnWaveEnemy(MovementFpsEnemyDefinition definition) { EnemyCount++; return true; }
            public void ApplyUpgradeContent(RunUpgradeId id) => Events.Add("upgrade:" + id.Value);
            public void SpawnExperiencePickup(Vector3 position, int amount) { LastPickup = position; Events.Add("pickup:" + amount); }
            public void Defeated() => Events.Add("defeat-feedback");
            public void ExperienceCollected(bool draftOpened) => Events.Add("experience-feedback");
            public void UpgradeChosen() => Events.Add("upgrade-feedback");
            public void Victory() => Events.Add("victory-feedback");
            public void EnemyKilled(Vector3 position, bool miniBoss) => Events.Add("enemy-feedback:" + miniBoss);
        }
    }
}
