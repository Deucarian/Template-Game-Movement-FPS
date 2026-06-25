using System;
using Deucarian.Combat;

namespace Deucarian.TemplateGameMovementFps
{
    public enum MovementFpsGunKind
    {
        Hitscan,
        Projectile
    }

    public readonly struct MovementFpsGunDefinition
    {
        public MovementFpsGunDefinition(
            string id,
            string displayName,
            double damage,
            float fireIntervalSeconds,
            float range,
            float spreadDegrees,
            float recoilPitchDegrees)
            : this(
                id,
                displayName,
                MovementFpsGunKind.Hitscan,
                BasicMovementFpsGame.KineticDamageType,
                damage,
                fireIntervalSeconds,
                range,
                magazineSize: 28,
                reloadSeconds: 1.35f,
                spreadDegrees,
                recoilPitchDegrees,
                projectileSpeed: 30f,
                projectileLifetimeSeconds: 3f,
                projectileCollisionRadius: 0.18f)
        {
        }

        public MovementFpsGunDefinition(
            string id,
            string displayName,
            MovementFpsGunKind kind,
            DamageTypeId damageType,
            double damage,
            float fireIntervalSeconds,
            float range,
            int magazineSize,
            float reloadSeconds,
            float spreadDegrees,
            float recoilPitchDegrees,
            float projectileSpeed,
            float projectileLifetimeSeconds,
            float projectileCollisionRadius)
        {
            Id = string.IsNullOrWhiteSpace(id) ? throw new ArgumentException("Gun id is required.", nameof(id)) : id.Trim();
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? Id : displayName.Trim();
            Kind = kind;
            DamageType = damageType.IsEmpty ? BasicMovementFpsGame.KineticDamageType : damageType;
            Damage = Math.Max(0d, damage);
            FireIntervalSeconds = Math.Max(0.01f, fireIntervalSeconds);
            Range = Math.Max(1f, range);
            MagazineSize = Math.Max(1, magazineSize);
            ReloadSeconds = Math.Max(0.05f, reloadSeconds);
            SpreadDegrees = Math.Max(0f, spreadDegrees);
            RecoilPitchDegrees = Math.Max(0f, recoilPitchDegrees);
            ProjectileSpeed = Math.Max(1f, projectileSpeed);
            ProjectileLifetimeSeconds = Math.Max(0.1f, projectileLifetimeSeconds);
            ProjectileCollisionRadius = Math.Max(0.02f, projectileCollisionRadius);
        }

        public string Id { get; }
        public string DisplayName { get; }
        public MovementFpsGunKind Kind { get; }
        public DamageTypeId DamageType { get; }
        public double Damage { get; }
        public float FireIntervalSeconds { get; }
        public float Range { get; }
        public int MagazineSize { get; }
        public float ReloadSeconds { get; }
        public float SpreadDegrees { get; }
        public float RecoilPitchDegrees { get; }
        public float ProjectileSpeed { get; }
        public float ProjectileLifetimeSeconds { get; }
        public float ProjectileCollisionRadius { get; }
    }

    public enum MovementFpsAutoPowerKind
    {
        OrbitPulse,
        ChainBolt,
        GroundRift
    }

    public readonly struct MovementFpsAutoPowerDefinition
    {
        public MovementFpsAutoPowerDefinition(
            string id,
            string displayName,
            MovementFpsAutoPowerKind kind,
            DamageTypeId damageType,
            double damage,
            float cooldownSeconds,
            float range,
            float radius,
            int targetCount)
        {
            Id = string.IsNullOrWhiteSpace(id) ? throw new ArgumentException("Auto power id is required.", nameof(id)) : id.Trim();
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? Id : displayName.Trim();
            Kind = kind;
            DamageType = damageType.IsEmpty ? BasicMovementFpsGame.KineticDamageType : damageType;
            Damage = Math.Max(0d, damage);
            CooldownSeconds = Math.Max(0.1f, cooldownSeconds);
            Range = Math.Max(0.2f, range);
            Radius = Math.Max(0.2f, radius);
            TargetCount = Math.Max(1, targetCount);
        }

        public string Id { get; }
        public string DisplayName { get; }
        public MovementFpsAutoPowerKind Kind { get; }
        public DamageTypeId DamageType { get; }
        public double Damage { get; }
        public float CooldownSeconds { get; }
        public float Range { get; }
        public float Radius { get; }
        public int TargetCount { get; }
    }

    public readonly struct MovementFpsEnemyDefinition
    {
        public MovementFpsEnemyDefinition(string id, double maximumHealth, float moveSpeed, double contactDamage, int experienceDrop)
            : this(id, id, maximumHealth, moveSpeed, contactDamage, experienceDrop, isMiniBoss: false, visualScale: 1f, contactIntervalSeconds: 0.75f)
        {
        }

        public MovementFpsEnemyDefinition(
            string id,
            string displayName,
            double maximumHealth,
            float moveSpeed,
            double contactDamage,
            int experienceDrop,
            bool isMiniBoss,
            float visualScale,
            float contactIntervalSeconds)
        {
            Id = string.IsNullOrWhiteSpace(id) ? throw new ArgumentException("Enemy id is required.", nameof(id)) : id.Trim();
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? Id : displayName.Trim();
            MaximumHealth = Math.Max(1d, maximumHealth);
            MoveSpeed = Math.Max(0f, moveSpeed);
            ContactDamage = Math.Max(0d, contactDamage);
            ExperienceDrop = Math.Max(0, experienceDrop);
            IsMiniBoss = isMiniBoss;
            VisualScale = Math.Max(0.25f, visualScale);
            ContactIntervalSeconds = Math.Max(0.1f, contactIntervalSeconds);
        }

        public string Id { get; }
        public string DisplayName { get; }
        public double MaximumHealth { get; }
        public float MoveSpeed { get; }
        public double ContactDamage { get; }
        public int ExperienceDrop { get; }
        public bool IsMiniBoss { get; }
        public float VisualScale { get; }
        public float ContactIntervalSeconds { get; }
    }

    public readonly struct MovementFpsPlayerDefinition
    {
        public MovementFpsPlayerDefinition(double maximumHealth, float pickupRadius)
        {
            MaximumHealth = Math.Max(1d, maximumHealth);
            PickupRadius = Math.Max(0.5f, pickupRadius);
        }

        public double MaximumHealth { get; }
        public float PickupRadius { get; }
    }
}
