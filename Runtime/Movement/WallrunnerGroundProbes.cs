using UnityEngine;

namespace Deucarian.TemplateGameMovementFps.Movement
{
    internal sealed class WallrunnerGroundProbes
    {
        private readonly WallrunnerSettings _settings;
        private readonly IWallrunnerEnvironment _environment;

        internal WallrunnerGroundProbes(WallrunnerSettings settings, IWallrunnerEnvironment environment)
        {
            _settings = settings;
            _environment = environment;
        }

        private bool IsStandableGround(Vector3 normal)
        {
            return normal.y > _settings.MinimumGroundNormalY;
        }

        private Vector3 BuildGroundProbeOrigin(Vector3 capsuleBottom, float probeRadius)
        {
            float radiusInset = Mathf.Max(0f, _environment.CapsuleRadius - probeRadius);
            return capsuleBottom + Vector3.down * radiusInset + Vector3.up * 0.03f;
        }

        private float ResolveSphereGroundSnapOffset(float groundHitDistance)
        {
            float desiredProbeDistance = Mathf.Max(0.01f, 0.03f + _settings.SkinWidth * 0.5f);
            return Mathf.Max(0f, groundHitDistance - desiredProbeDistance);
        }

        internal bool TryFindGroundContact(float probeDistance, out WallrunnerGroundContact groundHit)
        {
            return TryFindGroundContactAtPosition(_environment.Position, probeDistance, out groundHit);
        }

        internal bool TryFindGroundContactAtPosition(Vector3 position, float probeDistance, out WallrunnerGroundContact groundHit)
        {
            groundHit = default;
            _environment.GetCapsulePoints(position, out Vector3 bottom, out _);
            float probeRadius = Mathf.Max(0.025f, _environment.CapsuleRadius * _settings.GroundContactRadiusMultiplier);
            Vector3 sphereOrigin = BuildGroundProbeOrigin(bottom, probeRadius);

            if (_environment.SphereCast(sphereOrigin, probeRadius, Vector3.down, out WallrunnerHit sphereHit, probeDistance)
                && IsStandableGround(sphereHit.Normal))
            {
                groundHit = new WallrunnerGroundContact(sphereHit.Point, sphereHit.Normal, ResolveSphereGroundSnapOffset(sphereHit.Distance));

                return true;
            }

            float bestSnapOffset = float.MaxValue;
            bool foundRayGround = TryProbeGroundRay(sphereOrigin, probeRadius, probeDistance, ref groundHit, ref bestSnapOffset);

            return foundRayGround;
        }

        private bool TryFindGroundFromRaycasts(Vector3 sphereOrigin, float probeRadius, float probeDistance, out WallrunnerGroundContact groundHit)
        {
            groundHit = default;
            float bestSnapOffset = float.MaxValue;
            float rayOffset = Mathf.Max(0.05f, probeRadius * 0.55f);
            Vector3 forward = Vector3.ProjectOnPlane(_environment.Forward, Vector3.up);
            if (forward.sqrMagnitude <= 0.001f)
            {
                forward = Vector3.forward;
            }

            forward.Normalize();
            Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;

            bool found = TryProbeGroundRay(sphereOrigin, probeRadius, probeDistance, ref groundHit, ref bestSnapOffset);
            found |= TryProbeGroundRay(sphereOrigin + forward * rayOffset, probeRadius, probeDistance, ref groundHit, ref bestSnapOffset);
            found |= TryProbeGroundRay(sphereOrigin - forward * rayOffset, probeRadius, probeDistance, ref groundHit, ref bestSnapOffset);
            found |= TryProbeGroundRay(sphereOrigin + right * rayOffset, probeRadius, probeDistance, ref groundHit, ref bestSnapOffset);
            found |= TryProbeGroundRay(sphereOrigin - right * rayOffset, probeRadius, probeDistance, ref groundHit, ref bestSnapOffset);
            return found;
        }

        private bool TryProbeGroundRay(Vector3 origin, float probeRadius, float probeDistance, ref WallrunnerGroundContact bestGroundHit, ref float bestSnapOffset)
        {
            float rayDistance = probeRadius + probeDistance + _settings.SkinWidth;
            if (!_environment.Raycast(origin, Vector3.down, out WallrunnerHit rayHit, rayDistance))
            {
                return false;
            }

            if (!IsStandableGround(rayHit.Normal))
            {
                return false;
            }

            float snapOffset = Mathf.Max(0f, origin.y - probeRadius - rayHit.Point.y - _settings.SkinWidth * 0.5f);
            if (snapOffset > probeDistance + _settings.SkinWidth || snapOffset >= bestSnapOffset)
            {
                return false;
            }

            bestSnapOffset = snapOffset;
            bestGroundHit = new WallrunnerGroundContact(rayHit.Point, rayHit.Normal, snapOffset);
            return true;
        }

        internal bool TryFindGround(float probeDistance, out WallrunnerGroundContact groundHit)
        {
            groundHit = default;
            _environment.GetCapsulePoints(_environment.Position, out Vector3 bottom, out _);
            float probeRadius = _environment.CapsuleRadius * 0.92f;
            Vector3 sphereOrigin = BuildGroundProbeOrigin(bottom, probeRadius);

            if (_environment.SphereCast(sphereOrigin, probeRadius, Vector3.down, out WallrunnerHit sphereHit, probeDistance)
                && IsStandableGround(sphereHit.Normal))
            {
                groundHit = new WallrunnerGroundContact(sphereHit.Point, sphereHit.Normal, ResolveSphereGroundSnapOffset(sphereHit.Distance));

                return true;
            }

            bool foundRayGround = TryFindGroundFromRaycasts(sphereOrigin, probeRadius, probeDistance, out groundHit);

            return foundRayGround;
        }
    }
}
