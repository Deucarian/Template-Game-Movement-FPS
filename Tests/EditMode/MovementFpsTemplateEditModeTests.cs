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
            MovementFpsEnemyDefinition enemy = BasicMovementFpsGame.CreateEnemyDefinition();
            MovementFpsPlayerDefinition player = BasicMovementFpsGame.CreatePlayerDefinition();
            RunUpgradeCatalog upgrades = BasicMovementFpsGame.CreateUpgradeCatalog();
            CombatCatalog combat = BasicMovementFpsGame.CreateCombatCatalog();

            Assert.AreEqual("weapon.aether-carbine", gun.Id);
            Assert.AreEqual(26d, gun.Damage);
            Assert.AreEqual(0.16f, gun.FireIntervalSeconds);
            Assert.AreEqual(80f, gun.Range);
            Assert.AreEqual("enemy.husk-thrall", enemy.Id);
            Assert.AreEqual(52d, enemy.MaximumHealth);
            Assert.AreEqual(12, enemy.ExperienceDrop);
            Assert.AreEqual(100d, player.MaximumHealth);
            Assert.AreEqual(4.2f, player.PickupRadius);
            Assert.AreEqual(3, upgrades.Definitions.Count);
            Assert.IsTrue(combat.TryGetDamageType(BasicMovementFpsGame.KineticDamageType, out _));
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
                progression.PickupRadiusBonus > 0d);
        }

        [Test]
        public void InvalidDraftChoiceIsRejected()
        {
            var progression = new MovementFpsRunProgression(BasicMovementFpsGame.CreateUpgradeCatalog());

            RunUpgradeSelectionResult result = progression.ChooseDraft(0);

            Assert.AreEqual(RunUpgradeSelectionStatus.UnknownUpgrade, result.Status);
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
            StringAssert.Contains("enemy.husk-thrall", json);
        }
    }
}
