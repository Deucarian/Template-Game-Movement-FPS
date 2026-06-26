using System;
using System.Collections.Generic;
using Deucarian.GameplayFoundation;
using Deucarian.RunUpgrades;
using Deucarian.TemplateGameMovementFps.Run;

namespace Deucarian.TemplateGameMovementFps.Content
{
    public sealed class MovementFpsStartingLoadoutDefinition
    {
        public MovementFpsStartingLoadoutDefinition(string id, IReadOnlyList<string> startingGunIds, IReadOnlyList<string> startingPowerIds)
        {
            Id = string.IsNullOrWhiteSpace(id) ? "loadout.movement-fps.default" : id.Trim();
            StartingGunIds = Copy(startingGunIds);
            StartingPowerIds = Copy(startingPowerIds);
        }

        public string Id { get; }
        public IReadOnlyList<string> StartingGunIds { get; }
        public IReadOnlyList<string> StartingPowerIds { get; }

        private static IReadOnlyList<string> Copy(IReadOnlyList<string> source)
        {
            if (source == null || source.Count == 0)
            {
                return Array.Empty<string>();
            }

            string[] copy = new string[source.Count];
            for (int index = 0; index < source.Count; index++)
            {
                copy[index] = source[index] ?? string.Empty;
            }

            return copy;
        }
    }

    public sealed class MovementFpsContentLibrary
    {
        public MovementFpsContentLibrary(
            IReadOnlyList<MovementFpsGunDefinition> guns,
            IReadOnlyList<MovementFpsAutoPowerDefinition> powers,
            IReadOnlyList<MovementFpsEnemyDefinition> enemies,
            RunUpgradeCatalog upgrades,
            MovementFpsStartingLoadoutDefinition startingLoadout,
            MovementFpsWaveDefinition wave)
        {
            Guns = Copy(guns);
            Powers = Copy(powers);
            Enemies = Copy(enemies);
            Upgrades = upgrades;
            StartingLoadout = startingLoadout;
            Wave = wave;
        }

        public IReadOnlyList<MovementFpsGunDefinition> Guns { get; }
        public IReadOnlyList<MovementFpsAutoPowerDefinition> Powers { get; }
        public IReadOnlyList<MovementFpsEnemyDefinition> Enemies { get; }
        public RunUpgradeCatalog Upgrades { get; }
        public MovementFpsStartingLoadoutDefinition StartingLoadout { get; }
        public MovementFpsWaveDefinition Wave { get; }

        private static IReadOnlyList<T> Copy<T>(IReadOnlyList<T> source)
        {
            if (source == null || source.Count == 0)
            {
                return Array.Empty<T>();
            }

            T[] copy = new T[source.Count];
            for (int index = 0; index < source.Count; index++)
            {
                copy[index] = source[index];
            }

            return copy;
        }
    }

    public sealed class MovementFpsContentValidationReport
    {
        private readonly string[] _errors;

        public MovementFpsContentValidationReport(IReadOnlyList<string> errors)
        {
            _errors = Copy(errors);
        }

        public IReadOnlyList<string> Errors => _errors;
        public bool IsValid => _errors.Length == 0;

        private static string[] Copy(IReadOnlyList<string> errors)
        {
            if (errors == null || errors.Count == 0)
            {
                return Array.Empty<string>();
            }

            string[] copy = new string[errors.Count];
            for (int index = 0; index < errors.Count; index++)
            {
                copy[index] = errors[index] ?? string.Empty;
            }

            return copy;
        }
    }

    public static class MovementFpsContentValidator
    {
        public static MovementFpsContentValidationReport Validate(MovementFpsContentLibrary library)
        {
            var report = new ContentValidationReport();
            if (library == null)
            {
                report.AddError("Movement FPS content library is missing.");
                return new MovementFpsContentValidationReport(report.GetMessages());
            }

            ContentReferenceSet gunIds = ContentValidation.RequireUniqueIds(library.Guns, "weapon", definition => definition.Id, report, requireAtLeastOne: true);
            ContentReferenceSet powerIds = ContentValidation.RequireUniqueIds(library.Powers, "power", definition => definition.Id, report, requireAtLeastOne: true);
            ContentReferenceSet enemyIds = ContentValidation.RequireUniqueIds(library.Enemies, "enemy", definition => definition.Id, report, requireAtLeastOne: true);
            ValidateUpgrades(library.Upgrades, gunIds, powerIds, report);
            ValidateLoadout(library.StartingLoadout, gunIds, powerIds, report);
            ValidateWave(library.Wave, enemyIds, report);
            return new MovementFpsContentValidationReport(report.GetMessages());
        }

        private static void ValidateUpgrades(
            RunUpgradeCatalog upgrades,
            ContentReferenceSet gunIds,
            ContentReferenceSet powerIds,
            ContentValidationReport report)
        {
            if (upgrades == null || upgrades.Definitions.Count == 0)
            {
                report.AddError("At least one upgrade definition is required.");
                return;
            }

            ContentValidation.RequireUniqueIds(upgrades.Definitions, "upgrade", definition => definition.Id.Value, report, requireAtLeastOne: true);
            for (int index = 0; index < upgrades.Definitions.Count; index++)
            {
                RunUpgradeDefinition upgrade = upgrades.Definitions[index];
                ValidateUpgradeReferences(upgrade, gunIds, powerIds, report);
            }
        }

        private static void ValidateUpgradeReferences(
            RunUpgradeDefinition upgrade,
            ContentReferenceSet gunIds,
            ContentReferenceSet powerIds,
            ContentValidationReport report)
        {
            for (int index = 0; index < upgrade.Effects.Count; index++)
            {
                RunUpgradeEffectDescriptor effect = upgrade.Effects[index];
                if (!IsKnownUpgradeTarget(effect.TargetId))
                {
                    report.AddError($"Upgrade '{upgrade.Id.Value}' references unknown target '{effect.TargetId.Value}'.");
                    continue;
                }

                if (effect.TargetId.Equals(BasicMovementFpsGame.RiftLauncherUnlockTargetId) && !gunIds.Contains(BasicMovementFpsGame.RiftLauncherId))
                {
                    report.AddError($"Upgrade '{upgrade.Id.Value}' unlocks missing gun '{BasicMovementFpsGame.RiftLauncherId}'.");
                }
                else if (effect.TargetId.Equals(BasicMovementFpsGame.ChainBoltUnlockTargetId) && !powerIds.Contains(BasicMovementFpsGame.ChainBoltId))
                {
                    report.AddError($"Upgrade '{upgrade.Id.Value}' unlocks missing power '{BasicMovementFpsGame.ChainBoltId}'.");
                }
                else if (effect.TargetId.Equals(BasicMovementFpsGame.GroundRiftUnlockTargetId) && !powerIds.Contains(BasicMovementFpsGame.GroundRiftId))
                {
                    report.AddError($"Upgrade '{upgrade.Id.Value}' unlocks missing power '{BasicMovementFpsGame.GroundRiftId}'.");
                }
            }

            for (int index = 0; index < upgrade.Prerequisites.Count; index++)
            {
                if (upgrade.Prerequisites[index].IsEmpty)
                {
                    report.AddError($"Upgrade '{upgrade.Id.Value}' has an empty prerequisite reference.");
                }
            }

            for (int index = 0; index < upgrade.Exclusions.Count; index++)
            {
                if (upgrade.Exclusions[index].IsEmpty)
                {
                    report.AddError($"Upgrade '{upgrade.Id.Value}' has an empty exclusion reference.");
                }
            }
        }

        private static void ValidateLoadout(
            MovementFpsStartingLoadoutDefinition loadout,
            ContentReferenceSet gunIds,
            ContentReferenceSet powerIds,
            ContentValidationReport report)
        {
            if (loadout == null)
            {
                report.AddError("Starting loadout is missing.");
                return;
            }

            ContentValidation.RequireReferences(loadout.StartingGunIds, "starting gun", gunIds, report, requireAtLeastOne: true);
            ContentValidation.RequireReferences(loadout.StartingPowerIds, "starting power", powerIds, report, requireAtLeastOne: true);
        }

        private static void ValidateWave(MovementFpsWaveDefinition wave, ContentReferenceSet enemyIds, ContentValidationReport report)
        {
            if (wave == null)
            {
                report.AddError("Wave definition is missing.");
                return;
            }

            if (wave.Segments.Count == 0)
            {
                report.AddError($"Wave '{wave.Id}' must contain at least one segment.");
            }

            for (int segmentIndex = 0; segmentIndex < wave.Segments.Count; segmentIndex++)
            {
                MovementFpsWaveSegmentDefinition segment = wave.Segments[segmentIndex];
                if (segment.Enemies.Count == 0)
                {
                    report.AddError($"Wave '{wave.Id}' segment {segmentIndex} has no weighted enemies.");
                }

                for (int enemyIndex = 0; enemyIndex < segment.Enemies.Count; enemyIndex++)
                {
                    MovementFpsWeightedEnemyDefinition weighted = segment.Enemies[enemyIndex];
                    if (weighted.Weight <= 0f)
                    {
                        report.AddError($"Wave '{wave.Id}' segment {segmentIndex} enemy {enemyIndex} has non-positive weight.");
                    }

                    string enemyId = weighted.Enemy.Id;
                    if (string.IsNullOrWhiteSpace(enemyId) || !enemyIds.Contains(enemyId))
                    {
                        report.AddError($"Wave '{wave.Id}' segment {segmentIndex} references missing enemy '{enemyId}'.");
                    }
                }
            }

            if (wave.HasMiniBoss && !enemyIds.Contains(wave.MiniBossEnemy.Id))
            {
                report.AddError($"Wave '{wave.Id}' references missing miniboss enemy '{wave.MiniBossEnemy.Id}'.");
            }
        }

        private static bool IsKnownUpgradeTarget(RunUpgradeTargetId targetId)
        {
            return targetId.Equals(BasicMovementFpsGame.GunDamageTargetId) ||
                targetId.Equals(BasicMovementFpsGame.GunCadenceTargetId) ||
                targetId.Equals(BasicMovementFpsGame.PickupRadiusTargetId) ||
                targetId.Equals(BasicMovementFpsGame.ProjectileSpeedTargetId) ||
                targetId.Equals(BasicMovementFpsGame.AutoPowerDamageTargetId) ||
                targetId.Equals(BasicMovementFpsGame.RiftLauncherUnlockTargetId) ||
                targetId.Equals(BasicMovementFpsGame.ChainBoltUnlockTargetId) ||
                targetId.Equals(BasicMovementFpsGame.GroundRiftUnlockTargetId);
        }
    }
}
