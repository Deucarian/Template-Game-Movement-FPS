using System.IO;
using Deucarian.Combat;
using Deucarian.RunUpgrades;
using Deucarian.TemplateGameMovementFps.Content;
using Deucarian.TemplateGameMovementFps.Progression;
using Deucarian.TemplateGameMovementFps.Run;
using NUnit.Framework;
using UnityEditor.PackageManager;

namespace Deucarian.TemplateGameMovementFps.Tests
{
    public sealed class MovementFpsTemplateEditModeTests
    {
        [Test]
        public void DefaultContentUsesStableIdsAndReferenceLikeTuning()
        {
            MovementFpsGunDefinition gun = BasicMovementFpsGame.CreateCarbineDefinition();
            MovementFpsGunDefinition launcher = BasicMovementFpsGame.CreateRiftLauncherDefinition();
            MovementFpsAutoPowerDefinition pulse = BasicMovementFpsGame.CreateOrbitPulseDefinition();
            MovementFpsAutoPowerDefinition chain = BasicMovementFpsGame.CreateChainBoltDefinition();
            MovementFpsAutoPowerDefinition rift = BasicMovementFpsGame.CreateGroundRiftDefinition();
            MovementFpsEnemyDefinition enemy = BasicMovementFpsGame.CreateEnemyDefinition();
            MovementFpsEnemyDefinition runner = BasicMovementFpsGame.CreateLeapingRunnerDefinition();
            MovementFpsEnemyDefinition bulwark = BasicMovementFpsGame.CreateBoneBulwarkDefinition();
            MovementFpsEnemyDefinition miniBoss = BasicMovementFpsGame.CreateChoirOgreDefinition();
            MovementFpsWaveDefinition wave = BasicMovementFpsGame.CreatePrototypeWaveDefinition();
            MovementFpsPlayerDefinition player = BasicMovementFpsGame.CreatePlayerDefinition();
            RunUpgradeCatalog upgrades = BasicMovementFpsGame.CreateUpgradeCatalog();
            CombatCatalog combat = BasicMovementFpsGame.CreateCombatCatalog();

            Assert.AreEqual("weapon.aether-carbine", gun.Id);
            Assert.AreEqual(26d, gun.Damage);
            Assert.AreEqual(0.16f, gun.FireIntervalSeconds);
            Assert.AreEqual(80f, gun.Range);
            Assert.AreEqual("gun.rift-launcher", launcher.Id);
            Assert.AreEqual(MovementFpsGunKind.Projectile, launcher.Kind);
            Assert.AreEqual(38d, launcher.Damage);
            Assert.AreEqual(6, launcher.MagazineSize);
            Assert.AreEqual(30f, launcher.ProjectileSpeed);
            Assert.AreEqual("power.orbit-pulse", pulse.Id);
            Assert.AreEqual(MovementFpsAutoPowerKind.OrbitPulse, pulse.Kind);
            Assert.AreEqual("power.chain-bolt", chain.Id);
            Assert.AreEqual(3, chain.TargetCount);
            Assert.AreEqual("power.ground-rift", rift.Id);
            Assert.AreEqual("enemy.husk-thrall", enemy.Id);
            Assert.AreEqual(32d, enemy.MaximumHealth);
            Assert.AreEqual(1, enemy.ExperienceDrop);
            Assert.AreEqual("enemy.leaping-runner", runner.Id);
            Assert.AreEqual(6.4f, runner.MoveSpeed);
            Assert.AreEqual("enemy.bone-bulwark", bulwark.Id);
            Assert.AreEqual(96d, bulwark.MaximumHealth);
            Assert.AreEqual("enemy.choir-ogre", miniBoss.Id);
            Assert.IsTrue(miniBoss.IsMiniBoss);
            Assert.AreEqual("wave.prototype-five-minute", wave.Id);
            Assert.AreEqual(3, wave.Segments.Count);
            Assert.AreEqual(115f, wave.MiniBossSpawnTimeSeconds);
            Assert.AreEqual(135f, wave.VictoryTimeSeconds);
            Assert.AreEqual(100d, player.MaximumHealth);
            Assert.AreEqual(4.2f, player.PickupRadius);
            Assert.AreEqual(8, upgrades.Definitions.Count);
            Assert.IsTrue(combat.TryGetDamageType(BasicMovementFpsGame.KineticDamageType, out _));
            Assert.IsTrue(combat.TryGetDamageType(BasicMovementFpsGame.VoidDamageType, out _));
            Assert.IsTrue(combat.TryGetDamageType(BasicMovementFpsGame.StormDamageType, out _));
            Assert.IsTrue(combat.TryGetDamageType(BasicMovementFpsGame.BoneDamageType, out _));
        }

        [Test]
        public void RunProgressionOpensDraftAndAppliesSelection()
        {
            var progression = new MovementFpsRunProgression(BasicMovementFpsGame.CreateUpgradeCatalog(), baseRequirement: 5);

            int levels = progression.GainExperience(5);
            RunUpgradeDefinition first = progression.CurrentDraft[0];
            RunUpgradeSelectionResult result = progression.ChooseDraft(0);

            Assert.AreEqual(1, levels);
            Assert.AreEqual(2, progression.Level);
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(1, progression.GetUpgradeRank(first.Id));
            Assert.IsFalse(progression.HasDraft);
            Assert.IsTrue(
                progression.GunDamageBonus > 0d ||
                progression.GunCadenceMultiplier > 0d ||
                progression.PickupRadiusBonus > 0d ||
                progression.ProjectileSpeedMultiplier > 0d ||
                progression.AutoPowerDamageMultiplier > 0d ||
                progression.GetUpgradeRank(BasicMovementFpsGame.RiftLauncherUnlockUpgradeId) > 0 ||
                progression.GetUpgradeRank(BasicMovementFpsGame.ChainBoltUnlockUpgradeId) > 0 ||
                progression.GetUpgradeRank(BasicMovementFpsGame.GroundRiftUnlockUpgradeId) > 0);
        }

        [Test]
        public void InvalidDraftChoiceIsRejected()
        {
            var progression = new MovementFpsRunProgression(BasicMovementFpsGame.CreateUpgradeCatalog());

            RunUpgradeSelectionResult result = progression.ChooseDraft(0);

            Assert.AreEqual(RunUpgradeSelectionStatus.UnknownUpgrade, result.Status);
        }

        [Test]
        public void ProjectileAndPowerUpgradeEffectsApplyLocally()
        {
            var progression = new MovementFpsRunProgression(BasicMovementFpsGame.CreateUpgradeCatalog());

            RunUpgradeSelectionResult projectileResult = progression.ApplyUpgradeById(BasicMovementFpsGame.ProjectileSpeedUpgradeId);
            RunUpgradeSelectionResult powerResult = progression.ApplyUpgradeById(BasicMovementFpsGame.AutoPowerDamageUpgradeId);

            Assert.IsTrue(projectileResult.Succeeded);
            Assert.IsTrue(powerResult.Succeeded);
            Assert.That(progression.ProjectileSpeedMultiplier, Is.GreaterThan(0d));
            Assert.That(progression.AutoPowerDamageMultiplier, Is.GreaterThan(0d));
        }

        [Test]
        public void WaveEscalationResolvesLaterSegmentsAndPressure()
        {
            MovementFpsWaveDefinition wave = BasicMovementFpsGame.CreatePrototypeWaveDefinition();

            MovementFpsWaveSpawnSnapshot start = wave.ResolveSpawnSnapshot(0f, 1f);
            MovementFpsWaveSpawnSnapshot mid = wave.ResolveSpawnSnapshot(80f, 1f);
            MovementFpsWaveSpawnSnapshot late = wave.ResolveSpawnSnapshot(190f, 1f);

            Assert.AreEqual(0.9f, start.SpawnIntervalSeconds, 0.001f);
            Assert.AreEqual(3, start.BatchSize);
            Assert.AreEqual(28, start.MaxAlive);
            Assert.That(mid.SpawnIntervalSeconds, Is.LessThan(0.72f));
            Assert.That(mid.BatchSize, Is.GreaterThanOrEqualTo(3));
            Assert.That(late.SpawnIntervalSeconds, Is.LessThan(0.58f));
            Assert.That(late.MaxAlive, Is.GreaterThan(52));
        }

        [Test]
        public void WaveDirectorAppliesRuntimeTuning()
        {
            var director = new MovementFpsWaveDirector(BasicMovementFpsGame.CreatePrototypeWaveDefinition(), seed: 123)
            {
                SpawnIntervalMultiplier = 2f,
                SpawnBatchMultiplier = 0.5f,
                MaxAliveOverride = 9
            };

            MovementFpsWaveSpawnSnapshot snapshot = director.ResolveRuntimeSnapshot(190f);

            Assert.That(snapshot.SpawnIntervalSeconds, Is.GreaterThan(0.58f));
            Assert.AreEqual(3, snapshot.BatchSize);
            Assert.AreEqual(9, snapshot.MaxAlive);
        }

        [Test]
        public void DefaultContentValidationPasses()
        {
            MovementFpsContentValidationReport report = MovementFpsContentValidator.Validate(BasicMovementFpsGame.CreateContentLibrary());

            Assert.IsTrue(report.IsValid, string.Join("\n", report.Errors));
        }

        [Test]
        public void ContentValidationReportsInvalidIdsAndReferences()
        {
            MovementFpsGunDefinition carbine = BasicMovementFpsGame.CreateCarbineDefinition();
            MovementFpsAutoPowerDefinition pulse = BasicMovementFpsGame.CreateOrbitPulseDefinition();
            MovementFpsEnemyDefinition husk = BasicMovementFpsGame.CreateEnemyDefinition();
            MovementFpsEnemyDefinition missingRunner = BasicMovementFpsGame.CreateLeapingRunnerDefinition();
            MovementFpsEnemyDefinition missingBoss = BasicMovementFpsGame.CreateChoirOgreDefinition();
            var invalidUpgrade = new RunUpgradeDefinition(
                new RunUpgradeId("upgrade.invalid-target"),
                RunUpgradeRarity.Common,
                weight: 1,
                maxRank: 1,
                effects: new[]
                {
                    new RunUpgradeEffectDescriptor(
                        BasicMovementFpsGame.AdditiveStatEffectId,
                        new RunUpgradeTargetId("stat.missing.target"),
                        1d)
                });
            var invalidWave = new MovementFpsWaveDefinition(
                id: "wave.invalid",
                displayName: "Invalid Wave",
                segments: new[]
                {
                    new MovementFpsWaveSegmentDefinition(
                        startTimeSeconds: 0f,
                        spawnIntervalSeconds: 1f,
                        batchSize: 1,
                        maxAlive: 1,
                        enemies: new[]
                        {
                            new MovementFpsWeightedEnemyDefinition(missingRunner, 0f)
                        })
                },
                miniBossEnemy: missingBoss,
                miniBossSpawnTimeSeconds: 1f,
                victoryTimeSeconds: 2f,
                escalationPerMinute: 1f);
            var invalidLibrary = new MovementFpsContentLibrary(
                guns: new[] { carbine, carbine },
                powers: new[] { pulse, pulse },
                enemies: new[] { husk, husk },
                upgrades: new RunUpgradeCatalog(new[] { invalidUpgrade }),
                startingLoadout: new MovementFpsStartingLoadoutDefinition(
                    "loadout.invalid",
                    new[] { "gun.missing" },
                    new[] { "power.missing" }),
                wave: invalidWave);

            MovementFpsContentValidationReport report = MovementFpsContentValidator.Validate(invalidLibrary);

            Assert.IsFalse(report.IsValid);
            string errors = string.Join("\n", report.Errors);
            StringAssert.Contains("Duplicate weapon id", errors);
            StringAssert.Contains("Duplicate power id", errors);
            StringAssert.Contains("Duplicate enemy id", errors);
            StringAssert.Contains("unknown target", errors);
            StringAssert.Contains("starting gun reference 'gun.missing' does not exist", errors);
            StringAssert.Contains("starting power reference 'power.missing' does not exist", errors);
            StringAssert.Contains("non-positive weight", errors);
            StringAssert.Contains("references missing enemy", errors);
            StringAssert.Contains("references missing miniboss enemy", errors);
        }

        [Test]
        public void RunSummaryTracksKillsExperienceUpgradesAndRewards()
        {
            var tracker = new MovementFpsRunSummaryTracker();

            tracker.RecordKill(miniBoss: false);
            tracker.RecordKill(miniBoss: true);
            tracker.RecordExperience(12);
            tracker.RecordUpgrade(BasicMovementFpsGame.CarbineDamageUpgradeId.Value);
            tracker.RecordReward(new MovementFpsRunReward(BasicMovementFpsGame.ChoirOgreRewardId, "Choir Ogre Banished", 1));
            tracker.Complete(MovementFpsRunOutcome.Victory);

            MovementFpsRunSummary summary = tracker.CreateSummary(91.5f);

            Assert.AreEqual(MovementFpsRunOutcome.Victory, summary.Outcome);
            Assert.AreEqual(2, summary.KillCount);
            Assert.AreEqual(1, summary.MiniBossKills);
            Assert.AreEqual(12, summary.ExperienceGained);
            Assert.AreEqual(1, summary.UpgradesChosen.Count);
            Assert.AreEqual(1, summary.Rewards.Count);
            Assert.IsTrue(summary.Completed);

            tracker.Reset();
            MovementFpsRunSummary reset = tracker.CreateSummary(0f);
            Assert.AreEqual(MovementFpsRunOutcome.Running, reset.Outcome);
            Assert.AreEqual(0, reset.KillCount);
            Assert.AreEqual(0, reset.ExperienceGained);
            Assert.AreEqual(0, reset.UpgradesChosen.Count);
        }

        [Test]
        public void SampleLoadoutJsonIsPresent()
        {
            var packageInfo = PackageInfo.FindForAssembly(typeof(BasicMovementFpsGame).Assembly);
            Assert.IsNotNull(packageInfo);

            string loadoutPath = Path.Combine(packageInfo.resolvedPath, "Samples~", "BasicMovementFpsGame", "Content", "DefaultLoadout", "loadout.json");

            Assert.IsTrue(File.Exists(loadoutPath), "Sample loadout missing: " + loadoutPath);
            string json = File.ReadAllText(loadoutPath);
            StringAssert.Contains("weapon.aether-carbine", json);
            StringAssert.Contains("loadout.wallrunner-default", json);
            StringAssert.Contains("gun.rift-launcher", json);
            StringAssert.Contains("power.orbit-pulse", json);
            StringAssert.Contains("power.chain-bolt", json);
            StringAssert.Contains("power.ground-rift", json);
            StringAssert.Contains("enemy.husk-thrall", json);
            StringAssert.Contains("enemy.leaping-runner", json);
            StringAssert.Contains("enemy.bone-bulwark", json);
            StringAssert.Contains("enemy.choir-ogre", json);
            StringAssert.Contains("wave.prototype-five-minute", json);
            StringAssert.Contains("reward.choir-ogre-banished", json);
        }
    }
}
