using UnityEngine;

namespace Deucarian.TemplateGameMovementFps.Movement
{
    /// <summary>
    /// Local motor boundary for pose and collision access. Query implementations
    /// exclude the motor's own capsule and ignore triggers. Surface identities
    /// are opaque tokens compared by identity, not gameplay-owned Unity objects.
    /// </summary>
    internal interface IWallrunnerEnvironment
    {
        Vector3 Position { get; set; }
        Quaternion Rotation { get; }
        Vector3 Forward { get; }
        Vector3 Right { get; }
        float CapsuleRadius { get; }
        float CapsuleHeight { get; }
        bool IsSameSurface(object first, object second);
        void SetPositionAndRotation(Vector3 position, Quaternion rotation);
        void SynchronizePose();
        void GetCapsulePoints(Vector3 position, out Vector3 bottom, out Vector3 top);
        bool Raycast(Vector3 origin, Vector3 direction, out WallrunnerHit hit, float distance);
        bool SphereCast(Vector3 origin, float radius, Vector3 direction, out WallrunnerHit hit, float distance);
        bool CapsuleCast(Vector3 bottom, Vector3 top, float radius, Vector3 direction, out WallrunnerHit hit, float distance);
        bool CheckCapsule(Vector3 bottom, Vector3 top, float radius);
    }

    internal readonly struct WallrunnerHit
    {
        public WallrunnerHit(Vector3 point, Vector3 normal, float distance, object surface = null)
        {
            Point = point;
            Normal = normal;
            Distance = distance;
            Surface = surface;
        }

        public Vector3 Point { get; }
        public Vector3 Normal { get; }
        public float Distance { get; }
        public object Surface { get; }
    }
}
