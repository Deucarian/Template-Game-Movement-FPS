using UnityEngine;

namespace Deucarian.TemplateGameMovementFps.Movement
{
    internal sealed class WallrunnerVelocityPolicy
    {
        private readonly WallrunnerSettings _settings;
        private readonly WallrunnerState _state;
        private readonly IWallrunnerEnvironment _environment;
        private readonly WallrunnerDirections _directions;

        internal WallrunnerVelocityPolicy(WallrunnerSettings settings, WallrunnerState state, IWallrunnerEnvironment environment, WallrunnerDirections directions)
        {
            _settings = settings;
            _state = state;
            _environment = environment;
            _directions = directions;
        }

        internal Vector3 ApplyMomentumPreservingAirControl(Vector3 lateralVelocity, Vector3 wishDirection, float targetSpeed, float deltaTime)
        {
            if (wishDirection.sqrMagnitude <= 0.1f)
            {
                return lateralVelocity;
            }

            Vector3 wish = wishDirection.normalized;
            float currentSpeed = lateralVelocity.magnitude;
            if (currentSpeed <= 0.01f)
            {
                return wish * Mathf.Min(targetSpeed, _settings.AirAcceleration * deltaTime);
            }

            float speed = currentSpeed < targetSpeed ? Mathf.Min(targetSpeed, currentSpeed + _settings.AirAcceleration * deltaTime) : currentSpeed;
            float maxRadians = _settings.AirAcceleration * deltaTime / Mathf.Max(currentSpeed, 0.1f);
            Vector3 steeredDirection = Vector3.RotateTowards(lateralVelocity.normalized, wish, maxRadians, 0f);
            return steeredDirection * speed;
        }

        internal void ApplyVelocityScaledLateralBoost(Vector3 wishDirection, float speedMultiplier, float speedCap, float minimumBaseSpeed)
        {
            Vector3 lateralVelocity = Vector3.ProjectOnPlane(_state.Velocity, Vector3.up);
            float currentSpeed = lateralVelocity.magnitude;
            Vector3 direction = currentSpeed > 0.01f ? lateralVelocity.normalized : Vector3.ProjectOnPlane(_environment.Forward, Vector3.up).normalized;
            if (wishDirection.sqrMagnitude > 0.1f)
            {
                direction = (direction + wishDirection.normalized * 0.35f).normalized;
            }

            float baseSpeed = Mathf.Max(currentSpeed, minimumBaseSpeed);
            float boostedSpeed = Mathf.Min(baseSpeed * speedMultiplier, Mathf.Max(speedCap, baseSpeed));
            _state.Velocity = direction * boostedSpeed + Vector3.up * _state.Velocity.y;
        }

        internal void ApplyWallJumpLaunch(Vector3 lateralVelocity, bool useTimingMultiplier)
        {
            float currentSpeed = lateralVelocity.magnitude;
            Vector3 launchDirection = _directions.BuildWallJumpLaunchDirection(lateralVelocity, out float forwardPreserveRatio);
            float speedMultiplier = Mathf.Lerp(_settings.WallJumpTurnaroundSpeedMultiplier, _settings.WallJumpSpeedMultiplier, forwardPreserveRatio);
            float timingMultiplier = useTimingMultiplier ? ResolveWallJumpTimingMultiplier() : 1f;
            float baseSpeed = Mathf.Max(currentSpeed, _settings.SprintSpeed * _settings.WallJumpMinimumSpeedFraction);
            Vector3 lateralBoost = Vector3.ClampMagnitude(
                launchDirection * (baseSpeed * speedMultiplier * timingMultiplier),
                baseSpeed * _settings.WallJumpSpeedCapMultiplier);
            _state.Velocity = lateralBoost + Vector3.up * _settings.WallJumpVerticalVelocity;
        }

        internal void ApplyBunnyHopLaunch(Vector3 wishDirection)
        {
            Vector3 currentLateralVelocity = Vector3.ProjectOnPlane(_state.Velocity, Vector3.up);
            Vector3 landingCarryVelocity = Vector3.ProjectOnPlane(_state.BunnyHopCarryVelocity, Vector3.up);
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
                direction = Vector3.Slerp(direction, wishDirection.normalized, _settings.BunnyHopSteerStrength).normalized;
            }

            _state.Velocity = direction * (currentSpeed * _settings.BunnyHopSpeedMultiplier) + Vector3.up * _state.Velocity.y;
        }

        internal void ApplyMaxVelocity(float deltaTime)
        {
            _state.Velocity = ResolveMaxVelocity(_state.Velocity, deltaTime);
        }

        internal Vector3 ResolveMaxVelocity(Vector3 velocity, float deltaTime)
        {
            if (!_settings.MaxVelocityEnabled)
            {
                return velocity;
            }

            float maxLateralSpeed = Mathf.Max(0.01f, _settings.MaxVelocityMetersPerSecond);
            Vector3 lateralVelocity = Vector3.ProjectOnPlane(velocity, Vector3.up);
            float currentSpeed = lateralVelocity.magnitude;
            if (currentSpeed <= maxLateralSpeed || currentSpeed <= 0.0001f)
            {
                return velocity;
            }

            float targetSpeed = _settings.MaxVelocityHardCap
                ? maxLateralSpeed
                : Mathf.Max(maxLateralSpeed, currentSpeed - _settings.MaxVelocitySoftPullPerSecond * Mathf.Max(0f, deltaTime));
            return lateralVelocity.normalized * targetSpeed + Vector3.up * velocity.y;
        }

        private float ResolveWallJumpTimingMultiplier()
        {
            float duration = Mathf.Max(0.01f, _settings.WallrunDurationSeconds);
            float fatigue = Mathf.Clamp01(_state.WallrunTimer / duration);
            float distanceFromApex = Mathf.Abs(fatigue - _settings.WallrunApexTime);
            float timingScore = Mathf.Clamp01(1f - distanceFromApex / Mathf.Max(0.01f, _settings.WallJumpApexTimingWindow));
            timingScore = Mathf.SmoothStep(0f, 1f, timingScore);
            return Mathf.Lerp(1f, _settings.WallJumpPeakTimingMultiplier, timingScore);
        }
    }
}
