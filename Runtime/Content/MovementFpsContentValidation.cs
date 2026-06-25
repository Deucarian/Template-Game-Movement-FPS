using System;
using System.Collections.Generic;
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
            var errors = new List<string>();
            if (library == null)
            {
                errors.Add("Movement FPS content library is missing.");
                return new MovementFpsContentValidationReport(errors);
            }

            HashSet<string> gunIds = ValidateStableIds(library.Guns, "weapon", definition => definition.Id, errors);
            HashSet<string> powerIds = ValidateStableIds(library.Powers, "power", definition => definition.Id, errors);
            HashSet<string> enemyIds = ValidateStableIds(library.Enemies, "enemy", definition => definition.Id, errors);
            ValidateUpgrades(library.Upgrades, gunIds, powerIds, errors);
            ValidateLoadout(library.StartingLoadout, gunIds, powerIds, errors);
            ValidateWave(library.Wave, enemyIds, errors);
            return new MovementFpsContentValidationReport(errors);
        }

        private static HashSet<string> ValidateStableIds<T>(
            IReadOnlyList<T> definitions,
            string label,
            Func<T, string> resolveId,
            ICollection<string> errors)
        {
            var ids = new HashSet<string>(StringComparer.Ordinal);
            if (definitions == null || definitions.Count == 0)
            {
                errors.Add($"At least one {label} definition is required.");
                return ids;
            }

            for (int index = 0; index < definitions.Count; index++)
            {
                string id = resolveId(definitions[index]);
                if (string.IsNullOrWhiteSpace(id))
                {
                    errors.Add($"{label} definition at index {index} is missing a stable id.");
                    continue;
                }

                if (!ids.Add(id))
                {
                    errors.Add($"Duplicate {label} id '{id}'.");
                }
            }

            return ids;
        }

        private static void ValidateUpgrades(
            RunUpgradeCatalog upgrades,
            ISet<string> gunIds,
            ISet<string> powerIds,
            ICollection<string> errors)
        {
            if (upgrades == null || upgrades.Definitions.Count == 0)
            {
                errors.Add("At least one upgrade definition is required.");
                return;
            }

            var upgradeIds = new HashSet<string>(StringComparer.Ordinal);
            for (int index = 0; index < upgrades.Definitions.Count; index++)
            {
                RunUpgradeDefinition upgrade = upgrades.Definitions[index];
                string id = upgrade.Id.Value;
                if (string.IsNullOrWhiteSpace(id))
                {
                    errors.Add($"upgrade definition at index {index} is missing a stable id.");
                }
                else if (!upgradeIds.Add(id))
                {
                    errors.Add($"Duplicate upgrade id '{id}'.");
                }

                ValidateUpgradeReferences(upgrade, gunIds, powerIds, errors);
            }
        }

        private static void ValidateUpgradeReferences(
            RunUpgradeDefinition upgrade,
            ISet<string> gunIds,
            ISet<string> powerIds,
            ICollection<string> errors)
        {
            for (int index = 0; index < upgrade.Effects.Count; index++)
            {
                RunUpgradeEffectDescriptor effect = upgrade.Effects[index];
                if (!IsKnownUpgradeTarget(effect.TargetId))
                {
                    errors.Add($"Upgrade '{upgrade.Id.Value}' references unknown target '{effect.TargetId.Value}'.");
                    continue;
                }

                if (effect.TargetId.Equals(BasicMovementFpsGame.RiftLauncherUnlockTargetId) && !gunIds.Contains(BasicMovementFpsGame.RiftLauncherId))
                {
                    errors.Add($"Upgrade '{upgrade.Id.Value}' unlocks missing gun '{BasicMovementFpsGame.RiftLauncherId}'.");
                }
                else if (effect.TargetId.Equals(BasicMovementFpsGame.ChainBoltUnlockTargetId) && !powerIds.Contains(BasicMovementFpsGame.ChainBoltId))
                {
                    errors.Add($"Upgrade '{upgrade.Id.Value}' unlocks missing power '{BasicMovementFpsGame.ChainBoltId}'.");
                }
                else if (effect.TargetId.Equals(BasicMovementFpsGame.GroundRiftUnlockTargetId) && !powerIds.Contains(BasicMovementFpsGame.GroundRiftId))
                {
                    errors.Add($"Upgrade '{upgrade.Id.Value}' unlocks missing power '{BasicMovementFpsGame.GroundRiftId}'.");
                }
            }

            for (int index = 0; index < upgrade.Prerequisites.Count; index++)
            {
                if (upgrade.Prerequisites[index].IsEmpty)
                {
                    errors.Add($"Upgrade '{upgrade.Id.Value}' has an empty prerequisite reference.");
                }
            }

            for (int index = 0; index < upgrade.Exclusions.Count; index++)
            {
                if (upgrade.Exclusions[index].IsEmpty)
                {
                    errors.Add($"Upgrade '{upgrade.Id.Value}' has an empty exclusion reference.");
                }
            }
        }

        private static void ValidateLoadout(
            MovementFpsStartingLoadoutDefinition loadout,
            ISet<string> gunIds,
            ISet<string> powerIds,
            ICollection<string> errors)
        {
            if (loadout == null)
            {
                errors.Add("Starting loadout is missing.");
                return;
            }

            ValidateReferences(loadout.StartingGunIds, "starting gun", gunIds, errors);
            ValidateReferences(loadout.StartingPowerIds, "starting power", powerIds, errors);
        }

        private static void ValidateWave(MovementFpsWaveDefinition wave, ISet<string> enemyIds, ICollection<string> errors)
        {
            if (wave == null)
            {
                errors.Add("Wave definition is missing.");
                return;
            }

            if (wave.Segments.Count == 0)
            {
                errors.Add($"Wave '{wave.Id}' must contain at least one segment.");
            }

            for (int segmentIndex = 0; segmentIndex < wave.Segments.Count; segmentIndex++)
            {
                MovementFpsWaveSegmentDefinition segment = wave.Segments[segmentIndex];
                if (segment.Enemies.Count == 0)
                {
                    errors.Add($"Wave '{wave.Id}' segment {segmentIndex} has no weighted enemies.");
                }

                for (int enemyIndex = 0; enemyIndex < segment.Enemies.Count; enemyIndex++)
                {
                    MovementFpsWeightedEnemyDefinition weighted = segment.Enemies[enemyIndex];
                    if (weighted.Weight <= 0f)
                    {
                        errors.Add($"Wave '{wave.Id}' segment {segmentIndex} enemy {enemyIndex} has non-positive weight.");
                    }

                    string enemyId = weighted.Enemy.Id;
                    if (string.IsNullOrWhiteSpace(enemyId) || !enemyIds.Contains(enemyId))
                    {
                        errors.Add($"Wave '{wave.Id}' segment {segmentIndex} references missing enemy '{enemyId}'.");
                    }
                }
            }

            if (wave.HasMiniBoss && !enemyIds.Contains(wave.MiniBossEnemy.Id))
            {
                errors.Add($"Wave '{wave.Id}' references missing miniboss enemy '{wave.MiniBossEnemy.Id}'.");
            }
        }

        private static void ValidateReferences(
            IReadOnlyList<string> references,
            string label,
            ISet<string> validIds,
            ICollection<string> errors)
        {
            if (references == null || references.Count == 0)
            {
                errors.Add($"At least one {label} reference is required.");
                return;
            }

            var seen = new HashSet<string>(StringComparer.Ordinal);
            for (int index = 0; index < references.Count; index++)
            {
                string id = references[index];
                if (string.IsNullOrWhiteSpace(id))
                {
                    errors.Add($"{label} reference at index {index} is empty.");
                    continue;
                }

                if (!seen.Add(id))
                {
                    errors.Add($"Duplicate {label} reference '{id}'.");
                }

                if (!validIds.Contains(id))
                {
                    errors.Add($"{label} reference '{id}' does not exist.");
                }
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
