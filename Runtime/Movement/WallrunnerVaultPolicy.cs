using Deucarian.TemplateGameMovementFps.Definitions;
using UnityEngine;

namespace Deucarian.TemplateGameMovementFps.Movement
{
    internal sealed class WallrunnerVaultPolicy
    {
        private readonly WallrunnerSettings _settings;
        private readonly WallrunnerState _state;
        private readonly IWallrunnerEnvironment _environment;
        private readonly WallrunnerDirections _directions;
        private readonly WallrunnerVelocityPolicy _velocityPolicy;

        internal WallrunnerVaultPolicy(WallrunnerSettings settings, WallrunnerState state, IWallrunnerEnvironment environment, WallrunnerDirections directions, WallrunnerVelocityPolicy velocityPolicy)
        {
            _settings = settings;
            _state = state;
            _environment = environment;
            _directions = directions;
            _velocityPolicy = velocityPolicy;
        }

        internal bool TryVault(Vector3 wishDirection)
        {
            Vector3 direction = ResolveVaultProbeDirection(wishDirection);
            if (direction.sqrMagnitude <= 0.01f)
            {
                return false;
            }

            bool foundTarget = TryBuildVaultCandidate(direction, out VaultCandidate candidate);

            if (!foundTarget)
            {
                return false;
            }

            StartVaultBlend(candidate);
            return true;
        }

        private Vector3 ResolveVaultProbeDirection(Vector3 wishDirection)
        {
            Vector3 lookDirection = _directions.BuildLookDirection();
            Vector3 wish = Vector3.ProjectOnPlane(wishDirection, Vector3.up);
            if (wish.sqrMagnitude <= 0.01f)
            {
                return lookDirection;
            }

            return Vector3.Slerp(lookDirection, wish.normalized, 0.35f).normalized;
        }

        private void StartVaultBlend(VaultCandidate candidate)
        {
            _state.ClearWallrunChainLock();
            _state.WallrunSameWallLockoutTimer = 0f;
            _state.WallrunDetachReattachTimer = 0f;
            _state.WallrunLockedNormal = Vector3.zero;
            _state.WallrunLockedDirection = Vector3.zero;
            _state.WallrunActiveDirection = Vector3.zero;
            _state.WallrunBlockedNormal = Vector3.zero;
            _state.WallrunSurfaceTransform = null;
            _state.WallrunNearTop = false;
            _state.WallrunStyle = WallrunTraversalStyle.Horizontal;
            _state.Grounded = false;
            _state.ResetAirJumps();
            _state.GroundedJumpGraceTimer = 0f;
            _state.SlideJumpGraceTimer = 0f;
            _state.AirborneSlideBufferTimer = 0f;
            _state.GroundSnapLockoutTimer = Mathf.Max(_settings.JumpGroundLockoutSeconds, 0.12f);
            _state.VaultStartPosition = _environment.Position;
            _state.VaultTargetPosition = candidate.TargetPosition;
            _state.VaultExitVelocity = _velocityPolicy.ResolveMaxVelocity(candidate.ExitVelocity, 0f);
            _state.VaultBlendDuration = Mathf.Max(0.02f, ResolveVaultBlendDuration(candidate.Style));
            _state.VaultBlendTimer = 0.0001f;
            _state.Velocity = _state.VaultExitVelocity;
            _state.ActiveVaultStyle = candidate.Style;
            _state.LastVaultStyle = candidate.Style;
            _state.State = WallrunnerMovementState.Vaulting;
        }

        internal void TickVaultBlend(float deltaTime)
        {
            _state.VaultBlendTimer += deltaTime;
            float normalizedTime = Mathf.Clamp01(_state.VaultBlendTimer / Mathf.Max(0.01f, _state.VaultBlendDuration));
            float easedTime = Mathf.SmoothStep(0f, 1f, normalizedTime);
            float arcLift = Mathf.Sin(normalizedTime * Mathf.PI) * _environment.CapsuleRadius * 0.18f;
            _environment.Position = Vector3.Lerp(_state.VaultStartPosition, _state.VaultTargetPosition, easedTime) + Vector3.up * arcLift;
            _state.Grounded = false;
            _state.State = WallrunnerMovementState.Vaulting;

            if (normalizedTime < 1f)
            {
                return;
            }

            _environment.Position = _state.VaultTargetPosition;
            _state.Velocity = _state.VaultExitVelocity;
            _state.ResetAirJumps();
            _state.VaultBlendTimer = 0f;
            _state.VaultBlendDuration = 0f;
            _state.ActiveVaultStyle = VaultTraversalStyle.None;
            _state.GroundSnapLockoutTimer = Mathf.Max(_state.GroundSnapLockoutTimer, _settings.JumpGroundLockoutSeconds);
            _state.State = WallrunnerMovementState.Airborne;
        }

        private bool TryBuildVaultCandidate(Vector3 direction, out VaultCandidate candidate)
        {
            candidate = default;
            if (!TryFindMantleSideHit(direction, out WallrunnerHit lowHit))
            {
                return false;
            }

            if (!TryResolveProceduralMantleSurface(lowHit, direction))
            {
                return false;
            }

            if (!TryFindMantleTop(lowHit, direction, out WallrunnerHit topHit))
            {
                return false;
            }

            float ledgeHeight = ResolveVaultLedgeHeight(topHit.Point);
            if (!TryResolveVaultStyle(ledgeHeight, out VaultTraversalStyle style))
            {
                return false;
            }

            return TryBuildVaultTarget(topHit, direction, style, ledgeHeight, out candidate);
        }

        private bool TryFindMantleSideHit(Vector3 direction, out WallrunnerHit sideHit)
        {
            return TryFindMantleSideHitAtHeight(direction, _environment.CapsuleHeight * 0.16f, out sideHit)
                || TryFindMantleSideHitAtHeight(direction, _environment.CapsuleHeight * 0.34f, out sideHit)
                || TryFindMantleSideHitAtHeight(direction, _environment.CapsuleHeight * 0.56f, out sideHit)
                || TryFindMantleSideHitAtHeight(direction, _environment.CapsuleHeight * 0.72f, out sideHit);
        }

        private bool TryFindMantleSideHitAtHeight(Vector3 direction, float heightOffset, out WallrunnerHit sideHit)
        {
            Vector3 origin = _environment.Position;
            origin.y = ResolveCapsuleFeetY() + heightOffset;
            float radius = Mathf.Max(0.05f, _environment.CapsuleRadius * 0.35f);
            return _environment.SphereCast(origin, radius, direction, out sideHit, _settings.MantleCheckDistance)
                && TryResolveProceduralMantleSurface(sideHit, direction);
        }

        private bool TryResolveProceduralMantleSurface(WallrunnerHit hit, Vector3 direction)
        {
            if (hit.Normal.y > _settings.MantleMaximumWallNormalY || hit.Normal.y < -0.25f)
            {
                return false;
            }

            Vector3 approach = Vector3.ProjectOnPlane(direction, Vector3.up);
            Vector3 faceDirection = Vector3.ProjectOnPlane(-hit.Normal, Vector3.up);
            if (approach.sqrMagnitude <= 0.001f || faceDirection.sqrMagnitude <= 0.001f)
            {
                return false;
            }

            return Vector3.Dot(approach.normalized, faceDirection.normalized) >= _settings.MantleMinimumApproachDot;
        }

        private bool TryFindMantleTop(WallrunnerHit lowHit, Vector3 direction, out WallrunnerHit topHit)
        {
            topHit = default;
            bool found = false;
            float bestHeight = float.NegativeInfinity;
            float bestProbeDistance = 0f;
            float minimumTopY = Mathf.Max(ResolveCapsuleFeetY() + 0.2f, lowHit.Point.y - 0.15f);
            float maximumProbeDistance = _settings.MantleCheckDistance + Mathf.Max(_settings.FlowVaultOverDistanceMeters, _settings.SafetyMantleOverDistanceMeters, _settings.MantleVaultOverDistance);
            float firstProbeDistance = Mathf.Clamp(lowHit.Distance + _environment.CapsuleRadius * 0.35f, 0.25f, maximumProbeDistance);
            float probeStep = Mathf.Max(0.12f, _environment.CapsuleRadius * 0.35f);
            float maximumVaultHeight = Mathf.Max(_settings.VaultMantleMaxHeightMeters, _settings.MantleHeight);
            float highStartY = ResolveCapsuleFeetY() + maximumVaultHeight + _environment.CapsuleHeight + 0.2f;
            float rayDistance = maximumVaultHeight + _environment.CapsuleHeight + 0.6f;

            for (int index = 0; index < 8; index++)
            {
                float ledgeProbeDistance = Mathf.Clamp(firstProbeDistance + probeStep * index, 0.25f, maximumProbeDistance);
                Vector3 highStart = _environment.Position + direction * ledgeProbeDistance;
                highStart.y = highStartY;
                if (!_environment.Raycast(highStart, Vector3.down, out WallrunnerHit candidate, rayDistance))
                {
                    continue;
                }

                if (candidate.Normal.y < _settings.MantleMinimumTopNormalY || candidate.Point.y < minimumTopY)
                {
                    continue;
                }

                bool higher = candidate.Point.y > bestHeight + 0.02f;
                bool sameHeightAndFarther = Mathf.Abs(candidate.Point.y - bestHeight) <= 0.02f && ledgeProbeDistance > bestProbeDistance;
                if (!found || higher || sameHeightAndFarther)
                {
                    found = true;
                    bestHeight = candidate.Point.y;
                    bestProbeDistance = ledgeProbeDistance;
                    topHit = candidate;
                }
            }

            return found;
        }

        private bool TryBuildVaultTarget(WallrunnerHit topHit, Vector3 direction, VaultTraversalStyle style, float ledgeHeight, out VaultCandidate candidate)
        {
            candidate = default;
            float overDistance = ResolveVaultOverDistance(style);
            float airborneLift = style == VaultTraversalStyle.Flow
                ? Mathf.Max(0.32f, _environment.CapsuleRadius * 0.55f)
                : Mathf.Max(0.08f, _environment.CapsuleRadius * 0.12f);
            Vector3 targetPosition = topHit.Point + direction * overDistance + Vector3.up * (_environment.CapsuleHeight * 0.5f + _settings.SkinWidth + airborneLift);
            if (!IsCapsuleClearAt(targetPosition))
            {
                return false;
            }

            float timingScore = ResolveVaultTimingScore(topHit.Point, style);
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
            if (ledgeHeight < 0.2f || ledgeHeight > _settings.VaultMantleMaxHeightMeters)
            {
                return VaultTraversalStyle.None;
            }

            switch (_settings.VaultMode)
            {
                case VaultMode.Flow:
                    return VaultTraversalStyle.Flow;
                case VaultMode.SafetyMantle:
                    return VaultTraversalStyle.SafetyMantle;
                default:
                    return ledgeHeight <= _settings.VaultFlowMaxHeightMeters ? VaultTraversalStyle.Flow : VaultTraversalStyle.SafetyMantle;
            }
        }

        private float ResolveVaultLedgeHeight(Vector3 ledgePoint)
        {
            return ledgePoint.y - ResolveCapsuleFeetY();
        }

        private float ResolveCapsuleFeetY()
        {
            _environment.GetCapsulePoints(_environment.Position, out Vector3 bottom, out _);
            return bottom.y - _environment.CapsuleRadius;
        }

        private float ResolveVaultTimingScore(Vector3 ledgePoint, VaultTraversalStyle style)
        {
            float upwardScore = Mathf.InverseLerp(-_settings.JumpVelocity * 0.55f, _settings.JumpVelocity * 0.45f, _state.Velocity.y);
            float ledgeHeight = ResolveVaultLedgeHeight(ledgePoint);
            float idealHeight = style == VaultTraversalStyle.Flow
                ? Mathf.Max(0.35f, _settings.VaultFlowMaxHeightMeters * 0.65f)
                : Mathf.Lerp(_settings.VaultFlowMaxHeightMeters, _settings.VaultMantleMaxHeightMeters, 0.55f);
            float heightWindow = style == VaultTraversalStyle.Flow
                ? Mathf.Max(0.35f, _settings.VaultFlowMaxHeightMeters * 0.55f)
                : Mathf.Max(0.35f, _settings.VaultMantleMaxHeightMeters - _settings.VaultFlowMaxHeightMeters);
            float heightScore = Mathf.Clamp01(1f - Mathf.Abs(ledgeHeight - idealHeight) / heightWindow);
            return Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(upwardScore * 0.7f + heightScore * 0.3f));
        }

        private Vector3 BuildVaultExitVelocity(Vector3 direction, float timingScore, VaultTraversalStyle style)
        {
            Vector3 carryVelocity = Vector3.ProjectOnPlane(_state.Velocity, Vector3.up);
            Vector3 carryDirection = carryVelocity.sqrMagnitude > 0.01f ? carryVelocity.normalized : direction;
            float directionBlend = style == VaultTraversalStyle.Flow ? 0.35f : 0.58f;
            Vector3 exitDirection = Vector3.Slerp(carryDirection, direction, directionBlend).normalized;
            float carrySpeed = Mathf.Max(carryVelocity.magnitude, _settings.SprintSpeed * _settings.VaultMinimumSpeedFraction);
            float speedMultiplier = ResolveVaultSpeedMultiplier(style, timingScore);
            float exitSpeed = Mathf.Max(carrySpeed, carrySpeed * speedMultiplier, _settings.SprintSpeed * _settings.VaultMinimumSpeedFraction * speedMultiplier);
            float verticalBoost = ResolveVaultVerticalBoost(style, timingScore);
            return exitDirection * exitSpeed + Vector3.up * (_settings.JumpVelocity * verticalBoost);
        }

        private float ResolveVaultBlendDuration(VaultTraversalStyle style)
        {
            return style == VaultTraversalStyle.SafetyMantle
                ? _settings.SafetyMantleBlendDurationSeconds
                : _settings.FlowVaultBlendDurationSeconds;
        }

        private float ResolveVaultOverDistance(VaultTraversalStyle style)
        {
            return style == VaultTraversalStyle.SafetyMantle
                ? _settings.SafetyMantleOverDistanceMeters
                : _settings.FlowVaultOverDistanceMeters;
        }

        private float ResolveVaultSpeedMultiplier(VaultTraversalStyle style, float timingScore)
        {
            return style == VaultTraversalStyle.SafetyMantle
                ? Mathf.Lerp(_settings.SafetyMantleBaseSpeedMultiplier, _settings.SafetyMantlePerfectSpeedMultiplier, timingScore)
                : Mathf.Lerp(_settings.FlowVaultBaseSpeedMultiplier, _settings.FlowVaultPerfectSpeedMultiplier, timingScore);
        }

        private float ResolveVaultVerticalBoost(VaultTraversalStyle style, float timingScore)
        {
            return style == VaultTraversalStyle.SafetyMantle
                ? Mathf.Lerp(_settings.SafetyMantleMinimumVerticalBoost, _settings.SafetyMantlePerfectVerticalBoost, timingScore)
                : Mathf.Lerp(_settings.FlowVaultMinimumVerticalBoost, _settings.FlowVaultPerfectVerticalBoost, timingScore);
        }

        private bool IsCapsuleClearAt(Vector3 capsulePosition)
        {
            _environment.GetCapsulePoints(capsulePosition, out Vector3 bottom, out Vector3 top);
            float clearanceRadius = Mathf.Max(0.01f, _environment.CapsuleRadius - _settings.SkinWidth);
            return !_environment.CheckCapsule(bottom, top, clearanceRadius);
        }
        internal readonly struct VaultCandidate
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
    }
}
