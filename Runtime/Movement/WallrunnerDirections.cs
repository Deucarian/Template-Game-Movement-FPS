using UnityEngine;

namespace Deucarian.TemplateGameMovementFps.Movement
{
    internal sealed class WallrunnerDirections
    {
        private readonly WallrunnerSettings _settings;
        private readonly WallrunnerState _state;
        private readonly IWallrunnerEnvironment _environment;

        internal WallrunnerDirections(WallrunnerSettings settings, WallrunnerState state, IWallrunnerEnvironment environment)
        {
            _settings = settings;
            _state = state;
            _environment = environment;
        }

        internal Vector3 BuildWishDirection(Vector2 moveInput)
        {
            Vector3 forward = Vector3.ProjectOnPlane(_environment.Forward, Vector3.up).normalized;
            Vector3 right = Vector3.ProjectOnPlane(_environment.Right, Vector3.up).normalized;
            Vector3 wish = (forward * moveInput.y) + (right * moveInput.x);
            return wish.sqrMagnitude > 1f ? wish.normalized : wish;
        }

        internal Vector3 BuildWallJumpLaunchDirection(Vector3 lateralVelocity, out float forwardPreserveRatio)
        {
            Vector3 awayFromWall = BuildWallAwayDirection();
            Vector3 lookDirection = BuildLookDirection();
            Vector3 carriedDirection = lateralVelocity.sqrMagnitude > 0.01f ? lateralVelocity.normalized : lookDirection;
            float lookCarryDot = Vector3.Dot(lookDirection, carriedDirection);
            forwardPreserveRatio = Mathf.Clamp01(Mathf.InverseLerp(0.05f, 0.65f, lookCarryDot));

            Vector3 launchDirection =
                lookDirection * _settings.WallJumpLookWeight +
                awayFromWall * _settings.WallJumpAwayWeight +
                carriedDirection * (_settings.WallJumpCarryWeight * forwardPreserveRatio);

            if (launchDirection.sqrMagnitude <= 0.0001f)
            {
                launchDirection = awayFromWall;
            }

            launchDirection.Normalize();
            float awayDot = Vector3.Dot(launchDirection, awayFromWall);
            if (awayDot < _settings.WallJumpMinimumAwayDot)
            {
                launchDirection = (launchDirection + awayFromWall * (_settings.WallJumpMinimumAwayDot - awayDot + 0.05f)).normalized;
            }

            return launchDirection;
        }

        internal Vector3 ResolveWallrunAlongWall(Vector3 normal, Vector3 directionReference)
        {
            Vector3 alongWall = Vector3.Cross(Vector3.up, normal);
            if (alongWall.sqrMagnitude <= 0.0001f)
            {
                return Vector3.ProjectOnPlane(_environment.Forward, Vector3.up).normalized;
            }

            if (directionReference.sqrMagnitude > 0.01f && Vector3.Dot(alongWall, directionReference) < 0f)
            {
                alongWall = -alongWall;
            }

            return alongWall.normalized;
        }

        internal Vector3 ResolveActiveHorizontalWallrunReference(Vector3 fallback)
        {
            Vector3 reference = Vector3.ProjectOnPlane(_state.WallrunGuidanceDirection, Vector3.up);
            if (reference.sqrMagnitude <= 0.01f)
            {
                reference = Vector3.ProjectOnPlane(_state.WallrunActiveDirection, Vector3.up);
            }

            if (reference.sqrMagnitude <= 0.01f)
            {
                reference = Vector3.ProjectOnPlane(_state.Velocity, Vector3.up);
            }

            if (reference.sqrMagnitude <= 0.01f)
            {
                reference = Vector3.ProjectOnPlane(fallback, Vector3.up);
            }

            if (reference.sqrMagnitude <= 0.01f)
            {
                reference = Vector3.ProjectOnPlane(_environment.Forward, Vector3.up);
            }

            if (reference.sqrMagnitude <= 0.01f)
            {
                return Vector3.forward;
            }

            return reference.normalized;
        }

        internal Vector3 ResolveContinuingHorizontalWallrunDirection(Vector3 normal, Vector3 fallback)
        {
            Vector3 guidedReference = Vector3.ProjectOnPlane(_state.WallrunGuidanceDirection, Vector3.up);
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
            Vector3 activeDirection = Vector3.ProjectOnPlane(_state.WallrunActiveDirection, Vector3.up);
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

        internal Vector3 BuildActiveHorizontalWallrunProbeDirection()
        {
            Vector3 inward = Vector3.ProjectOnPlane(-_state.WallNormal, Vector3.up);
            if (inward.sqrMagnitude <= 0.0001f)
            {
                inward = Vector3.ProjectOnPlane(_environment.Right, Vector3.up);
            }

            if (inward.sqrMagnitude <= 0.0001f)
            {
                return Vector3.right;
            }

            return inward.normalized;
        }

        internal bool IsWallrunContinuation(Vector3 normal, Vector3 referenceNormal)
        {
            if (normal.sqrMagnitude <= 0.0001f || referenceNormal.sqrMagnitude <= 0.0001f)
            {
                return false;
            }

            float continuationDot = Mathf.Cos(_settings.WallrunMaximumSurfaceTurnAngle * Mathf.Deg2Rad);
            return Vector3.Dot(normal.normalized, referenceNormal.normalized) >= continuationDot;
        }

        internal bool IsWallrunSurfaceTurnTooSharp(Vector3 candidateNormal)
        {
            if (_state.WallNormal.sqrMagnitude <= 0.0001f || candidateNormal.sqrMagnitude <= 0.0001f)
            {
                return false;
            }

            return !IsWallrunContinuation(candidateNormal, _state.WallNormal);
        }

        private Vector3 BuildWallAwayDirection()
        {
            Vector3 away = Vector3.ProjectOnPlane(_state.WallNormal, Vector3.up);
            if (away.sqrMagnitude <= 0.0001f)
            {
                away = -_environment.Forward;
            }

            return away.normalized;
        }

        internal Vector3 BuildLookDirection()
        {
            Vector3 lookDirection = Vector3.ProjectOnPlane(BuildFullLookDirection(), Vector3.up);
            if (lookDirection.sqrMagnitude <= 0.0001f)
            {
                lookDirection = Vector3.ProjectOnPlane(_environment.Forward, Vector3.up);
                return lookDirection.sqrMagnitude <= 0.0001f ? Vector3.forward : lookDirection.normalized;
            }

            return lookDirection.normalized;
        }

        internal Vector3 BuildFullLookDirection()
        {
            if (_state.LookForward.sqrMagnitude > 0.0001f)
            {
                return _state.LookForward.normalized;
            }

            return _environment.Forward.sqrMagnitude <= 0.0001f ? Vector3.forward : _environment.Forward.normalized;
        }
    }
}
