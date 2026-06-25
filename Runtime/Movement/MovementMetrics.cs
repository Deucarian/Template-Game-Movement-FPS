using UnityEngine;

namespace Deucarian.TemplateGameMovementFps.Movement
{
    public readonly struct MovementMetricsInput
    {
        public MovementMetricsInput(
            float walkSpeed,
            float sprintSpeed,
            float slideSpeed,
            float jumpVelocity,
            float gravity,
            float wallrunSpeed,
            float wallrunDurationSeconds,
            float wallrunApexTime,
            float wallrunApexSpeedMultiplier,
            float wallrunPostApexRetention,
            float wallrunLateDecelStart,
            float wallrunEndSpeedRetention,
            float wallrunApexHeight,
            float wallJumpVerticalVelocity,
            float wallJumpMinimumSpeedFraction,
            float wallJumpPeakTimingMultiplier,
            float slideJumpMinimumSpeedMultiplier,
            float slideJumpPeakSpeedMultiplier,
            float slideJumpSpeedCapMultiplier,
            float vaultMinimumSpeedFraction,
            float vaultBaseSpeedMultiplier,
            float vaultPerfectSpeedMultiplier,
            bool maxVelocityEnabled = false,
            float maxVelocityMetersPerSecond = 36f)
        {
            WalkSpeed = walkSpeed;
            SprintSpeed = sprintSpeed;
            SlideSpeed = slideSpeed;
            JumpVelocity = jumpVelocity;
            Gravity = gravity;
            WallrunSpeed = wallrunSpeed;
            WallrunDurationSeconds = wallrunDurationSeconds;
            WallrunApexTime = wallrunApexTime;
            WallrunApexSpeedMultiplier = wallrunApexSpeedMultiplier;
            WallrunPostApexRetention = wallrunPostApexRetention;
            WallrunLateDecelStart = wallrunLateDecelStart;
            WallrunEndSpeedRetention = wallrunEndSpeedRetention;
            WallrunApexHeight = wallrunApexHeight;
            WallJumpVerticalVelocity = wallJumpVerticalVelocity;
            WallJumpMinimumSpeedFraction = wallJumpMinimumSpeedFraction;
            WallJumpPeakTimingMultiplier = wallJumpPeakTimingMultiplier;
            SlideJumpMinimumSpeedMultiplier = slideJumpMinimumSpeedMultiplier;
            SlideJumpPeakSpeedMultiplier = slideJumpPeakSpeedMultiplier;
            SlideJumpSpeedCapMultiplier = slideJumpSpeedCapMultiplier;
            VaultMinimumSpeedFraction = vaultMinimumSpeedFraction;
            VaultBaseSpeedMultiplier = vaultBaseSpeedMultiplier;
            VaultPerfectSpeedMultiplier = vaultPerfectSpeedMultiplier;
            MaxVelocityEnabled = maxVelocityEnabled;
            MaxVelocityMetersPerSecond = Mathf.Max(0.01f, maxVelocityMetersPerSecond);
        }

        public float WalkSpeed { get; }
        public float SprintSpeed { get; }
        public float SlideSpeed { get; }
        public float JumpVelocity { get; }
        public float Gravity { get; }
        public float WallrunSpeed { get; }
        public float WallrunDurationSeconds { get; }
        public float WallrunApexTime { get; }
        public float WallrunApexSpeedMultiplier { get; }
        public float WallrunPostApexRetention { get; }
        public float WallrunLateDecelStart { get; }
        public float WallrunEndSpeedRetention { get; }
        public float WallrunApexHeight { get; }
        public float WallJumpVerticalVelocity { get; }
        public float WallJumpMinimumSpeedFraction { get; }
        public float WallJumpPeakTimingMultiplier { get; }
        public float SlideJumpMinimumSpeedMultiplier { get; }
        public float SlideJumpPeakSpeedMultiplier { get; }
        public float SlideJumpSpeedCapMultiplier { get; }
        public float VaultMinimumSpeedFraction { get; }
        public float VaultBaseSpeedMultiplier { get; }
        public float VaultPerfectSpeedMultiplier { get; }
        public bool MaxVelocityEnabled { get; }
        public float MaxVelocityMetersPerSecond { get; }

        public static MovementMetricsInput Default => new MovementMetricsInput(
            7f,
            10.5f,
            14.5f,
            8.3f,
            24f,
            12f,
            1.45f,
            0.48f,
            1.22f,
            0.92f,
            0.8f,
            0.42f,
            1.35f,
            5.6f,
            0.2f,
            1.24f,
            1.2f,
            1.55f,
            2.2f,
            0.2f,
            1.08f,
            1.35f);
    }
}
