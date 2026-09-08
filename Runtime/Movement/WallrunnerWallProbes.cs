using UnityEngine;

namespace Deucarian.TemplateGameMovementFps.Movement
{
    internal sealed class WallrunnerWallProbes
    {
        private readonly WallrunnerSettings _settings;
        private readonly WallrunnerState _state;
        private readonly IWallrunnerEnvironment _environment;
        private readonly WallrunnerDirections _directions;

        internal WallrunnerWallProbes(WallrunnerSettings settings, WallrunnerState state, IWallrunnerEnvironment environment, WallrunnerDirections directions)
        {
            _settings = settings;
            _state = state;
            _environment = environment;
            _directions = directions;
        }

        internal bool TryFindWallrunCandidate(Vector3 wishDirection, out Vector3 normal, out object surfaceTransform, out WallrunTraversalStyle style, out Vector3 runDirection)
        {
            normal = Vector3.zero;
            surfaceTransform = null;
            style = WallrunTraversalStyle.Horizontal;
            runDirection = Vector3.zero;

            if (_state.State == WallrunnerMovementState.Wallrunning)
            {
                return _state.WallrunStyle == WallrunTraversalStyle.Vertical
                    ? TryFindVerticalWallrunCandidate(wishDirection, out normal, out surfaceTransform, out style, out runDirection)
                    : TryFindHorizontalWallrunCandidate(wishDirection, out normal, out surfaceTransform, out style, out runDirection);
            }

            if (TryFindVerticalWallrunCandidate(wishDirection, out normal, out surfaceTransform, out style, out runDirection))
            {
                return true;
            }

            return TryFindHorizontalWallrunCandidate(wishDirection, out normal, out surfaceTransform, out style, out runDirection);
        }

        private bool TryFindHorizontalWallrunCandidate(Vector3 wishDirection, out Vector3 normal, out object surfaceTransform, out WallrunTraversalStyle style, out Vector3 runDirection)
        {
            normal = Vector3.zero;
            surfaceTransform = null;
            style = WallrunTraversalStyle.Horizontal;
            runDirection = Vector3.zero;
            Vector3 directionReference = _state.State == WallrunnerMovementState.Wallrunning
                ? _directions.ResolveActiveHorizontalWallrunReference(wishDirection)
                : wishDirection;
            if (!FindWall(directionReference, out normal, out surfaceTransform))
            {
                return false;
            }

            style = WallrunTraversalStyle.Horizontal;
            runDirection = _state.State == WallrunnerMovementState.Wallrunning
                ? _directions.ResolveContinuingHorizontalWallrunDirection(normal, directionReference)
                : _directions.ResolveWallrunAlongWall(normal, directionReference);
            return true;
        }

        internal bool ShouldResetActiveWallrunForCandidate(WallrunTraversalStyle candidateStyle, Vector3 candidateNormal, object candidateSurface)
        {
            if (_state.State != WallrunnerMovementState.Wallrunning)
            {
                return false;
            }

            if (_state.WallrunStyle != candidateStyle)
            {
                return true;
            }

            if (!_environment.IsSameSurface(_state.WallrunSurfaceTransform, null) && !_environment.IsSameSurface(candidateSurface, _state.WallrunSurfaceTransform))
            {
                return true;
            }

            return _directions.IsWallrunSurfaceTurnTooSharp(candidateNormal);
        }

        private bool TryFindVerticalWallrunCandidate(Vector3 wishDirection, out Vector3 normal, out object surfaceTransform, out WallrunTraversalStyle style, out Vector3 runDirection)
        {
            normal = Vector3.zero;
            surfaceTransform = null;
            style = WallrunTraversalStyle.Horizontal;
            runDirection = Vector3.zero;
            if (!_settings.VerticalWallrunEnabled)
            {
                return false;
            }

            Vector3 fullLookDirection = _directions.BuildFullLookDirection();
            Vector3 lookDirection = _directions.BuildLookDirection();
            object requiredSurface = _state.State == WallrunnerMovementState.Wallrunning ? _state.WallrunSurfaceTransform : null;

            bool foundProbe = TryAcceptWallProbe(lookDirection, requiredSurface, out WallProbeResult probe);

            bool requiresUpwardLook = _state.State != WallrunnerMovementState.Wallrunning || _state.WallrunStyle != WallrunTraversalStyle.Vertical;
            if (!foundProbe || !IsVerticalWallrunIntent(probe.Normal, wishDirection, lookDirection, fullLookDirection, requiresUpwardLook))
            {
                return false;
            }

            if (_state.State != WallrunnerMovementState.Wallrunning)
            {
                float lateralSpeed = Vector3.ProjectOnPlane(_state.Velocity, Vector3.up).magnitude;
                if (lateralSpeed < _settings.SprintSpeed * _settings.VerticalWallrunMinimumEntrySpeedFraction)
                {
                    return false;
                }
            }

            normal = probe.Normal;
            surfaceTransform = probe.SurfaceTransform;
            _state.WallrunNearTop = probe.NearTop;
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
            float minimumLookDot = Mathf.Cos(_settings.VerticalWallrunLookMaxAngleDegrees * Mathf.Deg2Rad);
            float minimumMoveDot = Mathf.Cos(_settings.VerticalWallrunMoveMaxAngleDegrees * Mathf.Deg2Rad);
            if (lookDot < minimumLookDot || moveDot < minimumMoveDot)
            {
                return false;
            }

            return !requiresUpwardLook
                || fullLookDirection.y >= Mathf.Sin(_settings.VerticalWallrunMinimumLookUpAngleDegrees * Mathf.Deg2Rad);
        }

        private bool FindWall(Vector3 wishDirection, out Vector3 normal, out object surfaceTransform)
        {
            surfaceTransform = null;

            object requiredSurface = _state.State == WallrunnerMovementState.Wallrunning ? _state.WallrunSurfaceTransform : null;
            WallProbeResult bestProbe = default;
            float bestScore = float.NegativeInfinity;
            bool foundProbe = false;
            bool activeHorizontalWallrun = _state.State == WallrunnerMovementState.Wallrunning
                && _state.WallrunStyle == WallrunTraversalStyle.Horizontal
                && !_environment.IsSameSurface(requiredSurface, null);
            if (activeHorizontalWallrun)
            {
                Vector3 inwardProbe = _directions.BuildActiveHorizontalWallrunProbeDirection();
                Vector3 activeDirection = _directions.ResolveActiveHorizontalWallrunReference(wishDirection);
                float activeCurvedProbeBias = Mathf.Clamp01(_settings.WallrunCurvedSurfaceProbeForwardBias);
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

            if (_state.State == WallrunnerMovementState.Wallrunning
                && _state.WallNormal.sqrMagnitude > 0.0001f
                && TryAcceptWallProbe(-_state.WallNormal, requiredSurface, out WallProbeResult continuityProbe))
            {
                foundProbe |= TryUseWallProbe(continuityProbe, wishDirection, 4f, ref bestProbe, ref bestScore);
            }

            Vector3 right = Vector3.ProjectOnPlane(_environment.Right, Vector3.up);
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

            Vector3 forward = activeHorizontalWallrun ? _directions.ResolveActiveHorizontalWallrunReference(wishDirection) : _directions.BuildLookDirection();
            float curvedProbeBias = Mathf.Clamp01(_settings.WallrunCurvedSurfaceProbeForwardBias);
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
                _state.WallrunNearTop = bestProbe.NearTop;
                normal = bestProbe.Normal;
                surfaceTransform = bestProbe.SurfaceTransform;

                return true;
            }

            normal = Vector3.zero;
            _state.WallrunNearTop = false;
            return false;
        }

        private bool TryUseWallProbe(WallProbeResult probe, Vector3 wishDirection, float bonus, ref WallProbeResult bestProbe, ref float bestScore)
        {
            if (_state.State == WallrunnerMovementState.Wallrunning && _directions.IsWallrunSurfaceTurnTooSharp(probe.Normal))
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
            Vector3 look = _directions.BuildLookDirection();
            Vector3 intentDirection = hasWish ? wish : look;
            float strafeToward = hasWish ? Vector3.Dot(wish, towardWall) : 0f;
            float lookToward = Vector3.Dot(look, towardWall);
            Vector3 alongWall = _directions.ResolveWallrunAlongWall(probe.Normal, intentDirection);
            float alongIntent = hasWish ? Mathf.Abs(Vector3.Dot(alongWall, wish)) : Mathf.Abs(Vector3.Dot(alongWall, look));
            float castIntent = Vector3.Dot(probe.CastDirection, intentDirection);
            float distanceScore = 1f / (1f + Mathf.Max(0f, probe.Distance));
            return (strafeToward * 2.4f)
                + (lookToward * 1.35f)
                + (alongIntent * 0.65f)
                + (castIntent * 0.35f)
                + (distanceScore * 0.2f);
        }

        private bool TryAcceptWallProbe(Vector3 direction, object requiredSurface, out WallProbeResult probe)
        {
            probe = default;
            if (direction.sqrMagnitude <= 0.0001f)
            {
                return false;
            }

            direction.Normalize();
            if (!TryFindWallOnSide(direction, requiredSurface, out Vector3 normal, out bool nearTop, out object surfaceTransform, out float distance))
            {
                return false;
            }

            probe = new WallProbeResult(normal, nearTop, surfaceTransform, distance, direction);
            return true;
        }

        private bool TryFindWallOnSide(Vector3 direction, object requiredSurface, out Vector3 normal, out bool nearTop, out object surfaceTransform, out float distance)
        {
            normal = Vector3.zero;
            nearTop = false;
            surfaceTransform = null;
            distance = 0f;
            float radius = _environment.CapsuleRadius * 0.72f;
            float castDistance = _environment.CapsuleRadius + 0.28f;
            Vector3 lowerOrigin = _environment.Position + Vector3.up * (_environment.CapsuleHeight * 0.18f);
            Vector3 upperOrigin = _environment.Position + Vector3.up * (_environment.CapsuleHeight * _settings.WallrunUpperProbeHeightFraction);
            if (!TryFindWallProbe(lowerOrigin, radius, direction, castDistance, out WallrunnerHit lowerHit))
            {
                return false;
            }

            surfaceTransform = lowerHit.Surface;
            if (_environment.IsSameSurface(surfaceTransform, null) || (!_environment.IsSameSurface(requiredSurface, null) && !_environment.IsSameSurface(surfaceTransform, requiredSurface)))
            {
                surfaceTransform = null;
                return false;
            }

            bool hasUpperHit = TryFindWallProbe(upperOrigin, radius, direction, castDistance, out WallrunnerHit upperHit);
            if (!hasUpperHit)
            {
                if (_state.State != WallrunnerMovementState.Wallrunning || _environment.IsSameSurface(requiredSurface, null) || _directions.IsWallrunSurfaceTurnTooSharp(lowerHit.Normal))
                {
                    surfaceTransform = null;
                    return false;
                }

                normal = lowerHit.Normal.normalized;
                nearTop = true;
                distance = lowerHit.Distance;
                return true;
            }

            if (!_environment.IsSameSurface(surfaceTransform, upperHit.Surface))
            {
                surfaceTransform = null;
                return false;
            }

            if (_state.State == WallrunnerMovementState.Wallrunning
                && (_directions.IsWallrunSurfaceTurnTooSharp(lowerHit.Normal) || _directions.IsWallrunSurfaceTurnTooSharp(upperHit.Normal)))
            {
                surfaceTransform = null;
                return false;
            }

            float maximumNormalDelta = Mathf.Cos(_settings.WallrunMaximumSurfaceTurnAngle * Mathf.Deg2Rad);
            if (Vector3.Dot(lowerHit.Normal, upperHit.Normal) < maximumNormalDelta)
            {
                surfaceTransform = null;
                return false;
            }

            float topProbeFraction = Mathf.Max(_settings.WallrunTopFlattenProbeHeightFraction, _settings.WallrunUpperProbeHeightFraction + 0.01f);
            Vector3 topOrigin = _environment.Position + Vector3.up * (_environment.CapsuleHeight * topProbeFraction);
            bool hasStableTopContact = TryFindWallProbe(topOrigin, radius, direction, castDistance, out WallrunnerHit topHit)
                && _environment.IsSameSurface(surfaceTransform, topHit.Surface)
                && Vector3.Dot(upperHit.Normal, topHit.Normal) >= maximumNormalDelta;
            nearTop = !hasStableTopContact;

            normal = (lowerHit.Normal + upperHit.Normal).normalized;
            bool accepted = Mathf.Abs(Vector3.Dot(normal, Vector3.up)) < 0.25f;
            if (!accepted)
            {
                surfaceTransform = null;
            }
            else
            {
                distance = (lowerHit.Distance + upperHit.Distance) * 0.5f;
            }

            return accepted;
        }

        private bool TryFindWallProbe(Vector3 origin, float radius, Vector3 direction, float castDistance, out WallrunnerHit hit)
        {
            if (!_environment.SphereCast(origin, radius, direction, out hit, castDistance))
            {
                return false;
            }

            return Mathf.Abs(Vector3.Dot(hit.Normal, Vector3.up)) < 0.25f;
        }
        internal readonly struct WallProbeResult
        {
            public WallProbeResult(Vector3 normal, bool nearTop, object surfaceTransform, float distance, Vector3 castDirection)
            {
                Normal = normal;
                NearTop = nearTop;
                SurfaceTransform = surfaceTransform;
                Distance = distance;
                CastDirection = castDirection;
            }

            public Vector3 Normal { get; }

            public bool NearTop { get; }

            public object SurfaceTransform { get; }

            public float Distance { get; }

            public Vector3 CastDirection { get; }
        }
    }
}
