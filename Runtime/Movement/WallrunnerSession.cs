using UnityEngine;

namespace Deucarian.TemplateGameMovementFps.Movement
{
    internal sealed class WallrunnerSession
    {
        private readonly WallrunnerSettings _settings;
        private readonly IWallrunnerEnvironment _environment;
        private readonly WallrunnerDirections _directions;
        private readonly WallrunnerVelocityPolicy _velocityPolicy;
        private readonly WallrunnerSlidePolicy _slidePolicy;
        private readonly WallrunnerGrounding _grounding;
        private readonly WallrunnerGroundProbes _groundProbes;
        private readonly WallrunnerWallProbes _wallProbes;
        private readonly WallrunnerWallrunPolicy _wallrunPolicy;
        private readonly WallrunnerJumpPolicy _jumpPolicy;
        private readonly WallrunnerVaultPolicy _vaultPolicy;
        private readonly WallrunnerCollisionPolicy _collisionPolicy;
        private readonly WallrunnerState _state;

        public WallrunnerSession(WallrunnerSettings settings, IWallrunnerEnvironment environment)
        {
            _settings = settings ?? throw new System.ArgumentNullException(nameof(settings));
            _environment = environment ?? throw new System.ArgumentNullException(nameof(environment));
            _state = new WallrunnerState(_settings);
            _directions = new WallrunnerDirections(_settings, _state, _environment);
            _velocityPolicy = new WallrunnerVelocityPolicy(_settings, _state, _environment, _directions);
            _slidePolicy = new WallrunnerSlidePolicy(_settings, _state, _environment);
            _groundProbes = new WallrunnerGroundProbes(_settings, _environment);
            _grounding = new WallrunnerGrounding(_settings, _state, _environment, _groundProbes);
            _wallProbes = new WallrunnerWallProbes(_settings, _state, _environment, _directions);
            _wallrunPolicy = new WallrunnerWallrunPolicy(_settings, _state, _directions, _wallProbes);
            _jumpPolicy = new WallrunnerJumpPolicy(_settings, _state, _environment, _slidePolicy, _velocityPolicy, _wallrunPolicy);
            _vaultPolicy = new WallrunnerVaultPolicy(_settings, _state, _environment, _directions, _velocityPolicy);
            _collisionPolicy = new WallrunnerCollisionPolicy(_settings, _state, _environment);
            _state.ResetAirJumps();
        }

        public WallrunnerMovementState State => _state.State;

        public Vector3 Velocity => _state.Velocity;

        public bool IsGrounded => _state.Grounded;

        public int AirJumpsRemaining => _state.AirJumpsRemaining;

        public WallrunTraversalStyle ActiveWallrunStyle => State == WallrunnerMovementState.Wallrunning ? _state.WallrunStyle : WallrunTraversalStyle.Horizontal;

        public Vector3 ActiveWallrunNormal => State == WallrunnerMovementState.Wallrunning ? _state.WallNormal : Vector3.zero;

        public VaultTraversalStyle ActiveVaultStyle => _state.ActiveVaultStyle;

        public VaultTraversalStyle LastVaultStyle => _state.LastVaultStyle;

        internal void ClearDetachDelay() => _state.WallrunDetachReattachTimer = 0f;

        public void Reset(Vector3 position)
        {
            _environment.Position = position;
            _state.Velocity = Vector3.zero;
            _state.ResetAirJumps();
            _state.SlideTimer = 0f;
            _state.SlideEntrySpeed = 0f;
            _state.WallrunTimer = 0f;
            _state.WallrunStartSpeed = 0f;
            _state.WallrunPeakSpeed = 0f;
            _state.WallrunSameWallLockoutTimer = 0f;
            _state.WallrunDetachReattachTimer = 0f;
            _state.WallrunLockedNormal = Vector3.zero;
            _state.WallrunLockedDirection = Vector3.zero;
            _state.WallrunActiveDirection = Vector3.zero;
            _state.WallrunGuidanceDirection = Vector3.zero;
            _state.WallrunBlockedNormal = Vector3.zero;
            _state.WallrunChainLockedNormal = Vector3.zero;
            _state.WallrunChainLockedDirection = Vector3.zero;
            _state.WallrunSurfaceTransform = null;
            _state.WallrunChainSameDirectionLocked = false;
            _state.WallrunNearTop = false;
            _state.WallrunStyle = WallrunTraversalStyle.Horizontal;
            _state.WallrunFatigued = false;
            _state.GroundSnapLockoutTimer = 0f;
            _state.GroundedJumpGraceTimer = _settings.GroundedJumpGraceSeconds;
            _state.SlideJumpGraceTimer = 0f;
            _state.AirborneSlideBufferTimer = 0f;
            _state.BunnyHopWindowTimer = 0f;
            _state.BunnyHopCarryVelocity = Vector3.zero;
            _state.VaultBlendTimer = 0f;
            _state.VaultBlendDuration = 0f;
            _state.VaultStartPosition = position;
            _state.VaultTargetPosition = position;
            _state.VaultExitVelocity = Vector3.zero;
            _state.ActiveVaultStyle = VaultTraversalStyle.None;
            _state.LastVaultStyle = VaultTraversalStyle.None;
            _state.State = WallrunnerMovementState.Grounded;
        }

        public void SetVelocity(Vector3 velocity)
        {
            _state.Velocity = velocity;
        }

        public void SetLookDirection(Vector3 lookDirection)
        {
            if (lookDirection.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            _state.LookForward = lookDirection.normalized;
        }

        public void SetWallrunGuidanceDirection(Vector3 direction)
        {
            Vector3 lateralDirection = Vector3.ProjectOnPlane(direction, Vector3.up);
            _state.WallrunGuidanceDirection = lateralDirection.sqrMagnitude <= 0.0001f ? Vector3.zero : lateralDirection.normalized;
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
            if (_state.VaultBlendTimer > 0f)
            {
                _vaultPolicy.TickVaultBlend(deltaTime);
                return;
            }

            if (_state.GroundSnapLockoutTimer > 0f)
            {
                _state.GroundSnapLockoutTimer = Mathf.Max(0f, _state.GroundSnapLockoutTimer - deltaTime);
            }

            if (_state.AirborneSlideBufferTimer > 0f)
            {
                _state.AirborneSlideBufferTimer = Mathf.Max(0f, _state.AirborneSlideBufferTimer - deltaTime);
            }

            if (_state.WallrunSameWallLockoutTimer > 0f)
            {
                _state.WallrunSameWallLockoutTimer = Mathf.Max(0f, _state.WallrunSameWallLockoutTimer - deltaTime);
            }

            if (!_settings.WallrunDetachReattachDelayEnabled)
            {
                _state.WallrunDetachReattachTimer = 0f;
            }
            else if (_state.WallrunDetachReattachTimer > 0f)
            {
                _state.WallrunDetachReattachTimer = Mathf.Max(0f, _state.WallrunDetachReattachTimer - deltaTime);
            }

            bool canSnapToGround = _state.GroundSnapLockoutTimer <= 0f;
            bool wasGrounded = _state.Grounded;
            bool wasSliding = _state.State == WallrunnerMovementState.Sliding;
            Vector3 preProbeGroundNormal = _state.GroundNormal;
            Vector3 preGroundLateralVelocity = Vector3.ProjectOnPlane(_state.Velocity, Vector3.up);
            _state.Grounded = canSnapToGround && _grounding.CheckGrounded();
            if (canSnapToGround && !jumpPressed && !_state.Grounded)
            {
                _state.Grounded = wasSliding
                    ? _grounding.TrySnapToSlideGround(preProbeGroundNormal, deltaTime)
                    : wasGrounded && _grounding.TrySnapToGround(preProbeGroundNormal, deltaTime);
            }

            if (_state.Grounded)
            {
                bool landedThisFrame = !wasGrounded;
                if (landedThisFrame)
                {
                    _state.BunnyHopWindowTimer = _settings.BunnyHopLandingWindowSeconds;
                    _state.BunnyHopCarryVelocity = preGroundLateralVelocity;
                }
                else
                {
                    _state.BunnyHopWindowTimer = Mathf.Max(0f, _state.BunnyHopWindowTimer - deltaTime);
                    if (_state.BunnyHopWindowTimer <= 0f)
                    {
                        _state.BunnyHopCarryVelocity = Vector3.zero;
                    }
                }

                _state.GroundedJumpGraceTimer = _settings.GroundedJumpGraceSeconds;
                if (_state.State == WallrunnerMovementState.Sliding)
                {
                    _state.SlideJumpGraceTimer = _settings.GroundedJumpGraceSeconds;
                }

                if (_state.Velocity.y <= 0f)
                {
                    if (_state.State == WallrunnerMovementState.Sliding)
                    {
                        _state.Velocity = _grounding.BuildGroundedVelocity(Vector3.ProjectOnPlane(_state.Velocity, Vector3.up), 0f);
                    }
                    else
                    {
                        _state.Velocity.y = 0f;
                    }
                }

                _state.ResetAirJumps();
                _state.WallrunTimer = 0f;
                _state.WallrunStartSpeed = 0f;
                _state.WallrunPeakSpeed = 0f;
                _state.WallrunSameWallLockoutTimer = 0f;
                _state.WallrunDetachReattachTimer = 0f;
                _state.WallrunLockedNormal = Vector3.zero;
                _state.WallrunLockedDirection = Vector3.zero;
                _state.WallrunActiveDirection = Vector3.zero;
                _state.WallrunGuidanceDirection = Vector3.zero;
                _state.WallrunBlockedNormal = Vector3.zero;
                _state.WallrunSurfaceTransform = null;
                _state.ClearWallrunChainLock();
                _state.WallrunNearTop = false;
                _state.WallrunStyle = WallrunTraversalStyle.Horizontal;
                _state.WallrunFatigued = false;
            }
            else
            {
                _state.GroundedJumpGraceTimer = Mathf.Max(0f, _state.GroundedJumpGraceTimer - deltaTime);
                if (_state.GroundedJumpGraceTimer <= 0f)
                {
                    _state.BunnyHopWindowTimer = 0f;
                    _state.BunnyHopCarryVelocity = Vector3.zero;
                }

                _state.SlideJumpGraceTimer = Mathf.Max(0f, _state.SlideJumpGraceTimer - deltaTime);
            }

            Vector3 wishDirection = _directions.BuildWishDirection(moveInput);
            if (!_state.Grounded && (slidePressed || slideHeld))
            {
                _state.AirborneSlideBufferTimer = _settings.AirborneSlideBufferSeconds;
            }

            _slidePolicy.TryStartSlide(slidePressed || _state.AirborneSlideBufferTimer > 0f, wishDirection);
            bool startedGrounded = _state.Grounded;

            if (jumpPressed && _state.State == WallrunnerMovementState.Wallrunning)
            {
                _jumpPolicy.TryJump(wishDirection);
            }
            else
            {
                bool vaultRequested = _settings.VaultHoldAssistEnabled ? jumpHeld : jumpPressed;
                if (vaultRequested && _vaultPolicy.TryVault(wishDirection))
                {
                    return;
                }

                if (jumpPressed)
                {
                    _jumpPolicy.TryJump(wishDirection);
                }
            }

            bool wallrunning = !_state.Grounded && _wallrunPolicy.TryResolveWallrun(wishDirection, jumpPressed, deltaTime);
            if (!wallrunning && !_state.Grounded)
            {
                _state.Velocity.y -= _settings.Gravity * deltaTime;
            }

            float targetSpeed = ResolveTargetSpeed(sprintHeld, wallrunning);
            float acceleration = _state.Grounded ? _settings.GroundAcceleration : _settings.AirAcceleration;
            Vector3 lateralVelocity = Vector3.ProjectOnPlane(_state.Velocity, Vector3.up);
            float verticalVelocity = _state.Velocity.y;
            Vector3 desiredLateral = wishDirection * targetSpeed;
            if (wallrunning)
            {
                desiredLateral = lateralVelocity;
                acceleration = 0f;
            }
            else if (_state.State == WallrunnerMovementState.Sliding)
            {
                _state.SlideTimer -= deltaTime;
                Vector3 slideDirection = lateralVelocity.sqrMagnitude > 0.1f ? lateralVelocity.normalized : wishDirection.normalized;
                if (!_state.Grounded)
                {
                    desiredLateral = lateralVelocity;
                }
                else
                {
                    float slideSpeedAfterDrag = _slidePolicy.ApplySlideDrag(lateralVelocity.magnitude, deltaTime);
                    slideSpeedAfterDrag = _slidePolicy.ResolveSlopeBoostedSlideSpeed(slideDirection, slideSpeedAfterDrag, deltaTime);
                    desiredLateral = slideDirection.sqrMagnitude > 0.1f ? slideDirection.normalized * slideSpeedAfterDrag : Vector3.zero;
                    lateralVelocity = desiredLateral;
                    if (!_grounding.HasSlideSupportAhead(lateralVelocity, deltaTime))
                    {
                        verticalVelocity = _grounding.ReleaseGroundAtEdge(verticalVelocity, deltaTime);
                    }
                    else
                    {
                        verticalVelocity = _grounding.ResolveGroundFollowerVerticalVelocity(lateralVelocity, verticalVelocity);
                    }
                }

                acceleration = 0f;
                if (_state.SlideTimer <= 0f || !_state.Grounded)
                {
                    _state.State = _state.Grounded ? WallrunnerMovementState.Grounded : WallrunnerMovementState.Airborne;
                }
            }
            else if (!_state.Grounded)
            {
                if (!(_settings.WallrunDetachReattachDelayEnabled && _state.WallrunDetachReattachTimer > 0f))
                {
                    lateralVelocity = _velocityPolicy.ApplyMomentumPreservingAirControl(lateralVelocity, wishDirection, targetSpeed, deltaTime);
                }

                desiredLateral = lateralVelocity;
                acceleration = 0f;
            }

            lateralVelocity = Vector3.MoveTowards(lateralVelocity, desiredLateral, acceleration * deltaTime);
            if (_state.Grounded && !wallrunning && _state.State != WallrunnerMovementState.Sliding)
            {
                if (!_grounding.HasGroundSupportAhead(lateralVelocity, deltaTime, _settings.GroundSnapReleaseDropAngleDegrees, _settings.GroundSnapBreakAngleDegrees))
                {
                    verticalVelocity = _grounding.ReleaseGroundAtEdge(verticalVelocity, deltaTime);
                }
                else
                {
                    verticalVelocity = _grounding.ResolveGroundFollowerVerticalVelocity(lateralVelocity, verticalVelocity);
                }
            }

            _state.Velocity = lateralVelocity + (Vector3.up * verticalVelocity);
            _velocityPolicy.ApplyMaxVelocity(deltaTime);
            _collisionPolicy.MoveWithCollision(_state.Velocity * deltaTime);
            _grounding.ResolvePostMoveGrounding(jumpPressed, canSnapToGround && startedGrounded && !wallrunning, deltaTime);

            if (!wallrunning && _state.State != WallrunnerMovementState.Sliding)
            {
                _state.State = _state.Grounded ? sprintHeld && moveInput.y > 0.1f ? WallrunnerMovementState.Sprinting : WallrunnerMovementState.Grounded : WallrunnerMovementState.Airborne;
            }
        }

        internal float ResolveTargetSpeed(bool sprintHeld, bool wallrunning)
        {
            if (wallrunning)
            {
                return _settings.WallrunSpeed;
            }

            if (_state.State == WallrunnerMovementState.Sliding)
            {
                return _settings.SlideSpeed;
            }

            return sprintHeld ? _settings.SprintSpeed : _settings.WalkSpeed;
        }
        public WallrunnerSnapshot CaptureSnapshot()
        {
            return new WallrunnerSnapshot(
                _environment.Position,
                _environment.Rotation,
                _state.Velocity,
                _state.WallNormal,
                _state.GroundNormal,
                _state.WallrunLockedNormal,
                _state.WallrunLockedDirection,
                _state.WallrunActiveDirection,
                _state.WallrunGuidanceDirection,
                _state.WallrunBlockedNormal,
                _state.WallrunChainLockedNormal,
                _state.WallrunChainLockedDirection,
                _state.WallrunSurfaceTransform,
                _state.BunnyHopCarryVelocity,
                _state.VaultStartPosition,
                _state.VaultTargetPosition,
                _state.VaultExitVelocity,
                _state.LookForward,
                _state.SlideTimer,
                _state.WallrunTimer,
                _state.WallrunStartSpeed,
                _state.WallrunPeakSpeed,
                _state.WallrunSameWallLockoutTimer,
                _state.WallrunDetachReattachTimer,
                _state.SlideEntrySpeed,
                _state.GroundSnapLockoutTimer,
                _state.GroundedJumpGraceTimer,
                _state.SlideJumpGraceTimer,
                _state.AirborneSlideBufferTimer,
                _state.BunnyHopWindowTimer,
                _state.VaultBlendTimer,
                _state.VaultBlendDuration,
                _state.AirJumpsRemaining,
                _state.Grounded,
                _state.WallrunFatigued,
                _state.WallrunChainSameDirectionLocked,
                _state.WallrunNearTop,
                _state.WallrunStyle,
                _state.ActiveVaultStyle,
                _state.LastVaultStyle,
                _state.State);
        }

        public void RestoreSnapshot(WallrunnerSnapshot snapshot)
        {
            _environment.SetPositionAndRotation(snapshot.Position, snapshot.Rotation);
            _state.Velocity = snapshot.Velocity;
            _state.WallNormal = snapshot.WallNormal;
            _state.GroundNormal = snapshot.GroundNormal.sqrMagnitude <= 0.0001f ? Vector3.up : snapshot.GroundNormal;
            _state.WallrunLockedNormal = snapshot.WallrunLockedNormal;
            _state.WallrunLockedDirection = snapshot.WallrunLockedDirection;
            _state.WallrunActiveDirection = snapshot.WallrunActiveDirection;
            _state.WallrunGuidanceDirection = snapshot.WallrunGuidanceDirection;
            _state.WallrunBlockedNormal = snapshot.WallrunBlockedNormal;
            _state.WallrunChainLockedNormal = snapshot.WallrunChainLockedNormal;
            _state.WallrunChainLockedDirection = snapshot.WallrunChainLockedDirection;
            _state.WallrunSurfaceTransform = snapshot.WallrunSurfaceTransform;
            _state.BunnyHopCarryVelocity = snapshot.BunnyHopCarryVelocity;
            _state.VaultStartPosition = snapshot.VaultStartPosition;
            _state.VaultTargetPosition = snapshot.VaultTargetPosition;
            _state.VaultExitVelocity = snapshot.VaultExitVelocity;
            _state.LookForward = snapshot.LookForward;
            _state.SlideTimer = snapshot.SlideTimer;
            _state.WallrunTimer = snapshot.WallrunTimer;
            _state.WallrunStartSpeed = snapshot.WallrunStartSpeed;
            _state.WallrunPeakSpeed = snapshot.WallrunPeakSpeed;
            _state.WallrunSameWallLockoutTimer = snapshot.WallrunSameWallLockoutTimer;
            _state.WallrunDetachReattachTimer = snapshot.WallrunDetachReattachTimer;
            _state.SlideEntrySpeed = snapshot.SlideEntrySpeed;
            _state.GroundSnapLockoutTimer = snapshot.GroundSnapLockoutTimer;
            _state.GroundedJumpGraceTimer = snapshot.GroundedJumpGraceTimer;
            _state.SlideJumpGraceTimer = snapshot.SlideJumpGraceTimer;
            _state.AirborneSlideBufferTimer = snapshot.AirborneSlideBufferTimer;
            _state.BunnyHopWindowTimer = snapshot.BunnyHopWindowTimer;
            _state.VaultBlendTimer = snapshot.VaultBlendTimer;
            _state.VaultBlendDuration = snapshot.VaultBlendDuration;
            _state.AirJumpsRemaining = snapshot.AirJumpsRemaining;
            _state.Grounded = snapshot.Grounded;
            _state.WallrunFatigued = snapshot.WallrunFatigued;
            _state.WallrunChainSameDirectionLocked = snapshot.WallrunChainSameDirectionLocked;
            _state.WallrunNearTop = snapshot.WallrunNearTop;
            _state.WallrunStyle = snapshot.WallrunStyle;
            _state.ActiveVaultStyle = snapshot.ActiveVaultStyle;
            _state.LastVaultStyle = snapshot.LastVaultStyle;
            _state.State = snapshot.State;
            _environment.SynchronizePose();
        }
    }
}
