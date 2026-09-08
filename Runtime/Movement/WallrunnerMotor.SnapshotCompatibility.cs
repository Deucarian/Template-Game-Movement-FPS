using UnityEngine;

namespace Deucarian.TemplateGameMovementFps.Movement
{
    public sealed partial class WallrunnerMotor
    {
        public readonly struct RuntimeSnapshot
        {
            public RuntimeSnapshot(
                Vector3 position,
                Quaternion rotation,
                Vector3 velocity,
                Vector3 wallNormal,
                Vector3 groundNormal,
                Vector3 wallrunLockedNormal,
                Vector3 wallrunLockedDirection,
                Vector3 wallrunActiveDirection,
                Vector3 wallrunGuidanceDirection,
                Vector3 wallrunBlockedNormal,
                Vector3 wallrunChainLockedNormal,
                Vector3 wallrunChainLockedDirection,
                Transform wallrunSurfaceTransform,
                Vector3 bunnyHopCarryVelocity,
                Vector3 vaultStartPosition,
                Vector3 vaultTargetPosition,
                Vector3 vaultExitVelocity,
                Vector3 lookForward,
                float slideTimer,
                float wallrunTimer,
                float wallrunStartSpeed,
                float wallrunPeakSpeed,
                float wallrunSameWallLockoutTimer,
                float wallrunDetachReattachTimer,
                float slideEntrySpeed,
                float groundSnapLockoutTimer,
                float groundedJumpGraceTimer,
                float slideJumpGraceTimer,
                float airborneSlideBufferTimer,
                float bunnyHopWindowTimer,
                float vaultBlendTimer,
                float vaultBlendDuration,
                int airJumpsRemaining,
                bool grounded,
                bool wallrunFatigued,
                bool wallrunChainSameDirectionLocked,
                bool wallrunNearTop,
                WallrunTraversalStyle wallrunStyle,
                VaultTraversalStyle activeVaultStyle,
                VaultTraversalStyle lastVaultStyle,
                WallrunnerMovementState state)
            {
                Position = position;
                Rotation = rotation;
                Velocity = velocity;
                WallNormal = wallNormal;
                GroundNormal = groundNormal;
                WallrunLockedNormal = wallrunLockedNormal;
                WallrunLockedDirection = wallrunLockedDirection;
                WallrunActiveDirection = wallrunActiveDirection;
                WallrunGuidanceDirection = wallrunGuidanceDirection;
                WallrunBlockedNormal = wallrunBlockedNormal;
                WallrunChainLockedNormal = wallrunChainLockedNormal;
                WallrunChainLockedDirection = wallrunChainLockedDirection;
                WallrunSurfaceTransform = wallrunSurfaceTransform;
                BunnyHopCarryVelocity = bunnyHopCarryVelocity;
                VaultStartPosition = vaultStartPosition;
                VaultTargetPosition = vaultTargetPosition;
                VaultExitVelocity = vaultExitVelocity;
                LookForward = lookForward;
                SlideTimer = slideTimer;
                WallrunTimer = wallrunTimer;
                WallrunStartSpeed = wallrunStartSpeed;
                WallrunPeakSpeed = wallrunPeakSpeed;
                WallrunSameWallLockoutTimer = wallrunSameWallLockoutTimer;
                WallrunDetachReattachTimer = wallrunDetachReattachTimer;
                SlideEntrySpeed = slideEntrySpeed;
                GroundSnapLockoutTimer = groundSnapLockoutTimer;
                GroundedJumpGraceTimer = groundedJumpGraceTimer;
                SlideJumpGraceTimer = slideJumpGraceTimer;
                AirborneSlideBufferTimer = airborneSlideBufferTimer;
                BunnyHopWindowTimer = bunnyHopWindowTimer;
                VaultBlendTimer = vaultBlendTimer;
                VaultBlendDuration = vaultBlendDuration;
                AirJumpsRemaining = airJumpsRemaining;
                Grounded = grounded;
                WallrunFatigued = wallrunFatigued;
                WallrunChainSameDirectionLocked = wallrunChainSameDirectionLocked;
                WallrunNearTop = wallrunNearTop;
                WallrunStyle = wallrunStyle;
                ActiveVaultStyle = activeVaultStyle;
                LastVaultStyle = lastVaultStyle;
                State = state;
            }

            public Vector3 Position { get; }

            public Quaternion Rotation { get; }

            public Vector3 Velocity { get; }

            public Vector3 WallNormal { get; }

            public Vector3 GroundNormal { get; }

            public Vector3 WallrunLockedNormal { get; }

            public Vector3 WallrunLockedDirection { get; }

            public Vector3 WallrunActiveDirection { get; }

            public Vector3 WallrunGuidanceDirection { get; }

            public Vector3 WallrunBlockedNormal { get; }

            public Vector3 WallrunChainLockedNormal { get; }

            public Vector3 WallrunChainLockedDirection { get; }

            public Transform WallrunSurfaceTransform { get; }

            public Vector3 BunnyHopCarryVelocity { get; }

            public Vector3 VaultStartPosition { get; }

            public Vector3 VaultTargetPosition { get; }

            public Vector3 VaultExitVelocity { get; }

            public Vector3 LookForward { get; }

            public float SlideTimer { get; }

            public float WallrunTimer { get; }

            public float WallrunStartSpeed { get; }

            public float WallrunPeakSpeed { get; }

            public float WallrunSameWallLockoutTimer { get; }

            public float WallrunDetachReattachTimer { get; }

            public float SlideEntrySpeed { get; }

            public float GroundSnapLockoutTimer { get; }

            public float GroundedJumpGraceTimer { get; }

            public float SlideJumpGraceTimer { get; }

            public float AirborneSlideBufferTimer { get; }

            public float BunnyHopWindowTimer { get; }

            public float VaultBlendTimer { get; }

            public float VaultBlendDuration { get; }

            public int AirJumpsRemaining { get; }

            public bool Grounded { get; }

            public bool WallrunFatigued { get; }

            public bool WallrunChainSameDirectionLocked { get; }

            public bool WallrunNearTop { get; }

            public WallrunTraversalStyle WallrunStyle { get; }

            public VaultTraversalStyle ActiveVaultStyle { get; }

            public VaultTraversalStyle LastVaultStyle { get; }

            public WallrunnerMovementState State { get; }
        }

        public RuntimeSnapshot CaptureRuntimeSnapshot()
        {
            WallrunnerSnapshot snapshot = Session.CaptureSnapshot();
            return new RuntimeSnapshot(
                snapshot.Position,
                snapshot.Rotation,
                snapshot.Velocity,
                snapshot.WallNormal,
                snapshot.GroundNormal,
                snapshot.WallrunLockedNormal,
                snapshot.WallrunLockedDirection,
                snapshot.WallrunActiveDirection,
                snapshot.WallrunGuidanceDirection,
                snapshot.WallrunBlockedNormal,
                snapshot.WallrunChainLockedNormal,
                snapshot.WallrunChainLockedDirection,
                snapshot.WallrunSurfaceTransform as Transform,
                snapshot.BunnyHopCarryVelocity,
                snapshot.VaultStartPosition,
                snapshot.VaultTargetPosition,
                snapshot.VaultExitVelocity,
                snapshot.LookForward,
                snapshot.SlideTimer,
                snapshot.WallrunTimer,
                snapshot.WallrunStartSpeed,
                snapshot.WallrunPeakSpeed,
                snapshot.WallrunSameWallLockoutTimer,
                snapshot.WallrunDetachReattachTimer,
                snapshot.SlideEntrySpeed,
                snapshot.GroundSnapLockoutTimer,
                snapshot.GroundedJumpGraceTimer,
                snapshot.SlideJumpGraceTimer,
                snapshot.AirborneSlideBufferTimer,
                snapshot.BunnyHopWindowTimer,
                snapshot.VaultBlendTimer,
                snapshot.VaultBlendDuration,
                snapshot.AirJumpsRemaining,
                snapshot.Grounded,
                snapshot.WallrunFatigued,
                snapshot.WallrunChainSameDirectionLocked,
                snapshot.WallrunNearTop,
                snapshot.WallrunStyle,
                snapshot.ActiveVaultStyle,
                snapshot.LastVaultStyle,
                snapshot.State);
        }

        public void RestoreRuntimeSnapshot(RuntimeSnapshot snapshot)
        {
            Session.RestoreSnapshot(new WallrunnerSnapshot(
                snapshot.Position,
                snapshot.Rotation,
                snapshot.Velocity,
                snapshot.WallNormal,
                snapshot.GroundNormal,
                snapshot.WallrunLockedNormal,
                snapshot.WallrunLockedDirection,
                snapshot.WallrunActiveDirection,
                snapshot.WallrunGuidanceDirection,
                snapshot.WallrunBlockedNormal,
                snapshot.WallrunChainLockedNormal,
                snapshot.WallrunChainLockedDirection,
                snapshot.WallrunSurfaceTransform,
                snapshot.BunnyHopCarryVelocity,
                snapshot.VaultStartPosition,
                snapshot.VaultTargetPosition,
                snapshot.VaultExitVelocity,
                snapshot.LookForward,
                snapshot.SlideTimer,
                snapshot.WallrunTimer,
                snapshot.WallrunStartSpeed,
                snapshot.WallrunPeakSpeed,
                snapshot.WallrunSameWallLockoutTimer,
                snapshot.WallrunDetachReattachTimer,
                snapshot.SlideEntrySpeed,
                snapshot.GroundSnapLockoutTimer,
                snapshot.GroundedJumpGraceTimer,
                snapshot.SlideJumpGraceTimer,
                snapshot.AirborneSlideBufferTimer,
                snapshot.BunnyHopWindowTimer,
                snapshot.VaultBlendTimer,
                snapshot.VaultBlendDuration,
                snapshot.AirJumpsRemaining,
                snapshot.Grounded,
                snapshot.WallrunFatigued,
                snapshot.WallrunChainSameDirectionLocked,
                snapshot.WallrunNearTop,
                snapshot.WallrunStyle,
                snapshot.ActiveVaultStyle,
                snapshot.LastVaultStyle,
                snapshot.State));
        }
    }
}
