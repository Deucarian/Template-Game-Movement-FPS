using Deucarian.TemplateGameMovementFps.Progression;

namespace Deucarian.TemplateGameMovementFps.Actors
{
    internal static class MovementFpsPlayerStats
    {
        internal static double GunDamage(MovementFpsGunDefinition definition, MovementFpsRunProgression progression)
        {
            return definition.Damage + (progression == null ? 0d : progression.GunDamageBonus);
        }

        internal static float GunCadence(MovementFpsRunProgression progression)
        {
            return 1f + (float)(progression == null ? 0d : progression.GunCadenceMultiplier);
        }

        internal static float ProjectileSpeed(MovementFpsGunDefinition definition, MovementFpsRunProgression progression)
        {
            return definition.ProjectileSpeed * (1f + (float)(progression == null ? 0d : progression.ProjectileSpeedMultiplier));
        }

        internal static double PowerDamage(MovementFpsAutoPowerDefinition definition, MovementFpsRunProgression progression)
        {
            return definition.Damage * (1d + (progression == null ? 0d : progression.AutoPowerDamageMultiplier));
        }
    }
}
