using Deucarian.Combat;
using Deucarian.RunUpgrades;

namespace Deucarian.TemplateGameMovementFps
{
    public static class BasicMovementFpsGame
    {
        public static readonly DamageTypeId KineticDamageType = new DamageTypeId("damage.movement-fps.kinetic");
        public static readonly DamageTypeId VoidDamageType = new DamageTypeId("damage.movement-fps.void");
        public static readonly DamageTypeId StormDamageType = new DamageTypeId("damage.movement-fps.storm");
        public static readonly DamageTypeId BoneDamageType = new DamageTypeId("damage.movement-fps.bone");
        public static readonly RunUpgradeId CarbineDamageUpgradeId = new RunUpgradeId("upgrade.movement-fps.carbine-damage");
        public static readonly RunUpgradeId CarbineCadenceUpgradeId = new RunUpgradeId("upgrade.movement-fps.carbine-cadence");
        public static readonly RunUpgradeId PickupRangeUpgradeId = new RunUpgradeId("upgrade.movement-fps.pickup-range");
        public static readonly RunUpgradeId RiftLauncherUnlockUpgradeId = new RunUpgradeId("upgrade.unlock.rift-launcher");
        public static readonly RunUpgradeId ChainBoltUnlockUpgradeId = new RunUpgradeId("upgrade.unlock.chain-bolt");
        public static readonly RunUpgradeId GroundRiftUnlockUpgradeId = new RunUpgradeId("upgrade.unlock.ground-rift");
        public static readonly RunUpgradeId ProjectileSpeedUpgradeId = new RunUpgradeId("upgrade.movement-fps.projectile-speed");
        public static readonly RunUpgradeId AutoPowerDamageUpgradeId = new RunUpgradeId("upgrade.hungry-powers");
        public static readonly RunUpgradeEffectId AdditiveStatEffectId = new RunUpgradeEffectId("effect.stat.additive");
        public static readonly RunUpgradeEffectId MultiplierStatEffectId = new RunUpgradeEffectId("effect.stat.multiplier");
        public static readonly RunUpgradeEffectId UnlockEffectId = new RunUpgradeEffectId("effect.unlock");
        public static readonly RunUpgradeTargetId GunDamageTargetId = new RunUpgradeTargetId("stat.weapon.primary.damage");
        public static readonly RunUpgradeTargetId GunCadenceTargetId = new RunUpgradeTargetId("stat.weapon.primary.cadence");
        public static readonly RunUpgradeTargetId PickupRadiusTargetId = new RunUpgradeTargetId("stat.player.pickup-radius");
        public static readonly RunUpgradeTargetId ProjectileSpeedTargetId = new RunUpgradeTargetId("stat.weapon.projectile.speed");
        public static readonly RunUpgradeTargetId AutoPowerDamageTargetId = new RunUpgradeTargetId("stat.autopower.damage");
        public static readonly RunUpgradeTargetId RiftLauncherUnlockTargetId = new RunUpgradeTargetId("unlock.gun.rift-launcher");
        public static readonly RunUpgradeTargetId ChainBoltUnlockTargetId = new RunUpgradeTargetId("unlock.power.chain-bolt");
        public static readonly RunUpgradeTargetId GroundRiftUnlockTargetId = new RunUpgradeTargetId("unlock.power.ground-rift");

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

        public static MovementFpsGunDefinition CreateRiftLauncherDefinition()
        {
            return new MovementFpsGunDefinition(
                id: "gun.rift-launcher",
                displayName: "Rift Launcher",
                kind: MovementFpsGunKind.Projectile,
                damageType: StormDamageType,
                damage: 38d,
                fireIntervalSeconds: 0.72f,
                range: 70f,
                magazineSize: 6,
                reloadSeconds: 1.55f,
                spreadDegrees: 0.6f,
                recoilPitchDegrees: 1.2f,
                projectileSpeed: 30f,
                projectileLifetimeSeconds: 3f,
                projectileCollisionRadius: 0.18f);
        }

        public static MovementFpsAutoPowerDefinition CreateOrbitPulseDefinition()
        {
            return new MovementFpsAutoPowerDefinition(
                id: "power.orbit-pulse",
                displayName: "Orbit Pulse",
                kind: MovementFpsAutoPowerKind.OrbitPulse,
                damageType: VoidDamageType,
                damage: 9d,
                cooldownSeconds: 1.6f,
                range: 3.6f,
                radius: 3f,
                targetCount: 1);
        }

        public static MovementFpsAutoPowerDefinition CreateChainBoltDefinition()
        {
            return new MovementFpsAutoPowerDefinition(
                id: "power.chain-bolt",
                displayName: "Chain Bolt",
                kind: MovementFpsAutoPowerKind.ChainBolt,
                damageType: StormDamageType,
                damage: 16d,
                cooldownSeconds: 2.8f,
                range: 24f,
                radius: 3f,
                targetCount: 3);
        }

        public static MovementFpsAutoPowerDefinition CreateGroundRiftDefinition()
        {
            return new MovementFpsAutoPowerDefinition(
                id: "power.ground-rift",
                displayName: "Ground Rift",
                kind: MovementFpsAutoPowerKind.GroundRift,
                damageType: BoneDamageType,
                damage: 22d,
                cooldownSeconds: 4.5f,
                range: 18f,
                radius: 3f,
                targetCount: 1);
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
            return new CombatCatalog(new[]
            {
                new DamageTypeDefinition(KineticDamageType),
                new DamageTypeDefinition(VoidDamageType),
                new DamageTypeDefinition(StormDamageType),
                new DamageTypeDefinition(BoneDamageType)
            });
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
                    }),
                new RunUpgradeDefinition(
                    RiftLauncherUnlockUpgradeId,
                    RunUpgradeRarity.Rare,
                    weight: 4,
                    maxRank: 1,
                    effects: new[]
                    {
                        new RunUpgradeEffectDescriptor(UnlockEffectId, RiftLauncherUnlockTargetId, 1d)
                    }),
                new RunUpgradeDefinition(
                    ChainBoltUnlockUpgradeId,
                    RunUpgradeRarity.Rare,
                    weight: 4,
                    maxRank: 1,
                    effects: new[]
                    {
                        new RunUpgradeEffectDescriptor(UnlockEffectId, ChainBoltUnlockTargetId, 1d)
                    }),
                new RunUpgradeDefinition(
                    GroundRiftUnlockUpgradeId,
                    RunUpgradeRarity.Legendary,
                    weight: 1,
                    maxRank: 1,
                    effects: new[]
                    {
                        new RunUpgradeEffectDescriptor(UnlockEffectId, GroundRiftUnlockTargetId, 1d)
                    }),
                new RunUpgradeDefinition(
                    ProjectileSpeedUpgradeId,
                    RunUpgradeRarity.Uncommon,
                    weight: 6,
                    maxRank: 4,
                    effects: new[]
                    {
                        new RunUpgradeEffectDescriptor(MultiplierStatEffectId, ProjectileSpeedTargetId, 0.15d)
                    }),
                new RunUpgradeDefinition(
                    AutoPowerDamageUpgradeId,
                    RunUpgradeRarity.Common,
                    weight: 7,
                    maxRank: 8,
                    effects: new[]
                    {
                        new RunUpgradeEffectDescriptor(MultiplierStatEffectId, AutoPowerDamageTargetId, 0.2d)
                    })
            });
        }
    }
}
