using UnityEngine;

namespace Deucarian.TemplateGameMovementFps.Movement
{
    internal readonly struct WallrunnerGroundContact
    {
        public WallrunnerGroundContact(Vector3 point, Vector3 normal, float snapOffset)
        {
            Point = point;
            Normal = normal;
            SnapOffset = snapOffset;
        }

        public Vector3 Point { get; }

        public Vector3 Normal { get; }

        public float SnapOffset { get; }
    }
}
