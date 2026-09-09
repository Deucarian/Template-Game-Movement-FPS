using UnityEngine;

namespace Deucarian.TemplateGameMovementFps.Movement
{
    internal sealed class WallrunnerSlidePolicy
    {
        private readonly WallrunnerSettings _settings;
        private readonly WallrunnerState _state;
        private readonly IWallrunnerEnvironment _environment;

        internal WallrunnerSlidePolicy(WallrunnerSettings settings, WallrunnerState state, IWallrunnerEnvironment environment)
        {
            _settings = settings;
            _state = state;
            _environment = environment;
        }

        internal bool TryStartSlide(bool slidePressed, Vector3 wishDirection)
        {
            if (!slidePressed || !_state.Grounded)
            {
                return false;
            }

            _state.State = WallrunnerMovementState.Sliding;
            _state.AirborneSlideBufferTimer = 0f;
            _state.SlideTimer = _settings.SlideDurationSeconds;
            Vector3 direction = wishDirection.sqrMagnitude > 0.1f ? wishDirection.normalized : _environment.Forward;
            Vector3 lateralVelocity = Vector3.ProjectOnPlane(_state.Velocity, Vector3.up);
            float currentSpeed = lateralVelocity.magnitude;
            float baseEntrySpeed = Mathf.Max(currentSpeed, _settings.SprintSpeed * _settings.SlideMinimumEntrySpeedFraction);
            float slideEntrySpeed = baseEntrySpeed * _settings.SlideEnterSpeedMultiplier;
            slideEntrySpeed = ResolveSlopeAdjustedSlideEntrySpeed(direction, slideEntrySpeed);
            _state.SlideEntrySpeed = slideEntrySpeed;
            _state.Velocity = Vector3.Project(_state.Velocity, Vector3.up) + (direction * slideEntrySpeed);
            return true;
        }

        internal float ResolveSlopeBoostedSlideSpeed(Vector3 slideDirection, float currentSpeed, float deltaTime)
        {
            if (!_state.Grounded || currentSpeed <= 0.01f)
            {
                return currentSpeed;
            }

            if (!TryGetSlideSlope(out Vector3 downhill, out float slopeStrength) || slideDirection.sqrMagnitude <= 0.01f)
            {
                return currentSpeed;
            }

            float downhillAlignment = Vector3.Dot(slideDirection.normalized, downhill);
            if (downhillAlignment > 0f && _settings.SlideDownhillSpeedGainPerSecond > 0f)
            {
                float boost = currentSpeed * _settings.SlideDownhillSpeedGainPerSecond * slopeStrength * downhillAlignment * deltaTime;
                float speedCap = Mathf.Max(currentSpeed, _settings.SlideSpeed) * _settings.SlideDownhillSpeedCapMultiplier;
                return Mathf.Min(currentSpeed + boost, speedCap);
            }

            if (downhillAlignment < 0f && _settings.SlideUphillSpeedLossPerSecond > 0f)
            {
                float loss = currentSpeed * _settings.SlideUphillSpeedLossPerSecond * slopeStrength * -downhillAlignment * deltaTime;
                return Mathf.Max(0f, currentSpeed - loss);
            }

            return currentSpeed;
        }

        internal float ApplySlideDrag(float currentSpeed, float deltaTime)
        {
            if (currentSpeed <= 0.01f || _settings.SlideSpeedDecayPerSecond <= 0f)
            {
                return currentSpeed;
            }

            float duration = Mathf.Max(0.01f, _settings.SlideDurationSeconds);
            float elapsed01 = Mathf.Clamp01(1f - _state.SlideTimer / duration);
            float entryRatio = Mathf.Max(1f, _state.SlideEntrySpeed / Mathf.Max(0.01f, _settings.SlideSpeed));
            float delayedFraction = Mathf.Min(0.85f, _settings.SlideDragDelayFraction * Mathf.Pow(entryRatio, _settings.SlideEntrySpeedDelayExponent));
            float drag01 = Mathf.InverseLerp(delayedFraction, 1f, elapsed01);
            float dragRamp = Mathf.Pow(Mathf.Clamp01(drag01), _settings.SlideDragRampExponent);
            float entryResistance = Mathf.Pow(entryRatio, _settings.SlideEntrySpeedDecayResistanceExponent);
            float drag = _settings.SlideSpeedDecayPerSecond * dragRamp / Mathf.Max(0.01f, entryResistance);
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

            return currentSpeed * Mathf.Lerp(1f, _settings.SlideDownhillEnterSpeedMultiplier, downhillAlignment);
        }

        private bool TryGetSlideSlope(out Vector3 downhill, out float slopeStrength)
        {
            downhill = Vector3.zero;
            slopeStrength = 0f;
            if (!_state.Grounded)
            {
                return false;
            }

            float slopeAngle = Vector3.Angle(_state.GroundNormal, Vector3.up);
            if (slopeAngle < _settings.SlideSlopeEffectMinimumAngleDegrees)
            {
                return false;
            }

            downhill = Vector3.ProjectOnPlane(Vector3.down, _state.GroundNormal);
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

        internal float ResolveSlideJumpTimingScore()
        {
            float duration = Mathf.Max(0.01f, _settings.SlideDurationSeconds);
            return Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(_state.SlideTimer / duration));
        }
    }
}
