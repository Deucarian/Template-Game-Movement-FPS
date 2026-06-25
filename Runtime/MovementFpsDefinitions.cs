using System;

namespace Deucarian.TemplateGameMovementFps
{
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
        {
            Id = string.IsNullOrWhiteSpace(id) ? throw new ArgumentException("Gun id is required.", nameof(id)) : id.Trim();
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? Id : displayName.Trim();
            Damage = Math.Max(0d, damage);
            FireIntervalSeconds = Math.Max(0.01f, fireIntervalSeconds);
            Range = Math.Max(1f, range);
            SpreadDegrees = Math.Max(0f, spreadDegrees);
            RecoilPitchDegrees = Math.Max(0f, recoilPitchDegrees);
        }

        public string Id { get; }
        public string DisplayName { get; }
        public double Damage { get; }
        public float FireIntervalSeconds { get; }
        public float Range { get; }
        public float SpreadDegrees { get; }
        public float RecoilPitchDegrees { get; }
    }

    public readonly struct MovementFpsEnemyDefinition
    {
        public MovementFpsEnemyDefinition(string id, double maximumHealth, float moveSpeed, double contactDamage, int experienceDrop)
        {
            Id = string.IsNullOrWhiteSpace(id) ? throw new ArgumentException("Enemy id is required.", nameof(id)) : id.Trim();
            MaximumHealth = Math.Max(1d, maximumHealth);
            MoveSpeed = Math.Max(0f, moveSpeed);
            ContactDamage = Math.Max(0d, contactDamage);
            ExperienceDrop = Math.Max(0, experienceDrop);
        }

        public string Id { get; }
        public double MaximumHealth { get; }
        public float MoveSpeed { get; }
        public double ContactDamage { get; }
        public int ExperienceDrop { get; }
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
