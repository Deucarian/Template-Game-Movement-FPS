using UnityEngine;

namespace Deucarian.TemplateGameMovementFps.Definitions
{
    public enum VaultMode
    {
        Hybrid,
        Flow,
        SafetyMantle
    }

    [CreateAssetMenu(menuName = "Deucarian/Movement FPS/Movement Tuning", fileName = "MovementFpsTuning")]
    public sealed class MovementTuningDefinition : ScriptableObject
    {
        [Header("Grounding")]
        [SerializeField, Min(0.01f)]
        private float groundContactProbeDistance = 0.12f;

        [SerializeField, Range(0.05f, 1f)]
        private float groundContactRadiusMultiplier = 0.45f;

        [SerializeField, Range(1f, 89f)]
        private float standableGroundMaxAngleDegrees = 60f;

        [SerializeField, Min(0f)]
        private float runGroundSnapDistance = 0.18f;

        [SerializeField, Min(0f)]
        private float slideGroundSnapDistance = 0.52f;

        [SerializeField, Range(0f, 89f)]
        private float runGroundSnapBreakAngleDegrees = 38f;

        [SerializeField, Range(0f, 89f)]
        private float slideGroundSnapBreakAngleDegrees = 38f;

        [SerializeField, Range(0f, 89f)]
        private float runEdgeReleaseAngleDegrees = 45f;

        [SerializeField, Range(0f, 89f)]
        private float slideEdgeReleaseAngleDegrees = 45f;

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

        [Header("Slide Boosts")]
        [SerializeField, Min(0f)]
        private float airborneSlideBufferSeconds = 0.22f;

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

        [SerializeField, Min(1f)]
        private float slideJumpMinimumSpeedMultiplier = 1.2f;

        [SerializeField, Min(1f)]
        private float slideJumpPeakSpeedMultiplier = 1.55f;

        [SerializeField, Min(1f)]
        private float slideJumpSpeedCapMultiplier = 2.2f;

        [Header("Wallrun Boosts")]
        [SerializeField, Range(0f, 1f)]
        private float wallJumpMinimumSpeedFraction = 0.2f;

        [SerializeField, Range(0.02f, 0.6f)]
        private float wallJumpApexTimingWindow = 0.24f;

        [SerializeField, Min(1f)]
        private float wallJumpPeakTimingMultiplier = 1.24f;

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

        public float GroundContactProbeDistance { get => groundContactProbeDistance; set => groundContactProbeDistance = Mathf.Max(0.01f, value); }
        public float GroundContactRadiusMultiplier { get => groundContactRadiusMultiplier; set => groundContactRadiusMultiplier = Mathf.Clamp(value, 0.05f, 1f); }
        public float StandableGroundMaxAngleDegrees { get => standableGroundMaxAngleDegrees; set => standableGroundMaxAngleDegrees = Mathf.Clamp(value, 1f, 89f); }
        public float RunGroundSnapDistance { get => runGroundSnapDistance; set => runGroundSnapDistance = Mathf.Max(0f, value); }
        public float SlideGroundSnapDistance { get => slideGroundSnapDistance; set => slideGroundSnapDistance = Mathf.Max(0f, value); }
        public float RunGroundSnapBreakAngleDegrees { get => runGroundSnapBreakAngleDegrees; set => runGroundSnapBreakAngleDegrees = Mathf.Clamp(value, 0f, 89f); }
        public float SlideGroundSnapBreakAngleDegrees { get => slideGroundSnapBreakAngleDegrees; set => slideGroundSnapBreakAngleDegrees = Mathf.Clamp(value, 0f, 89f); }
        public float RunEdgeReleaseAngleDegrees { get => runEdgeReleaseAngleDegrees; set => runEdgeReleaseAngleDegrees = Mathf.Clamp(value, 0f, 89f); }
        public float SlideEdgeReleaseAngleDegrees { get => slideEdgeReleaseAngleDegrees; set => slideEdgeReleaseAngleDegrees = Mathf.Clamp(value, 0f, 89f); }
        public float GroundSnapReleaseMinimumDrop { get => groundSnapReleaseMinimumDrop; set => groundSnapReleaseMinimumDrop = Mathf.Max(0f, value); }
        public float DownhillGroundStickExtraProbeDistance { get => downhillGroundStickExtraProbeDistance; set => downhillGroundStickExtraProbeDistance = Mathf.Max(0f, value); }
        public float DownhillGroundStickExtraLookAhead { get => downhillGroundStickExtraLookAhead; set => downhillGroundStickExtraLookAhead = Mathf.Max(0f, value); }
        public float DownhillGroundStickMinimumAngleDegrees { get => downhillGroundStickMinimumAngleDegrees; set => downhillGroundStickMinimumAngleDegrees = Mathf.Clamp(value, 0f, 45f); }
        public float DownhillGroundStickFadeStartAngleDegrees { get => downhillGroundStickFadeStartAngleDegrees; set => downhillGroundStickFadeStartAngleDegrees = Mathf.Clamp(value, 1f, 89f); }
        public float DownhillGroundStickFadeEndAngleDegrees { get => downhillGroundStickFadeEndAngleDegrees; set => downhillGroundStickFadeEndAngleDegrees = Mathf.Clamp(value, 1f, 89f); }
        public float JumpGroundLockoutSeconds { get => jumpGroundLockoutSeconds; set => jumpGroundLockoutSeconds = Mathf.Max(0f, value); }
        public bool MaxVelocityEnabled { get => maxVelocityEnabled; set => maxVelocityEnabled = value; }
        public float MaxVelocityMetersPerSecond { get => maxVelocityMetersPerSecond; set => maxVelocityMetersPerSecond = Mathf.Max(0.01f, value); }
        public bool MaxVelocityHardCap { get => maxVelocityHardCap; set => maxVelocityHardCap = value; }
        public float MaxVelocitySoftPullPerSecond { get => maxVelocitySoftPullPerSecond; set => maxVelocitySoftPullPerSecond = Mathf.Max(0f, value); }
        public bool BunnyHopEnabled { get => bunnyHopEnabled; set => bunnyHopEnabled = value; }
        public float BunnyHopLandingWindowSeconds { get => bunnyHopLandingWindowSeconds; set => bunnyHopLandingWindowSeconds = Mathf.Clamp(value, 0f, 0.4f); }
        public float BunnyHopSteerStrength { get => bunnyHopSteerStrength; set => bunnyHopSteerStrength = Mathf.Clamp01(value); }
        public float BunnyHopSpeedMultiplier { get => bunnyHopSpeedMultiplier; set => bunnyHopSpeedMultiplier = Mathf.Max(1f, value); }
        public float AirborneSlideBufferSeconds { get => airborneSlideBufferSeconds; set => airborneSlideBufferSeconds = Mathf.Max(0f, value); }
        public float SlideMinimumEntrySpeedFraction { get => slideMinimumEntrySpeedFraction; set => slideMinimumEntrySpeedFraction = Mathf.Clamp01(value); }
        public float SlideDragDelayFraction { get => slideDragDelayFraction; set => slideDragDelayFraction = Mathf.Clamp(value, 0f, 0.9f); }
        public float SlideEntrySpeedDelayExponent { get => slideEntrySpeedDelayExponent; set => slideEntrySpeedDelayExponent = Mathf.Max(0.1f, value); }
        public float SlideDragRampExponent { get => slideDragRampExponent; set => slideDragRampExponent = Mathf.Max(0.1f, value); }
        public float SlideEntrySpeedDecayResistanceExponent { get => slideEntrySpeedDecayResistanceExponent; set => slideEntrySpeedDecayResistanceExponent = Mathf.Max(0f, value); }
        public float SlideJumpMinimumSpeedMultiplier { get => slideJumpMinimumSpeedMultiplier; set => slideJumpMinimumSpeedMultiplier = Mathf.Max(1f, value); }
        public float SlideJumpPeakSpeedMultiplier { get => slideJumpPeakSpeedMultiplier; set => slideJumpPeakSpeedMultiplier = Mathf.Max(1f, value); }
        public float SlideJumpSpeedCapMultiplier { get => slideJumpSpeedCapMultiplier; set => slideJumpSpeedCapMultiplier = Mathf.Max(1f, value); }
        public float WallJumpMinimumSpeedFraction { get => wallJumpMinimumSpeedFraction; set => wallJumpMinimumSpeedFraction = Mathf.Clamp01(value); }
        public float WallJumpApexTimingWindow { get => wallJumpApexTimingWindow; set => wallJumpApexTimingWindow = Mathf.Clamp(value, 0.02f, 0.6f); }
        public float WallJumpPeakTimingMultiplier { get => wallJumpPeakTimingMultiplier; set => wallJumpPeakTimingMultiplier = Mathf.Max(1f, value); }
        public float WallJumpLookWeight { get => wallJumpLookWeight; set => wallJumpLookWeight = Mathf.Clamp(value, 0.2f, 0.9f); }
        public float WallJumpAwayWeight { get => wallJumpAwayWeight; set => wallJumpAwayWeight = Mathf.Clamp(value, 0.1f, 0.6f); }
        public float WallJumpCarryWeight { get => wallJumpCarryWeight; set => wallJumpCarryWeight = Mathf.Clamp(value, 0f, 0.5f); }
        public float WallJumpMinimumAwayDot { get => wallJumpMinimumAwayDot; set => wallJumpMinimumAwayDot = Mathf.Clamp(value, 0f, 0.75f); }
        public float WallrunSameWallLockoutSeconds { get => wallrunSameWallLockoutSeconds; set => wallrunSameWallLockoutSeconds = Mathf.Max(0f, value); }
        public bool WallrunDetachReattachDelayEnabled { get => wallrunDetachReattachDelayEnabled; set => wallrunDetachReattachDelayEnabled = value; }
        public float WallrunDetachReattachDelaySeconds { get => wallrunDetachReattachDelaySeconds; set => wallrunDetachReattachDelaySeconds = Mathf.Max(0f, value); }
        public float WallrunReverseReentryDot { get => wallrunReverseReentryDot; set => wallrunReverseReentryDot = Mathf.Clamp(value, -1f, 0f); }
        public float WallrunMaximumSurfaceTurnAngle { get => wallrunMaximumSurfaceTurnAngle; set => wallrunMaximumSurfaceTurnAngle = Mathf.Clamp(value, 1f, 89f); }
        public float WallrunUpperProbeHeightFraction { get => wallrunUpperProbeHeightFraction; set => wallrunUpperProbeHeightFraction = Mathf.Clamp(value, 0.25f, 0.5f); }
        public float WallrunTopFlattenProbeHeightFraction { get => wallrunTopFlattenProbeHeightFraction; set => wallrunTopFlattenProbeHeightFraction = Mathf.Clamp(value, 0.43f, 0.65f); }
        public float WallrunTopUpwardVelocityMultiplier { get => wallrunTopUpwardVelocityMultiplier; set => wallrunTopUpwardVelocityMultiplier = Mathf.Clamp01(value); }
        public float WallrunCurvedSurfaceProbeForwardBias { get => wallrunCurvedSurfaceProbeForwardBias; set => wallrunCurvedSurfaceProbeForwardBias = Mathf.Clamp01(value); }
        public float WallrunHorizontalWallStickSpeed { get => wallrunHorizontalWallStickSpeed; set => wallrunHorizontalWallStickSpeed = Mathf.Clamp(value, 0f, 4f); }
        public bool VerticalWallrunEnabled { get => verticalWallrunEnabled; set => verticalWallrunEnabled = value; }
        public float VerticalWallrunLookMaxAngleDegrees { get => verticalWallrunLookMaxAngleDegrees; set => verticalWallrunLookMaxAngleDegrees = Mathf.Clamp(value, 1f, 89f); }
        public float VerticalWallrunMinimumLookUpAngleDegrees { get => verticalWallrunMinimumLookUpAngleDegrees; set => verticalWallrunMinimumLookUpAngleDegrees = Mathf.Clamp(value, 0f, 75f); }
        public float VerticalWallrunMoveMaxAngleDegrees { get => verticalWallrunMoveMaxAngleDegrees; set => verticalWallrunMoveMaxAngleDegrees = Mathf.Clamp(value, 1f, 89f); }
        public float VerticalWallrunMinimumEntrySpeedFraction { get => verticalWallrunMinimumEntrySpeedFraction; set => verticalWallrunMinimumEntrySpeedFraction = Mathf.Clamp01(value); }
        public float VerticalWallrunUpSpeed { get => verticalWallrunUpSpeed; set => verticalWallrunUpSpeed = Mathf.Max(0.1f, value); }
        public float VerticalWallrunDurationSeconds { get => verticalWallrunDurationSeconds; set => verticalWallrunDurationSeconds = Mathf.Max(0.05f, value); }
        public float VerticalWallrunLateDecelStart { get => verticalWallrunLateDecelStart; set => verticalWallrunLateDecelStart = Mathf.Clamp(value, 0.1f, 0.95f); }
        public float VerticalWallrunLateDecelExponent { get => verticalWallrunLateDecelExponent; set => verticalWallrunLateDecelExponent = Mathf.Max(1f, value); }
        public float VerticalWallrunEndSpeedRetention { get => verticalWallrunEndSpeedRetention; set => verticalWallrunEndSpeedRetention = Mathf.Clamp01(value); }
        public float VerticalWallrunWallStickSpeed { get => verticalWallrunWallStickSpeed; set => verticalWallrunWallStickSpeed = Mathf.Max(0f, value); }
        public VaultMode VaultMode { get => vaultMode; set => vaultMode = value; }
        public bool VaultHoldAssistEnabled { get => vaultHoldAssistEnabled; set => vaultHoldAssistEnabled = value; }
        public float VaultFlowMaxHeightMeters
        {
            get => vaultFlowMaxHeightMeters;
            set
            {
                vaultFlowMaxHeightMeters = Mathf.Max(0.25f, value);
                vaultMantleMaxHeightMeters = Mathf.Max(vaultFlowMaxHeightMeters, vaultMantleMaxHeightMeters);
            }
        }

        public float VaultMantleMaxHeightMeters
        {
            get => vaultMantleMaxHeightMeters;
            set => vaultMantleMaxHeightMeters = Mathf.Max(VaultFlowMaxHeightMeters, value);
        }

        public float FlowVaultOverDistanceMeters { get => flowVaultOverDistanceMeters; set => flowVaultOverDistanceMeters = Mathf.Max(0.05f, value); }
        public float SafetyMantleOverDistanceMeters { get => safetyMantleOverDistanceMeters; set => safetyMantleOverDistanceMeters = Mathf.Max(0.05f, value); }
        public float FlowVaultBlendDurationSeconds { get => flowVaultBlendDurationSeconds; set => flowVaultBlendDurationSeconds = Mathf.Clamp(value, 0.02f, 0.3f); }
        public float SafetyMantleBlendDurationSeconds { get => safetyMantleBlendDurationSeconds; set => safetyMantleBlendDurationSeconds = Mathf.Clamp(value, 0.04f, 0.6f); }
        public float FlowVaultBaseSpeedMultiplier { get => flowVaultBaseSpeedMultiplier; set => flowVaultBaseSpeedMultiplier = Mathf.Max(0.1f, value); }
        public float FlowVaultPerfectSpeedMultiplier { get => flowVaultPerfectSpeedMultiplier; set => flowVaultPerfectSpeedMultiplier = Mathf.Max(FlowVaultBaseSpeedMultiplier, value); }
        public float SafetyMantleBaseSpeedMultiplier { get => safetyMantleBaseSpeedMultiplier; set => safetyMantleBaseSpeedMultiplier = Mathf.Max(0.1f, value); }
        public float SafetyMantlePerfectSpeedMultiplier { get => safetyMantlePerfectSpeedMultiplier; set => safetyMantlePerfectSpeedMultiplier = Mathf.Max(SafetyMantleBaseSpeedMultiplier, value); }
        public float FlowVaultMinimumVerticalBoost { get => flowVaultMinimumVerticalBoost; set => flowVaultMinimumVerticalBoost = Mathf.Clamp01(value); }
        public float FlowVaultPerfectVerticalBoost { get => flowVaultPerfectVerticalBoost; set => flowVaultPerfectVerticalBoost = Mathf.Max(FlowVaultMinimumVerticalBoost, Mathf.Clamp01(value)); }
        public float SafetyMantleMinimumVerticalBoost { get => safetyMantleMinimumVerticalBoost; set => safetyMantleMinimumVerticalBoost = Mathf.Clamp01(value); }
        public float SafetyMantlePerfectVerticalBoost { get => safetyMantlePerfectVerticalBoost; set => safetyMantlePerfectVerticalBoost = Mathf.Max(SafetyMantleMinimumVerticalBoost, Mathf.Clamp01(value)); }
        public float VaultMinimumSpeedFraction { get => vaultMinimumSpeedFraction; set => vaultMinimumSpeedFraction = Mathf.Clamp01(value); }
        public float VaultBaseSpeedMultiplier { get => vaultBaseSpeedMultiplier; set => vaultBaseSpeedMultiplier = Mathf.Max(1f, value); }
        public float VaultPerfectSpeedMultiplier { get => vaultPerfectSpeedMultiplier; set => vaultPerfectSpeedMultiplier = Mathf.Max(1f, value); }
        public float VaultMinimumVerticalBoost { get => vaultMinimumVerticalBoost; set => vaultMinimumVerticalBoost = Mathf.Clamp01(value); }
        public float VaultPerfectVerticalBoost { get => vaultPerfectVerticalBoost; set => vaultPerfectVerticalBoost = Mathf.Clamp01(value); }
        public float VaultBlendDurationSeconds { get => vaultBlendDurationSeconds; set => vaultBlendDurationSeconds = Mathf.Clamp(value, 0.02f, 0.3f); }

        private void OnValidate()
        {
            VaultFlowMaxHeightMeters = vaultFlowMaxHeightMeters;
            VaultMantleMaxHeightMeters = vaultMantleMaxHeightMeters;
            FlowVaultOverDistanceMeters = flowVaultOverDistanceMeters;
            SafetyMantleOverDistanceMeters = safetyMantleOverDistanceMeters;
            FlowVaultBlendDurationSeconds = flowVaultBlendDurationSeconds;
            SafetyMantleBlendDurationSeconds = safetyMantleBlendDurationSeconds;
            FlowVaultBaseSpeedMultiplier = flowVaultBaseSpeedMultiplier;
            FlowVaultPerfectSpeedMultiplier = flowVaultPerfectSpeedMultiplier;
            SafetyMantleBaseSpeedMultiplier = safetyMantleBaseSpeedMultiplier;
            SafetyMantlePerfectSpeedMultiplier = safetyMantlePerfectSpeedMultiplier;
            FlowVaultMinimumVerticalBoost = flowVaultMinimumVerticalBoost;
            FlowVaultPerfectVerticalBoost = flowVaultPerfectVerticalBoost;
            SafetyMantleMinimumVerticalBoost = safetyMantleMinimumVerticalBoost;
            SafetyMantlePerfectVerticalBoost = safetyMantlePerfectVerticalBoost;
            VerticalWallrunLookMaxAngleDegrees = verticalWallrunLookMaxAngleDegrees;
            WallrunHorizontalWallStickSpeed = wallrunHorizontalWallStickSpeed;
            VerticalWallrunMinimumLookUpAngleDegrees = verticalWallrunMinimumLookUpAngleDegrees;
            VerticalWallrunMoveMaxAngleDegrees = verticalWallrunMoveMaxAngleDegrees;
            VerticalWallrunMinimumEntrySpeedFraction = verticalWallrunMinimumEntrySpeedFraction;
            VerticalWallrunUpSpeed = verticalWallrunUpSpeed;
            VerticalWallrunDurationSeconds = verticalWallrunDurationSeconds;
            VerticalWallrunLateDecelStart = verticalWallrunLateDecelStart;
            VerticalWallrunLateDecelExponent = verticalWallrunLateDecelExponent;
            VerticalWallrunEndSpeedRetention = verticalWallrunEndSpeedRetention;
            VerticalWallrunWallStickSpeed = verticalWallrunWallStickSpeed;
        }
    }
}
