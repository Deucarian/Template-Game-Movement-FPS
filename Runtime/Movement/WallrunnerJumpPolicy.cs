using UnityEngine;

namespace Deucarian.TemplateGameMovementFps.Movement
{
    internal sealed class WallrunnerJumpPolicy
    {
        private readonly WallrunnerSettings _settings;
        private readonly WallrunnerState _state;
        private readonly IWallrunnerEnvironment _environment;
        private readonly WallrunnerSlidePolicy _slidePolicy;
        private readonly WallrunnerVelocityPolicy _velocityPolicy;
        private readonly WallrunnerWallrunPolicy _wallrunPolicy;

        internal WallrunnerJumpPolicy(WallrunnerSettings settings, WallrunnerState state, IWallrunnerEnvironment environment, WallrunnerSlidePolicy slidePolicy, WallrunnerVelocityPolicy velocityPolicy, WallrunnerWallrunPolicy wallrunPolicy)
        {
            _settings = settings;
            _state = state;
            _environment = environment;
            _slidePolicy = slidePolicy;
            _velocityPolicy = velocityPolicy;
            _wallrunPolicy = wallrunPolicy;
        }

        internal bool TryJump(Vector3 wishDirection)
        {
            bool slidingJump = _state.State == WallrunnerMovementState.Sliding || _state.SlideJumpGraceTimer > 0f;
            bool canGroundJump = _state.Grounded || _state.GroundedJumpGraceTimer > 0f;
            if (canGroundJump || slidingJump)
            {
                if (slidingJump)
                {
                    float currentSlideSpeed = Vector3.ProjectOnPlane(_state.Velocity, Vector3.up).magnitude;
                    Vector3 boostDirection = wishDirection.sqrMagnitude > 0.1f ? wishDirection : Vector3.ProjectOnPlane(_state.Velocity, Vector3.up);
                    if (boostDirection.sqrMagnitude <= 0.1f)
                    {
                        boostDirection = Vector3.ProjectOnPlane(_environment.Forward, Vector3.up);
                    }

                    float timingScore = _slidePolicy.ResolveSlideJumpTimingScore();
                    float speedMultiplier = Mathf.Lerp(_settings.SlideJumpMinimumSpeedMultiplier, _settings.SlideJumpPeakSpeedMultiplier, timingScore);
                    float baseSpeed = Mathf.Max(currentSlideSpeed, _settings.SprintSpeed * _settings.SlideMinimumEntrySpeedFraction);
                    _velocityPolicy.ApplyVelocityScaledLateralBoost(boostDirection, speedMultiplier, Mathf.Max(_settings.SlideSpeed, baseSpeed) * _settings.SlideJumpSpeedCapMultiplier, baseSpeed);
                    _state.SlideTimer = 0f;
                    _state.SlideJumpGraceTimer = 0f;
                }

                bool bunnyHopJump = _settings.BunnyHopEnabled && !slidingJump && (_state.Grounded || _state.GroundedJumpGraceTimer > 0f) && _state.BunnyHopWindowTimer > 0f;
                if (bunnyHopJump)
                {
                    _velocityPolicy.ApplyBunnyHopLaunch(wishDirection);
                }

                _state.Velocity.y = _settings.JumpVelocity;
                _state.BunnyHopWindowTimer = 0f;
                _state.BunnyHopCarryVelocity = Vector3.zero;
                ForceAirborneAfterJump();
                return true;
            }

            if (_state.State == WallrunnerMovementState.Wallrunning)
            {
                Vector3 lateralVelocity = Vector3.ProjectOnPlane(_state.Velocity, Vector3.up);
                _wallrunPolicy.LockWallrunReentry();
                _wallrunPolicy.LockSameWallSameDirectionUntilAnotherWall();
                _velocityPolicy.ApplyWallJumpLaunch(lateralVelocity, true);
                _state.ResetAirJumps();
                ForceAirborneAfterJump();
                return true;
            }

            if (_state.AirJumpsRemaining > 0)
            {
                _state.AirJumpsRemaining--;
                _state.Velocity.y = _settings.JumpVelocity * 0.94f;
                if (wishDirection.sqrMagnitude > 0.1f)
                {
                    Vector3 lateral = Vector3.ProjectOnPlane(_state.Velocity, Vector3.up);
                    if (lateral.sqrMagnitude > 0.01f)
                    {
                        Vector3 redirected = (lateral.normalized + wishDirection.normalized * 0.45f).normalized * lateral.magnitude;
                        _state.Velocity = redirected + (Vector3.up * _state.Velocity.y);
                    }
                }

                ForceAirborneAfterJump();
                return true;
            }

            return false;
        }

        private void ForceAirborneAfterJump()
        {
            if (_state.State == WallrunnerMovementState.Wallrunning)
            {
                _state.WallrunActiveDirection = Vector3.zero;
                _state.WallrunSurfaceTransform = null;
                _state.WallrunStyle = WallrunTraversalStyle.Horizontal;
            }

            _state.Grounded = false;
            _state.GroundedJumpGraceTimer = 0f;
            _state.BunnyHopWindowTimer = 0f;
            _state.BunnyHopCarryVelocity = Vector3.zero;
            _state.GroundSnapLockoutTimer = Mathf.Max(_state.GroundSnapLockoutTimer, _settings.JumpGroundLockoutSeconds);
            _state.State = WallrunnerMovementState.Airborne;
        }
    }
}
