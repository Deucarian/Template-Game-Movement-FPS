using System;
using System.Collections.Generic;
using Deucarian.RunUpgrades;

namespace Deucarian.TemplateGameMovementFps.Progression
{
    public sealed class MovementFpsRunProgression
    {
        private readonly RunUpgradeCatalog _catalog;
        private readonly RunUpgradeState _upgradeState = new RunUpgradeState();
        private readonly List<RunUpgradeDefinition> _currentDraft = new List<RunUpgradeDefinition>();
        private int _draftCounter;

        public MovementFpsRunProgression(RunUpgradeCatalog catalog = null, int baseRequirement = BasicMovementFpsGame.FirstLevelExperienceRequirement)
        {
            _catalog = catalog ?? BasicMovementFpsGame.CreateUpgradeCatalog();
            BaseRequirement = Math.Max(1, baseRequirement);
            Reset();
        }

        public int BaseRequirement { get; }
        public int Level { get; private set; }
        public int CurrentExperience { get; private set; }
        public int PendingDrafts { get; private set; }
        public double GunDamageBonus { get; private set; }
        public double GunCadenceMultiplier { get; private set; }
        public double PickupRadiusBonus { get; private set; }
        public IReadOnlyList<RunUpgradeDefinition> CurrentDraft => _currentDraft;
        public bool HasDraft => _currentDraft.Count > 0;
        public int RequiredExperience => BaseRequirement + (Level - 1) * 6;

        public void Reset()
        {
            Level = 1;
            CurrentExperience = 0;
            PendingDrafts = 0;
            GunDamageBonus = 0d;
            GunCadenceMultiplier = 0d;
            PickupRadiusBonus = 0d;
            _currentDraft.Clear();
            _draftCounter = 0;
        }

        public int GainExperience(int amount)
        {
            int gainedLevels = 0;
            CurrentExperience += Math.Max(0, amount);
            while (CurrentExperience >= RequiredExperience)
            {
                CurrentExperience -= RequiredExperience;
                Level++;
                PendingDrafts++;
                gainedLevels++;
            }

            if (gainedLevels > 0 && !HasDraft)
            {
                OpenDraft();
            }

            return gainedLevels;
        }

        public RunUpgradeDraft OpenDraft(int? seed = null)
        {
            _currentDraft.Clear();
            if (PendingDrafts <= 0)
            {
                return new RunUpgradeDraft(Array.Empty<RunUpgradeDefinition>());
            }

            int resolvedSeed = seed ?? ((Level * 397) + (_draftCounter * 53));
            _draftCounter++;
            RunUpgradeDraft draft = RunUpgradeDraftService.Generate(
                _catalog,
                _upgradeState,
                new RunUpgradeDraftRequest(BasicMovementFpsGame.DraftChoiceCount, resolvedSeed));

            for (int index = 0; index < draft.Choices.Count; index++)
            {
                _currentDraft.Add(draft.Choices[index]);
            }

            return draft;
        }

        public RunUpgradeSelectionResult ChooseDraft(int index)
        {
            if (index < 0 || index >= _currentDraft.Count)
            {
                return new RunUpgradeSelectionResult(RunUpgradeSelectionStatus.UnknownUpgrade, default, 0);
            }

            RunUpgradeDefinition selected = _currentDraft[index];
            RunUpgradeSelectionResult result = _upgradeState.Select(_catalog, selected.Id);
            if (!result.Succeeded)
            {
                return result;
            }

            ApplyEffects(selected);
            if (PendingDrafts > 0)
            {
                PendingDrafts--;
            }

            _currentDraft.Clear();
            if (PendingDrafts > 0)
            {
                OpenDraft();
            }

            return result;
        }

        public int GetUpgradeRank(RunUpgradeId id)
        {
            return _upgradeState.GetRank(id);
        }

        private void ApplyEffects(RunUpgradeDefinition upgrade)
        {
            for (int index = 0; index < upgrade.Effects.Count; index++)
            {
                RunUpgradeEffectDescriptor effect = upgrade.Effects[index];
                if (effect.TargetId.Equals(BasicMovementFpsGame.GunDamageTargetId))
                {
                    GunDamageBonus += effect.Amount;
                }
                else if (effect.TargetId.Equals(BasicMovementFpsGame.GunCadenceTargetId))
                {
                    GunCadenceMultiplier += effect.Amount;
                }
                else if (effect.TargetId.Equals(BasicMovementFpsGame.PickupRadiusTargetId))
                {
                    PickupRadiusBonus += effect.Amount;
                }
            }
        }
    }
}
