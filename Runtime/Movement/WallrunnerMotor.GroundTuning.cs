using UnityEngine;

namespace Deucarian.TemplateGameMovementFps.Movement
{
    public sealed partial class WallrunnerMotor
    {
        public float GroundContactProbeDistance
        {
            get => groundContactProbeDistance;
            set => groundContactProbeDistance = Mathf.Max(0.01f, value);
        }

        public float GroundContactRadiusMultiplier
        {
            get => groundContactRadiusMultiplier;
            set => groundContactRadiusMultiplier = Mathf.Clamp(value, 0.05f, 1f);
        }

        public float StandableGroundMaxAngleDegrees
        {
            get => standableGroundMaxAngleDegrees;
            set => standableGroundMaxAngleDegrees = Mathf.Clamp(value, 1f, 89f);
        }

        public float RunGroundSnapDistance
        {
            get => groundSnapDistance;
            set => groundSnapDistance = Mathf.Max(0f, value);
        }

        public float SlideGroundSnapDistance
        {
            get => slideGroundSnapDistance;
            set => slideGroundSnapDistance = Mathf.Max(0f, value);
        }

        public float RunGroundSnapBreakAngleDegrees
        {
            get => groundSnapBreakAngleDegrees;
            set => groundSnapBreakAngleDegrees = Mathf.Clamp(value, 0f, 89f);
        }

        public float SlideGroundSnapBreakAngleDegrees
        {
            get => slideGroundSnapBreakAngleDegrees;
            set => slideGroundSnapBreakAngleDegrees = Mathf.Clamp(value, 0f, 89f);
        }

        public float GroundSnapReleaseDropAngleDegrees
        {
            get => groundSnapReleaseDropAngleDegrees;
            set => groundSnapReleaseDropAngleDegrees = Mathf.Clamp(value, 0f, 89f);
        }

        public float SlideGroundSnapReleaseDropAngleDegrees
        {
            get => slideGroundSnapReleaseDropAngleDegrees;
            set => slideGroundSnapReleaseDropAngleDegrees = Mathf.Clamp(value, 0f, 89f);
        }

        public float GroundSnapReleaseMinimumDrop
        {
            get => groundSnapReleaseMinimumDrop;
            set => groundSnapReleaseMinimumDrop = Mathf.Max(0f, value);
        }

        public float DownhillGroundStickExtraProbeDistance
        {
            get => downhillGroundStickExtraProbeDistance;
            set => downhillGroundStickExtraProbeDistance = Mathf.Max(0f, value);
        }

        public float DownhillGroundStickExtraLookAhead
        {
            get => downhillGroundStickExtraLookAhead;
            set => downhillGroundStickExtraLookAhead = Mathf.Max(0f, value);
        }

        public float DownhillGroundStickMinimumAngleDegrees
        {
            get => downhillGroundStickMinimumAngleDegrees;
            set => downhillGroundStickMinimumAngleDegrees = Mathf.Clamp(value, 0f, 45f);
        }

        public float DownhillGroundStickFadeStartAngleDegrees
        {
            get => downhillGroundStickFadeStartAngleDegrees;
            set => downhillGroundStickFadeStartAngleDegrees = Mathf.Clamp(value, 1f, 89f);
        }

        public float DownhillGroundStickFadeEndAngleDegrees
        {
            get => downhillGroundStickFadeEndAngleDegrees;
            set => downhillGroundStickFadeEndAngleDegrees = Mathf.Clamp(value, 1f, 89f);
        }

        public float AirborneSlideBufferSeconds
        {
            get => airborneSlideBufferSeconds;
            set => airborneSlideBufferSeconds = Mathf.Max(0f, value);
        }

        public bool MaxVelocityEnabled
        {
            get => maxVelocityEnabled;
            set => maxVelocityEnabled = value;
        }

        public float MaxVelocityMetersPerSecond
        {
            get => maxVelocityMetersPerSecond;
            set => maxVelocityMetersPerSecond = Mathf.Max(0.01f, value);
        }

        public bool MaxVelocityHardCap
        {
            get => maxVelocityHardCap;
            set => maxVelocityHardCap = value;
        }

        public float MaxVelocitySoftPullPerSecond
        {
            get => maxVelocitySoftPullPerSecond;
            set => maxVelocitySoftPullPerSecond = Mathf.Max(0f, value);
        }

        public bool BunnyHopEnabled
        {
            get => bunnyHopEnabled;
            set => bunnyHopEnabled = value;
        }

        public float BunnyHopLandingWindowSeconds
        {
            get => bunnyHopLandingWindowSeconds;
            set => bunnyHopLandingWindowSeconds = Mathf.Clamp(value, 0f, 0.4f);
        }

        public float BunnyHopSteerStrength
        {
            get => bunnyHopSteerStrength;
            set => bunnyHopSteerStrength = Mathf.Clamp01(value);
        }

        public float BunnyHopSpeedMultiplier
        {
            get => bunnyHopSpeedMultiplier;
            set => bunnyHopSpeedMultiplier = Mathf.Max(1f, value);
        }

    }
}
