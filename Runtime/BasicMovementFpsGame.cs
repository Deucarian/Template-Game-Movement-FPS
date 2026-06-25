using Deucarian.Combat;
using Deucarian.RunUpgrades;

namespace Deucarian.TemplateGameMovementFps
{
    public static class BasicMovementFpsGame
    {
        public static readonly DamageTypeId KineticDamageType = new DamageTypeId("damage.movement-fps.kinetic");
        public static readonly RunUpgradeId CarbineDamageUpgradeId = new RunUpgradeId("upgrade.movement-fps.carbine-damage");
        public static readonly RunUpgradeId CarbineCadenceUpgradeId = new RunUpgradeId("upgrade.movement-fps.carbine-cadence");
        public static readonly RunUpgradeId PickupRangeUpgradeId = new RunUpgradeId("upgrade.movement-fps.pickup-range");
        public static readonly RunUpgradeEffectId AdditiveStatEffectId = new RunUpgradeEffectId("effect.stat.additive");
        public static readonly RunUpgradeEffectId MultiplierStatEffectId = new RunUpgradeEffectId("effect.stat.multiplier");
        public static readonly RunUpgradeTargetId GunDamageTargetId = new RunUpgradeTargetId("stat.weapon.primary.damage");
        public static readonly RunUpgradeTargetId GunCadenceTargetId = new RunUpgradeTargetId("stat.weapon.primary.cadence");
        public static readonly RunUpgradeTargetId PickupRadiusTargetId = new RunUpgradeTargetId("stat.player.pickup-radius");

        public const int FirstLevelExperienceRequirement = 10;
        public const int DraftChoiceCount = 3;

        public static MovementFpsGunDefinition CreateCarbineDefinition()
        {
            return new MovementFpsGunDefinition(
                id: "weapon.aether-carbine",
                displayName: "Aether Carbine",
                damage: 26d,
                fireIntervalSeconds: 0.16f,
                range: 80f,
                spreadDegrees: 0.45f,
                recoilPitchDegrees: 0.75f);
        }

        public static MovementFpsEnemyDefinition CreateEnemyDefinition()
        {
            return new MovementFpsEnemyDefinition(
                id: "enemy.husk-thrall",
                maximumHealth: 52d,
                moveSpeed: 3.8f,
                contactDamage: 8d,
                experienceDrop: 12);
        }

        public static MovementFpsPlayerDefinition CreatePlayerDefinition()
        {
            return new MovementFpsPlayerDefinition(
                maximumHealth: 100d,
                pickupRadius: 4.2f);
        }

        public static CombatCatalog CreateCombatCatalog()
        {
            return new CombatCatalog(new[] { new DamageTypeDefinition(KineticDamageType) });
        }

        public static RunUpgradeCatalog CreateUpgradeCatalog()
        {
            return new RunUpgradeCatalog(new[]
            {
                new RunUpgradeDefinition(
                    CarbineDamageUpgradeId,
                    RunUpgradeRarity.Common,
                    weight: 12,
                    maxRank: 5,
                    effects: new[]
                    {
                        new RunUpgradeEffectDescriptor(AdditiveStatEffectId, GunDamageTargetId, 8d)
                    }),
                new RunUpgradeDefinition(
                    CarbineCadenceUpgradeId,
                    RunUpgradeRarity.Uncommon,
                    weight: 7,
                    maxRank: 4,
                    effects: new[]
                    {
                        new RunUpgradeEffectDescriptor(MultiplierStatEffectId, GunCadenceTargetId, 0.12d)
                    }),
                new RunUpgradeDefinition(
                    PickupRangeUpgradeId,
                    RunUpgradeRarity.Common,
                    weight: 8,
                    maxRank: 3,
                    effects: new[]
                    {
                        new RunUpgradeEffectDescriptor(AdditiveStatEffectId, PickupRadiusTargetId, 1.25d)
                    })
            });
        }
    }
}
