using System.IO;
using Deucarian.Combat;
using Deucarian.RunUpgrades;
using Deucarian.TemplateGameMovementFps.Progression;
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
            Assert.AreEqual(52d, enemy.MaximumHealth);
            Assert.AreEqual(12, enemy.ExperienceDrop);
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
        public void SampleLoadoutJsonIsPresent()
        {
            var packageInfo = PackageInfo.FindForAssembly(typeof(BasicMovementFpsGame).Assembly);
            Assert.IsNotNull(packageInfo);

            string loadoutPath = Path.Combine(packageInfo.resolvedPath, "Samples~", "BasicMovementFpsGame", "Content", "DefaultLoadout", "loadout.json");

            Assert.IsTrue(File.Exists(loadoutPath), "Sample loadout missing: " + loadoutPath);
            string json = File.ReadAllText(loadoutPath);
            StringAssert.Contains("weapon.aether-carbine", json);
            StringAssert.Contains("gun.rift-launcher", json);
            StringAssert.Contains("power.orbit-pulse", json);
            StringAssert.Contains("power.chain-bolt", json);
            StringAssert.Contains("power.ground-rift", json);
            StringAssert.Contains("enemy.husk-thrall", json);
        }
    }
}
