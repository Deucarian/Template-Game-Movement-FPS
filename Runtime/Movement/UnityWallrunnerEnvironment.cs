using System;
using UnityEngine;

namespace Deucarian.TemplateGameMovementFps.Movement
{
    /// <summary>Unity owns transforms, collider lifetime, and physics queries here.</summary>
    internal sealed class UnityWallrunnerEnvironment : IWallrunnerEnvironment
    {
        private readonly Transform _pose;
        private readonly CapsuleCollider _capsule;
        private readonly Func<LayerMask> _collisionMask;

        internal UnityWallrunnerEnvironment(Transform pose, CapsuleCollider capsule, Func<LayerMask> collisionMask)
        {
            _pose = pose;
            _capsule = capsule;
            _collisionMask = collisionMask;
        }

        public Vector3 Position { get => _pose.position; set => _pose.position = value; }
        public Quaternion Rotation => _pose.rotation;
        public Vector3 Forward => _pose.forward;
        public Vector3 Right => _pose.right;
        public float CapsuleRadius => _capsule.radius;
        public float CapsuleHeight => _capsule.height;

        public bool IsSameSurface(object first, object second)
        {
            // Preserve Unity's destroyed-object-as-null semantics at the adapter.
            return first as UnityEngine.Object == second as UnityEngine.Object;
        }

        public void SetPositionAndRotation(Vector3 position, Quaternion rotation)
        {
            _pose.SetPositionAndRotation(position, rotation);
        }

        public void SynchronizePose() => Physics.SyncTransforms();

        public void GetCapsulePoints(Vector3 position, out Vector3 bottom, out Vector3 top)
        {
            float halfHeight = Mathf.Max(_capsule.height * 0.5f, _capsule.radius);
            Vector3 center = position + _capsule.center;
            bottom = center + Vector3.down * (halfHeight - _capsule.radius);
            top = center + Vector3.up * (halfHeight - _capsule.radius);
        }

        public bool Raycast(Vector3 origin, Vector3 direction, out WallrunnerHit hit, float distance)
        {
            bool wasEnabled = _capsule.enabled;
            try
            {
                _capsule.enabled = false;
                bool found = Physics.Raycast(origin, direction, out RaycastHit unityHit, distance,
                    _collisionMask(), QueryTriggerInteraction.Ignore);
                hit = Convert(unityHit);
                return found;
            }
            finally { _capsule.enabled = wasEnabled; }
        }

        public bool SphereCast(Vector3 origin, float radius, Vector3 direction, out WallrunnerHit hit, float distance)
        {
            bool wasEnabled = _capsule.enabled;
            try
            {
                _capsule.enabled = false;
                bool found = Physics.SphereCast(origin, radius, direction, out RaycastHit unityHit, distance,
                    _collisionMask(), QueryTriggerInteraction.Ignore);
                hit = Convert(unityHit);
                return found;
            }
            finally { _capsule.enabled = wasEnabled; }
        }

        public bool CapsuleCast(Vector3 bottom, Vector3 top, float radius, Vector3 direction, out WallrunnerHit hit, float distance)
        {
            bool wasEnabled = _capsule.enabled;
            try
            {
                _capsule.enabled = false;
                bool found = Physics.CapsuleCast(bottom, top, radius, direction, out RaycastHit unityHit, distance,
                    _collisionMask(), QueryTriggerInteraction.Ignore);
                hit = Convert(unityHit);
                return found;
            }
            finally { _capsule.enabled = wasEnabled; }
        }

        public bool CheckCapsule(Vector3 bottom, Vector3 top, float radius)
        {
            bool wasEnabled = _capsule.enabled;
            try
            {
                _capsule.enabled = false;
                return Physics.CheckCapsule(bottom, top, radius, _collisionMask(), QueryTriggerInteraction.Ignore);
            }
            finally { _capsule.enabled = wasEnabled; }
        }

        private static WallrunnerHit Convert(RaycastHit hit)
        {
            Transform surface = null;
            if (hit.collider != null)
            {
                WallrunSurfaceGroup group = hit.collider.GetComponentInParent<WallrunSurfaceGroup>();
                surface = group == null ? hit.collider.transform : group.transform;
            }

            return new WallrunnerHit(hit.point, hit.normal, hit.distance, surface);
        }
    }
}
