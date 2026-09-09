using Deucarian.TemplateGameMovementFps.Movement;
using UnityEngine;

namespace Deucarian.TemplateGameMovementFps.Tests
{
    // Analytic query responses, with no GameObject, collider, or Physics world.
    internal sealed class WallrunnerTestEnvironment : IWallrunnerEnvironment
    {
        internal bool Ground;
        internal Vector3 GroundNormal = Vector3.up;
        internal bool Wall;
        internal Vector3 WallNormal = Vector3.left;
        internal object Surface = new object();
        internal bool Ledge;
        internal float LedgeHeight = 1f;
        internal bool BlockVault;
        internal WallrunnerHit? NextCollision;
        internal int Queries;
        internal int Synchronizations;

        public Vector3 Position { get; set; } = new Vector3(0f, 1f, 0f);
        public Quaternion Rotation { get; private set; } = Quaternion.identity;
        public Vector3 Forward => Rotation * Vector3.forward;
        public Vector3 Right => Rotation * Vector3.right;
        public float CapsuleRadius => 0.5f;
        public float CapsuleHeight => 2f;
        public bool IsSameSurface(object first, object second) => ReferenceEquals(first, second);

        public void SetPositionAndRotation(Vector3 position, Quaternion rotation)
        {
            Position = position;
            Rotation = rotation;
        }

        public void SynchronizePose() => Synchronizations++;

        public void GetCapsulePoints(Vector3 position, out Vector3 bottom, out Vector3 top)
        {
            bottom = position + Vector3.down * 0.5f;
            top = position + Vector3.up * 0.5f;
        }

        public bool Raycast(Vector3 origin, Vector3 direction, out WallrunnerHit hit, float distance)
        {
            Queries++;
            if (Ledge && direction.y < -0.9f && origin.y > Position.y + 2f)
            {
                Vector3 point = new Vector3(origin.x, Position.y - 1f + LedgeHeight, origin.z);
                hit = new WallrunnerHit(point, Vector3.up, origin.y - point.y, Surface);
                return true;
            }

            hit = default;
            return false;
        }

        public bool SphereCast(Vector3 origin, float radius, Vector3 direction, out WallrunnerHit hit, float distance)
        {
            Queries++;
            if (Ground && direction.y < -0.9f)
            {
                hit = new WallrunnerHit(origin + Vector3.down * radius, GroundNormal, 0.03f, Surface);
                return true;
            }

            if (Ledge && direction.z > 0.9f)
            {
                hit = new WallrunnerHit(origin + direction * 0.4f, Vector3.back, 0.4f, Surface);
                return true;
            }

            if (Wall && Vector3.Dot(direction, -WallNormal) > 0.5f)
            {
                hit = new WallrunnerHit(origin + direction * 0.4f, WallNormal, 0.4f, Surface);
                return true;
            }

            hit = default;
            return false;
        }

        public bool CapsuleCast(Vector3 bottom, Vector3 top, float radius, Vector3 direction, out WallrunnerHit hit, float distance)
        {
            Queries++;
            hit = NextCollision.GetValueOrDefault();
            bool found = NextCollision.HasValue;
            NextCollision = null;
            return found;
        }

        public bool CheckCapsule(Vector3 bottom, Vector3 top, float radius)
        {
            Queries++;
            return BlockVault;
        }
    }
}
