using System;
using System.Collections.Generic;
using Deucarian.Combat;
using Deucarian.TemplateGameMovementFps.Combat;
using Deucarian.TemplateGameMovementFps.Movement;
using Deucarian.TemplateGameMovementFps.Progression;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Deucarian.TemplateGameMovementFps.Actors
{
    internal sealed class MovementFpsCameraSettings
    {
        internal float MouseSensitivity = 0.08f;
        internal float SlideCameraDrop = 0.46f;
        internal float SlideCameraFollowSpeed = 16f;
        internal bool MovementCameraFeelEnabled = true;
        internal float BaseFieldOfView = 76f;
        internal float SpeedFieldOfViewKick = 10f;
        internal float SpeedFieldOfViewReference = 28f;
        internal float SlideFieldOfViewKick = 3.5f;
        internal float WallrunFieldOfViewKick = 5.5f;
        internal float VaultFieldOfViewKick = 4f;
        internal float WallrunCameraRollDegrees = 7f;
        internal float SlideCameraRollDegrees = 3f;
        internal float CameraFeelFollowSpeed = 12f;
    }
}
