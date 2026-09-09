using UnityEngine;

namespace Deucarian.TemplateGameMovementFps.Movement
{
    internal sealed class WallrunnerWallrunPolicy
    {
        private readonly WallrunnerSettings _settings;
        private readonly WallrunnerState _state;
        private readonly WallrunnerDirections _directions;
        private readonly WallrunnerWallProbes _wallProbes;

        internal WallrunnerWallrunPolicy(WallrunnerSettings settings, WallrunnerState state, WallrunnerDirections directions, WallrunnerWallProbes wallProbes)
        {
            _settings = settings;
            _state = state;
            _directions = directions;
            _wallProbes = wallProbes;
        }

        internal bool TryResolveWallrun(Vector3 wishDirection, bool jumpPressed, float deltaTime)
        {
            if (jumpPressed)
            {
                return false;
            }

            if (_state.State != WallrunnerMovementState.Wallrunning && (_settings.WallrunDetachReattachDelayEnabled && _state.WallrunDetachReattachTimer > 0f))
            {
                return false;
            }

            if (wishDirection.sqrMagnitude < 0.1f || _state.Velocity.y < -15f)
            {
                return false;
            }

            if (!_wallProbes.TryFindWallrunCandidate(wishDirection, out Vector3 normal, out object wallSurface, out WallrunTraversalStyle wallrunStyle, out Vector3 wallrunDirection))
            {
                if (_state.State == WallrunnerMovementState.Wallrunning)
                {
                    ExitWallrunFromSurfaceLoss();
                }

                return false;
            }

            if (_wallProbes.ShouldResetActiveWallrunForCandidate(wallrunStyle, normal, wallSurface))
            {
                ExitWallrunFromSurfaceLoss(normal);
                return false;
            }

            if (IsSameWallReentryBlocked(normal, wallrunDirection))
            {
                return false;
            }

            bool startsNewWallrun = _state.State != WallrunnerMovementState.Wallrunning || _state.WallrunTimer <= 0f;
            bool clearsSameDirectionChainLock = startsNewWallrun && ShouldClearWallrunChainLock(normal, wallrunDirection);
            if (!startsNewWallrun && wallrunStyle == WallrunTraversalStyle.Horizontal)
            {
                wallrunDirection = _directions.ResolveContinuingHorizontalWallrunDirection(normal, wallrunDirection);
            }

            _state.WallNormal = normal;
            Vector3 currentLateralVelocity = Vector3.ProjectOnPlane(_state.Velocity, Vector3.up);
            float currentSpeed = currentLateralVelocity.magnitude;
            if (startsNewWallrun)
            {
                if (clearsSameDirectionChainLock)
                {
                    _state.ClearWallrunChainLock();
                }

                _state.WallrunTimer = 0f;
                _state.WallrunStartSpeed = currentSpeed;
                _state.WallrunPeakSpeed = Mathf.Max(currentSpeed, _settings.SprintSpeed) * _settings.WallrunApexSpeedMultiplier;
                _state.ResetAirJumps();
                _state.WallrunFatigued = false;
                _state.WallrunSameWallLockoutTimer = 0f;
                _state.WallrunDetachReattachTimer = 0f;
                _state.WallrunLockedNormal = Vector3.zero;
                _state.WallrunLockedDirection = Vector3.zero;
                _state.WallrunActiveDirection = wallrunStyle == WallrunTraversalStyle.Horizontal ? wallrunDirection : Vector3.zero;
                _state.WallrunBlockedNormal = Vector3.zero;
                _state.WallrunSurfaceTransform = wallSurface;
            }

            _state.WallrunStyle = wallrunStyle;
            if (wallrunStyle == WallrunTraversalStyle.Horizontal)
            {
                _state.WallrunActiveDirection = wallrunDirection;
            }

            _state.WallrunTimer += deltaTime;
            float activeWallrunDuration = wallrunStyle == WallrunTraversalStyle.Vertical ? _settings.VerticalWallrunDurationSeconds : _settings.WallrunDurationSeconds;
            if (_state.WallrunTimer > activeWallrunDuration)
            {
                FatigueOutOfWallrun();
                return false;
            }

            if (wallrunStyle == WallrunTraversalStyle.Horizontal && currentSpeed <= _settings.WalkSpeed * 0.45f)
            {
                FatigueOutOfWallrun();
                return false;
            }

            float fatigue = Mathf.Clamp01(_state.WallrunTimer / Mathf.Max(0.01f, activeWallrunDuration));
            _state.Velocity = wallrunStyle == WallrunTraversalStyle.Vertical
                ? BuildVerticalWallrunVelocity(normal, currentLateralVelocity, fatigue)
                : BuildHorizontalWallrunVelocity(wallrunDirection, fatigue);
            _state.State = WallrunnerMovementState.Wallrunning;
            return !jumpPressed;
        }

        private Vector3 BuildHorizontalWallrunVelocity(Vector3 alongWall, float fatigue)
        {
            float lateDecelStart = Mathf.Max(_settings.WallrunApexTime + 0.05f, _settings.WallrunLateDecelStart);
            float profileSpeed = ResolveWallrunProfileSpeed(fatigue, lateDecelStart);
            float lateFatigue = Mathf.InverseLerp(lateDecelStart, 1f, fatigue);
            float endDrop = Mathf.Pow(lateFatigue, _settings.WallrunLateDecelExponent);
            float verticalVelocity = ResolveWallrunVerticalVelocity(fatigue, endDrop);
            if (_state.WallrunNearTop)
            {
                verticalVelocity = 0f;
            }

            Vector3 wallStick = Vector3.ProjectOnPlane(-_state.WallNormal, Vector3.up);
            wallStick = wallStick.sqrMagnitude <= 0.0001f ? Vector3.zero : wallStick.normalized * _settings.WallrunHorizontalWallStickSpeed;
            return alongWall.normalized * profileSpeed + wallStick + Vector3.up * verticalVelocity;
        }

        private Vector3 BuildVerticalWallrunVelocity(Vector3 normal, Vector3 currentLateralVelocity, float fatigue)
        {
            float lateT = Mathf.Pow(
                Mathf.Clamp01(Mathf.InverseLerp(_settings.VerticalWallrunLateDecelStart, 1f, fatigue)),
                _settings.VerticalWallrunLateDecelExponent);
            float upwardSpeed = Mathf.Lerp(_settings.VerticalWallrunUpSpeed, _settings.VerticalWallrunUpSpeed * _settings.VerticalWallrunEndSpeedRetention, lateT);
            if (_state.WallrunNearTop)
            {
                upwardSpeed *= _settings.WallrunTopUpwardVelocityMultiplier;
            }

            Vector3 horizontalCarry = Vector3.ProjectOnPlane(currentLateralVelocity, normal);
            horizontalCarry = Vector3.ProjectOnPlane(horizontalCarry, Vector3.up);
            horizontalCarry = Vector3.ClampMagnitude(horizontalCarry, _settings.SprintSpeed * 0.35f);
            Vector3 wallStick = Vector3.ProjectOnPlane(-normal, Vector3.up).normalized * _settings.VerticalWallrunWallStickSpeed;
            if (wallStick.sqrMagnitude <= 0.0001f)
            {
                wallStick = Vector3.zero;
            }

            return horizontalCarry + wallStick + Vector3.up * upwardSpeed;
        }

        private float ResolveWallrunProfileSpeed(float fatigue, float lateDecelStart)
        {
            float apexSpeed = Mathf.Max(_state.WallrunPeakSpeed, _state.WallrunStartSpeed);
            if (fatigue <= _settings.WallrunApexTime)
            {
                float apexT = Mathf.SmoothStep(0f, 1f, fatigue / Mathf.Max(0.01f, _settings.WallrunApexTime));
                return Mathf.Lerp(_state.WallrunStartSpeed, apexSpeed, apexT);
            }

            float postApexSpeed = apexSpeed * _settings.WallrunPostApexRetention;
            if (fatigue < lateDecelStart)
            {
                float postApexT = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(_settings.WallrunApexTime, lateDecelStart, fatigue));
                return Mathf.Lerp(apexSpeed, postApexSpeed, postApexT);
            }

            float endDrop = Mathf.Pow(Mathf.InverseLerp(lateDecelStart, 1f, fatigue), _settings.WallrunLateDecelExponent);
            return Mathf.Lerp(postApexSpeed, apexSpeed * _settings.WallrunEndSpeedRetention, endDrop);
        }

        private float ResolveWallrunVerticalVelocity(float fatigue, float endDrop)
        {
            float duration = Mathf.Max(0.01f, _settings.WallrunDurationSeconds);
            float arcVelocity = _settings.WallrunApexHeight * Mathf.PI / duration * Mathf.Cos(fatigue * Mathf.PI);
            float lateDropVelocity = _settings.WallrunGravity * _settings.WallrunFatigueGravityMultiplier * endDrop * 0.35f;
            return Mathf.Max(arcVelocity - lateDropVelocity, -_settings.WallrunMaxDownwardVelocity);
        }

        private bool IsSameWallReentryBlocked(Vector3 normal, Vector3 candidateAlongWall)
        {
            if (IsSameWallSameDirectionChainLocked(normal, candidateAlongWall))
            {
                return true;
            }

            if (_state.WallrunSameWallLockoutTimer <= 0f)
            {
                return false;
            }

            if (_state.WallrunBlockedNormal.sqrMagnitude > 0.0001f && _directions.IsWallrunContinuation(normal, _state.WallrunBlockedNormal))
            {
                return true;
            }

            if (!_directions.IsWallrunContinuation(normal, _state.WallrunLockedNormal))
            {
                return false;
            }

            if (_state.WallrunLockedDirection.sqrMagnitude <= 0.0001f || candidateAlongWall.sqrMagnitude <= 0.0001f)
            {
                return true;
            }

            return Vector3.Dot(candidateAlongWall.normalized, _state.WallrunLockedDirection.normalized) > _settings.WallrunReverseReentryDot;
        }

        private bool IsSameWallSameDirectionChainLocked(Vector3 normal, Vector3 candidateAlongWall)
        {
            if (!_state.WallrunChainSameDirectionLocked || !_directions.IsWallrunContinuation(normal, _state.WallrunChainLockedNormal))
            {
                return false;
            }

            if (_state.WallrunChainLockedDirection.sqrMagnitude <= 0.0001f || candidateAlongWall.sqrMagnitude <= 0.0001f)
            {
                return true;
            }

            return Vector3.Dot(candidateAlongWall.normalized, _state.WallrunChainLockedDirection.normalized) > _settings.WallrunReverseReentryDot;
        }

        private bool ShouldClearWallrunChainLock(Vector3 normal, Vector3 candidateAlongWall)
        {
            if (!_state.WallrunChainSameDirectionLocked)
            {
                return false;
            }

            if (!_directions.IsWallrunContinuation(normal, _state.WallrunChainLockedNormal))
            {
                return true;
            }

            if (_state.WallrunChainLockedDirection.sqrMagnitude <= 0.0001f || candidateAlongWall.sqrMagnitude <= 0.0001f)
            {
                return false;
            }

            return Vector3.Dot(candidateAlongWall.normalized, _state.WallrunChainLockedDirection.normalized) <= _settings.WallrunReverseReentryDot;
        }

        private void FatigueOutOfWallrun()
        {
            if (_state.WallrunFatigued)
            {
                return;
            }

            LockWallrunReentry();
            LockSameWallSameDirectionUntilAnotherWall();
            _state.WallrunActiveDirection = Vector3.zero;
            _state.WallrunSurfaceTransform = null;
            _state.State = WallrunnerMovementState.Airborne;
        }

        private void ExitWallrunFromSurfaceLoss()
        {
            ExitWallrunFromSurfaceLoss(Vector3.zero);
        }

        private void ExitWallrunFromSurfaceLoss(Vector3 blockedNormal)
        {
            LockWallrunReentry();
            LockSameWallSameDirectionUntilAnotherWall();
            _state.WallrunDetachReattachTimer = _settings.WallrunDetachReattachDelayEnabled
                ? Mathf.Max(_state.WallrunDetachReattachTimer, _settings.WallrunDetachReattachDelaySeconds)
                : 0f;
            _state.WallrunBlockedNormal = blockedNormal;
            _state.WallrunActiveDirection = Vector3.zero;
            _state.WallrunSurfaceTransform = null;
            _state.WallrunStyle = WallrunTraversalStyle.Horizontal;
            _state.State = WallrunnerMovementState.Airborne;
        }

        internal void LockWallrunReentry()
        {
            _state.WallrunFatigued = true;
            _state.WallrunLockedNormal = _state.WallNormal;
            Vector3 lateralVelocity = Vector3.ProjectOnPlane(_state.Velocity, Vector3.up);
            _state.WallrunLockedDirection = _state.WallrunLockedNormal.sqrMagnitude <= 0.0001f
                ? Vector3.zero
                : _state.WallrunStyle == WallrunTraversalStyle.Vertical ? Vector3.up : _directions.ResolveWallrunAlongWall(_state.WallrunLockedNormal, lateralVelocity);
            _state.WallrunSameWallLockoutTimer = _settings.WallrunSameWallLockoutSeconds;
        }

        internal void LockSameWallSameDirectionUntilAnotherWall()
        {
            if (_state.WallrunLockedNormal.sqrMagnitude <= 0.0001f || _state.WallrunLockedDirection.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            _state.WallrunChainLockedNormal = _state.WallrunLockedNormal;
            _state.WallrunChainLockedDirection = _state.WallrunLockedDirection;
            _state.WallrunChainSameDirectionLocked = true;
        }
    }
}
