using UnityEngine;

namespace Deucarian.TemplateGameMovementFps.Movement
{
    internal sealed class WallrunnerCollisionPolicy
    {
        private readonly WallrunnerSettings _settings;
        private readonly WallrunnerState _state;
        private readonly IWallrunnerEnvironment _environment;

        internal WallrunnerCollisionPolicy(WallrunnerSettings settings, WallrunnerState state, IWallrunnerEnvironment environment)
        {
            _settings = settings;
            _state = state;
            _environment = environment;
        }

        internal void MoveWithCollision(Vector3 displacement)
        {
            Vector3 remaining = displacement;
            for (int iteration = 0; iteration < 3; iteration++)
            {
                if (remaining.sqrMagnitude <= 0.000001f)
                {
                    break;
                }

                _environment.GetCapsulePoints(_environment.Position, out Vector3 bottom, out Vector3 top);

                if (_environment.CapsuleCast(bottom, top, _environment.CapsuleRadius, remaining.normalized, out WallrunnerHit hit, remaining.magnitude + _settings.SkinWidth))
                {
                    Vector3 velocityBeforeHit = _state.Velocity;
                    float moveDistance = Mathf.Max(0f, hit.Distance - _settings.SkinWidth);
                    _environment.Position += remaining.normalized * moveDistance;
                    Vector3 unconsumed = remaining - remaining.normalized * moveDistance;
                    bool floorContact = hit.Normal.y > _settings.MinimumGroundNormalY && remaining.y <= 0f;
                    if (floorContact && Vector3.ProjectOnPlane(velocityBeforeHit, Vector3.up).sqrMagnitude <= 0.0001f)
                    {
                        remaining = Vector3.zero;
                        _state.Velocity = Vector3.zero;
                    }
                    else
                    {
                        remaining = Vector3.ProjectOnPlane(unconsumed, hit.Normal);
                        _state.Velocity = floorContact ? Vector3.ProjectOnPlane(velocityBeforeHit, Vector3.up) : Vector3.ProjectOnPlane(velocityBeforeHit, hit.Normal);
                    }
                }
                else
                {
                    _environment.Position += remaining;
                    break;
                }
            }
        }
    }
}
