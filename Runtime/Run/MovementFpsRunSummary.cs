using System;
using System.Collections.Generic;

namespace Deucarian.TemplateGameMovementFps.Run
{
    public enum MovementFpsRunOutcome
    {
        Running,
        Victory,
        Defeat
    }

    public readonly struct MovementFpsRunReward
    {
        public MovementFpsRunReward(string id, string displayName, int amount)
        {
            Id = string.IsNullOrWhiteSpace(id) ? "reward.movement-fps.unknown" : id.Trim();
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? Id : displayName.Trim();
            Amount = Math.Max(0, amount);
        }

        public string Id { get; }
        public string DisplayName { get; }
        public int Amount { get; }
    }

    public sealed class MovementFpsRunSummary
    {
        public static readonly MovementFpsRunSummary Empty = new MovementFpsRunSummary(
            MovementFpsRunOutcome.Running,
            elapsedSeconds: 0f,
            killCount: 0,
            miniBossKills: 0,
            experienceGained: 0,
            upgradesChosen: Array.Empty<string>(),
            rewards: Array.Empty<MovementFpsRunReward>());

        public MovementFpsRunSummary(
            MovementFpsRunOutcome outcome,
            float elapsedSeconds,
            int killCount,
            int miniBossKills,
            int experienceGained,
            IReadOnlyList<string> upgradesChosen,
            IReadOnlyList<MovementFpsRunReward> rewards)
        {
            Outcome = outcome;
            ElapsedSeconds = Math.Max(0f, elapsedSeconds);
            KillCount = Math.Max(0, killCount);
            MiniBossKills = Math.Max(0, miniBossKills);
            ExperienceGained = Math.Max(0, experienceGained);
            UpgradesChosen = Copy(upgradesChosen);
            Rewards = Copy(rewards);
        }

        public MovementFpsRunOutcome Outcome { get; }
        public float ElapsedSeconds { get; }
        public int KillCount { get; }
        public int MiniBossKills { get; }
        public int ExperienceGained { get; }
        public IReadOnlyList<string> UpgradesChosen { get; }
        public IReadOnlyList<MovementFpsRunReward> Rewards { get; }
        public bool Completed => Outcome == MovementFpsRunOutcome.Victory || Outcome == MovementFpsRunOutcome.Defeat;

        private static string[] Copy(IReadOnlyList<string> source)
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

        private static MovementFpsRunReward[] Copy(IReadOnlyList<MovementFpsRunReward> source)
        {
            if (source == null || source.Count == 0)
            {
                return Array.Empty<MovementFpsRunReward>();
            }

            MovementFpsRunReward[] copy = new MovementFpsRunReward[source.Count];
            for (int index = 0; index < source.Count; index++)
            {
                copy[index] = source[index];
            }

            return copy;
        }
    }

    public sealed class MovementFpsRunSummaryTracker
    {
        private readonly List<string> _upgradesChosen = new List<string>();
        private readonly List<MovementFpsRunReward> _rewards = new List<MovementFpsRunReward>();
        private int _killCount;
        private int _miniBossKills;
        private int _experienceGained;
        private MovementFpsRunOutcome _outcome = MovementFpsRunOutcome.Running;

        public int KillCount => _killCount;
        public int MiniBossKills => _miniBossKills;
        public int ExperienceGained => _experienceGained;
        public IReadOnlyList<string> UpgradesChosen => _upgradesChosen;
        public IReadOnlyList<MovementFpsRunReward> Rewards => _rewards;
        public MovementFpsRunOutcome Outcome => _outcome;

        public void Reset()
        {
            _killCount = 0;
            _miniBossKills = 0;
            _experienceGained = 0;
            _outcome = MovementFpsRunOutcome.Running;
            _upgradesChosen.Clear();
            _rewards.Clear();
        }

        public void RecordKill(bool miniBoss)
        {
            _killCount++;
            if (miniBoss)
            {
                _miniBossKills++;
            }
        }

        public void RecordExperience(int amount)
        {
            _experienceGained += Math.Max(0, amount);
        }

        public void RecordUpgrade(string upgradeId)
        {
            if (!string.IsNullOrWhiteSpace(upgradeId))
            {
                _upgradesChosen.Add(upgradeId.Trim());
            }
        }

        public void RecordReward(MovementFpsRunReward reward)
        {
            if (!string.IsNullOrWhiteSpace(reward.Id) && reward.Amount > 0)
            {
                _rewards.Add(reward);
            }
        }

        public void Complete(MovementFpsRunOutcome outcome)
        {
            _outcome = outcome == MovementFpsRunOutcome.Victory || outcome == MovementFpsRunOutcome.Defeat
                ? outcome
                : MovementFpsRunOutcome.Running;
        }

        public MovementFpsRunSummary CreateSummary(float elapsedSeconds)
        {
            return new MovementFpsRunSummary(
                _outcome,
                elapsedSeconds,
                _killCount,
                _miniBossKills,
                _experienceGained,
                _upgradesChosen,
                _rewards);
        }
    }
}
