using UnityEngine;

namespace Deucarian.TemplateGameMovementFps.Movement
{
    internal sealed class WallrunnerState
    {
        private readonly WallrunnerSettings _settings;

        internal WallrunnerState(WallrunnerSettings settings)
        {
            _settings = settings;
        }

        internal Vector3 Velocity;
        internal Vector3 WallNormal;
        internal float SlideTimer;
        internal float WallrunTimer;
        internal float WallrunStartSpeed;
        internal float WallrunPeakSpeed;
        internal float WallrunSameWallLockoutTimer;
        internal float WallrunDetachReattachTimer;
        internal float SlideEntrySpeed;
        internal int AirJumpsRemaining;
        internal bool Grounded;
        internal bool WallrunFatigued;
        internal Vector3 GroundNormal = Vector3.up;
        internal Vector3 WallrunLockedNormal;
        internal Vector3 WallrunLockedDirection;
        internal Vector3 WallrunActiveDirection;
        internal Vector3 WallrunGuidanceDirection;
        internal Vector3 WallrunBlockedNormal;
        internal Vector3 WallrunChainLockedNormal;
        internal Vector3 WallrunChainLockedDirection;
        internal object WallrunSurfaceTransform;
        internal bool WallrunChainSameDirectionLocked;
        internal bool WallrunNearTop;
        internal float GroundSnapLockoutTimer;
        internal WallrunTraversalStyle WallrunStyle;
        internal float GroundedJumpGraceTimer;
        internal float SlideJumpGraceTimer;
        internal float AirborneSlideBufferTimer;
        internal float BunnyHopWindowTimer;
        internal Vector3 BunnyHopCarryVelocity;
        internal float VaultBlendTimer;
        internal float VaultBlendDuration;
        internal Vector3 VaultStartPosition;
        internal Vector3 VaultTargetPosition;
        internal Vector3 VaultExitVelocity;
        internal Vector3 LookForward;
        internal VaultTraversalStyle ActiveVaultStyle;
        internal VaultTraversalStyle LastVaultStyle;
        internal WallrunnerMovementState State;

        internal void ClearWallrunChainLock()
        {
            WallrunChainLockedNormal = Vector3.zero;
            WallrunChainLockedDirection = Vector3.zero;
            WallrunChainSameDirectionLocked = false;
        }

        internal void ResetAirJumps()
        {
            AirJumpsRemaining = Mathf.Max(0, _settings.ExtraAirJumps);
        }
    }
}
