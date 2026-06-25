using Deucarian.TemplateGameMovementFps.Definitions;
using UnityEngine;

namespace Deucarian.TemplateGameMovementFps.Movement
{
    public enum WallrunnerMovementState
    {
        Grounded,
        Sprinting,
        Sliding,
        Airborne,
        Wallrunning,
        Vaulting
    }

    public enum VaultTraversalStyle
    {
        None,
        Flow,
        SafetyMantle
    }

    public enum WallrunTraversalStyle
    {
        Horizontal,
        Vertical
    }

    [RequireComponent(typeof(CapsuleCollider))]
    public sealed class WallrunnerMotor : MonoBehaviour
    {
        private const float GroundProbeBottomClearance = 0.03f;

        public readonly struct RuntimeSnapshot
        {
            public RuntimeSnapshot(
                Vector3 position,
                Quaternion rotation,
                Vector3 velocity,
                Vector3 wallNormal,
                Vector3 groundNormal,
                Vector3 wallrunLockedNormal,
                Vector3 wallrunLockedDirection,
                Vector3 wallrunActiveDirection,
                Vector3 wallrunGuidanceDirection,
                Vector3 wallrunBlockedNormal,
                Vector3 wallrunChainLockedNormal,
                Vector3 wallrunChainLockedDirection,
                Transform wallrunSurfaceTransform,
                Vector3 bunnyHopCarryVelocity,
                Vector3 vaultStartPosition,
                Vector3 vaultTargetPosition,
                Vector3 vaultExitVelocity,
                Vector3 lookForward,
                float slideTimer,
                float wallrunTimer,
                float wallrunStartSpeed,
                float wallrunPeakSpeed,
                float wallrunSameWallLockoutTimer,
                float wallrunDetachReattachTimer,
                float slideEntrySpeed,
                float groundSnapLockoutTimer,
                float groundedJumpGraceTimer,
                float slideJumpGraceTimer,
                float airborneSlideBufferTimer,
                float bunnyHopWindowTimer,
                float vaultBlendTimer,
                float vaultBlendDuration,
                int airJumpsRemaining,
                bool grounded,
                bool wallrunFatigued,
                bool wallrunChainSameDirectionLocked,
                bool wallrunNearTop,
                WallrunTraversalStyle wallrunStyle,
                VaultTraversalStyle activeVaultStyle,
                VaultTraversalStyle lastVaultStyle,
                WallrunnerMovementState state)
            {
                Position = position;
                Rotation = rotation;
                Velocity = velocity;
                WallNormal = wallNormal;
                GroundNormal = groundNormal;
                WallrunLockedNormal = wallrunLockedNormal;
                WallrunLockedDirection = wallrunLockedDirection;
                WallrunActiveDirection = wallrunActiveDirection;
                WallrunGuidanceDirection = wallrunGuidanceDirection;
                WallrunBlockedNormal = wallrunBlockedNormal;
                WallrunChainLockedNormal = wallrunChainLockedNormal;
                WallrunChainLockedDirection = wallrunChainLockedDirection;
                WallrunSurfaceTransform = wallrunSurfaceTransform;
                BunnyHopCarryVelocity = bunnyHopCarryVelocity;
                VaultStartPosition = vaultStartPosition;
                VaultTargetPosition = vaultTargetPosition;
                VaultExitVelocity = vaultExitVelocity;
                LookForward = lookForward;
                SlideTimer = slideTimer;
                WallrunTimer = wallrunTimer;
                WallrunStartSpeed = wallrunStartSpeed;
                WallrunPeakSpeed = wallrunPeakSpeed;
                WallrunSameWallLockoutTimer = wallrunSameWallLockoutTimer;
                WallrunDetachReattachTimer = wallrunDetachReattachTimer;
                SlideEntrySpeed = slideEntrySpeed;
                GroundSnapLockoutTimer = groundSnapLockoutTimer;
                GroundedJumpGraceTimer = groundedJumpGraceTimer;
                SlideJumpGraceTimer = slideJumpGraceTimer;
                AirborneSlideBufferTimer = airborneSlideBufferTimer;
                BunnyHopWindowTimer = bunnyHopWindowTimer;
                VaultBlendTimer = vaultBlendTimer;
                VaultBlendDuration = vaultBlendDuration;
                AirJumpsRemaining = airJumpsRemaining;
                Grounded = grounded;
                WallrunFatigued = wallrunFatigued;
                WallrunChainSameDirectionLocked = wallrunChainSameDirectionLocked;
                WallrunNearTop = wallrunNearTop;
                WallrunStyle = wallrunStyle;
                ActiveVaultStyle = activeVaultStyle;
                LastVaultStyle = lastVaultStyle;
                State = state;
            }

            public Vector3 Position { get; }

            public Quaternion Rotation { get; }

            public Vector3 Velocity { get; }

            public Vector3 WallNormal { get; }

            public Vector3 GroundNormal { get; }

            public Vector3 WallrunLockedNormal { get; }

            public Vector3 WallrunLockedDirection { get; }

            public Vector3 WallrunActiveDirection { get; }

            public Vector3 WallrunGuidanceDirection { get; }

            public Vector3 WallrunBlockedNormal { get; }

            public Vector3 WallrunChainLockedNormal { get; }

            public Vector3 WallrunChainLockedDirection { get; }

            public Transform WallrunSurfaceTransform { get; }

            public Vector3 BunnyHopCarryVelocity { get; }

            public Vector3 VaultStartPosition { get; }

            public Vector3 VaultTargetPosition { get; }

            public Vector3 VaultExitVelocity { get; }

            public Vector3 LookForward { get; }

            public float SlideTimer { get; }

            public float WallrunTimer { get; }

            public float WallrunStartSpeed { get; }

            public float WallrunPeakSpeed { get; }

            public float WallrunSameWallLockoutTimer { get; }

            public float WallrunDetachReattachTimer { get; }

            public float SlideEntrySpeed { get; }

            public float GroundSnapLockoutTimer { get; }

            public float GroundedJumpGraceTimer { get; }

            public float SlideJumpGraceTimer { get; }

            public float AirborneSlideBufferTimer { get; }

            public float BunnyHopWindowTimer { get; }

            public float VaultBlendTimer { get; }

            public float VaultBlendDuration { get; }

            public int AirJumpsRemaining { get; }

            public bool Grounded { get; }

            public bool WallrunFatigued { get; }

            public bool WallrunChainSameDirectionLocked { get; }

            public bool WallrunNearTop { get; }

            public WallrunTraversalStyle WallrunStyle { get; }

            public VaultTraversalStyle ActiveVaultStyle { get; }

            public VaultTraversalStyle LastVaultStyle { get; }

            public WallrunnerMovementState State { get; }
        }

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

        private float MinimumGroundNormalY => Mathf.Cos(standableGroundMaxAngleDegrees * Mathf.Deg2Rad);

        private CapsuleCollider _capsule;
        private Vector3 _velocity;
        private Vector3 _wallNormal;
        private float _slideTimer;
        private float _wallrunTimer;
        private float _wallrunStartSpeed;
        private float _wallrunPeakSpeed;
        private float _wallrunSameWallLockoutTimer;
        private float _wallrunDetachReattachTimer;
        private float _slideEntrySpeed;
        private int _airJumpsRemaining;
        private bool _grounded;
        private bool _wallrunFatigued;
        private Vector3 _groundNormal = Vector3.up;
        private Vector3 _wallrunLockedNormal;
        private Vector3 _wallrunLockedDirection;
        private Vector3 _wallrunActiveDirection;
        private Vector3 _wallrunGuidanceDirection;
        private Vector3 _wallrunBlockedNormal;
        private Vector3 _wallrunChainLockedNormal;
        private Vector3 _wallrunChainLockedDirection;
        private Transform _wallrunSurfaceTransform;
        private bool _wallrunChainSameDirectionLocked;
        private bool _wallrunNearTop;
        private float _groundSnapLockoutTimer;
        private WallrunTraversalStyle _wallrunStyle;
        private float _groundedJumpGraceTimer;
        private float _slideJumpGraceTimer;
        private float _airborneSlideBufferTimer;
        private float _bunnyHopWindowTimer;
        private Vector3 _bunnyHopCarryVelocity;
        private float _vaultBlendTimer;
        private float _vaultBlendDuration;
        private Vector3 _vaultStartPosition;
        private Vector3 _vaultTargetPosition;
        private Vector3 _vaultExitVelocity;
        private Vector3 _lookForward;
        private VaultTraversalStyle _activeVaultStyle;
        private VaultTraversalStyle _lastVaultStyle;

        public WallrunnerMovementState State { get; private set; }

        public Vector3 Velocity => _velocity;

        public bool IsGrounded => _grounded;

        public int AirJumpsRemaining => _airJumpsRemaining;

        public WallrunTraversalStyle ActiveWallrunStyle => State == WallrunnerMovementState.Wallrunning ? _wallrunStyle : WallrunTraversalStyle.Horizontal;

        public Vector3 ActiveWallrunNormal => State == WallrunnerMovementState.Wallrunning ? _wallNormal : Vector3.zero;

        public VaultTraversalStyle ActiveVaultStyle => _activeVaultStyle;

        public VaultTraversalStyle LastVaultStyle => _lastVaultStyle;

        public MovementMetricsInput CreateMovementMetricsInput()
        {
            return new MovementMetricsInput(
                walkSpeed,
                sprintSpeed,
                slideSpeed,
                jumpVelocity,
                gravity,
                wallrunSpeed,
                wallrunDurationSeconds,
                wallrunApexTime,
                wallrunApexSpeedMultiplier,
                wallrunPostApexRetention,
                wallrunLateDecelStart,
                wallrunEndSpeedRetention,
                wallrunApexHeight,
                wallJumpVerticalVelocity,
                wallJumpMinimumSpeedFraction,
                wallJumpPeakTimingMultiplier,
                slideJumpMinimumSpeedMultiplier,
                slideJumpPeakSpeedMultiplier,
                slideJumpSpeedCapMultiplier,
                vaultMinimumSpeedFraction,
                flowVaultBaseSpeedMultiplier,
                flowVaultPerfectSpeedMultiplier,
                maxVelocityEnabled,
                maxVelocityMetersPerSecond);
        }

        public float GroundContactProbeDistance
        {
            get => groundContactProbeDistance;
            set => groundContactProbeDistance = Mathf.Max(0.01f, value);
        }

        public float GroundContactRadiusMultiplier
        {
            get => groundContactRadiusMultiplier;
            set => groundContactRadiusMultiplier = Mathf.Clamp(value, 0.05f, 1f);
        }

        public float StandableGroundMaxAngleDegrees
        {
            get => standableGroundMaxAngleDegrees;
            set => standableGroundMaxAngleDegrees = Mathf.Clamp(value, 1f, 89f);
        }

        public float RunGroundSnapDistance
        {
            get => groundSnapDistance;
            set => groundSnapDistance = Mathf.Max(0f, value);
        }

        public float SlideGroundSnapDistance
        {
            get => slideGroundSnapDistance;
            set => slideGroundSnapDistance = Mathf.Max(0f, value);
        }

        public float RunGroundSnapBreakAngleDegrees
        {
            get => groundSnapBreakAngleDegrees;
            set => groundSnapBreakAngleDegrees = Mathf.Clamp(value, 0f, 89f);
        }

        public float SlideGroundSnapBreakAngleDegrees
        {
            get => slideGroundSnapBreakAngleDegrees;
            set => slideGroundSnapBreakAngleDegrees = Mathf.Clamp(value, 0f, 89f);
        }

        public float GroundSnapReleaseDropAngleDegrees
        {
            get => groundSnapReleaseDropAngleDegrees;
            set => groundSnapReleaseDropAngleDegrees = Mathf.Clamp(value, 0f, 89f);
        }

        public float SlideGroundSnapReleaseDropAngleDegrees
        {
            get => slideGroundSnapReleaseDropAngleDegrees;
            set => slideGroundSnapReleaseDropAngleDegrees = Mathf.Clamp(value, 0f, 89f);
        }

        public float GroundSnapReleaseMinimumDrop
        {
            get => groundSnapReleaseMinimumDrop;
            set => groundSnapReleaseMinimumDrop = Mathf.Max(0f, value);
        }

        public float DownhillGroundStickExtraProbeDistance
        {
            get => downhillGroundStickExtraProbeDistance;
            set => downhillGroundStickExtraProbeDistance = Mathf.Max(0f, value);
        }

        public float DownhillGroundStickExtraLookAhead
        {
            get => downhillGroundStickExtraLookAhead;
            set => downhillGroundStickExtraLookAhead = Mathf.Max(0f, value);
        }

        public float DownhillGroundStickMinimumAngleDegrees
        {
            get => downhillGroundStickMinimumAngleDegrees;
            set => downhillGroundStickMinimumAngleDegrees = Mathf.Clamp(value, 0f, 45f);
        }

        public float DownhillGroundStickFadeStartAngleDegrees
        {
            get => downhillGroundStickFadeStartAngleDegrees;
            set => downhillGroundStickFadeStartAngleDegrees = Mathf.Clamp(value, 1f, 89f);
        }

        public float DownhillGroundStickFadeEndAngleDegrees
        {
            get => downhillGroundStickFadeEndAngleDegrees;
            set => downhillGroundStickFadeEndAngleDegrees = Mathf.Clamp(value, 1f, 89f);
        }

        public float AirborneSlideBufferSeconds
        {
            get => airborneSlideBufferSeconds;
            set => airborneSlideBufferSeconds = Mathf.Max(0f, value);
        }

        public bool MaxVelocityEnabled
        {
            get => maxVelocityEnabled;
            set => maxVelocityEnabled = value;
        }

        public float MaxVelocityMetersPerSecond
        {
            get => maxVelocityMetersPerSecond;
            set => maxVelocityMetersPerSecond = Mathf.Max(0.01f, value);
        }

        public bool MaxVelocityHardCap
        {
            get => maxVelocityHardCap;
            set => maxVelocityHardCap = value;
        }

        public float MaxVelocitySoftPullPerSecond
        {
            get => maxVelocitySoftPullPerSecond;
            set => maxVelocitySoftPullPerSecond = Mathf.Max(0f, value);
        }

        public bool BunnyHopEnabled
        {
            get => bunnyHopEnabled;
            set => bunnyHopEnabled = value;
        }

        public float BunnyHopLandingWindowSeconds
        {
            get => bunnyHopLandingWindowSeconds;
            set => bunnyHopLandingWindowSeconds = Mathf.Clamp(value, 0f, 0.4f);
        }

        public float BunnyHopSteerStrength
        {
            get => bunnyHopSteerStrength;
            set => bunnyHopSteerStrength = Mathf.Clamp01(value);
        }

        public float BunnyHopSpeedMultiplier
        {
            get => bunnyHopSpeedMultiplier;
            set => bunnyHopSpeedMultiplier = Mathf.Max(1f, value);
        }

        public bool VerticalWallrunEnabled
        {
            get => verticalWallrunEnabled;
            set => verticalWallrunEnabled = value;
        }

        public float VerticalWallrunLookMaxAngleDegrees
        {
            get => verticalWallrunLookMaxAngleDegrees;
            set => verticalWallrunLookMaxAngleDegrees = Mathf.Clamp(value, 1f, 89f);
        }

        public float VerticalWallrunMinimumLookUpAngleDegrees
        {
            get => verticalWallrunMinimumLookUpAngleDegrees;
            set => verticalWallrunMinimumLookUpAngleDegrees = Mathf.Clamp(value, 0f, 75f);
        }

        public float VerticalWallrunMoveMaxAngleDegrees
        {
            get => verticalWallrunMoveMaxAngleDegrees;
            set => verticalWallrunMoveMaxAngleDegrees = Mathf.Clamp(value, 1f, 89f);
        }

        public float VerticalWallrunMinimumEntrySpeedFraction
        {
            get => verticalWallrunMinimumEntrySpeedFraction;
            set => verticalWallrunMinimumEntrySpeedFraction = Mathf.Clamp01(value);
        }

        public float VerticalWallrunUpSpeed
        {
            get => verticalWallrunUpSpeed;
            set => verticalWallrunUpSpeed = Mathf.Max(0.1f, value);
        }

        public float VerticalWallrunDurationSeconds
        {
            get => verticalWallrunDurationSeconds;
            set => verticalWallrunDurationSeconds = Mathf.Max(0.05f, value);
        }

        public float VerticalWallrunLateDecelStart
        {
            get => verticalWallrunLateDecelStart;
            set => verticalWallrunLateDecelStart = Mathf.Clamp(value, 0.1f, 0.95f);
        }

        public float VerticalWallrunLateDecelExponent
        {
            get => verticalWallrunLateDecelExponent;
            set => verticalWallrunLateDecelExponent = Mathf.Max(1f, value);
        }

        public float VerticalWallrunEndSpeedRetention
        {
            get => verticalWallrunEndSpeedRetention;
            set => verticalWallrunEndSpeedRetention = Mathf.Clamp01(value);
        }

        public float VerticalWallrunWallStickSpeed
        {
            get => verticalWallrunWallStickSpeed;
            set => verticalWallrunWallStickSpeed = Mathf.Max(0f, value);
        }

        public VaultMode VaultMode
        {
            get => vaultMode;
            set => vaultMode = value;
        }

        public bool VaultHoldAssistEnabled
        {
            get => vaultHoldAssistEnabled;
            set => vaultHoldAssistEnabled = value;
        }

        public float VaultFlowMaxHeightMeters
        {
            get => vaultFlowMaxHeightMeters;
            set
            {
                vaultFlowMaxHeightMeters = Mathf.Max(0.25f, value);
                vaultMantleMaxHeightMeters = Mathf.Max(vaultFlowMaxHeightMeters, vaultMantleMaxHeightMeters);
                mantleHeight = Mathf.Max(mantleHeight, vaultMantleMaxHeightMeters);
            }
        }

        public float VaultMantleMaxHeightMeters
        {
            get => vaultMantleMaxHeightMeters;
            set
            {
                vaultMantleMaxHeightMeters = Mathf.Max(VaultFlowMaxHeightMeters, value);
                mantleHeight = Mathf.Max(mantleHeight, vaultMantleMaxHeightMeters);
            }
        }

        public float FlowVaultOverDistanceMeters
        {
            get => flowVaultOverDistanceMeters;
            set => flowVaultOverDistanceMeters = Mathf.Max(0.05f, value);
        }

        public float SafetyMantleOverDistanceMeters
        {
            get => safetyMantleOverDistanceMeters;
            set => safetyMantleOverDistanceMeters = Mathf.Max(0.05f, value);
        }

        public float FlowVaultBlendDurationSeconds
        {
            get => flowVaultBlendDurationSeconds;
            set => flowVaultBlendDurationSeconds = Mathf.Clamp(value, 0.02f, 0.3f);
        }

        public float SafetyMantleBlendDurationSeconds
        {
            get => safetyMantleBlendDurationSeconds;
            set => safetyMantleBlendDurationSeconds = Mathf.Clamp(value, 0.04f, 0.6f);
        }

        public float FlowVaultBaseSpeedMultiplier
        {
            get => flowVaultBaseSpeedMultiplier;
            set => flowVaultBaseSpeedMultiplier = Mathf.Max(0.1f, value);
        }

        public float FlowVaultPerfectSpeedMultiplier
        {
            get => flowVaultPerfectSpeedMultiplier;
            set => flowVaultPerfectSpeedMultiplier = Mathf.Max(FlowVaultBaseSpeedMultiplier, value);
        }

        public float SafetyMantleBaseSpeedMultiplier
        {
            get => safetyMantleBaseSpeedMultiplier;
            set => safetyMantleBaseSpeedMultiplier = Mathf.Max(0.1f, value);
        }

        public float SafetyMantlePerfectSpeedMultiplier
        {
            get => safetyMantlePerfectSpeedMultiplier;
            set => safetyMantlePerfectSpeedMultiplier = Mathf.Max(SafetyMantleBaseSpeedMultiplier, value);
        }

        public float FlowVaultMinimumVerticalBoost
        {
            get => flowVaultMinimumVerticalBoost;
            set => flowVaultMinimumVerticalBoost = Mathf.Clamp01(value);
        }

        public float FlowVaultPerfectVerticalBoost
        {
            get => flowVaultPerfectVerticalBoost;
            set => flowVaultPerfectVerticalBoost = Mathf.Max(FlowVaultMinimumVerticalBoost, Mathf.Clamp01(value));
        }

        public float SafetyMantleMinimumVerticalBoost
        {
            get => safetyMantleMinimumVerticalBoost;
            set => safetyMantleMinimumVerticalBoost = Mathf.Clamp01(value);
        }

        public float SafetyMantlePerfectVerticalBoost
        {
            get => safetyMantlePerfectVerticalBoost;
            set => safetyMantlePerfectVerticalBoost = Mathf.Max(SafetyMantleMinimumVerticalBoost, Mathf.Clamp01(value));
        }

        public float VaultBlendDurationSeconds
        {
            get => FlowVaultBlendDurationSeconds;
            set => FlowVaultBlendDurationSeconds = value;
        }

        public float WallJumpLookWeight
        {
            get => wallJumpLookWeight;
            set => wallJumpLookWeight = Mathf.Clamp(value, 0.2f, 0.9f);
        }

        public float WallJumpAwayWeight
        {
            get => wallJumpAwayWeight;
            set => wallJumpAwayWeight = Mathf.Clamp(value, 0.1f, 0.6f);
        }

        public float WallJumpCarryWeight
        {
            get => wallJumpCarryWeight;
            set => wallJumpCarryWeight = Mathf.Clamp(value, 0f, 0.5f);
        }

        public float WallJumpMinimumAwayDot
        {
            get => wallJumpMinimumAwayDot;
            set => wallJumpMinimumAwayDot = Mathf.Clamp(value, 0f, 0.75f);
        }

        public float WallrunSameWallLockoutSeconds
        {
            get => wallrunSameWallLockoutSeconds;
            set => wallrunSameWallLockoutSeconds = Mathf.Max(0f, value);
        }

        public bool WallrunDetachReattachDelayEnabled
        {
            get => wallrunDetachReattachDelayEnabled;
            set
            {
                wallrunDetachReattachDelayEnabled = value;
                if (!wallrunDetachReattachDelayEnabled)
                {
                    _wallrunDetachReattachTimer = 0f;
                }
            }
        }

        public float WallrunDetachReattachDelaySeconds
        {
            get => wallrunDetachReattachDelaySeconds;
            set => wallrunDetachReattachDelaySeconds = Mathf.Max(0f, value);
        }

        public float WallrunReverseReentryDot
        {
            get => wallrunReverseReentryDot;
            set => wallrunReverseReentryDot = Mathf.Clamp(value, -1f, 0f);
        }

        public float WallrunMaximumSurfaceTurnAngle
        {
            get => wallrunMaximumSurfaceTurnAngle;
            set => wallrunMaximumSurfaceTurnAngle = Mathf.Clamp(value, 1f, 89f);
        }

        public float WallrunUpperProbeHeightFraction
        {
            get => wallrunUpperProbeHeightFraction;
            set => wallrunUpperProbeHeightFraction = Mathf.Clamp(value, 0.25f, 0.5f);
        }

        public float WallrunTopFlattenProbeHeightFraction
        {
            get => wallrunTopFlattenProbeHeightFraction;
            set => wallrunTopFlattenProbeHeightFraction = Mathf.Clamp(value, 0.43f, 0.65f);
        }

        public float WallrunTopUpwardVelocityMultiplier
        {
            get => wallrunTopUpwardVelocityMultiplier;
            set => wallrunTopUpwardVelocityMultiplier = Mathf.Clamp01(value);
        }

        public float WallrunCurvedSurfaceProbeForwardBias
        {
            get => wallrunCurvedSurfaceProbeForwardBias;
            set => wallrunCurvedSurfaceProbeForwardBias = Mathf.Clamp01(value);
        }

        public float WallrunHorizontalWallStickSpeed
        {
            get => wallrunHorizontalWallStickSpeed;
            set => wallrunHorizontalWallStickSpeed = Mathf.Clamp(value, 0f, 4f);
        }

        private readonly struct GroundProbeResult
        {
            public GroundProbeResult(Vector3 point, Vector3 normal, float snapOffset)
            {
                Point = point;
                Normal = normal;
                SnapOffset = snapOffset;
            }

            public Vector3 Point { get; }

            public Vector3 Normal { get; }

            public float SnapOffset { get; }
        }

        private readonly struct WallProbeResult
        {
            public WallProbeResult(Vector3 normal, bool nearTop, Transform surfaceTransform, float distance, Vector3 castDirection)
            {
                Normal = normal;
                NearTop = nearTop;
                SurfaceTransform = surfaceTransform;
                Distance = distance;
                CastDirection = castDirection;
            }

            public Vector3 Normal { get; }

            public bool NearTop { get; }

            public Transform SurfaceTransform { get; }

            public float Distance { get; }

            public Vector3 CastDirection { get; }
        }

        private void Awake()
        {
            _capsule = GetComponent<CapsuleCollider>();
            ResetAirJumps();
        }

        public void SetTuning(float moveSpeed, float sprint, float slide, float jump, float airControl, float wallGravity)
        {
            walkSpeed = Mathf.Max(0.1f, moveSpeed);
            sprintSpeed = Mathf.Max(walkSpeed, sprint);
            slideSpeed = Mathf.Max(sprintSpeed, slide);
            jumpVelocity = Mathf.Max(0.1f, jump);
            airAcceleration = Mathf.Max(1f, airControl);
            wallrunGravity = Mathf.Max(0f, wallGravity);
        }

        public void ApplyMovementTuning(MovementTuningDefinition tuning)
        {
            if (tuning == null)
            {
                return;
            }

            groundContactProbeDistance = tuning.GroundContactProbeDistance;
            groundContactRadiusMultiplier = tuning.GroundContactRadiusMultiplier;
            standableGroundMaxAngleDegrees = tuning.StandableGroundMaxAngleDegrees;
            groundSnapDistance = tuning.RunGroundSnapDistance;
            slideGroundSnapDistance = tuning.SlideGroundSnapDistance;
            groundSnapBreakAngleDegrees = tuning.RunGroundSnapBreakAngleDegrees;
            slideGroundSnapBreakAngleDegrees = tuning.SlideGroundSnapBreakAngleDegrees;
            groundSnapReleaseDropAngleDegrees = tuning.RunEdgeReleaseAngleDegrees;
            slideGroundSnapReleaseDropAngleDegrees = tuning.SlideEdgeReleaseAngleDegrees;
            groundSnapReleaseMinimumDrop = tuning.GroundSnapReleaseMinimumDrop;
            downhillGroundStickExtraProbeDistance = tuning.DownhillGroundStickExtraProbeDistance;
            downhillGroundStickExtraLookAhead = tuning.DownhillGroundStickExtraLookAhead;
            downhillGroundStickMinimumAngleDegrees = tuning.DownhillGroundStickMinimumAngleDegrees;
            downhillGroundStickFadeStartAngleDegrees = tuning.DownhillGroundStickFadeStartAngleDegrees;
            downhillGroundStickFadeEndAngleDegrees = tuning.DownhillGroundStickFadeEndAngleDegrees;
            jumpGroundLockoutSeconds = tuning.JumpGroundLockoutSeconds;
            maxVelocityEnabled = tuning.MaxVelocityEnabled;
            maxVelocityMetersPerSecond = tuning.MaxVelocityMetersPerSecond;
            maxVelocityHardCap = tuning.MaxVelocityHardCap;
            maxVelocitySoftPullPerSecond = tuning.MaxVelocitySoftPullPerSecond;
            bunnyHopEnabled = tuning.BunnyHopEnabled;
            bunnyHopLandingWindowSeconds = tuning.BunnyHopLandingWindowSeconds;
            bunnyHopSteerStrength = tuning.BunnyHopSteerStrength;
            bunnyHopSpeedMultiplier = tuning.BunnyHopSpeedMultiplier;
            airborneSlideBufferSeconds = tuning.AirborneSlideBufferSeconds;
            slideMinimumEntrySpeedFraction = tuning.SlideMinimumEntrySpeedFraction;
            slideDragDelayFraction = tuning.SlideDragDelayFraction;
            slideEntrySpeedDelayExponent = tuning.SlideEntrySpeedDelayExponent;
            slideDragRampExponent = tuning.SlideDragRampExponent;
            slideEntrySpeedDecayResistanceExponent = tuning.SlideEntrySpeedDecayResistanceExponent;
            slideJumpMinimumSpeedMultiplier = tuning.SlideJumpMinimumSpeedMultiplier;
            slideJumpPeakSpeedMultiplier = Mathf.Max(slideJumpMinimumSpeedMultiplier, tuning.SlideJumpPeakSpeedMultiplier);
            slideJumpSpeedCapMultiplier = tuning.SlideJumpSpeedCapMultiplier;
            wallJumpMinimumSpeedFraction = tuning.WallJumpMinimumSpeedFraction;
            wallJumpApexTimingWindow = tuning.WallJumpApexTimingWindow;
            wallJumpPeakTimingMultiplier = tuning.WallJumpPeakTimingMultiplier;
            wallJumpLookWeight = tuning.WallJumpLookWeight;
            wallJumpAwayWeight = tuning.WallJumpAwayWeight;
            wallJumpCarryWeight = tuning.WallJumpCarryWeight;
            wallJumpMinimumAwayDot = tuning.WallJumpMinimumAwayDot;
            wallrunSameWallLockoutSeconds = tuning.WallrunSameWallLockoutSeconds;
            WallrunDetachReattachDelayEnabled = tuning.WallrunDetachReattachDelayEnabled;
            wallrunDetachReattachDelaySeconds = tuning.WallrunDetachReattachDelaySeconds;
            wallrunReverseReentryDot = tuning.WallrunReverseReentryDot;
            wallrunMaximumSurfaceTurnAngle = tuning.WallrunMaximumSurfaceTurnAngle;
            wallrunUpperProbeHeightFraction = tuning.WallrunUpperProbeHeightFraction;
            wallrunTopFlattenProbeHeightFraction = tuning.WallrunTopFlattenProbeHeightFraction;
            wallrunTopUpwardVelocityMultiplier = tuning.WallrunTopUpwardVelocityMultiplier;
            wallrunCurvedSurfaceProbeForwardBias = tuning.WallrunCurvedSurfaceProbeForwardBias;
            wallrunHorizontalWallStickSpeed = tuning.WallrunHorizontalWallStickSpeed;
            VerticalWallrunEnabled = tuning.VerticalWallrunEnabled;
            VerticalWallrunLookMaxAngleDegrees = tuning.VerticalWallrunLookMaxAngleDegrees;
            VerticalWallrunMinimumLookUpAngleDegrees = tuning.VerticalWallrunMinimumLookUpAngleDegrees;
            VerticalWallrunMoveMaxAngleDegrees = tuning.VerticalWallrunMoveMaxAngleDegrees;
            VerticalWallrunMinimumEntrySpeedFraction = tuning.VerticalWallrunMinimumEntrySpeedFraction;
            VerticalWallrunUpSpeed = tuning.VerticalWallrunUpSpeed;
            VerticalWallrunDurationSeconds = tuning.VerticalWallrunDurationSeconds;
            VerticalWallrunLateDecelStart = tuning.VerticalWallrunLateDecelStart;
            VerticalWallrunLateDecelExponent = tuning.VerticalWallrunLateDecelExponent;
            VerticalWallrunEndSpeedRetention = tuning.VerticalWallrunEndSpeedRetention;
            VerticalWallrunWallStickSpeed = tuning.VerticalWallrunWallStickSpeed;
            vaultMode = tuning.VaultMode;
            vaultHoldAssistEnabled = tuning.VaultHoldAssistEnabled;
            VaultFlowMaxHeightMeters = tuning.VaultFlowMaxHeightMeters;
            VaultMantleMaxHeightMeters = tuning.VaultMantleMaxHeightMeters;
            FlowVaultOverDistanceMeters = tuning.FlowVaultOverDistanceMeters;
            SafetyMantleOverDistanceMeters = tuning.SafetyMantleOverDistanceMeters;
            FlowVaultBlendDurationSeconds = tuning.FlowVaultBlendDurationSeconds;
            SafetyMantleBlendDurationSeconds = tuning.SafetyMantleBlendDurationSeconds;
            FlowVaultBaseSpeedMultiplier = tuning.FlowVaultBaseSpeedMultiplier;
            FlowVaultPerfectSpeedMultiplier = tuning.FlowVaultPerfectSpeedMultiplier;
            SafetyMantleBaseSpeedMultiplier = tuning.SafetyMantleBaseSpeedMultiplier;
            SafetyMantlePerfectSpeedMultiplier = tuning.SafetyMantlePerfectSpeedMultiplier;
            FlowVaultMinimumVerticalBoost = tuning.FlowVaultMinimumVerticalBoost;
            FlowVaultPerfectVerticalBoost = tuning.FlowVaultPerfectVerticalBoost;
            SafetyMantleMinimumVerticalBoost = tuning.SafetyMantleMinimumVerticalBoost;
            SafetyMantlePerfectVerticalBoost = tuning.SafetyMantlePerfectVerticalBoost;
            vaultMinimumSpeedFraction = tuning.VaultMinimumSpeedFraction;
            vaultBaseSpeedMultiplier = tuning.VaultBaseSpeedMultiplier;
            vaultPerfectSpeedMultiplier = Mathf.Max(vaultBaseSpeedMultiplier, tuning.VaultPerfectSpeedMultiplier);
            vaultMinimumVerticalBoost = tuning.VaultMinimumVerticalBoost;
            vaultPerfectVerticalBoost = Mathf.Max(vaultMinimumVerticalBoost, tuning.VaultPerfectVerticalBoost);
            vaultBlendDurationSeconds = tuning.VaultBlendDurationSeconds;
        }

        public void ResetMotor(Vector3 position)
        {
            transform.position = position;
            _velocity = Vector3.zero;
            ResetAirJumps();
            _slideTimer = 0f;
            _slideEntrySpeed = 0f;
            _wallrunTimer = 0f;
            _wallrunStartSpeed = 0f;
            _wallrunPeakSpeed = 0f;
            _wallrunSameWallLockoutTimer = 0f;
            _wallrunDetachReattachTimer = 0f;
            _wallrunLockedNormal = Vector3.zero;
            _wallrunLockedDirection = Vector3.zero;
            _wallrunActiveDirection = Vector3.zero;
            _wallrunGuidanceDirection = Vector3.zero;
            _wallrunBlockedNormal = Vector3.zero;
            _wallrunChainLockedNormal = Vector3.zero;
            _wallrunChainLockedDirection = Vector3.zero;
            _wallrunSurfaceTransform = null;
            _wallrunChainSameDirectionLocked = false;
            _wallrunNearTop = false;
            _wallrunStyle = WallrunTraversalStyle.Horizontal;
            _wallrunFatigued = false;
            _groundSnapLockoutTimer = 0f;
            _groundedJumpGraceTimer = groundedJumpGraceSeconds;
            _slideJumpGraceTimer = 0f;
            _airborneSlideBufferTimer = 0f;
            _bunnyHopWindowTimer = 0f;
            _bunnyHopCarryVelocity = Vector3.zero;
            _vaultBlendTimer = 0f;
            _vaultBlendDuration = 0f;
            _vaultStartPosition = position;
            _vaultTargetPosition = position;
            _vaultExitVelocity = Vector3.zero;
            _activeVaultStyle = VaultTraversalStyle.None;
            _lastVaultStyle = VaultTraversalStyle.None;
            State = WallrunnerMovementState.Grounded;
        }

        public RuntimeSnapshot CaptureRuntimeSnapshot()
        {
            return new RuntimeSnapshot(
                transform.position,
                transform.rotation,
                _velocity,
                _wallNormal,
                _groundNormal,
                _wallrunLockedNormal,
                _wallrunLockedDirection,
                _wallrunActiveDirection,
                _wallrunGuidanceDirection,
                _wallrunBlockedNormal,
                _wallrunChainLockedNormal,
                _wallrunChainLockedDirection,
                _wallrunSurfaceTransform,
                _bunnyHopCarryVelocity,
                _vaultStartPosition,
                _vaultTargetPosition,
                _vaultExitVelocity,
                _lookForward,
                _slideTimer,
                _wallrunTimer,
                _wallrunStartSpeed,
                _wallrunPeakSpeed,
                _wallrunSameWallLockoutTimer,
                _wallrunDetachReattachTimer,
                _slideEntrySpeed,
                _groundSnapLockoutTimer,
                _groundedJumpGraceTimer,
                _slideJumpGraceTimer,
                _airborneSlideBufferTimer,
                _bunnyHopWindowTimer,
                _vaultBlendTimer,
                _vaultBlendDuration,
                _airJumpsRemaining,
                _grounded,
                _wallrunFatigued,
                _wallrunChainSameDirectionLocked,
                _wallrunNearTop,
                _wallrunStyle,
                _activeVaultStyle,
                _lastVaultStyle,
                State);
        }

        public void RestoreRuntimeSnapshot(RuntimeSnapshot snapshot)
        {
            transform.SetPositionAndRotation(snapshot.Position, snapshot.Rotation);
            _velocity = snapshot.Velocity;
            _wallNormal = snapshot.WallNormal;
            _groundNormal = snapshot.GroundNormal.sqrMagnitude <= 0.0001f ? Vector3.up : snapshot.GroundNormal;
            _wallrunLockedNormal = snapshot.WallrunLockedNormal;
            _wallrunLockedDirection = snapshot.WallrunLockedDirection;
            _wallrunActiveDirection = snapshot.WallrunActiveDirection;
            _wallrunGuidanceDirection = snapshot.WallrunGuidanceDirection;
            _wallrunBlockedNormal = snapshot.WallrunBlockedNormal;
            _wallrunChainLockedNormal = snapshot.WallrunChainLockedNormal;
            _wallrunChainLockedDirection = snapshot.WallrunChainLockedDirection;
            _wallrunSurfaceTransform = snapshot.WallrunSurfaceTransform;
            _bunnyHopCarryVelocity = snapshot.BunnyHopCarryVelocity;
            _vaultStartPosition = snapshot.VaultStartPosition;
            _vaultTargetPosition = snapshot.VaultTargetPosition;
            _vaultExitVelocity = snapshot.VaultExitVelocity;
            _lookForward = snapshot.LookForward;
            _slideTimer = snapshot.SlideTimer;
            _wallrunTimer = snapshot.WallrunTimer;
            _wallrunStartSpeed = snapshot.WallrunStartSpeed;
            _wallrunPeakSpeed = snapshot.WallrunPeakSpeed;
            _wallrunSameWallLockoutTimer = snapshot.WallrunSameWallLockoutTimer;
            _wallrunDetachReattachTimer = snapshot.WallrunDetachReattachTimer;
            _slideEntrySpeed = snapshot.SlideEntrySpeed;
            _groundSnapLockoutTimer = snapshot.GroundSnapLockoutTimer;
            _groundedJumpGraceTimer = snapshot.GroundedJumpGraceTimer;
            _slideJumpGraceTimer = snapshot.SlideJumpGraceTimer;
            _airborneSlideBufferTimer = snapshot.AirborneSlideBufferTimer;
            _bunnyHopWindowTimer = snapshot.BunnyHopWindowTimer;
            _vaultBlendTimer = snapshot.VaultBlendTimer;
            _vaultBlendDuration = snapshot.VaultBlendDuration;
            _airJumpsRemaining = snapshot.AirJumpsRemaining;
            _grounded = snapshot.Grounded;
            _wallrunFatigued = snapshot.WallrunFatigued;
            _wallrunChainSameDirectionLocked = snapshot.WallrunChainSameDirectionLocked;
            _wallrunNearTop = snapshot.WallrunNearTop;
            _wallrunStyle = snapshot.WallrunStyle;
            _activeVaultStyle = snapshot.ActiveVaultStyle;
            _lastVaultStyle = snapshot.LastVaultStyle;
            State = snapshot.State;
            Physics.SyncTransforms();
        }

        public void SetVelocity(Vector3 velocity)
        {
            _velocity = velocity;
        }

        public void SetLookDirection(Vector3 lookDirection)
        {
            if (lookDirection.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            _lookForward = lookDirection.normalized;
        }

        public void SetWallrunGuidanceDirection(Vector3 direction)
        {
            Vector3 lateralDirection = Vector3.ProjectOnPlane(direction, Vector3.up);
            _wallrunGuidanceDirection = lateralDirection.sqrMagnitude <= 0.0001f ? Vector3.zero : lateralDirection.normalized;
        }

        public void Tick(Vector2 moveInput, bool sprintHeld, bool slidePressed, bool jumpPressed, float deltaTime)
        {
            Tick(moveInput, sprintHeld, slidePressed, slidePressed, jumpPressed, deltaTime);
        }

        public void Tick(Vector2 moveInput, bool sprintHeld, bool slidePressed, bool slideHeld, bool jumpPressed, float deltaTime)
        {
            Tick(moveInput, sprintHeld, slidePressed, slideHeld, jumpPressed, jumpPressed, deltaTime);
        }

        public void Tick(Vector2 moveInput, bool sprintHeld, bool slidePressed, bool slideHeld, bool jumpPressed, bool jumpHeld, float deltaTime)
        {
            if (_capsule == null)
            {
                _capsule = GetComponent<CapsuleCollider>();
            }

            if (_vaultBlendTimer > 0f)
            {
                TickVaultBlend(deltaTime);
                return;
            }

            if (_groundSnapLockoutTimer > 0f)
            {
                _groundSnapLockoutTimer = Mathf.Max(0f, _groundSnapLockoutTimer - deltaTime);
            }

            if (_airborneSlideBufferTimer > 0f)
            {
                _airborneSlideBufferTimer = Mathf.Max(0f, _airborneSlideBufferTimer - deltaTime);
            }

            if (_wallrunSameWallLockoutTimer > 0f)
            {
                _wallrunSameWallLockoutTimer = Mathf.Max(0f, _wallrunSameWallLockoutTimer - deltaTime);
            }

            if (!wallrunDetachReattachDelayEnabled)
            {
                _wallrunDetachReattachTimer = 0f;
            }
            else if (_wallrunDetachReattachTimer > 0f)
            {
                _wallrunDetachReattachTimer = Mathf.Max(0f, _wallrunDetachReattachTimer - deltaTime);
            }

            bool canSnapToGround = _groundSnapLockoutTimer <= 0f;
            bool wasGrounded = _grounded;
            bool wasSliding = State == WallrunnerMovementState.Sliding;
            Vector3 preProbeGroundNormal = _groundNormal;
            Vector3 preGroundLateralVelocity = Vector3.ProjectOnPlane(_velocity, Vector3.up);
            _grounded = canSnapToGround && CheckGrounded();
            if (canSnapToGround && !jumpPressed && !_grounded)
            {
                _grounded = wasSliding
                    ? TrySnapToSlideGround(preProbeGroundNormal, deltaTime)
                    : wasGrounded && TrySnapToGround(preProbeGroundNormal, deltaTime);
            }

            if (_grounded)
            {
                bool landedThisFrame = !wasGrounded;
                if (landedThisFrame)
                {
                    _bunnyHopWindowTimer = bunnyHopLandingWindowSeconds;
                    _bunnyHopCarryVelocity = preGroundLateralVelocity;
                }
                else
                {
                    _bunnyHopWindowTimer = Mathf.Max(0f, _bunnyHopWindowTimer - deltaTime);
                    if (_bunnyHopWindowTimer <= 0f)
                    {
                        _bunnyHopCarryVelocity = Vector3.zero;
                    }
                }

                _groundedJumpGraceTimer = groundedJumpGraceSeconds;
                if (State == WallrunnerMovementState.Sliding)
                {
                    _slideJumpGraceTimer = groundedJumpGraceSeconds;
                }

                if (_velocity.y <= 0f)
                {
                    if (State == WallrunnerMovementState.Sliding)
                    {
                        _velocity = BuildGroundedVelocity(Vector3.ProjectOnPlane(_velocity, Vector3.up), 0f);
                    }
                    else
                    {
                        _velocity.y = 0f;
                    }
                }

                ResetAirJumps();
                _wallrunTimer = 0f;
                _wallrunStartSpeed = 0f;
                _wallrunPeakSpeed = 0f;
                _wallrunSameWallLockoutTimer = 0f;
                _wallrunDetachReattachTimer = 0f;
                _wallrunLockedNormal = Vector3.zero;
                _wallrunLockedDirection = Vector3.zero;
                _wallrunActiveDirection = Vector3.zero;
                _wallrunGuidanceDirection = Vector3.zero;
                _wallrunBlockedNormal = Vector3.zero;
                _wallrunSurfaceTransform = null;
                ClearWallrunChainLock();
                _wallrunNearTop = false;
                _wallrunStyle = WallrunTraversalStyle.Horizontal;
                _wallrunFatigued = false;
            }
            else
            {
                _groundedJumpGraceTimer = Mathf.Max(0f, _groundedJumpGraceTimer - deltaTime);
                if (_groundedJumpGraceTimer <= 0f)
                {
                    _bunnyHopWindowTimer = 0f;
                    _bunnyHopCarryVelocity = Vector3.zero;
                }

                _slideJumpGraceTimer = Mathf.Max(0f, _slideJumpGraceTimer - deltaTime);
            }

            Vector3 wishDirection = BuildWishDirection(moveInput);
            if (!_grounded && (slidePressed || slideHeld))
            {
                _airborneSlideBufferTimer = airborneSlideBufferSeconds;
            }

            TryStartSlide(slidePressed || _airborneSlideBufferTimer > 0f, wishDirection);
            bool startedGrounded = _grounded;

            if (jumpPressed && State == WallrunnerMovementState.Wallrunning)
            {
                TryJump(wishDirection);
            }
            else
            {
                bool vaultRequested = vaultHoldAssistEnabled ? jumpHeld : jumpPressed;
                if (vaultRequested && TryVault(wishDirection))
                {
                    return;
                }

                if (jumpPressed)
                {
                    TryJump(wishDirection);
                }
            }

            bool wallrunning = !_grounded && TryResolveWallrun(wishDirection, jumpPressed, deltaTime);
            if (!wallrunning && !_grounded)
            {
                _velocity.y -= gravity * deltaTime;
            }

            float targetSpeed = ResolveTargetSpeed(sprintHeld, wallrunning);
            float acceleration = _grounded ? groundAcceleration : airAcceleration;
            Vector3 lateralVelocity = Vector3.ProjectOnPlane(_velocity, Vector3.up);
            float verticalVelocity = _velocity.y;
            Vector3 desiredLateral = wishDirection * targetSpeed;
            if (wallrunning)
            {
                desiredLateral = lateralVelocity;
                acceleration = 0f;
            }
            else if (State == WallrunnerMovementState.Sliding)
            {
                _slideTimer -= deltaTime;
                Vector3 slideDirection = lateralVelocity.sqrMagnitude > 0.1f ? lateralVelocity.normalized : wishDirection.normalized;
                if (!_grounded)
                {
                    desiredLateral = lateralVelocity;
                }
                else
                {
                    float slideSpeedAfterDrag = ApplySlideDrag(lateralVelocity.magnitude, deltaTime);
                    slideSpeedAfterDrag = ResolveSlopeBoostedSlideSpeed(slideDirection, slideSpeedAfterDrag, deltaTime);
                    desiredLateral = slideDirection.sqrMagnitude > 0.1f ? slideDirection.normalized * slideSpeedAfterDrag : Vector3.zero;
                    lateralVelocity = desiredLateral;
                    if (!HasSlideSupportAhead(lateralVelocity, deltaTime))
                    {
                        verticalVelocity = ReleaseGroundAtEdge(verticalVelocity, deltaTime);
                    }
                    else
                    {
                        verticalVelocity = ResolveGroundFollowerVerticalVelocity(lateralVelocity, verticalVelocity);
                    }
                }

                acceleration = 0f;
                if (_slideTimer <= 0f || !_grounded)
                {
                    State = _grounded ? WallrunnerMovementState.Grounded : WallrunnerMovementState.Airborne;
                }
            }
            else if (!_grounded)
            {
                if (!WallrunDetachReattachDelayActive)
                {
                    lateralVelocity = ApplyMomentumPreservingAirControl(lateralVelocity, wishDirection, targetSpeed, deltaTime);
                }

                desiredLateral = lateralVelocity;
                acceleration = 0f;
            }

            lateralVelocity = Vector3.MoveTowards(lateralVelocity, desiredLateral, acceleration * deltaTime);
            if (_grounded && !wallrunning && State != WallrunnerMovementState.Sliding)
            {
                if (!HasGroundSupportAhead(lateralVelocity, deltaTime, groundSnapReleaseDropAngleDegrees, groundSnapBreakAngleDegrees))
                {
                    verticalVelocity = ReleaseGroundAtEdge(verticalVelocity, deltaTime);
                }
                else
                {
                    verticalVelocity = ResolveGroundFollowerVerticalVelocity(lateralVelocity, verticalVelocity);
                }
            }

            _velocity = lateralVelocity + (Vector3.up * verticalVelocity);
            ApplyMaxVelocity(deltaTime);
            MoveWithCollision(_velocity * deltaTime);
            ResolvePostMoveGrounding(jumpPressed, canSnapToGround && startedGrounded && !wallrunning, deltaTime);

            if (!wallrunning && State != WallrunnerMovementState.Sliding)
            {
                State = _grounded ? sprintHeld && moveInput.y > 0.1f ? WallrunnerMovementState.Sprinting : WallrunnerMovementState.Grounded : WallrunnerMovementState.Airborne;
            }
        }

        private Vector3 BuildWishDirection(Vector2 moveInput)
        {
            Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
            Vector3 right = Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized;
            Vector3 wish = (forward * moveInput.y) + (right * moveInput.x);
            return wish.sqrMagnitude > 1f ? wish.normalized : wish;
        }

        private bool TryStartSlide(bool slidePressed, Vector3 wishDirection)
        {
            if (!slidePressed || !_grounded)
            {
                return false;
            }

            State = WallrunnerMovementState.Sliding;
            _airborneSlideBufferTimer = 0f;
            _slideTimer = slideDurationSeconds;
            Vector3 direction = wishDirection.sqrMagnitude > 0.1f ? wishDirection.normalized : transform.forward;
            Vector3 lateralVelocity = Vector3.ProjectOnPlane(_velocity, Vector3.up);
            float currentSpeed = lateralVelocity.magnitude;
            float baseEntrySpeed = Mathf.Max(currentSpeed, sprintSpeed * slideMinimumEntrySpeedFraction);
            float slideEntrySpeed = baseEntrySpeed * slideEnterSpeedMultiplier;
            slideEntrySpeed = ResolveSlopeAdjustedSlideEntrySpeed(direction, slideEntrySpeed);
            _slideEntrySpeed = slideEntrySpeed;
            _velocity = Vector3.Project(_velocity, Vector3.up) + (direction * slideEntrySpeed);
            return true;
        }

        private bool TryJump(Vector3 wishDirection)
        {
            bool slidingJump = State == WallrunnerMovementState.Sliding || _slideJumpGraceTimer > 0f;
            bool canGroundJump = _grounded || _groundedJumpGraceTimer > 0f;
            if (canGroundJump || slidingJump)
            {
                if (slidingJump)
                {
                    float currentSlideSpeed = Vector3.ProjectOnPlane(_velocity, Vector3.up).magnitude;
                    Vector3 boostDirection = wishDirection.sqrMagnitude > 0.1f ? wishDirection : Vector3.ProjectOnPlane(_velocity, Vector3.up);
                    if (boostDirection.sqrMagnitude <= 0.1f)
                    {
                        boostDirection = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
                    }

                    float timingScore = ResolveSlideJumpTimingScore();
                    float speedMultiplier = Mathf.Lerp(slideJumpMinimumSpeedMultiplier, slideJumpPeakSpeedMultiplier, timingScore);
                    float baseSpeed = Mathf.Max(currentSlideSpeed, sprintSpeed * slideMinimumEntrySpeedFraction);
                    ApplyVelocityScaledLateralBoost(boostDirection, speedMultiplier, Mathf.Max(slideSpeed, baseSpeed) * slideJumpSpeedCapMultiplier, baseSpeed);
                    _slideTimer = 0f;
                    _slideJumpGraceTimer = 0f;
                }

                bool bunnyHopJump = bunnyHopEnabled && !slidingJump && (_grounded || _groundedJumpGraceTimer > 0f) && _bunnyHopWindowTimer > 0f;
                if (bunnyHopJump)
                {
                    ApplyBunnyHopLaunch(wishDirection);
                }

                _velocity.y = jumpVelocity;
                _bunnyHopWindowTimer = 0f;
                _bunnyHopCarryVelocity = Vector3.zero;
                ForceAirborneAfterJump();
                return true;
            }

            if (State == WallrunnerMovementState.Wallrunning)
            {
                Vector3 lateralVelocity = Vector3.ProjectOnPlane(_velocity, Vector3.up);
                LockWallrunReentry();
                LockSameWallSameDirectionUntilAnotherWall();
                ApplyWallJumpLaunch(lateralVelocity, true);
                ResetAirJumps();
                ForceAirborneAfterJump();
                return true;
            }

            if (_airJumpsRemaining > 0)
            {
                _airJumpsRemaining--;
                _velocity.y = jumpVelocity * 0.94f;
                if (wishDirection.sqrMagnitude > 0.1f)
                {
                    Vector3 lateral = Vector3.ProjectOnPlane(_velocity, Vector3.up);
                    if (lateral.sqrMagnitude > 0.01f)
                    {
                        Vector3 redirected = (lateral.normalized + wishDirection.normalized * 0.45f).normalized * lateral.magnitude;
                        _velocity = redirected + (Vector3.up * _velocity.y);
                    }
                }

                ForceAirborneAfterJump();
                return true;
            }

            return false;
        }

        private void ForceAirborneAfterJump()
        {
            if (State == WallrunnerMovementState.Wallrunning)
            {
                _wallrunActiveDirection = Vector3.zero;
                _wallrunSurfaceTransform = null;
                _wallrunStyle = WallrunTraversalStyle.Horizontal;
            }

            _grounded = false;
            _groundedJumpGraceTimer = 0f;
            _bunnyHopWindowTimer = 0f;
            _bunnyHopCarryVelocity = Vector3.zero;
            _groundSnapLockoutTimer = Mathf.Max(_groundSnapLockoutTimer, jumpGroundLockoutSeconds);
            State = WallrunnerMovementState.Airborne;
        }

        private bool TryResolveWallrun(Vector3 wishDirection, bool jumpPressed, float deltaTime)
        {
            if (jumpPressed)
            {
                return false;
            }

            if (State != WallrunnerMovementState.Wallrunning && WallrunDetachReattachDelayActive)
            {
                return false;
            }

            if (wishDirection.sqrMagnitude < 0.1f || _velocity.y < -15f)
            {
                return false;
            }

            if (!TryFindWallrunCandidate(wishDirection, out Vector3 normal, out Transform wallSurface, out WallrunTraversalStyle wallrunStyle, out Vector3 wallrunDirection))
            {
                if (State == WallrunnerMovementState.Wallrunning)
                {
                    ExitWallrunFromSurfaceLoss();
                }

                return false;
            }

            if (ShouldResetActiveWallrunForCandidate(wallrunStyle, normal, wallSurface))
            {
                ExitWallrunFromSurfaceLoss(normal);
                return false;
            }

            if (IsSameWallReentryBlocked(normal, wallrunDirection))
            {
                return false;
            }

            bool startsNewWallrun = State != WallrunnerMovementState.Wallrunning || _wallrunTimer <= 0f;
            bool clearsSameDirectionChainLock = startsNewWallrun && ShouldClearWallrunChainLock(normal, wallrunDirection);
            if (!startsNewWallrun && wallrunStyle == WallrunTraversalStyle.Horizontal)
            {
                wallrunDirection = ResolveContinuingHorizontalWallrunDirection(normal, wallrunDirection);
            }

            _wallNormal = normal;
            Vector3 currentLateralVelocity = Vector3.ProjectOnPlane(_velocity, Vector3.up);
            float currentSpeed = currentLateralVelocity.magnitude;
            if (startsNewWallrun)
            {
                if (clearsSameDirectionChainLock)
                {
                    ClearWallrunChainLock();
                }

                _wallrunTimer = 0f;
                _wallrunStartSpeed = currentSpeed;
                _wallrunPeakSpeed = Mathf.Max(currentSpeed, sprintSpeed) * wallrunApexSpeedMultiplier;
                ResetAirJumps();
                _wallrunFatigued = false;
                _wallrunSameWallLockoutTimer = 0f;
                _wallrunDetachReattachTimer = 0f;
                _wallrunLockedNormal = Vector3.zero;
                _wallrunLockedDirection = Vector3.zero;
                _wallrunActiveDirection = wallrunStyle == WallrunTraversalStyle.Horizontal ? wallrunDirection : Vector3.zero;
                _wallrunBlockedNormal = Vector3.zero;
                _wallrunSurfaceTransform = wallSurface;
            }

            _wallrunStyle = wallrunStyle;
            if (wallrunStyle == WallrunTraversalStyle.Horizontal)
            {
                _wallrunActiveDirection = wallrunDirection;
            }

            _wallrunTimer += deltaTime;
            float activeWallrunDuration = wallrunStyle == WallrunTraversalStyle.Vertical ? verticalWallrunDurationSeconds : wallrunDurationSeconds;
            if (_wallrunTimer > activeWallrunDuration)
            {
                FatigueOutOfWallrun();
                return false;
            }

            if (wallrunStyle == WallrunTraversalStyle.Horizontal && currentSpeed <= walkSpeed * 0.45f)
            {
                FatigueOutOfWallrun();
                return false;
            }

            float fatigue = Mathf.Clamp01(_wallrunTimer / Mathf.Max(0.01f, activeWallrunDuration));
            _velocity = wallrunStyle == WallrunTraversalStyle.Vertical
                ? BuildVerticalWallrunVelocity(normal, currentLateralVelocity, fatigue)
                : BuildHorizontalWallrunVelocity(wallrunDirection, fatigue);
            State = WallrunnerMovementState.Wallrunning;
            return !jumpPressed;
        }

        private Vector3 BuildHorizontalWallrunVelocity(Vector3 alongWall, float fatigue)
        {
            float lateDecelStart = Mathf.Max(wallrunApexTime + 0.05f, wallrunLateDecelStart);
            float profileSpeed = ResolveWallrunProfileSpeed(fatigue, lateDecelStart);
            float lateFatigue = Mathf.InverseLerp(lateDecelStart, 1f, fatigue);
            float endDrop = Mathf.Pow(lateFatigue, wallrunLateDecelExponent);
            float verticalVelocity = ResolveWallrunVerticalVelocity(fatigue, endDrop);
            if (_wallrunNearTop)
            {
                verticalVelocity = 0f;
            }

            Vector3 wallStick = Vector3.ProjectOnPlane(-_wallNormal, Vector3.up);
            wallStick = wallStick.sqrMagnitude <= 0.0001f ? Vector3.zero : wallStick.normalized * wallrunHorizontalWallStickSpeed;
            return alongWall.normalized * profileSpeed + wallStick + Vector3.up * verticalVelocity;
        }

        private Vector3 BuildVerticalWallrunVelocity(Vector3 normal, Vector3 currentLateralVelocity, float fatigue)
        {
            float lateT = Mathf.Pow(
                Mathf.Clamp01(Mathf.InverseLerp(verticalWallrunLateDecelStart, 1f, fatigue)),
                verticalWallrunLateDecelExponent);
            float upwardSpeed = Mathf.Lerp(verticalWallrunUpSpeed, verticalWallrunUpSpeed * verticalWallrunEndSpeedRetention, lateT);
            if (_wallrunNearTop)
            {
                upwardSpeed *= wallrunTopUpwardVelocityMultiplier;
            }

            Vector3 horizontalCarry = Vector3.ProjectOnPlane(currentLateralVelocity, normal);
            horizontalCarry = Vector3.ProjectOnPlane(horizontalCarry, Vector3.up);
            horizontalCarry = Vector3.ClampMagnitude(horizontalCarry, sprintSpeed * 0.35f);
            Vector3 wallStick = Vector3.ProjectOnPlane(-normal, Vector3.up).normalized * verticalWallrunWallStickSpeed;
            if (wallStick.sqrMagnitude <= 0.0001f)
            {
                wallStick = Vector3.zero;
            }

            return horizontalCarry + wallStick + Vector3.up * upwardSpeed;
        }

        private Vector3 ApplyMomentumPreservingAirControl(Vector3 lateralVelocity, Vector3 wishDirection, float targetSpeed, float deltaTime)
        {
            if (wishDirection.sqrMagnitude <= 0.1f)
            {
                return lateralVelocity;
            }

            Vector3 wish = wishDirection.normalized;
            float currentSpeed = lateralVelocity.magnitude;
            if (currentSpeed <= 0.01f)
            {
                return wish * Mathf.Min(targetSpeed, airAcceleration * deltaTime);
            }

            float speed = currentSpeed < targetSpeed ? Mathf.Min(targetSpeed, currentSpeed + airAcceleration * deltaTime) : currentSpeed;
            float maxRadians = airAcceleration * deltaTime / Mathf.Max(currentSpeed, 0.1f);
            Vector3 steeredDirection = Vector3.RotateTowards(lateralVelocity.normalized, wish, maxRadians, 0f);
            return steeredDirection * speed;
        }

        private float ResolveSlopeBoostedSlideSpeed(Vector3 slideDirection, float currentSpeed, float deltaTime)
        {
            if (!_grounded || currentSpeed <= 0.01f)
            {
                return currentSpeed;
            }

            if (!TryGetSlideSlope(out Vector3 downhill, out float slopeStrength) || slideDirection.sqrMagnitude <= 0.01f)
            {
                return currentSpeed;
            }

            float downhillAlignment = Vector3.Dot(slideDirection.normalized, downhill);
            if (downhillAlignment > 0f && slideDownhillSpeedGainPerSecond > 0f)
            {
                float boost = currentSpeed * slideDownhillSpeedGainPerSecond * slopeStrength * downhillAlignment * deltaTime;
                float speedCap = Mathf.Max(currentSpeed, slideSpeed) * slideDownhillSpeedCapMultiplier;
                return Mathf.Min(currentSpeed + boost, speedCap);
            }

            if (downhillAlignment < 0f && slideUphillSpeedLossPerSecond > 0f)
            {
                float loss = currentSpeed * slideUphillSpeedLossPerSecond * slopeStrength * -downhillAlignment * deltaTime;
                return Mathf.Max(0f, currentSpeed - loss);
            }

            return currentSpeed;
        }

        private float ApplySlideDrag(float currentSpeed, float deltaTime)
        {
            if (currentSpeed <= 0.01f || slideSpeedDecayPerSecond <= 0f)
            {
                return currentSpeed;
            }

            float duration = Mathf.Max(0.01f, slideDurationSeconds);
            float elapsed01 = Mathf.Clamp01(1f - _slideTimer / duration);
            float entryRatio = Mathf.Max(1f, _slideEntrySpeed / Mathf.Max(0.01f, slideSpeed));
            float delayedFraction = Mathf.Min(0.85f, slideDragDelayFraction * Mathf.Pow(entryRatio, slideEntrySpeedDelayExponent));
            float drag01 = Mathf.InverseLerp(delayedFraction, 1f, elapsed01);
            float dragRamp = Mathf.Pow(Mathf.Clamp01(drag01), slideDragRampExponent);
            float entryResistance = Mathf.Pow(entryRatio, slideEntrySpeedDecayResistanceExponent);
            float drag = slideSpeedDecayPerSecond * dragRamp / Mathf.Max(0.01f, entryResistance);
            return Mathf.Max(0f, currentSpeed - drag * deltaTime);
        }

        private float ResolveSlopeAdjustedSlideEntrySpeed(Vector3 slideDirection, float currentSpeed)
        {
            if (currentSpeed <= 0.01f || !TryGetSlideSlope(out Vector3 downhill, out _))
            {
                return currentSpeed;
            }

            float downhillAlignment = Mathf.Clamp01(Vector3.Dot(slideDirection.normalized, downhill));
            if (downhillAlignment <= 0f)
            {
                return currentSpeed;
            }

            return currentSpeed * Mathf.Lerp(1f, slideDownhillEnterSpeedMultiplier, downhillAlignment);
        }

        private Vector3 BuildGroundedVelocity(Vector3 lateralVelocity, float fallbackVerticalVelocity)
        {
            return lateralVelocity + Vector3.up * ResolveGroundFollowerVerticalVelocity(lateralVelocity, fallbackVerticalVelocity);
        }

        private float ResolveGroundFollowerVerticalVelocity(Vector3 lateralVelocity, float fallbackVerticalVelocity)
        {
            if (!_grounded || _groundNormal.y <= MinimumGroundNormalY)
            {
                return fallbackVerticalVelocity;
            }

            if (lateralVelocity.sqrMagnitude <= 0.0001f)
            {
                return Mathf.Min(fallbackVerticalVelocity, 0f);
            }

            return -Vector3.Dot(lateralVelocity, _groundNormal) / _groundNormal.y;
        }

        private float ResolveDownhillGroundStickiness(Vector3 lateralVelocity, Vector3 groundNormal)
        {
            if (lateralVelocity.sqrMagnitude <= 0.0001f || groundNormal.y <= MinimumGroundNormalY)
            {
                return 0f;
            }

            Vector3 downhill = Vector3.ProjectOnPlane(Vector3.down, groundNormal);
            if (downhill.sqrMagnitude <= 0.0001f)
            {
                return 0f;
            }

            float slopeAngle = Vector3.Angle(groundNormal, Vector3.up);
            float fadeStartAngle = Mathf.Clamp(downhillGroundStickFadeStartAngleDegrees, 1f, 89f);
            float fadeEndAngle = Mathf.Max(fadeStartAngle + 0.01f, downhillGroundStickFadeEndAngleDegrees);
            float minimumAngle = Mathf.Min(downhillGroundStickMinimumAngleDegrees, fadeStartAngle);
            float angleRamp = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(minimumAngle, minimumAngle + 8f, slopeAngle));
            float highAngleFade = 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(fadeStartAngle, fadeEndAngle, slopeAngle));
            float downhillAlignment = Mathf.Clamp01(Vector3.Dot(lateralVelocity.normalized, downhill.normalized));
            return downhillAlignment * angleRamp * Mathf.Clamp01(highAngleFade);
        }

        private float ResolveGroundReleaseHorizontalReference(float stickiness, float movementReference)
        {
            float defaultReference = Mathf.Max(0.001f, _capsule.radius * 0.35f);
            float stickyReference = Mathf.Max(defaultReference, movementReference);
            return Mathf.Lerp(defaultReference, stickyReference, Mathf.Clamp01(stickiness));
        }

        private float ReleaseGroundAtEdge(float verticalVelocity, float deltaTime)
        {
            _grounded = false;
            _groundNormal = Vector3.up;
            _groundedJumpGraceTimer = Mathf.Max(_groundedJumpGraceTimer, groundedJumpGraceSeconds);
            if (State == WallrunnerMovementState.Sliding)
            {
                _slideJumpGraceTimer = Mathf.Max(_slideJumpGraceTimer, groundedJumpGraceSeconds);
            }

            float edgeLockout = Mathf.Clamp(deltaTime * 1.5f, 0.025f, 0.06f);
            _groundSnapLockoutTimer = Mathf.Max(_groundSnapLockoutTimer, edgeLockout);
            State = WallrunnerMovementState.Airborne;
            return Mathf.Max(0f, verticalVelocity);
        }

        private bool HasSlideSupportAhead(Vector3 lateralVelocity, float deltaTime)
        {
            return HasGroundSupportAhead(
                lateralVelocity,
                deltaTime,
                slideGroundSnapReleaseDropAngleDegrees,
                slideGroundSnapBreakAngleDegrees,
                _capsule.radius * 1.35f);
        }

        private bool HasGroundSupportAhead(Vector3 lateralVelocity, float deltaTime, float releaseDropAngleDegrees, float breakAngleDegrees)
        {
            return HasGroundSupportAhead(
                lateralVelocity,
                deltaTime,
                releaseDropAngleDegrees,
                breakAngleDegrees,
                _capsule.radius);
        }

        private bool HasGroundSupportAhead(Vector3 lateralVelocity, float deltaTime, float releaseDropAngleDegrees, float breakAngleDegrees, float maximumLookAhead)
        {
            if (lateralVelocity.sqrMagnitude <= 0.0001f)
            {
                return true;
            }

            Vector3 direction = lateralVelocity.normalized;
            float stickiness = ResolveDownhillGroundStickiness(lateralVelocity, _groundNormal);
            float minimumLookAhead = _capsule.radius * 0.75f;
            float stickyMaximumLookAhead = maximumLookAhead + downhillGroundStickExtraLookAhead * stickiness;
            float lookAhead = Mathf.Clamp(lateralVelocity.magnitude * deltaTime, minimumLookAhead, Mathf.Max(minimumLookAhead, stickyMaximumLookAhead));
            Vector3 probePosition = transform.position + direction * lookAhead;
            float expectedDrop = Mathf.Max(0f, -ResolveGroundFollowerVerticalVelocity(lateralVelocity, 0f) * deltaTime);
            float probeDistance = groundContactProbeDistance + expectedDrop + skinWidth + downhillGroundStickExtraProbeDistance * stickiness;
            if (!TryFindGroundContactAtPosition(probePosition, probeDistance, out GroundProbeResult aheadGround))
            {
                return false;
            }

            float releaseHorizontalReference = ResolveGroundReleaseHorizontalReference(stickiness, lookAhead);
            if (ShouldReleaseFromGroundSnap(aheadGround, releaseDropAngleDegrees, releaseHorizontalReference))
            {
                return false;
            }

            return !IsSharpGroundChange(_groundNormal, aheadGround.Normal, breakAngleDegrees);
        }

        private bool TryGetSlideSlope(out Vector3 downhill, out float slopeStrength)
        {
            downhill = Vector3.zero;
            slopeStrength = 0f;
            if (!_grounded)
            {
                return false;
            }

            float slopeAngle = Vector3.Angle(_groundNormal, Vector3.up);
            if (slopeAngle < slideSlopeEffectMinimumAngleDegrees)
            {
                return false;
            }

            downhill = Vector3.ProjectOnPlane(Vector3.down, _groundNormal);
            slopeStrength = downhill.magnitude;
            if (slopeStrength <= 0.001f)
            {
                downhill = Vector3.zero;
                slopeStrength = 0f;
                return false;
            }

            downhill /= slopeStrength;
            return true;
        }

        private float ResolveWallrunProfileSpeed(float fatigue, float lateDecelStart)
        {
            float apexSpeed = Mathf.Max(_wallrunPeakSpeed, _wallrunStartSpeed);
            if (fatigue <= wallrunApexTime)
            {
                float apexT = Mathf.SmoothStep(0f, 1f, fatigue / Mathf.Max(0.01f, wallrunApexTime));
                return Mathf.Lerp(_wallrunStartSpeed, apexSpeed, apexT);
            }

            float postApexSpeed = apexSpeed * wallrunPostApexRetention;
            if (fatigue < lateDecelStart)
            {
                float postApexT = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(wallrunApexTime, lateDecelStart, fatigue));
                return Mathf.Lerp(apexSpeed, postApexSpeed, postApexT);
            }

            float endDrop = Mathf.Pow(Mathf.InverseLerp(lateDecelStart, 1f, fatigue), wallrunLateDecelExponent);
            return Mathf.Lerp(postApexSpeed, apexSpeed * wallrunEndSpeedRetention, endDrop);
        }

        private float ResolveWallrunVerticalVelocity(float fatigue, float endDrop)
        {
            float duration = Mathf.Max(0.01f, wallrunDurationSeconds);
            float arcVelocity = wallrunApexHeight * Mathf.PI / duration * Mathf.Cos(fatigue * Mathf.PI);
            float lateDropVelocity = wallrunGravity * wallrunFatigueGravityMultiplier * endDrop * 0.35f;
            return Mathf.Max(arcVelocity - lateDropVelocity, -wallrunMaxDownwardVelocity);
        }

        private float ResolveSlideJumpTimingScore()
        {
            float duration = Mathf.Max(0.01f, slideDurationSeconds);
            return Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(_slideTimer / duration));
        }

        private float ResolveWallJumpTimingMultiplier()
        {
            float duration = Mathf.Max(0.01f, wallrunDurationSeconds);
            float fatigue = Mathf.Clamp01(_wallrunTimer / duration);
            float distanceFromApex = Mathf.Abs(fatigue - wallrunApexTime);
            float timingScore = Mathf.Clamp01(1f - distanceFromApex / Mathf.Max(0.01f, wallJumpApexTimingWindow));
            timingScore = Mathf.SmoothStep(0f, 1f, timingScore);
            return Mathf.Lerp(1f, wallJumpPeakTimingMultiplier, timingScore);
        }

        private void ApplyVelocityScaledLateralBoost(Vector3 wishDirection, float speedMultiplier, float speedCap, float minimumBaseSpeed)
        {
            Vector3 lateralVelocity = Vector3.ProjectOnPlane(_velocity, Vector3.up);
            float currentSpeed = lateralVelocity.magnitude;
            Vector3 direction = currentSpeed > 0.01f ? lateralVelocity.normalized : Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
            if (wishDirection.sqrMagnitude > 0.1f)
            {
                direction = (direction + wishDirection.normalized * 0.35f).normalized;
            }

            float baseSpeed = Mathf.Max(currentSpeed, minimumBaseSpeed);
            float boostedSpeed = Mathf.Min(baseSpeed * speedMultiplier, Mathf.Max(speedCap, baseSpeed));
            _velocity = direction * boostedSpeed + Vector3.up * _velocity.y;
        }

        private void ApplyWallJumpLaunch(Vector3 lateralVelocity, bool useTimingMultiplier)
        {
            float currentSpeed = lateralVelocity.magnitude;
            Vector3 launchDirection = BuildWallJumpLaunchDirection(lateralVelocity, out float forwardPreserveRatio);
            float speedMultiplier = Mathf.Lerp(wallJumpTurnaroundSpeedMultiplier, wallJumpSpeedMultiplier, forwardPreserveRatio);
            float timingMultiplier = useTimingMultiplier ? ResolveWallJumpTimingMultiplier() : 1f;
            float baseSpeed = Mathf.Max(currentSpeed, sprintSpeed * wallJumpMinimumSpeedFraction);
            Vector3 lateralBoost = Vector3.ClampMagnitude(
                launchDirection * (baseSpeed * speedMultiplier * timingMultiplier),
                baseSpeed * wallJumpSpeedCapMultiplier);
            _velocity = lateralBoost + Vector3.up * wallJumpVerticalVelocity;
        }

        private void ApplyBunnyHopLaunch(Vector3 wishDirection)
        {
            Vector3 currentLateralVelocity = Vector3.ProjectOnPlane(_velocity, Vector3.up);
            Vector3 landingCarryVelocity = Vector3.ProjectOnPlane(_bunnyHopCarryVelocity, Vector3.up);
            Vector3 lateralVelocity = landingCarryVelocity.sqrMagnitude > currentLateralVelocity.sqrMagnitude
                ? landingCarryVelocity
                : currentLateralVelocity;
            float currentSpeed = lateralVelocity.magnitude;
            if (currentSpeed <= 0.01f)
            {
                return;
            }

            Vector3 direction = lateralVelocity.normalized;
            if (wishDirection.sqrMagnitude > 0.1f)
            {
                direction = Vector3.Slerp(direction, wishDirection.normalized, bunnyHopSteerStrength).normalized;
            }

            _velocity = direction * (currentSpeed * bunnyHopSpeedMultiplier) + Vector3.up * _velocity.y;
        }

        private void ApplyMaxVelocity(float deltaTime)
        {
            _velocity = ResolveMaxVelocity(_velocity, deltaTime);
        }

        private Vector3 ResolveMaxVelocity(Vector3 velocity, float deltaTime)
        {
            if (!maxVelocityEnabled)
            {
                return velocity;
            }

            float maxLateralSpeed = Mathf.Max(0.01f, maxVelocityMetersPerSecond);
            Vector3 lateralVelocity = Vector3.ProjectOnPlane(velocity, Vector3.up);
            float currentSpeed = lateralVelocity.magnitude;
            if (currentSpeed <= maxLateralSpeed || currentSpeed <= 0.0001f)
            {
                return velocity;
            }

            float targetSpeed = maxVelocityHardCap
                ? maxLateralSpeed
                : Mathf.Max(maxLateralSpeed, currentSpeed - maxVelocitySoftPullPerSecond * Mathf.Max(0f, deltaTime));
            return lateralVelocity.normalized * targetSpeed + Vector3.up * velocity.y;
        }

        private Vector3 BuildWallJumpLaunchDirection(Vector3 lateralVelocity, out float forwardPreserveRatio)
        {
            Vector3 awayFromWall = BuildWallAwayDirection();
            Vector3 lookDirection = BuildLookDirection();
            Vector3 carriedDirection = lateralVelocity.sqrMagnitude > 0.01f ? lateralVelocity.normalized : lookDirection;
            float lookCarryDot = Vector3.Dot(lookDirection, carriedDirection);
            forwardPreserveRatio = Mathf.Clamp01(Mathf.InverseLerp(0.05f, 0.65f, lookCarryDot));

            Vector3 launchDirection =
                lookDirection * wallJumpLookWeight +
                awayFromWall * wallJumpAwayWeight +
                carriedDirection * (wallJumpCarryWeight * forwardPreserveRatio);

            if (launchDirection.sqrMagnitude <= 0.0001f)
            {
                launchDirection = awayFromWall;
            }

            launchDirection.Normalize();
            float awayDot = Vector3.Dot(launchDirection, awayFromWall);
            if (awayDot < wallJumpMinimumAwayDot)
            {
                launchDirection = (launchDirection + awayFromWall * (wallJumpMinimumAwayDot - awayDot + 0.05f)).normalized;
            }

            return launchDirection;
        }

        private Vector3 ResolveWallrunAlongWall(Vector3 normal, Vector3 directionReference)
        {
            Vector3 alongWall = Vector3.Cross(Vector3.up, normal);
            if (alongWall.sqrMagnitude <= 0.0001f)
            {
                return Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
            }

            if (directionReference.sqrMagnitude > 0.01f && Vector3.Dot(alongWall, directionReference) < 0f)
            {
                alongWall = -alongWall;
            }

            return alongWall.normalized;
        }

        private Vector3 ResolveActiveHorizontalWallrunReference(Vector3 fallback)
        {
            Vector3 reference = Vector3.ProjectOnPlane(_wallrunGuidanceDirection, Vector3.up);
            if (reference.sqrMagnitude <= 0.01f)
            {
                reference = Vector3.ProjectOnPlane(_wallrunActiveDirection, Vector3.up);
            }

            if (reference.sqrMagnitude <= 0.01f)
            {
                reference = Vector3.ProjectOnPlane(_velocity, Vector3.up);
            }

            if (reference.sqrMagnitude <= 0.01f)
            {
                reference = Vector3.ProjectOnPlane(fallback, Vector3.up);
            }

            if (reference.sqrMagnitude <= 0.01f)
            {
                reference = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
            }

            if (reference.sqrMagnitude <= 0.01f)
            {
                return Vector3.forward;
            }

            return reference.normalized;
        }

        private Vector3 ResolveContinuingHorizontalWallrunDirection(Vector3 normal, Vector3 fallback)
        {
            Vector3 guidedReference = Vector3.ProjectOnPlane(_wallrunGuidanceDirection, Vector3.up);
            if (guidedReference.sqrMagnitude > 0.01f)
            {
                Vector3 guidedAlongWall = Vector3.ProjectOnPlane(guidedReference, normal);
                guidedAlongWall = Vector3.ProjectOnPlane(guidedAlongWall, Vector3.up);
                if (guidedAlongWall.sqrMagnitude > 0.01f)
                {
                    return KeepActiveWallrunDirectionSign(normal, guidedAlongWall.normalized);
                }
            }

            Vector3 reference = ResolveActiveHorizontalWallrunReference(fallback);
            return KeepActiveWallrunDirectionSign(normal, ResolveWallrunAlongWall(normal, reference));
        }

        private Vector3 KeepActiveWallrunDirectionSign(Vector3 normal, Vector3 candidate)
        {
            Vector3 activeDirection = Vector3.ProjectOnPlane(_wallrunActiveDirection, Vector3.up);
            Vector3 candidateDirection = Vector3.ProjectOnPlane(candidate, Vector3.up);
            if (activeDirection.sqrMagnitude <= 0.01f || candidateDirection.sqrMagnitude <= 0.01f)
            {
                return candidateDirection.sqrMagnitude <= 0.01f ? ResolveWallrunAlongWall(normal, candidate) : candidateDirection.normalized;
            }

            activeDirection.Normalize();
            candidateDirection.Normalize();
            if (Vector3.Dot(candidateDirection, activeDirection) >= 0f)
            {
                return candidateDirection;
            }

            return ResolveWallrunAlongWall(normal, activeDirection);
        }

        private Vector3 BuildActiveHorizontalWallrunProbeDirection()
        {
            Vector3 inward = Vector3.ProjectOnPlane(-_wallNormal, Vector3.up);
            if (inward.sqrMagnitude <= 0.0001f)
            {
                inward = Vector3.ProjectOnPlane(transform.right, Vector3.up);
            }

            if (inward.sqrMagnitude <= 0.0001f)
            {
                return Vector3.right;
            }

            return inward.normalized;
        }

        private bool IsSameWallReentryBlocked(Vector3 normal, Vector3 candidateAlongWall)
        {
            if (IsSameWallSameDirectionChainLocked(normal, candidateAlongWall))
            {
                return true;
            }

            if (_wallrunSameWallLockoutTimer <= 0f)
            {
                return false;
            }

            if (_wallrunBlockedNormal.sqrMagnitude > 0.0001f && IsWallrunContinuation(normal, _wallrunBlockedNormal))
            {
                return true;
            }

            if (!IsWallrunContinuation(normal, _wallrunLockedNormal))
            {
                return false;
            }

            if (_wallrunLockedDirection.sqrMagnitude <= 0.0001f || candidateAlongWall.sqrMagnitude <= 0.0001f)
            {
                return true;
            }

            return Vector3.Dot(candidateAlongWall.normalized, _wallrunLockedDirection.normalized) > wallrunReverseReentryDot;
        }

        private bool IsSameWallSameDirectionChainLocked(Vector3 normal, Vector3 candidateAlongWall)
        {
            if (!_wallrunChainSameDirectionLocked || !IsWallrunContinuation(normal, _wallrunChainLockedNormal))
            {
                return false;
            }

            if (_wallrunChainLockedDirection.sqrMagnitude <= 0.0001f || candidateAlongWall.sqrMagnitude <= 0.0001f)
            {
                return true;
            }

            return Vector3.Dot(candidateAlongWall.normalized, _wallrunChainLockedDirection.normalized) > wallrunReverseReentryDot;
        }

        private bool ShouldClearWallrunChainLock(Vector3 normal, Vector3 candidateAlongWall)
        {
            if (!_wallrunChainSameDirectionLocked)
            {
                return false;
            }

            if (!IsWallrunContinuation(normal, _wallrunChainLockedNormal))
            {
                return true;
            }

            if (_wallrunChainLockedDirection.sqrMagnitude <= 0.0001f || candidateAlongWall.sqrMagnitude <= 0.0001f)
            {
                return false;
            }

            return Vector3.Dot(candidateAlongWall.normalized, _wallrunChainLockedDirection.normalized) <= wallrunReverseReentryDot;
        }

        private bool IsWallrunContinuation(Vector3 normal, Vector3 referenceNormal)
        {
            if (normal.sqrMagnitude <= 0.0001f || referenceNormal.sqrMagnitude <= 0.0001f)
            {
                return false;
            }

            float continuationDot = Mathf.Cos(wallrunMaximumSurfaceTurnAngle * Mathf.Deg2Rad);
            return Vector3.Dot(normal.normalized, referenceNormal.normalized) >= continuationDot;
        }

        private void ClearWallrunChainLock()
        {
            _wallrunChainLockedNormal = Vector3.zero;
            _wallrunChainLockedDirection = Vector3.zero;
            _wallrunChainSameDirectionLocked = false;
        }

        private bool IsWallrunSurfaceTurnTooSharp(Vector3 candidateNormal)
        {
            if (_wallNormal.sqrMagnitude <= 0.0001f || candidateNormal.sqrMagnitude <= 0.0001f)
            {
                return false;
            }

            return !IsWallrunContinuation(candidateNormal, _wallNormal);
        }

        private Vector3 BuildWallAwayDirection()
        {
            Vector3 away = Vector3.ProjectOnPlane(_wallNormal, Vector3.up);
            if (away.sqrMagnitude <= 0.0001f)
            {
                away = -transform.forward;
            }

            return away.normalized;
        }

        private Vector3 BuildLookDirection()
        {
            Vector3 lookDirection = Vector3.ProjectOnPlane(BuildFullLookDirection(), Vector3.up);
            if (lookDirection.sqrMagnitude <= 0.0001f)
            {
                lookDirection = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
                return lookDirection.sqrMagnitude <= 0.0001f ? Vector3.forward : lookDirection.normalized;
            }

            return lookDirection.normalized;
        }

        private Vector3 BuildFullLookDirection()
        {
            if (_lookForward.sqrMagnitude > 0.0001f)
            {
                return _lookForward.normalized;
            }

            return transform.forward.sqrMagnitude <= 0.0001f ? Vector3.forward : transform.forward.normalized;
        }

        private void FatigueOutOfWallrun()
        {
            if (_wallrunFatigued)
            {
                return;
            }

            LockWallrunReentry();
            LockSameWallSameDirectionUntilAnotherWall();
            _wallrunActiveDirection = Vector3.zero;
            _wallrunSurfaceTransform = null;
            State = WallrunnerMovementState.Airborne;
        }

        private void ExitWallrunFromSurfaceLoss()
        {
            ExitWallrunFromSurfaceLoss(Vector3.zero);
        }

        private void ExitWallrunFromSurfaceLoss(Vector3 blockedNormal)
        {
            LockWallrunReentry();
            LockSameWallSameDirectionUntilAnotherWall();
            _wallrunDetachReattachTimer = wallrunDetachReattachDelayEnabled
                ? Mathf.Max(_wallrunDetachReattachTimer, wallrunDetachReattachDelaySeconds)
                : 0f;
            _wallrunBlockedNormal = blockedNormal;
            _wallrunActiveDirection = Vector3.zero;
            _wallrunSurfaceTransform = null;
            _wallrunStyle = WallrunTraversalStyle.Horizontal;
            State = WallrunnerMovementState.Airborne;
        }

        private void LockWallrunReentry()
        {
            _wallrunFatigued = true;
            _wallrunLockedNormal = _wallNormal;
            Vector3 lateralVelocity = Vector3.ProjectOnPlane(_velocity, Vector3.up);
            _wallrunLockedDirection = _wallrunLockedNormal.sqrMagnitude <= 0.0001f
                ? Vector3.zero
                : _wallrunStyle == WallrunTraversalStyle.Vertical ? Vector3.up : ResolveWallrunAlongWall(_wallrunLockedNormal, lateralVelocity);
            _wallrunSameWallLockoutTimer = wallrunSameWallLockoutSeconds;
        }

        private bool WallrunDetachReattachDelayActive => wallrunDetachReattachDelayEnabled && _wallrunDetachReattachTimer > 0f;

        private void LockSameWallSameDirectionUntilAnotherWall()
        {
            if (_wallrunLockedNormal.sqrMagnitude <= 0.0001f || _wallrunLockedDirection.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            _wallrunChainLockedNormal = _wallrunLockedNormal;
            _wallrunChainLockedDirection = _wallrunLockedDirection;
            _wallrunChainSameDirectionLocked = true;
        }

        private bool TryVault(Vector3 wishDirection)
        {
            Vector3 direction = ResolveVaultProbeDirection(wishDirection);
            if (direction.sqrMagnitude <= 0.01f)
            {
                return false;
            }

            bool capsuleWasEnabled = _capsule.enabled;
            _capsule.enabled = false;
            bool foundTarget = TryBuildVaultCandidate(direction, out VaultCandidate candidate);
            _capsule.enabled = capsuleWasEnabled;
            if (!foundTarget)
            {
                return false;
            }

            StartVaultBlend(candidate);
            return true;
        }

        private Vector3 ResolveVaultProbeDirection(Vector3 wishDirection)
        {
            Vector3 lookDirection = BuildLookDirection();
            Vector3 wish = Vector3.ProjectOnPlane(wishDirection, Vector3.up);
            if (wish.sqrMagnitude <= 0.01f)
            {
                return lookDirection;
            }

            return Vector3.Slerp(lookDirection, wish.normalized, 0.35f).normalized;
        }

        private void StartVaultBlend(VaultCandidate candidate)
        {
            ClearWallrunChainLock();
            _wallrunSameWallLockoutTimer = 0f;
            _wallrunDetachReattachTimer = 0f;
            _wallrunLockedNormal = Vector3.zero;
            _wallrunLockedDirection = Vector3.zero;
            _wallrunActiveDirection = Vector3.zero;
            _wallrunBlockedNormal = Vector3.zero;
            _wallrunSurfaceTransform = null;
            _wallrunNearTop = false;
            _wallrunStyle = WallrunTraversalStyle.Horizontal;
            _grounded = false;
            ResetAirJumps();
            _groundedJumpGraceTimer = 0f;
            _slideJumpGraceTimer = 0f;
            _airborneSlideBufferTimer = 0f;
            _groundSnapLockoutTimer = Mathf.Max(jumpGroundLockoutSeconds, 0.12f);
            _vaultStartPosition = transform.position;
            _vaultTargetPosition = candidate.TargetPosition;
            _vaultExitVelocity = ResolveMaxVelocity(candidate.ExitVelocity, 0f);
            _vaultBlendDuration = Mathf.Max(0.02f, ResolveVaultBlendDuration(candidate.Style));
            _vaultBlendTimer = 0.0001f;
            _velocity = _vaultExitVelocity;
            _activeVaultStyle = candidate.Style;
            _lastVaultStyle = candidate.Style;
            State = WallrunnerMovementState.Vaulting;
        }

        private void TickVaultBlend(float deltaTime)
        {
            _vaultBlendTimer += deltaTime;
            float normalizedTime = Mathf.Clamp01(_vaultBlendTimer / Mathf.Max(0.01f, _vaultBlendDuration));
            float easedTime = Mathf.SmoothStep(0f, 1f, normalizedTime);
            float arcLift = Mathf.Sin(normalizedTime * Mathf.PI) * _capsule.radius * 0.18f;
            transform.position = Vector3.Lerp(_vaultStartPosition, _vaultTargetPosition, easedTime) + Vector3.up * arcLift;
            _grounded = false;
            State = WallrunnerMovementState.Vaulting;

            if (normalizedTime < 1f)
            {
                return;
            }

            transform.position = _vaultTargetPosition;
            _velocity = _vaultExitVelocity;
            ResetAirJumps();
            _vaultBlendTimer = 0f;
            _vaultBlendDuration = 0f;
            _activeVaultStyle = VaultTraversalStyle.None;
            _groundSnapLockoutTimer = Mathf.Max(_groundSnapLockoutTimer, jumpGroundLockoutSeconds);
            State = WallrunnerMovementState.Airborne;
        }

        private bool TryBuildVaultCandidate(Vector3 direction, out VaultCandidate candidate)
        {
            candidate = default;
            if (!TryFindMantleSideHit(direction, out RaycastHit lowHit))
            {
                return false;
            }

            if (!TryResolveProceduralMantleSurface(lowHit, direction))
            {
                return false;
            }

            if (!TryFindMantleTop(lowHit, direction, out RaycastHit topHit))
            {
                return false;
            }

            float ledgeHeight = ResolveVaultLedgeHeight(topHit.point);
            if (!TryResolveVaultStyle(ledgeHeight, out VaultTraversalStyle style))
            {
                return false;
            }

            return TryBuildVaultTarget(topHit, direction, style, ledgeHeight, out candidate);
        }

        private bool TryFindMantleSideHit(Vector3 direction, out RaycastHit sideHit)
        {
            return TryFindMantleSideHitAtHeight(direction, _capsule.height * 0.16f, out sideHit)
                || TryFindMantleSideHitAtHeight(direction, _capsule.height * 0.34f, out sideHit)
                || TryFindMantleSideHitAtHeight(direction, _capsule.height * 0.56f, out sideHit)
                || TryFindMantleSideHitAtHeight(direction, _capsule.height * 0.72f, out sideHit);
        }

        private bool TryFindMantleSideHitAtHeight(Vector3 direction, float heightOffset, out RaycastHit sideHit)
        {
            Vector3 origin = transform.position;
            origin.y = ResolveCapsuleFeetY() + heightOffset;
            float radius = Mathf.Max(0.05f, _capsule.radius * 0.35f);
            return Physics.SphereCast(origin, radius, direction, out sideHit, mantleCheckDistance, collisionMask, QueryTriggerInteraction.Ignore)
                && TryResolveProceduralMantleSurface(sideHit, direction);
        }

        private bool TryResolveProceduralMantleSurface(RaycastHit hit, Vector3 direction)
        {
            if (hit.normal.y > mantleMaximumWallNormalY || hit.normal.y < -0.25f)
            {
                return false;
            }

            Vector3 approach = Vector3.ProjectOnPlane(direction, Vector3.up);
            Vector3 faceDirection = Vector3.ProjectOnPlane(-hit.normal, Vector3.up);
            if (approach.sqrMagnitude <= 0.001f || faceDirection.sqrMagnitude <= 0.001f)
            {
                return false;
            }

            return Vector3.Dot(approach.normalized, faceDirection.normalized) >= mantleMinimumApproachDot;
        }

        private bool TryFindMantleTop(RaycastHit lowHit, Vector3 direction, out RaycastHit topHit)
        {
            topHit = default;
            bool found = false;
            float bestHeight = float.NegativeInfinity;
            float bestProbeDistance = 0f;
            float minimumTopY = Mathf.Max(ResolveCapsuleFeetY() + 0.2f, lowHit.point.y - 0.15f);
            float maximumProbeDistance = mantleCheckDistance + Mathf.Max(flowVaultOverDistanceMeters, safetyMantleOverDistanceMeters, mantleVaultOverDistance);
            float firstProbeDistance = Mathf.Clamp(lowHit.distance + _capsule.radius * 0.35f, 0.25f, maximumProbeDistance);
            float probeStep = Mathf.Max(0.12f, _capsule.radius * 0.35f);
            float maximumVaultHeight = Mathf.Max(vaultMantleMaxHeightMeters, mantleHeight);
            float highStartY = ResolveCapsuleFeetY() + maximumVaultHeight + _capsule.height + 0.2f;
            float rayDistance = maximumVaultHeight + _capsule.height + 0.6f;

            for (int index = 0; index < 8; index++)
            {
                float ledgeProbeDistance = Mathf.Clamp(firstProbeDistance + probeStep * index, 0.25f, maximumProbeDistance);
                Vector3 highStart = transform.position + direction * ledgeProbeDistance;
                highStart.y = highStartY;
                if (!Physics.Raycast(highStart, Vector3.down, out RaycastHit candidate, rayDistance, collisionMask, QueryTriggerInteraction.Ignore))
                {
                    continue;
                }

                if (candidate.normal.y < mantleMinimumTopNormalY || candidate.point.y < minimumTopY)
                {
                    continue;
                }

                bool higher = candidate.point.y > bestHeight + 0.02f;
                bool sameHeightAndFarther = Mathf.Abs(candidate.point.y - bestHeight) <= 0.02f && ledgeProbeDistance > bestProbeDistance;
                if (!found || higher || sameHeightAndFarther)
                {
                    found = true;
                    bestHeight = candidate.point.y;
                    bestProbeDistance = ledgeProbeDistance;
                    topHit = candidate;
                }
            }

            return found;
        }

        private bool TryBuildVaultTarget(RaycastHit topHit, Vector3 direction, VaultTraversalStyle style, float ledgeHeight, out VaultCandidate candidate)
        {
            candidate = default;
            float overDistance = ResolveVaultOverDistance(style);
            float airborneLift = style == VaultTraversalStyle.Flow
                ? Mathf.Max(0.32f, _capsule.radius * 0.55f)
                : Mathf.Max(0.08f, _capsule.radius * 0.12f);
            Vector3 targetPosition = topHit.point + direction * overDistance + Vector3.up * (_capsule.height * 0.5f + skinWidth + airborneLift);
            if (!IsCapsuleClearAt(targetPosition))
            {
                return false;
            }

            float timingScore = ResolveVaultTimingScore(topHit.point, style);
            Vector3 exitVelocity = BuildVaultExitVelocity(direction, timingScore, style);
            candidate = new VaultCandidate(targetPosition, exitVelocity, style, ledgeHeight);
            return true;
        }

        private bool TryResolveVaultStyle(float ledgeHeight, out VaultTraversalStyle style)
        {
            style = ClassifyVaultHeight(ledgeHeight);
            return style != VaultTraversalStyle.None;
        }

        private VaultTraversalStyle ClassifyVaultHeight(float ledgeHeight)
        {
            if (ledgeHeight < 0.2f || ledgeHeight > vaultMantleMaxHeightMeters)
            {
                return VaultTraversalStyle.None;
            }

            switch (vaultMode)
            {
                case VaultMode.Flow:
                    return VaultTraversalStyle.Flow;
                case VaultMode.SafetyMantle:
                    return VaultTraversalStyle.SafetyMantle;
                default:
                    return ledgeHeight <= vaultFlowMaxHeightMeters ? VaultTraversalStyle.Flow : VaultTraversalStyle.SafetyMantle;
            }
        }

        private float ResolveVaultLedgeHeight(Vector3 ledgePoint)
        {
            return ledgePoint.y - ResolveCapsuleFeetY();
        }

        private float ResolveCapsuleFeetY()
        {
            GetCapsulePoints(transform.position, out Vector3 bottom, out _);
            return bottom.y - _capsule.radius;
        }

        private float ResolveVaultTimingScore(Vector3 ledgePoint, VaultTraversalStyle style)
        {
            float upwardScore = Mathf.InverseLerp(-jumpVelocity * 0.55f, jumpVelocity * 0.45f, _velocity.y);
            float ledgeHeight = ResolveVaultLedgeHeight(ledgePoint);
            float idealHeight = style == VaultTraversalStyle.Flow
                ? Mathf.Max(0.35f, vaultFlowMaxHeightMeters * 0.65f)
                : Mathf.Lerp(vaultFlowMaxHeightMeters, vaultMantleMaxHeightMeters, 0.55f);
            float heightWindow = style == VaultTraversalStyle.Flow
                ? Mathf.Max(0.35f, vaultFlowMaxHeightMeters * 0.55f)
                : Mathf.Max(0.35f, vaultMantleMaxHeightMeters - vaultFlowMaxHeightMeters);
            float heightScore = Mathf.Clamp01(1f - Mathf.Abs(ledgeHeight - idealHeight) / heightWindow);
            return Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(upwardScore * 0.7f + heightScore * 0.3f));
        }

        private Vector3 BuildVaultExitVelocity(Vector3 direction, float timingScore, VaultTraversalStyle style)
        {
            Vector3 carryVelocity = Vector3.ProjectOnPlane(_velocity, Vector3.up);
            Vector3 carryDirection = carryVelocity.sqrMagnitude > 0.01f ? carryVelocity.normalized : direction;
            float directionBlend = style == VaultTraversalStyle.Flow ? 0.35f : 0.58f;
            Vector3 exitDirection = Vector3.Slerp(carryDirection, direction, directionBlend).normalized;
            float carrySpeed = Mathf.Max(carryVelocity.magnitude, sprintSpeed * vaultMinimumSpeedFraction);
            float speedMultiplier = ResolveVaultSpeedMultiplier(style, timingScore);
            float exitSpeed = Mathf.Max(carrySpeed, carrySpeed * speedMultiplier, sprintSpeed * vaultMinimumSpeedFraction * speedMultiplier);
            float verticalBoost = ResolveVaultVerticalBoost(style, timingScore);
            return exitDirection * exitSpeed + Vector3.up * (jumpVelocity * verticalBoost);
        }

        private float ResolveVaultBlendDuration(VaultTraversalStyle style)
        {
            return style == VaultTraversalStyle.SafetyMantle
                ? safetyMantleBlendDurationSeconds
                : flowVaultBlendDurationSeconds;
        }

        private float ResolveVaultOverDistance(VaultTraversalStyle style)
        {
            return style == VaultTraversalStyle.SafetyMantle
                ? safetyMantleOverDistanceMeters
                : flowVaultOverDistanceMeters;
        }

        private float ResolveVaultSpeedMultiplier(VaultTraversalStyle style, float timingScore)
        {
            return style == VaultTraversalStyle.SafetyMantle
                ? Mathf.Lerp(safetyMantleBaseSpeedMultiplier, safetyMantlePerfectSpeedMultiplier, timingScore)
                : Mathf.Lerp(flowVaultBaseSpeedMultiplier, flowVaultPerfectSpeedMultiplier, timingScore);
        }

        private float ResolveVaultVerticalBoost(VaultTraversalStyle style, float timingScore)
        {
            return style == VaultTraversalStyle.SafetyMantle
                ? Mathf.Lerp(safetyMantleMinimumVerticalBoost, safetyMantlePerfectVerticalBoost, timingScore)
                : Mathf.Lerp(flowVaultMinimumVerticalBoost, flowVaultPerfectVerticalBoost, timingScore);
        }

        private readonly struct VaultCandidate
        {
            public VaultCandidate(Vector3 targetPosition, Vector3 exitVelocity, VaultTraversalStyle style, float ledgeHeight)
            {
                TargetPosition = targetPosition;
                ExitVelocity = exitVelocity;
                Style = style;
                LedgeHeight = ledgeHeight;
            }

            public Vector3 TargetPosition { get; }

            public Vector3 ExitVelocity { get; }

            public VaultTraversalStyle Style { get; }

            public float LedgeHeight { get; }
        }

        private bool IsCapsuleClearAt(Vector3 capsulePosition)
        {
            GetCapsulePoints(capsulePosition, out Vector3 bottom, out Vector3 top);
            float clearanceRadius = Mathf.Max(0.01f, _capsule.radius - skinWidth);
            return !Physics.CheckCapsule(bottom, top, clearanceRadius, collisionMask, QueryTriggerInteraction.Ignore);
        }

        private float ResolveTargetSpeed(bool sprintHeld, bool wallrunning)
        {
            if (wallrunning)
            {
                return wallrunSpeed;
            }

            if (State == WallrunnerMovementState.Sliding)
            {
                return slideSpeed;
            }

            return sprintHeld ? sprintSpeed : walkSpeed;
        }

        private bool CheckGrounded()
        {
            bool hit = TryFindGroundContact(groundContactProbeDistance, out GroundProbeResult groundHit);
            _groundNormal = hit ? groundHit.Normal : Vector3.up;
            return hit;
        }

        private void ResolvePostMoveGrounding(bool jumpPressed, bool shouldStickToGround, float deltaTime)
        {
            if (jumpPressed)
            {
                return;
            }

            if (_groundSnapLockoutTimer > 0f)
            {
                _grounded = false;
                if (State == WallrunnerMovementState.Sliding)
                {
                    State = WallrunnerMovementState.Airborne;
                }

                return;
            }

            if (State == WallrunnerMovementState.Sliding)
            {
                ResolvePostMoveSlideGrounding(deltaTime);
                return;
            }

            if (!shouldStickToGround || !_grounded)
            {
                return;
            }

            Vector3 groundReference = _groundNormal;
            _grounded = CheckGrounded();
            if (!_grounded)
            {
                _grounded = TrySnapToGround(groundReference, deltaTime);
            }

            if (_grounded)
            {
                _velocity = BuildGroundedVelocity(Vector3.ProjectOnPlane(_velocity, Vector3.up), _velocity.y);
            }
        }

        private void ResolvePostMoveSlideGrounding(float deltaTime)
        {
            Vector3 groundReference = _groundNormal;
            _grounded = CheckGrounded();
            if (!_grounded)
            {
                _grounded = TrySnapToSlideGround(groundReference, deltaTime);
            }

            if (_grounded)
            {
                _velocity = BuildGroundedVelocity(Vector3.ProjectOnPlane(_velocity, Vector3.up), _velocity.y);
                return;
            }

            State = WallrunnerMovementState.Airborne;
        }

        private bool TrySnapToGround(Vector3 referenceGroundNormal, float deltaTime)
        {
            return TrySnapToGround(groundSnapDistance, referenceGroundNormal, groundSnapBreakAngleDegrees, groundSnapReleaseDropAngleDegrees, deltaTime);
        }

        private bool TrySnapToSlideGround(Vector3 referenceGroundNormal, float deltaTime)
        {
            return TrySnapToGround(slideGroundSnapDistance, referenceGroundNormal, slideGroundSnapBreakAngleDegrees, slideGroundSnapReleaseDropAngleDegrees, deltaTime);
        }

        private bool TrySnapToGround(float snapDistance, Vector3 referenceGroundNormal, float breakAngleDegrees, float releaseDropAngleDegrees, float deltaTime)
        {
            Vector3 lateralVelocity = Vector3.ProjectOnPlane(_velocity, Vector3.up);
            float stickiness = ResolveDownhillGroundStickiness(lateralVelocity, referenceGroundNormal);
            float effectiveSnapDistance = snapDistance + downhillGroundStickExtraProbeDistance * stickiness;
            if (!TryFindGround(effectiveSnapDistance, out GroundProbeResult groundHit))
            {
                return false;
            }

            float releaseHorizontalReference = ResolveGroundReleaseHorizontalReference(stickiness, lateralVelocity.magnitude * deltaTime);
            if (ShouldReleaseFromGroundSnap(groundHit, releaseDropAngleDegrees, releaseHorizontalReference))
            {
                return false;
            }

            if (IsSharpGroundChange(referenceGroundNormal, groundHit.Normal, breakAngleDegrees))
            {
                return false;
            }

            if (groundHit.SnapOffset > 0f)
            {
                transform.position += Vector3.down * groundHit.SnapOffset;
            }

            _groundNormal = groundHit.Normal;
            return true;
        }

        private bool ShouldReleaseFromGroundSnap(GroundProbeResult groundHit, float releaseDropAngleDegrees)
        {
            return ShouldReleaseFromGroundSnap(groundHit, releaseDropAngleDegrees, _capsule.radius * 0.35f);
        }

        private bool ShouldReleaseFromGroundSnap(GroundProbeResult groundHit, float releaseDropAngleDegrees, float horizontalReference)
        {
            if (releaseDropAngleDegrees >= 89f || groundHit.SnapOffset < groundSnapReleaseMinimumDrop)
            {
                return false;
            }

            float dropAngle = Mathf.Atan2(groundHit.SnapOffset, Mathf.Max(0.001f, horizontalReference)) * Mathf.Rad2Deg;
            return dropAngle >= releaseDropAngleDegrees;
        }

        private bool IsSharpGroundChange(Vector3 referenceGroundNormal, Vector3 candidateGroundNormal, float breakAngleDegrees)
        {
            if (referenceGroundNormal.sqrMagnitude <= 0.001f)
            {
                return false;
            }

            return Vector3.Angle(referenceGroundNormal, candidateGroundNormal) > breakAngleDegrees;
        }

        private bool IsStandableGround(Vector3 normal)
        {
            return normal.y > MinimumGroundNormalY;
        }

        private Vector3 BuildGroundProbeOrigin(Vector3 capsuleBottom, float probeRadius)
        {
            float radiusInset = Mathf.Max(0f, _capsule.radius - probeRadius);
            return capsuleBottom + Vector3.down * radiusInset + Vector3.up * GroundProbeBottomClearance;
        }

        private float ResolveSphereGroundSnapOffset(float groundHitDistance)
        {
            float desiredProbeDistance = Mathf.Max(0.01f, GroundProbeBottomClearance + skinWidth * 0.5f);
            return Mathf.Max(0f, groundHitDistance - desiredProbeDistance);
        }

        private bool TryFindGroundContact(float probeDistance, out GroundProbeResult groundHit)
        {
            return TryFindGroundContactAtPosition(transform.position, probeDistance, out groundHit);
        }

        private bool TryFindGroundContactAtPosition(Vector3 position, float probeDistance, out GroundProbeResult groundHit)
        {
            groundHit = default;
            GetCapsulePoints(position, out Vector3 bottom, out _);
            float probeRadius = Mathf.Max(0.025f, _capsule.radius * groundContactRadiusMultiplier);
            Vector3 sphereOrigin = BuildGroundProbeOrigin(bottom, probeRadius);
            bool capsuleWasEnabled = _capsule.enabled;
            _capsule.enabled = false;
            if (Physics.SphereCast(sphereOrigin, probeRadius, Vector3.down, out RaycastHit sphereHit, probeDistance, collisionMask, QueryTriggerInteraction.Ignore)
                && IsStandableGround(sphereHit.normal))
            {
                groundHit = new GroundProbeResult(sphereHit.point, sphereHit.normal, ResolveSphereGroundSnapOffset(sphereHit.distance));
                _capsule.enabled = capsuleWasEnabled;
                return true;
            }

            float bestSnapOffset = float.MaxValue;
            bool foundRayGround = TryProbeGroundRay(sphereOrigin, probeRadius, probeDistance, ref groundHit, ref bestSnapOffset);
            _capsule.enabled = capsuleWasEnabled;
            return foundRayGround;
        }

        private bool TryFindGroundFromRaycasts(Vector3 sphereOrigin, float probeRadius, float probeDistance, out GroundProbeResult groundHit)
        {
            groundHit = default;
            float bestSnapOffset = float.MaxValue;
            float rayOffset = Mathf.Max(0.05f, probeRadius * 0.55f);
            Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
            if (forward.sqrMagnitude <= 0.001f)
            {
                forward = Vector3.forward;
            }

            forward.Normalize();
            Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;

            bool found = TryProbeGroundRay(sphereOrigin, probeRadius, probeDistance, ref groundHit, ref bestSnapOffset);
            found |= TryProbeGroundRay(sphereOrigin + forward * rayOffset, probeRadius, probeDistance, ref groundHit, ref bestSnapOffset);
            found |= TryProbeGroundRay(sphereOrigin - forward * rayOffset, probeRadius, probeDistance, ref groundHit, ref bestSnapOffset);
            found |= TryProbeGroundRay(sphereOrigin + right * rayOffset, probeRadius, probeDistance, ref groundHit, ref bestSnapOffset);
            found |= TryProbeGroundRay(sphereOrigin - right * rayOffset, probeRadius, probeDistance, ref groundHit, ref bestSnapOffset);
            return found;
        }

        private bool TryProbeGroundRay(Vector3 origin, float probeRadius, float probeDistance, ref GroundProbeResult bestGroundHit, ref float bestSnapOffset)
        {
            float rayDistance = probeRadius + probeDistance + skinWidth;
            if (!Physics.Raycast(origin, Vector3.down, out RaycastHit rayHit, rayDistance, collisionMask, QueryTriggerInteraction.Ignore))
            {
                return false;
            }

            if (!IsStandableGround(rayHit.normal))
            {
                return false;
            }

            float snapOffset = Mathf.Max(0f, origin.y - probeRadius - rayHit.point.y - skinWidth * 0.5f);
            if (snapOffset > probeDistance + skinWidth || snapOffset >= bestSnapOffset)
            {
                return false;
            }

            bestSnapOffset = snapOffset;
            bestGroundHit = new GroundProbeResult(rayHit.point, rayHit.normal, snapOffset);
            return true;
        }

        private bool TryFindGround(float probeDistance, out GroundProbeResult groundHit)
        {
            groundHit = default;
            GetCapsulePoints(transform.position, out Vector3 bottom, out _);
            float probeRadius = _capsule.radius * 0.92f;
            Vector3 sphereOrigin = BuildGroundProbeOrigin(bottom, probeRadius);
            bool capsuleWasEnabled = _capsule.enabled;
            _capsule.enabled = false;
            if (Physics.SphereCast(sphereOrigin, probeRadius, Vector3.down, out RaycastHit sphereHit, probeDistance, collisionMask, QueryTriggerInteraction.Ignore)
                && IsStandableGround(sphereHit.normal))
            {
                groundHit = new GroundProbeResult(sphereHit.point, sphereHit.normal, ResolveSphereGroundSnapOffset(sphereHit.distance));
                _capsule.enabled = capsuleWasEnabled;
                return true;
            }

            bool foundRayGround = TryFindGroundFromRaycasts(sphereOrigin, probeRadius, probeDistance, out groundHit);
            _capsule.enabled = capsuleWasEnabled;
            return foundRayGround;
        }

        private bool TryFindWallrunCandidate(Vector3 wishDirection, out Vector3 normal, out Transform surfaceTransform, out WallrunTraversalStyle style, out Vector3 runDirection)
        {
            normal = Vector3.zero;
            surfaceTransform = null;
            style = WallrunTraversalStyle.Horizontal;
            runDirection = Vector3.zero;

            if (State == WallrunnerMovementState.Wallrunning)
            {
                return _wallrunStyle == WallrunTraversalStyle.Vertical
                    ? TryFindVerticalWallrunCandidate(wishDirection, out normal, out surfaceTransform, out style, out runDirection)
                    : TryFindHorizontalWallrunCandidate(wishDirection, out normal, out surfaceTransform, out style, out runDirection);
            }

            if (TryFindVerticalWallrunCandidate(wishDirection, out normal, out surfaceTransform, out style, out runDirection))
            {
                return true;
            }

            return TryFindHorizontalWallrunCandidate(wishDirection, out normal, out surfaceTransform, out style, out runDirection);
        }

        private bool TryFindHorizontalWallrunCandidate(Vector3 wishDirection, out Vector3 normal, out Transform surfaceTransform, out WallrunTraversalStyle style, out Vector3 runDirection)
        {
            normal = Vector3.zero;
            surfaceTransform = null;
            style = WallrunTraversalStyle.Horizontal;
            runDirection = Vector3.zero;
            Vector3 directionReference = State == WallrunnerMovementState.Wallrunning
                ? ResolveActiveHorizontalWallrunReference(wishDirection)
                : wishDirection;
            if (!FindWall(directionReference, out normal, out surfaceTransform))
            {
                return false;
            }

            style = WallrunTraversalStyle.Horizontal;
            runDirection = State == WallrunnerMovementState.Wallrunning
                ? ResolveContinuingHorizontalWallrunDirection(normal, directionReference)
                : ResolveWallrunAlongWall(normal, directionReference);
            return true;
        }

        private bool ShouldResetActiveWallrunForCandidate(WallrunTraversalStyle candidateStyle, Vector3 candidateNormal, Transform candidateSurface)
        {
            if (State != WallrunnerMovementState.Wallrunning)
            {
                return false;
            }

            if (_wallrunStyle != candidateStyle)
            {
                return true;
            }

            if (_wallrunSurfaceTransform != null && candidateSurface != _wallrunSurfaceTransform)
            {
                return true;
            }

            return IsWallrunSurfaceTurnTooSharp(candidateNormal);
        }

        private bool TryFindVerticalWallrunCandidate(Vector3 wishDirection, out Vector3 normal, out Transform surfaceTransform, out WallrunTraversalStyle style, out Vector3 runDirection)
        {
            normal = Vector3.zero;
            surfaceTransform = null;
            style = WallrunTraversalStyle.Horizontal;
            runDirection = Vector3.zero;
            if (!verticalWallrunEnabled)
            {
                return false;
            }

            Vector3 fullLookDirection = BuildFullLookDirection();
            Vector3 lookDirection = BuildLookDirection();
            Transform requiredSurface = State == WallrunnerMovementState.Wallrunning ? _wallrunSurfaceTransform : null;
            bool capsuleWasEnabled = _capsule.enabled;
            _capsule.enabled = false;
            bool foundProbe = TryAcceptWallProbe(lookDirection, requiredSurface, out WallProbeResult probe);
            _capsule.enabled = capsuleWasEnabled;
            bool requiresUpwardLook = State != WallrunnerMovementState.Wallrunning || _wallrunStyle != WallrunTraversalStyle.Vertical;
            if (!foundProbe || !IsVerticalWallrunIntent(probe.Normal, wishDirection, lookDirection, fullLookDirection, requiresUpwardLook))
            {
                return false;
            }

            if (State != WallrunnerMovementState.Wallrunning)
            {
                float lateralSpeed = Vector3.ProjectOnPlane(_velocity, Vector3.up).magnitude;
                if (lateralSpeed < sprintSpeed * verticalWallrunMinimumEntrySpeedFraction)
                {
                    return false;
                }
            }

            normal = probe.Normal;
            surfaceTransform = probe.SurfaceTransform;
            _wallrunNearTop = probe.NearTop;
            style = WallrunTraversalStyle.Vertical;
            runDirection = Vector3.up;
            return true;
        }

        private bool IsVerticalWallrunIntent(Vector3 wallNormal, Vector3 wishDirection, Vector3 lookDirection, Vector3 fullLookDirection, bool requiresUpwardLook)
        {
            Vector3 towardWall = Vector3.ProjectOnPlane(-wallNormal, Vector3.up);
            Vector3 flatWish = Vector3.ProjectOnPlane(wishDirection, Vector3.up);
            if (towardWall.sqrMagnitude <= 0.0001f || flatWish.sqrMagnitude <= 0.0001f)
            {
                return false;
            }

            towardWall.Normalize();
            float lookDot = Vector3.Dot(lookDirection, towardWall);
            float moveDot = Vector3.Dot(flatWish.normalized, towardWall);
            float minimumLookDot = Mathf.Cos(verticalWallrunLookMaxAngleDegrees * Mathf.Deg2Rad);
            float minimumMoveDot = Mathf.Cos(verticalWallrunMoveMaxAngleDegrees * Mathf.Deg2Rad);
            if (lookDot < minimumLookDot || moveDot < minimumMoveDot)
            {
                return false;
            }

            return !requiresUpwardLook
                || fullLookDirection.y >= Mathf.Sin(verticalWallrunMinimumLookUpAngleDegrees * Mathf.Deg2Rad);
        }

        private bool FindWall(Vector3 wishDirection, out Vector3 normal, out Transform surfaceTransform)
        {
            surfaceTransform = null;
            bool capsuleWasEnabled = _capsule.enabled;
            _capsule.enabled = false;
            Transform requiredSurface = State == WallrunnerMovementState.Wallrunning ? _wallrunSurfaceTransform : null;
            WallProbeResult bestProbe = default;
            float bestScore = float.NegativeInfinity;
            bool foundProbe = false;
            bool activeHorizontalWallrun = State == WallrunnerMovementState.Wallrunning
                && _wallrunStyle == WallrunTraversalStyle.Horizontal
                && requiredSurface != null;
            if (activeHorizontalWallrun)
            {
                Vector3 inwardProbe = BuildActiveHorizontalWallrunProbeDirection();
                Vector3 activeDirection = ResolveActiveHorizontalWallrunReference(wishDirection);
                float activeCurvedProbeBias = Mathf.Clamp01(wallrunCurvedSurfaceProbeForwardBias);
                if (TryAcceptWallProbe(inwardProbe, requiredSurface, out WallProbeResult activeProbe))
                {
                    foundProbe |= TryUseWallProbe(activeProbe, activeDirection, 8f, ref bestProbe, ref bestScore);
                }

                if (activeCurvedProbeBias > 0f)
                {
                    if (TryAcceptWallProbe((inwardProbe + activeDirection * activeCurvedProbeBias).normalized, requiredSurface, out WallProbeResult forwardProbe))
                    {
                        foundProbe |= TryUseWallProbe(forwardProbe, activeDirection, 3f, ref bestProbe, ref bestScore);
                    }

                    if (TryAcceptWallProbe((inwardProbe - activeDirection * activeCurvedProbeBias).normalized, requiredSurface, out WallProbeResult rearProbe))
                    {
                        foundProbe |= TryUseWallProbe(rearProbe, activeDirection, 1f, ref bestProbe, ref bestScore);
                    }
                }
            }

            if (State == WallrunnerMovementState.Wallrunning
                && _wallNormal.sqrMagnitude > 0.0001f
                && TryAcceptWallProbe(-_wallNormal, requiredSurface, out WallProbeResult continuityProbe))
            {
                foundProbe |= TryUseWallProbe(continuityProbe, wishDirection, 4f, ref bestProbe, ref bestScore);
            }

            Vector3 right = Vector3.ProjectOnPlane(transform.right, Vector3.up);
            if (right.sqrMagnitude <= 0.0001f)
            {
                right = Vector3.right;
            }

            right.Normalize();
            if (TryAcceptWallProbe(right, requiredSurface, out WallProbeResult rightProbe))
            {
                foundProbe |= TryUseWallProbe(rightProbe, wishDirection, 0f, ref bestProbe, ref bestScore);
            }

            if (TryAcceptWallProbe(-right, requiredSurface, out WallProbeResult leftProbe))
            {
                foundProbe |= TryUseWallProbe(leftProbe, wishDirection, 0f, ref bestProbe, ref bestScore);
            }

            Vector3 forward = activeHorizontalWallrun ? ResolveActiveHorizontalWallrunReference(wishDirection) : BuildLookDirection();
            float curvedProbeBias = Mathf.Clamp01(wallrunCurvedSurfaceProbeForwardBias);
            if (curvedProbeBias > 0f && TryAcceptWallProbe((right + forward * curvedProbeBias).normalized, requiredSurface, out WallProbeResult forwardRightProbe))
            {
                foundProbe |= TryUseWallProbe(forwardRightProbe, wishDirection, 0.15f, ref bestProbe, ref bestScore);
            }

            if (curvedProbeBias > 0f && TryAcceptWallProbe((-right + forward * curvedProbeBias).normalized, requiredSurface, out WallProbeResult forwardLeftProbe))
            {
                foundProbe |= TryUseWallProbe(forwardLeftProbe, wishDirection, 0.15f, ref bestProbe, ref bestScore);
            }

            if (foundProbe)
            {
                _wallrunNearTop = bestProbe.NearTop;
                normal = bestProbe.Normal;
                surfaceTransform = bestProbe.SurfaceTransform;
                _capsule.enabled = capsuleWasEnabled;
                return true;
            }

            _capsule.enabled = capsuleWasEnabled;
            normal = Vector3.zero;
            _wallrunNearTop = false;
            return false;
        }

        private bool TryUseWallProbe(WallProbeResult probe, Vector3 wishDirection, float bonus, ref WallProbeResult bestProbe, ref float bestScore)
        {
            if (State == WallrunnerMovementState.Wallrunning && IsWallrunSurfaceTurnTooSharp(probe.Normal))
            {
                return false;
            }

            float score = ScoreWallProbe(probe, wishDirection) + bonus;
            if (score <= bestScore)
            {
                return false;
            }

            bestProbe = probe;
            bestScore = score;
            return true;
        }

        private float ScoreWallProbe(WallProbeResult probe, Vector3 wishDirection)
        {
            Vector3 flatNormal = Vector3.ProjectOnPlane(probe.Normal, Vector3.up);
            if (flatNormal.sqrMagnitude <= 0.0001f)
            {
                return float.NegativeInfinity;
            }

            Vector3 towardWall = -flatNormal.normalized;
            Vector3 flatWish = Vector3.ProjectOnPlane(wishDirection, Vector3.up);
            bool hasWish = flatWish.sqrMagnitude > 0.0001f;
            Vector3 wish = hasWish ? flatWish.normalized : Vector3.zero;
            Vector3 look = BuildLookDirection();
            Vector3 intentDirection = hasWish ? wish : look;
            float strafeToward = hasWish ? Vector3.Dot(wish, towardWall) : 0f;
            float lookToward = Vector3.Dot(look, towardWall);
            Vector3 alongWall = ResolveWallrunAlongWall(probe.Normal, intentDirection);
            float alongIntent = hasWish ? Mathf.Abs(Vector3.Dot(alongWall, wish)) : Mathf.Abs(Vector3.Dot(alongWall, look));
            float castIntent = Vector3.Dot(probe.CastDirection, intentDirection);
            float distanceScore = 1f / (1f + Mathf.Max(0f, probe.Distance));
            return (strafeToward * 2.4f)
                + (lookToward * 1.35f)
                + (alongIntent * 0.65f)
                + (castIntent * 0.35f)
                + (distanceScore * 0.2f);
        }

        private bool TryAcceptWallProbe(Vector3 direction, Transform requiredSurface, out WallProbeResult probe)
        {
            probe = default;
            if (direction.sqrMagnitude <= 0.0001f)
            {
                return false;
            }

            direction.Normalize();
            if (!TryFindWallOnSide(direction, requiredSurface, out Vector3 normal, out bool nearTop, out Transform surfaceTransform, out float distance))
            {
                return false;
            }

            probe = new WallProbeResult(normal, nearTop, surfaceTransform, distance, direction);
            return true;
        }

        private bool TryFindWallOnSide(Vector3 direction, Transform requiredSurface, out Vector3 normal, out bool nearTop, out Transform surfaceTransform, out float distance)
        {
            normal = Vector3.zero;
            nearTop = false;
            surfaceTransform = null;
            distance = 0f;
            float radius = _capsule.radius * 0.72f;
            float castDistance = _capsule.radius + 0.28f;
            Vector3 lowerOrigin = transform.position + Vector3.up * (_capsule.height * 0.18f);
            Vector3 upperOrigin = transform.position + Vector3.up * (_capsule.height * wallrunUpperProbeHeightFraction);
            if (!TryFindWallProbe(lowerOrigin, radius, direction, castDistance, out RaycastHit lowerHit))
            {
                return false;
            }

            surfaceTransform = ResolveWallrunSurfaceTransform(lowerHit.collider);
            if (surfaceTransform == null || (requiredSurface != null && surfaceTransform != requiredSurface))
            {
                surfaceTransform = null;
                return false;
            }

            bool hasUpperHit = TryFindWallProbe(upperOrigin, radius, direction, castDistance, out RaycastHit upperHit);
            if (!hasUpperHit)
            {
                if (State != WallrunnerMovementState.Wallrunning || requiredSurface == null || IsWallrunSurfaceTurnTooSharp(lowerHit.normal))
                {
                    surfaceTransform = null;
                    return false;
                }

                normal = lowerHit.normal.normalized;
                nearTop = true;
                distance = lowerHit.distance;
                return true;
            }

            if (surfaceTransform != ResolveWallrunSurfaceTransform(upperHit.collider))
            {
                surfaceTransform = null;
                return false;
            }

            if (State == WallrunnerMovementState.Wallrunning
                && (IsWallrunSurfaceTurnTooSharp(lowerHit.normal) || IsWallrunSurfaceTurnTooSharp(upperHit.normal)))
            {
                surfaceTransform = null;
                return false;
            }

            float maximumNormalDelta = Mathf.Cos(wallrunMaximumSurfaceTurnAngle * Mathf.Deg2Rad);
            if (Vector3.Dot(lowerHit.normal, upperHit.normal) < maximumNormalDelta)
            {
                surfaceTransform = null;
                return false;
            }

            float topProbeFraction = Mathf.Max(wallrunTopFlattenProbeHeightFraction, wallrunUpperProbeHeightFraction + 0.01f);
            Vector3 topOrigin = transform.position + Vector3.up * (_capsule.height * topProbeFraction);
            bool hasStableTopContact = TryFindWallProbe(topOrigin, radius, direction, castDistance, out RaycastHit topHit)
                && surfaceTransform == ResolveWallrunSurfaceTransform(topHit.collider)
                && Vector3.Dot(upperHit.normal, topHit.normal) >= maximumNormalDelta;
            nearTop = !hasStableTopContact;

            normal = (lowerHit.normal + upperHit.normal).normalized;
            bool accepted = Mathf.Abs(Vector3.Dot(normal, Vector3.up)) < 0.25f;
            if (!accepted)
            {
                surfaceTransform = null;
            }
            else
            {
                distance = (lowerHit.distance + upperHit.distance) * 0.5f;
            }

            return accepted;
        }

        private void ResetAirJumps()
        {
            _airJumpsRemaining = Mathf.Max(0, extraAirJumps);
        }

        private static Transform ResolveWallrunSurfaceTransform(Collider collider)
        {
            if (collider == null)
            {
                return null;
            }

            WallrunSurfaceGroup surfaceGroup = collider.GetComponentInParent<WallrunSurfaceGroup>();
            return surfaceGroup == null ? collider.transform : surfaceGroup.transform;
        }

        private bool TryFindWallProbe(Vector3 origin, float radius, Vector3 direction, float castDistance, out RaycastHit hit)
        {
            if (!Physics.SphereCast(origin, radius, direction, out hit, castDistance, collisionMask, QueryTriggerInteraction.Ignore))
            {
                return false;
            }

            return Mathf.Abs(Vector3.Dot(hit.normal, Vector3.up)) < 0.25f;
        }

        private void MoveWithCollision(Vector3 displacement)
        {
            Vector3 remaining = displacement;
            for (int iteration = 0; iteration < 3; iteration++)
            {
                if (remaining.sqrMagnitude <= 0.000001f)
                {
                    break;
                }

                GetCapsulePoints(transform.position, out Vector3 bottom, out Vector3 top);
                bool capsuleWasEnabled = _capsule.enabled;
                _capsule.enabled = false;
                if (Physics.CapsuleCast(bottom, top, _capsule.radius, remaining.normalized, out RaycastHit hit, remaining.magnitude + skinWidth, collisionMask, QueryTriggerInteraction.Ignore))
                {
                    _capsule.enabled = capsuleWasEnabled;
                    Vector3 velocityBeforeHit = _velocity;
                    float moveDistance = Mathf.Max(0f, hit.distance - skinWidth);
                    transform.position += remaining.normalized * moveDistance;
                    Vector3 unconsumed = remaining - remaining.normalized * moveDistance;
                    bool floorContact = hit.normal.y > MinimumGroundNormalY && remaining.y <= 0f;
                    if (floorContact && Vector3.ProjectOnPlane(velocityBeforeHit, Vector3.up).sqrMagnitude <= 0.0001f)
                    {
                        remaining = Vector3.zero;
                        _velocity = Vector3.zero;
                    }
                    else
                    {
                        remaining = Vector3.ProjectOnPlane(unconsumed, hit.normal);
                        _velocity = floorContact ? Vector3.ProjectOnPlane(velocityBeforeHit, Vector3.up) : Vector3.ProjectOnPlane(velocityBeforeHit, hit.normal);
                    }
                }
                else
                {
                    _capsule.enabled = capsuleWasEnabled;
                    transform.position += remaining;
                    break;
                }
            }
        }

        private void GetCapsulePoints(Vector3 position, out Vector3 bottom, out Vector3 top)
        {
            float halfHeight = Mathf.Max(_capsule.height * 0.5f, _capsule.radius);
            Vector3 center = position + _capsule.center;
            bottom = center + Vector3.down * (halfHeight - _capsule.radius);
            top = center + Vector3.up * (halfHeight - _capsule.radius);
        }
    }
}
