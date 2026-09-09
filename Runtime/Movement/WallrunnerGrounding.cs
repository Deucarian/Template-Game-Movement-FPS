using UnityEngine;

namespace Deucarian.TemplateGameMovementFps.Movement
{
    internal sealed class WallrunnerGrounding
    {
        private readonly WallrunnerSettings _settings;
        private readonly WallrunnerState _state;
        private readonly IWallrunnerEnvironment _environment;
        private readonly WallrunnerGroundProbes _groundProbes;

        internal WallrunnerGrounding(WallrunnerSettings settings, WallrunnerState state, IWallrunnerEnvironment environment, WallrunnerGroundProbes groundProbes)
        {
            _settings = settings;
            _state = state;
            _environment = environment;
            _groundProbes = groundProbes;
        }

        internal Vector3 BuildGroundedVelocity(Vector3 lateralVelocity, float fallbackVerticalVelocity)
        {
            return lateralVelocity + Vector3.up * ResolveGroundFollowerVerticalVelocity(lateralVelocity, fallbackVerticalVelocity);
        }

        internal float ResolveGroundFollowerVerticalVelocity(Vector3 lateralVelocity, float fallbackVerticalVelocity)
        {
            if (!_state.Grounded || _state.GroundNormal.y <= _settings.MinimumGroundNormalY)
            {
                return fallbackVerticalVelocity;
            }

            if (lateralVelocity.sqrMagnitude <= 0.0001f)
            {
                return Mathf.Min(fallbackVerticalVelocity, 0f);
            }

            return -Vector3.Dot(lateralVelocity, _state.GroundNormal) / _state.GroundNormal.y;
        }

        private float ResolveDownhillGroundStickiness(Vector3 lateralVelocity, Vector3 groundNormal)
        {
            if (lateralVelocity.sqrMagnitude <= 0.0001f || groundNormal.y <= _settings.MinimumGroundNormalY)
            {
                return 0f;
            }

            Vector3 downhill = Vector3.ProjectOnPlane(Vector3.down, groundNormal);
            if (downhill.sqrMagnitude <= 0.0001f)
            {
                return 0f;
            }

            float slopeAngle = Vector3.Angle(groundNormal, Vector3.up);
            float fadeStartAngle = Mathf.Clamp(_settings.DownhillGroundStickFadeStartAngleDegrees, 1f, 89f);
            float fadeEndAngle = Mathf.Max(fadeStartAngle + 0.01f, _settings.DownhillGroundStickFadeEndAngleDegrees);
            float minimumAngle = Mathf.Min(_settings.DownhillGroundStickMinimumAngleDegrees, fadeStartAngle);
            float angleRamp = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(minimumAngle, minimumAngle + 8f, slopeAngle));
            float highAngleFade = 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(fadeStartAngle, fadeEndAngle, slopeAngle));
            float downhillAlignment = Mathf.Clamp01(Vector3.Dot(lateralVelocity.normalized, downhill.normalized));
            return downhillAlignment * angleRamp * Mathf.Clamp01(highAngleFade);
        }

        private float ResolveGroundReleaseHorizontalReference(float stickiness, float movementReference)
        {
            float defaultReference = Mathf.Max(0.001f, _environment.CapsuleRadius * 0.35f);
            float stickyReference = Mathf.Max(defaultReference, movementReference);
            return Mathf.Lerp(defaultReference, stickyReference, Mathf.Clamp01(stickiness));
        }

        internal float ReleaseGroundAtEdge(float verticalVelocity, float deltaTime)
        {
            _state.Grounded = false;
            _state.GroundNormal = Vector3.up;
            _state.GroundedJumpGraceTimer = Mathf.Max(_state.GroundedJumpGraceTimer, _settings.GroundedJumpGraceSeconds);
            if (_state.State == WallrunnerMovementState.Sliding)
            {
                _state.SlideJumpGraceTimer = Mathf.Max(_state.SlideJumpGraceTimer, _settings.GroundedJumpGraceSeconds);
            }

            float edgeLockout = Mathf.Clamp(deltaTime * 1.5f, 0.025f, 0.06f);
            _state.GroundSnapLockoutTimer = Mathf.Max(_state.GroundSnapLockoutTimer, edgeLockout);
            _state.State = WallrunnerMovementState.Airborne;
            return Mathf.Max(0f, verticalVelocity);
        }

        internal bool HasSlideSupportAhead(Vector3 lateralVelocity, float deltaTime)
        {
            return HasGroundSupportAhead(
                lateralVelocity,
                deltaTime,
                _settings.SlideGroundSnapReleaseDropAngleDegrees,
                _settings.SlideGroundSnapBreakAngleDegrees,
                _environment.CapsuleRadius * 1.35f);
        }

        internal bool HasGroundSupportAhead(Vector3 lateralVelocity, float deltaTime, float releaseDropAngleDegrees, float breakAngleDegrees)
        {
            return HasGroundSupportAhead(
                lateralVelocity,
                deltaTime,
                releaseDropAngleDegrees,
                breakAngleDegrees,
                _environment.CapsuleRadius);
        }

        internal bool HasGroundSupportAhead(Vector3 lateralVelocity, float deltaTime, float releaseDropAngleDegrees, float breakAngleDegrees, float maximumLookAhead)
        {
            if (lateralVelocity.sqrMagnitude <= 0.0001f)
            {
                return true;
            }

            Vector3 direction = lateralVelocity.normalized;
            float stickiness = ResolveDownhillGroundStickiness(lateralVelocity, _state.GroundNormal);
            float minimumLookAhead = _environment.CapsuleRadius * 0.75f;
            float stickyMaximumLookAhead = maximumLookAhead + _settings.DownhillGroundStickExtraLookAhead * stickiness;
            float lookAhead = Mathf.Clamp(lateralVelocity.magnitude * deltaTime, minimumLookAhead, Mathf.Max(minimumLookAhead, stickyMaximumLookAhead));
            Vector3 probePosition = _environment.Position + direction * lookAhead;
            float expectedDrop = Mathf.Max(0f, -ResolveGroundFollowerVerticalVelocity(lateralVelocity, 0f) * deltaTime);
            float probeDistance = _settings.GroundContactProbeDistance + expectedDrop + _settings.SkinWidth + _settings.DownhillGroundStickExtraProbeDistance * stickiness;
            if (!_groundProbes.TryFindGroundContactAtPosition(probePosition, probeDistance, out WallrunnerGroundContact aheadGround))
            {
                return false;
            }

            float releaseHorizontalReference = ResolveGroundReleaseHorizontalReference(stickiness, lookAhead);
            if (ShouldReleaseFromGroundSnap(aheadGround, releaseDropAngleDegrees, releaseHorizontalReference))
            {
                return false;
            }

            return !IsSharpGroundChange(_state.GroundNormal, aheadGround.Normal, breakAngleDegrees);
        }

        internal bool CheckGrounded()
        {
            bool hit = _groundProbes.TryFindGroundContact(_settings.GroundContactProbeDistance, out WallrunnerGroundContact groundHit);
            _state.GroundNormal = hit ? groundHit.Normal : Vector3.up;
            return hit;
        }

        internal void ResolvePostMoveGrounding(bool jumpPressed, bool shouldStickToGround, float deltaTime)
        {
            if (jumpPressed)
            {
                return;
            }

            if (_state.GroundSnapLockoutTimer > 0f)
            {
                _state.Grounded = false;
                if (_state.State == WallrunnerMovementState.Sliding)
                {
                    _state.State = WallrunnerMovementState.Airborne;
                }

                return;
            }

            if (_state.State == WallrunnerMovementState.Sliding)
            {
                ResolvePostMoveSlideGrounding(deltaTime);
                return;
            }

            if (!shouldStickToGround || !_state.Grounded)
            {
                return;
            }

            Vector3 groundReference = _state.GroundNormal;
            _state.Grounded = CheckGrounded();
            if (!_state.Grounded)
            {
                _state.Grounded = TrySnapToGround(groundReference, deltaTime);
            }

            if (_state.Grounded)
            {
                _state.Velocity = BuildGroundedVelocity(Vector3.ProjectOnPlane(_state.Velocity, Vector3.up), _state.Velocity.y);
            }
        }

        private void ResolvePostMoveSlideGrounding(float deltaTime)
        {
            Vector3 groundReference = _state.GroundNormal;
            _state.Grounded = CheckGrounded();
            if (!_state.Grounded)
            {
                _state.Grounded = TrySnapToSlideGround(groundReference, deltaTime);
            }

            if (_state.Grounded)
            {
                _state.Velocity = BuildGroundedVelocity(Vector3.ProjectOnPlane(_state.Velocity, Vector3.up), _state.Velocity.y);
                return;
            }

            _state.State = WallrunnerMovementState.Airborne;
        }

        internal bool TrySnapToGround(Vector3 referenceGroundNormal, float deltaTime)
        {
            return TrySnapToGround(_settings.GroundSnapDistance, referenceGroundNormal, _settings.GroundSnapBreakAngleDegrees, _settings.GroundSnapReleaseDropAngleDegrees, deltaTime);
        }

        internal bool TrySnapToGround(float snapDistance, Vector3 referenceGroundNormal, float breakAngleDegrees, float releaseDropAngleDegrees, float deltaTime)
        {
            Vector3 lateralVelocity = Vector3.ProjectOnPlane(_state.Velocity, Vector3.up);
            float stickiness = ResolveDownhillGroundStickiness(lateralVelocity, referenceGroundNormal);
            float effectiveSnapDistance = snapDistance + _settings.DownhillGroundStickExtraProbeDistance * stickiness;
            if (!_groundProbes.TryFindGround(effectiveSnapDistance, out WallrunnerGroundContact groundHit))
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
                _environment.Position += Vector3.down * groundHit.SnapOffset;
            }

            _state.GroundNormal = groundHit.Normal;
            return true;
        }

        internal bool TrySnapToSlideGround(Vector3 referenceGroundNormal, float deltaTime)
        {
            return TrySnapToGround(_settings.SlideGroundSnapDistance, referenceGroundNormal, _settings.SlideGroundSnapBreakAngleDegrees, _settings.SlideGroundSnapReleaseDropAngleDegrees, deltaTime);
        }

        private bool ShouldReleaseFromGroundSnap(WallrunnerGroundContact groundHit, float releaseDropAngleDegrees)
        {
            return ShouldReleaseFromGroundSnap(groundHit, releaseDropAngleDegrees, _environment.CapsuleRadius * 0.35f);
        }

        private bool ShouldReleaseFromGroundSnap(WallrunnerGroundContact groundHit, float releaseDropAngleDegrees, float horizontalReference)
        {
            if (releaseDropAngleDegrees >= 89f || groundHit.SnapOffset < _settings.GroundSnapReleaseMinimumDrop)
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
    }
}
