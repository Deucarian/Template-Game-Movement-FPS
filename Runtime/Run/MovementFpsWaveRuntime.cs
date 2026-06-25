using System;
using System.Collections.Generic;

namespace Deucarian.TemplateGameMovementFps.Run
{
    public enum MovementFpsRunState
    {
        Running,
        Draft,
        Victory,
        Defeated
    }

    public readonly struct MovementFpsWeightedEnemyDefinition
    {
        public MovementFpsWeightedEnemyDefinition(MovementFpsEnemyDefinition enemy, float weight)
        {
            Enemy = enemy;
            Weight = Math.Max(0f, weight);
        }

        public MovementFpsEnemyDefinition Enemy { get; }
        public float Weight { get; }
        public bool IsValid => !string.IsNullOrWhiteSpace(Enemy.Id) && Weight > 0f;
    }

    public sealed class MovementFpsWaveSegmentDefinition
    {
        public MovementFpsWaveSegmentDefinition(
            float startTimeSeconds,
            float spawnIntervalSeconds,
            int batchSize,
            int maxAlive,
            IReadOnlyList<MovementFpsWeightedEnemyDefinition> enemies)
        {
            StartTimeSeconds = Math.Max(0f, startTimeSeconds);
            SpawnIntervalSeconds = Math.Max(0.05f, spawnIntervalSeconds);
            BatchSize = Math.Max(1, batchSize);
            MaxAlive = Math.Max(1, maxAlive);
            Enemies = Copy(enemies);
        }

        public float StartTimeSeconds { get; }
        public float SpawnIntervalSeconds { get; }
        public int BatchSize { get; }
        public int MaxAlive { get; }
        public IReadOnlyList<MovementFpsWeightedEnemyDefinition> Enemies { get; }

        private static IReadOnlyList<MovementFpsWeightedEnemyDefinition> Copy(IReadOnlyList<MovementFpsWeightedEnemyDefinition> enemies)
        {
            if (enemies == null || enemies.Count == 0)
            {
                return Array.Empty<MovementFpsWeightedEnemyDefinition>();
            }

            MovementFpsWeightedEnemyDefinition[] copy = new MovementFpsWeightedEnemyDefinition[enemies.Count];
            for (int index = 0; index < enemies.Count; index++)
            {
                copy[index] = enemies[index];
            }

            return copy;
        }
    }

    public readonly struct MovementFpsWaveSpawnSnapshot
    {
        public MovementFpsWaveSpawnSnapshot(float spawnIntervalSeconds, int batchSize, int maxAlive)
        {
            SpawnIntervalSeconds = Math.Max(0.03f, spawnIntervalSeconds);
            BatchSize = Math.Max(1, batchSize);
            MaxAlive = Math.Max(1, maxAlive);
        }

        public float SpawnIntervalSeconds { get; }
        public int BatchSize { get; }
        public int MaxAlive { get; }
    }

    public sealed class MovementFpsWaveDefinition
    {
        private readonly List<MovementFpsWaveSegmentDefinition> _segments;

        public MovementFpsWaveDefinition(
            string id,
            string displayName,
            IReadOnlyList<MovementFpsWaveSegmentDefinition> segments,
            MovementFpsEnemyDefinition miniBossEnemy,
            float miniBossSpawnTimeSeconds,
            float victoryTimeSeconds,
            float escalationPerMinute)
        {
            Id = string.IsNullOrWhiteSpace(id) ? "wave.movement-fps.default" : id.Trim();
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? Id : displayName.Trim();
            _segments = new List<MovementFpsWaveSegmentDefinition>(segments ?? Array.Empty<MovementFpsWaveSegmentDefinition>());
            _segments.Sort((left, right) => left.StartTimeSeconds.CompareTo(right.StartTimeSeconds));
            MiniBossEnemy = miniBossEnemy;
            MiniBossSpawnTimeSeconds = Math.Max(0f, miniBossSpawnTimeSeconds);
            VictoryTimeSeconds = Math.Max(1f, victoryTimeSeconds);
            EscalationPerMinute = Math.Max(1f, escalationPerMinute);
        }

        public string Id { get; }
        public string DisplayName { get; }
        public IReadOnlyList<MovementFpsWaveSegmentDefinition> Segments => _segments;
        public MovementFpsEnemyDefinition MiniBossEnemy { get; }
        public float MiniBossSpawnTimeSeconds { get; }
        public float VictoryTimeSeconds { get; }
        public float EscalationPerMinute { get; }
        public bool HasMiniBoss => !string.IsNullOrWhiteSpace(MiniBossEnemy.Id);

        public MovementFpsWaveSegmentDefinition ResolveSegment(float elapsedSeconds)
        {
            if (_segments.Count == 0)
            {
                return null;
            }

            MovementFpsWaveSegmentDefinition resolved = _segments[0];
            for (int index = 1; index < _segments.Count; index++)
            {
                if (_segments[index].StartTimeSeconds > elapsedSeconds)
                {
                    break;
                }

                resolved = _segments[index];
            }

            return resolved;
        }

        public MovementFpsWaveSpawnSnapshot ResolveSpawnSnapshot(float elapsedSeconds, float escalationMultiplier)
        {
            MovementFpsWaveSegmentDefinition segment = ResolveSegment(elapsedSeconds);
            if (segment == null)
            {
                return new MovementFpsWaveSpawnSnapshot(1f, 1, 12);
            }

            float minutes = Math.Max(0f, elapsedSeconds / 60f);
            float pressure = (float)Math.Pow(Math.Max(1f, EscalationPerMinute * Math.Max(0f, escalationMultiplier)), minutes);
            float interval = Math.Max(0.08f, segment.SpawnIntervalSeconds / pressure);
            int batchSize = Math.Max(1, (int)Math.Round(segment.BatchSize * pressure));
            int maxAlive = Math.Max(1, (int)Math.Round(segment.MaxAlive * pressure));
            return new MovementFpsWaveSpawnSnapshot(interval, batchSize, maxAlive);
        }
    }

    public sealed class MovementFpsWaveDirector
    {
        private readonly MovementFpsWaveDefinition _wave;
        private readonly Random _random;
        private float _spawnTimer;

        public MovementFpsWaveDirector(MovementFpsWaveDefinition wave, int seed)
        {
            _wave = wave;
            _random = new Random(seed);
            CurrentSnapshot = new MovementFpsWaveSpawnSnapshot(1f, 1, 12);
            MiniBossSpawningEnabled = true;
        }

        public float SpawnIntervalMultiplier { get; set; } = 1f;
        public float SpawnBatchMultiplier { get; set; } = 1f;
        public float EscalationMultiplier { get; set; } = 1f;
        public int MaxAliveOverride { get; set; }
        public bool MiniBossSpawningEnabled { get; set; }
        public bool MiniBossSpawned { get; private set; }
        public MovementFpsWaveSpawnSnapshot CurrentSnapshot { get; private set; }

        public void Tick(
            float deltaTime,
            float elapsedSeconds,
            int aliveEnemyCount,
            Func<MovementFpsEnemyDefinition, bool> spawnEnemy)
        {
            if (_wave == null || spawnEnemy == null)
            {
                return;
            }

            int resolvedAliveCount = aliveEnemyCount;
            if (TrySpawnMiniBoss(elapsedSeconds, spawnEnemy))
            {
                resolvedAliveCount++;
            }

            MovementFpsWaveSpawnSnapshot snapshot = ResolveRuntimeSnapshot(elapsedSeconds);
            CurrentSnapshot = snapshot;
            if (resolvedAliveCount >= snapshot.MaxAlive)
            {
                return;
            }

            _spawnTimer += Math.Max(0f, deltaTime);
            if (_spawnTimer < snapshot.SpawnIntervalSeconds)
            {
                return;
            }

            _spawnTimer = 0f;
            MovementFpsWaveSegmentDefinition segment = _wave.ResolveSegment(elapsedSeconds);
            if (segment == null || segment.Enemies.Count == 0)
            {
                return;
            }

            int spawnCount = Math.Min(snapshot.BatchSize, snapshot.MaxAlive - resolvedAliveCount);
            for (int index = 0; index < spawnCount; index++)
            {
                MovementFpsWeightedEnemyDefinition selected = SelectEnemy(segment.Enemies);
                if (selected.IsValid)
                {
                    spawnEnemy(selected.Enemy);
                }
            }
        }

        public MovementFpsWaveSpawnSnapshot ResolveRuntimeSnapshot(float elapsedSeconds)
        {
            MovementFpsWaveSpawnSnapshot snapshot = _wave == null
                ? new MovementFpsWaveSpawnSnapshot(1f, 1, 12)
                : _wave.ResolveSpawnSnapshot(elapsedSeconds, EscalationMultiplier);

            float interval = Math.Max(0.03f, snapshot.SpawnIntervalSeconds * Math.Max(0.01f, SpawnIntervalMultiplier));
            int batchSize = Math.Max(1, (int)Math.Round(snapshot.BatchSize * Math.Max(0.01f, SpawnBatchMultiplier)));
            int maxAlive = MaxAliveOverride > 0 ? MaxAliveOverride : snapshot.MaxAlive;
            return new MovementFpsWaveSpawnSnapshot(interval, batchSize, maxAlive);
        }

        public void ResetRuntimeTuning()
        {
            SpawnIntervalMultiplier = 1f;
            SpawnBatchMultiplier = 1f;
            EscalationMultiplier = 1f;
            MaxAliveOverride = 0;
            MiniBossSpawningEnabled = true;
        }

        private bool TrySpawnMiniBoss(float elapsedSeconds, Func<MovementFpsEnemyDefinition, bool> spawnEnemy)
        {
            if (!MiniBossSpawningEnabled ||
                MiniBossSpawned ||
                _wave == null ||
                !_wave.HasMiniBoss ||
                elapsedSeconds < _wave.MiniBossSpawnTimeSeconds)
            {
                return false;
            }

            MiniBossSpawned = true;
            return spawnEnemy(_wave.MiniBossEnemy);
        }

        private MovementFpsWeightedEnemyDefinition SelectEnemy(IReadOnlyList<MovementFpsWeightedEnemyDefinition> entries)
        {
            float totalWeight = 0f;
            for (int index = 0; index < entries.Count; index++)
            {
                if (entries[index].IsValid)
                {
                    totalWeight += entries[index].Weight;
                }
            }

            if (totalWeight <= 0f)
            {
                return entries.Count == 0 ? default : entries[0];
            }

            float cursor = (float)_random.NextDouble() * totalWeight;
            for (int index = 0; index < entries.Count; index++)
            {
                if (!entries[index].IsValid)
                {
                    continue;
                }

                cursor -= entries[index].Weight;
                if (cursor <= 0f)
                {
                    return entries[index];
                }
            }

            return entries[entries.Count - 1];
        }
    }
}
