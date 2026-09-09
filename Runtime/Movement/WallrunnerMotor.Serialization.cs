using Deucarian.TemplateGameMovementFps.Definitions;
using UnityEngine;

namespace Deucarian.TemplateGameMovementFps.Movement
{
    public sealed partial class WallrunnerMotor
    {
        [Header("Ground")]
        [SerializeField]
        private float walkSpeed = 7f;

        [SerializeField]
        private float sprintSpeed = 10.5f;

        [SerializeField]
        private float slideSpeed = 14.5f;

        [SerializeField]
        private float slideDurationSeconds = 0.72f;

        [SerializeField, Min(0f)]
        private float airborneSlideBufferSeconds = 0.22f;

        [SerializeField, Min(1f)]
        private float slideEnterSpeedMultiplier = 1.18f;

        [SerializeField, Min(1f)]
        private float slideJumpMinimumSpeedMultiplier = 1.2f;

        [SerializeField, Min(1f)]
        private float slideJumpPeakSpeedMultiplier = 1.55f;

        [SerializeField, Min(1f)]
        private float slideJumpSpeedCapMultiplier = 2.2f;

        [SerializeField, Range(0f, 1f)]
        private float slideMinimumEntrySpeedFraction = 0.2f;

        [SerializeField, Range(0f, 0.9f)]
        private float slideDragDelayFraction = 0.35f;

        [SerializeField, Min(0.1f)]
        private float slideEntrySpeedDelayExponent = 1.45f;

        [SerializeField, Min(0.1f)]
        private float slideDragRampExponent = 2.8f;

        [SerializeField, Min(0f)]
        private float slideEntrySpeedDecayResistanceExponent = 1.15f;

        [SerializeField, Min(0f)]
        private float slideSpeedDecayPerSecond = 1.6f;

        [SerializeField, Min(0f)]
        private float slideDownhillSpeedGainPerSecond = 3.6f;

        [SerializeField, Min(1f)]
        private float slideDownhillEnterSpeedMultiplier = 1.35f;

        [SerializeField, Range(1f, 8f)]
        private float slideDownhillSpeedCapMultiplier = 4f;

        [SerializeField, Min(0f)]
        private float slideUphillSpeedLossPerSecond = 5.2f;

        [SerializeField, Range(0f, 60f)]
        private float slideSlopeEffectMinimumAngleDegrees = 20f;

        [SerializeField, Min(0f)]
        private float slideGroundSnapDistance = 0.52f;

        [SerializeField, Range(0f, 89f)]
        private float slideGroundSnapBreakAngleDegrees = 38f;

        [SerializeField, Range(0f, 89f)]
        private float slideGroundSnapReleaseDropAngleDegrees = 45f;

        [SerializeField]
        private float groundAcceleration = 48f;

        [SerializeField, Min(0f)]
        private float groundSnapDistance = 0.18f;

        [SerializeField, Range(0f, 89f)]
        private float groundSnapBreakAngleDegrees = 38f;

        [SerializeField, Range(0f, 89f)]
        private float groundSnapReleaseDropAngleDegrees = 45f;

        [SerializeField, Min(0f)]
        private float groundSnapReleaseMinimumDrop = 0.04f;

        [SerializeField, Min(0f)]
        private float downhillGroundStickExtraProbeDistance = 0.48f;

        [SerializeField, Min(0f)]
        private float downhillGroundStickExtraLookAhead = 0.75f;

        [SerializeField, Range(0f, 45f)]
        private float downhillGroundStickMinimumAngleDegrees = 3f;

        [SerializeField, Range(1f, 89f)]
        private float downhillGroundStickFadeStartAngleDegrees = 24f;

        [SerializeField, Range(1f, 89f)]
        private float downhillGroundStickFadeEndAngleDegrees = 56f;

        [Header("Air")]
        [SerializeField]
        private float jumpVelocity = 8.3f;

        [SerializeField]
        private int extraAirJumps = 1;

        [SerializeField]
        private float airAcceleration = 18f;

        [SerializeField]
        private float gravity = 24f;

        [SerializeField, Min(0.05f)]
        private float groundContactProbeDistance = 0.12f;

        [SerializeField, Range(0.05f, 1f)]
        private float groundContactRadiusMultiplier = 0.45f;

        [SerializeField, Range(1f, 89f)]
        private float standableGroundMaxAngleDegrees = 60f;

        [SerializeField, Min(0f)]
        private float groundedJumpGraceSeconds = 0.12f;

        [SerializeField, Min(0f)]
        private float jumpGroundLockoutSeconds = 0.14f;

        [Header("Velocity Limit")]
        [SerializeField]
        private bool maxVelocityEnabled;

        [SerializeField, Min(0.01f)]
        private float maxVelocityMetersPerSecond = 36f;

        [SerializeField]
        private bool maxVelocityHardCap = true;

        [SerializeField, Min(0f)]
        private float maxVelocitySoftPullPerSecond = 18f;

        [Header("Bunny Hop / Strafe Jump")]
        [SerializeField]
        private bool bunnyHopEnabled = true;

        [SerializeField, Range(0f, 0.4f)]
        private float bunnyHopLandingWindowSeconds = 0.16f;

        [SerializeField, Range(0f, 1f)]
        private float bunnyHopSteerStrength = 0.35f;

        [SerializeField, Min(1f)]
        private float bunnyHopSpeedMultiplier = 1.04f;

        [Header("Wallrun")]
        [SerializeField]
        private float wallrunSpeed = 12f;

        [SerializeField]
        private float wallrunGravity = 4.8f;

        [SerializeField]
        private float wallrunDurationSeconds = 1.45f;

        [SerializeField, Range(0.1f, 0.75f)]
        private float wallrunApexTime = 0.48f;

        [SerializeField, Min(1f)]
        private float wallrunApexSpeedMultiplier = 1.22f;

        [SerializeField, Range(0.1f, 1f)]
        private float wallrunPostApexRetention = 0.92f;

        [SerializeField, Range(0.1f, 0.95f)]
        private float wallrunLateDecelStart = 0.8f;

        [SerializeField, Min(1f)]
        private float wallrunLateDecelExponent = 3.2f;

        [SerializeField, Range(0.1f, 1f)]
        private float wallrunEndSpeedRetention = 0.42f;

        [SerializeField, Min(0f)]
        private float wallrunApexHeight = 1.35f;

        [SerializeField, Min(1f)]
        private float wallrunFatigueGravityMultiplier = 3.2f;

        [SerializeField, Min(0f)]
        private float wallrunMaxDownwardVelocity = 9f;

        [SerializeField, Min(1f)]
        private float wallJumpSpeedMultiplier = 1.42f;

        [SerializeField, Min(1f)]
        private float wallJumpTurnaroundSpeedMultiplier = 1.2f;

        [SerializeField, Min(1f)]
        private float wallJumpSpeedCapMultiplier = 2.45f;

        [SerializeField, Range(0f, 1f)]
        private float wallJumpMinimumSpeedFraction = 0.2f;

        [SerializeField, Range(0.02f, 0.6f)]
        private float wallJumpApexTimingWindow = 0.24f;

        [SerializeField, Min(1f)]
        private float wallJumpPeakTimingMultiplier = 1.24f;

        [SerializeField]
        private float wallJumpVerticalVelocity = 5.6f;

        [SerializeField, Range(0.2f, 0.9f)]
        private float wallJumpLookWeight = 0.52f;

        [SerializeField, Range(0.1f, 0.6f)]
        private float wallJumpAwayWeight = 0.44f;

        [SerializeField, Range(0f, 0.5f)]
        private float wallJumpCarryWeight = 0.2f;

        [SerializeField, Range(0f, 0.75f)]
        private float wallJumpMinimumAwayDot = 0.38f;

        [SerializeField, Min(0f)]
        private float wallrunSameWallLockoutSeconds = 0.16f;

        [SerializeField]
        private bool wallrunDetachReattachDelayEnabled;

        [SerializeField, Min(0f)]
        private float wallrunDetachReattachDelaySeconds = 0.1f;

        [SerializeField, Range(-1f, 0f)]
        private float wallrunReverseReentryDot = -0.25f;

        [SerializeField, Range(1f, 89f)]
        private float wallrunMaximumSurfaceTurnAngle = 60f;

        [SerializeField, Range(0.25f, 0.5f)]
        private float wallrunUpperProbeHeightFraction = 0.42f;

        [SerializeField, Range(0.43f, 0.65f)]
        private float wallrunTopFlattenProbeHeightFraction = 0.5f;

        [SerializeField, Range(0f, 1f)]
        private float wallrunTopUpwardVelocityMultiplier = 0.25f;

        [SerializeField, Range(0f, 1f)]
        private float wallrunCurvedSurfaceProbeForwardBias = 0.45f;

        [SerializeField, Range(0f, 4f)]
        private float wallrunHorizontalWallStickSpeed = 0f;

        [SerializeField]
        private bool verticalWallrunEnabled = true;

        [SerializeField, Range(1f, 89f)]
        private float verticalWallrunLookMaxAngleDegrees = 26f;

        [SerializeField, Range(0f, 75f)]
        private float verticalWallrunMinimumLookUpAngleDegrees = 10f;

        [SerializeField, Range(1f, 89f)]
        private float verticalWallrunMoveMaxAngleDegrees = 40f;

        [SerializeField, Range(0f, 1f)]
        private float verticalWallrunMinimumEntrySpeedFraction = 0.2f;

        [SerializeField, Min(0.1f)]
        private float verticalWallrunUpSpeed = 9.2f;

        [SerializeField, Min(0.05f)]
        private float verticalWallrunDurationSeconds = 0.9f;

        [SerializeField, Range(0.1f, 0.95f)]
        private float verticalWallrunLateDecelStart = 0.55f;

        [SerializeField, Min(1f)]
        private float verticalWallrunLateDecelExponent = 2.6f;

        [SerializeField, Range(0f, 1f)]
        private float verticalWallrunEndSpeedRetention = 0.22f;

        [SerializeField, Min(0f)]
        private float verticalWallrunWallStickSpeed = 2.2f;

        [Header("Vault")]
        [SerializeField]
        private VaultMode vaultMode = VaultMode.Hybrid;

        [SerializeField]
        private bool vaultHoldAssistEnabled = true;

        [SerializeField]
        private float mantleCheckDistance = 1.1f;

        [SerializeField]
        private float mantleHeight = 2.35f;

        [SerializeField, Min(0.25f)]
        private float vaultFlowMaxHeightMeters = 1.45f;

        [SerializeField, Min(0.25f)]
        private float vaultMantleMaxHeightMeters = 2.35f;

        [SerializeField, Min(0.05f)]
        private float flowVaultOverDistanceMeters = 1.45f;

        [SerializeField, Min(0.05f)]
        private float safetyMantleOverDistanceMeters = 0.75f;

        [SerializeField, Range(0.02f, 0.3f)]
        private float flowVaultBlendDurationSeconds = 0.08f;

        [SerializeField, Range(0.04f, 0.6f)]
        private float safetyMantleBlendDurationSeconds = 0.22f;

        [SerializeField, Min(0.1f)]
        private float flowVaultBaseSpeedMultiplier = 1.04f;

        [SerializeField, Min(0.1f)]
        private float flowVaultPerfectSpeedMultiplier = 1.16f;

        [SerializeField, Min(0.1f)]
        private float safetyMantleBaseSpeedMultiplier = 0.82f;

        [SerializeField, Min(0.1f)]
        private float safetyMantlePerfectSpeedMultiplier = 1.04f;

        [SerializeField, Range(0f, 1f)]
        private float flowVaultMinimumVerticalBoost = 0.22f;

        [SerializeField, Range(0f, 1f)]
        private float flowVaultPerfectVerticalBoost = 0.42f;

        [SerializeField, Range(0f, 1f)]
        private float safetyMantleMinimumVerticalBoost = 0.1f;

        [SerializeField, Range(0f, 1f)]
        private float safetyMantlePerfectVerticalBoost = 0.26f;

        [SerializeField, HideInInspector]
        private float mantleVaultOverDistance = 1.25f;

        [SerializeField, HideInInspector, Range(0f, 1f)]
        private float vaultMinimumSpeedFraction = 0.2f;

        [SerializeField, HideInInspector, Min(1f)]
        private float vaultBaseSpeedMultiplier = 1.08f;

        [SerializeField, HideInInspector, Min(1f)]
        private float vaultPerfectSpeedMultiplier = 1.35f;

        [SerializeField, HideInInspector, Range(0f, 1f)]
        private float vaultMinimumVerticalBoost = 0.28f;

        [SerializeField, HideInInspector, Range(0f, 1f)]
        private float vaultPerfectVerticalBoost = 0.58f;

        [SerializeField, HideInInspector, Range(0.02f, 0.3f)]
        private float vaultBlendDurationSeconds = 0.12f;

        [SerializeField, Range(0f, 0.65f)]
        private float mantleMaximumWallNormalY = 0.22f;

        [SerializeField, Range(0f, 1f)]
        private float mantleMinimumTopNormalY = 0.55f;

        [SerializeField, Range(0f, 1f)]
        private float mantleMinimumApproachDot = 0.2f;

        [Header("Collision")]
        [SerializeField]
        private LayerMask collisionMask = ~0;

        [SerializeField]
        private float skinWidth = 0.04f;

    }
}
