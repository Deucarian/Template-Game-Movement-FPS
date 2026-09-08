using UnityEngine;

namespace Deucarian.TemplateGameMovementFps.Movement
{
    [RequireComponent(typeof(CapsuleCollider))]
    public sealed partial class WallrunnerMotor : MonoBehaviour
    {
        private WallrunnerSettings _settings;
        private WallrunnerSession _session;

        private WallrunnerSession Session
        {
            get
            {
                if (_session == null)
                {
                    _settings = new WallrunnerSettings();
                    CopySettings();
                    _session = new WallrunnerSession(_settings,
                        new UnityWallrunnerEnvironment(transform, GetComponent<CapsuleCollider>(), () => collisionMask));
                }

                return _session;
            }
        }

        private void Awake() => _ = Session;

        public WallrunnerMovementState State => Session.State;

        public Vector3 Velocity => Session.Velocity;

        public bool IsGrounded => Session.IsGrounded;

        public int AirJumpsRemaining => Session.AirJumpsRemaining;

        public WallrunTraversalStyle ActiveWallrunStyle => Session.ActiveWallrunStyle;

        public Vector3 ActiveWallrunNormal => Session.ActiveWallrunNormal;

        public VaultTraversalStyle ActiveVaultStyle => Session.ActiveVaultStyle;

        public VaultTraversalStyle LastVaultStyle => Session.LastVaultStyle;

        public void SetVelocity(Vector3 velocity)
        {
            WallrunnerSession session = Session;
            CopySettings();
            session.SetVelocity(velocity);
        }

        public void SetLookDirection(Vector3 lookDirection)
        {
            WallrunnerSession session = Session;
            CopySettings();
            session.SetLookDirection(lookDirection);
        }

        public void SetWallrunGuidanceDirection(Vector3 direction)
        {
            WallrunnerSession session = Session;
            CopySettings();
            session.SetWallrunGuidanceDirection(direction);
        }

        public void ResetMotor(Vector3 position)
        {
            WallrunnerSession session = Session;
            CopySettings();
            session.Reset(position);
        }

        public void Tick(Vector2 moveInput, bool sprintHeld, bool slidePressed, bool jumpPressed, float deltaTime)
        {
            WallrunnerSession session = Session;
            CopySettings();
            session.Tick(moveInput, sprintHeld, slidePressed, jumpPressed, deltaTime);
        }

        public void Tick(Vector2 moveInput, bool sprintHeld, bool slidePressed, bool slideHeld, bool jumpPressed, float deltaTime)
        {
            WallrunnerSession session = Session;
            CopySettings();
            session.Tick(moveInput, sprintHeld, slidePressed, slideHeld, jumpPressed, deltaTime);
        }

        public void Tick(Vector2 moveInput, bool sprintHeld, bool slidePressed, bool slideHeld, bool jumpPressed, bool jumpHeld, float deltaTime)
        {
            WallrunnerSession session = Session;
            CopySettings();
            session.Tick(moveInput, sprintHeld, slidePressed, slideHeld, jumpPressed, jumpHeld, deltaTime);
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
    }
}
