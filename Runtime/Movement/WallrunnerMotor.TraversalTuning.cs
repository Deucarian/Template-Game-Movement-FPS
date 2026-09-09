using Deucarian.TemplateGameMovementFps.Definitions;
using UnityEngine;

namespace Deucarian.TemplateGameMovementFps.Movement
{
    public sealed partial class WallrunnerMotor
    {
        public bool VerticalWallrunEnabled
        {
            get => verticalWallrunEnabled;
            set => verticalWallrunEnabled = value;
        }

        public float VerticalWallrunLookMaxAngleDegrees
        {
            get => verticalWallrunLookMaxAngleDegrees;
            set => verticalWallrunLookMaxAngleDegrees = Mathf.Clamp(value, 1f, 89f);
        }

        public float VerticalWallrunMinimumLookUpAngleDegrees
        {
            get => verticalWallrunMinimumLookUpAngleDegrees;
            set => verticalWallrunMinimumLookUpAngleDegrees = Mathf.Clamp(value, 0f, 75f);
        }

        public float VerticalWallrunMoveMaxAngleDegrees
        {
            get => verticalWallrunMoveMaxAngleDegrees;
            set => verticalWallrunMoveMaxAngleDegrees = Mathf.Clamp(value, 1f, 89f);
        }

        public float VerticalWallrunMinimumEntrySpeedFraction
        {
            get => verticalWallrunMinimumEntrySpeedFraction;
            set => verticalWallrunMinimumEntrySpeedFraction = Mathf.Clamp01(value);
        }

        public float VerticalWallrunUpSpeed
        {
            get => verticalWallrunUpSpeed;
            set => verticalWallrunUpSpeed = Mathf.Max(0.1f, value);
        }

        public float VerticalWallrunDurationSeconds
        {
            get => verticalWallrunDurationSeconds;
            set => verticalWallrunDurationSeconds = Mathf.Max(0.05f, value);
        }

        public float VerticalWallrunLateDecelStart
        {
            get => verticalWallrunLateDecelStart;
            set => verticalWallrunLateDecelStart = Mathf.Clamp(value, 0.1f, 0.95f);
        }

        public float VerticalWallrunLateDecelExponent
        {
            get => verticalWallrunLateDecelExponent;
            set => verticalWallrunLateDecelExponent = Mathf.Max(1f, value);
        }

        public float VerticalWallrunEndSpeedRetention
        {
            get => verticalWallrunEndSpeedRetention;
            set => verticalWallrunEndSpeedRetention = Mathf.Clamp01(value);
        }

        public float VerticalWallrunWallStickSpeed
        {
            get => verticalWallrunWallStickSpeed;
            set => verticalWallrunWallStickSpeed = Mathf.Max(0f, value);
        }

        public VaultMode VaultMode
        {
            get => vaultMode;
            set => vaultMode = value;
        }

        public bool VaultHoldAssistEnabled
        {
            get => vaultHoldAssistEnabled;
            set => vaultHoldAssistEnabled = value;
        }

        public float VaultFlowMaxHeightMeters
        {
            get => vaultFlowMaxHeightMeters;
            set
            {
                vaultFlowMaxHeightMeters = Mathf.Max(0.25f, value);
                vaultMantleMaxHeightMeters = Mathf.Max(vaultFlowMaxHeightMeters, vaultMantleMaxHeightMeters);
                mantleHeight = Mathf.Max(mantleHeight, vaultMantleMaxHeightMeters);
            }
        }

        public float VaultMantleMaxHeightMeters
        {
            get => vaultMantleMaxHeightMeters;
            set
            {
                vaultMantleMaxHeightMeters = Mathf.Max(VaultFlowMaxHeightMeters, value);
                mantleHeight = Mathf.Max(mantleHeight, vaultMantleMaxHeightMeters);
            }
        }

        public float FlowVaultOverDistanceMeters
        {
            get => flowVaultOverDistanceMeters;
            set => flowVaultOverDistanceMeters = Mathf.Max(0.05f, value);
        }

        public float SafetyMantleOverDistanceMeters
        {
            get => safetyMantleOverDistanceMeters;
            set => safetyMantleOverDistanceMeters = Mathf.Max(0.05f, value);
        }

        public float FlowVaultBlendDurationSeconds
        {
            get => flowVaultBlendDurationSeconds;
            set => flowVaultBlendDurationSeconds = Mathf.Clamp(value, 0.02f, 0.3f);
        }

        public float SafetyMantleBlendDurationSeconds
        {
            get => safetyMantleBlendDurationSeconds;
            set => safetyMantleBlendDurationSeconds = Mathf.Clamp(value, 0.04f, 0.6f);
        }

        public float FlowVaultBaseSpeedMultiplier
        {
            get => flowVaultBaseSpeedMultiplier;
            set => flowVaultBaseSpeedMultiplier = Mathf.Max(0.1f, value);
        }

        public float FlowVaultPerfectSpeedMultiplier
        {
            get => flowVaultPerfectSpeedMultiplier;
            set => flowVaultPerfectSpeedMultiplier = Mathf.Max(FlowVaultBaseSpeedMultiplier, value);
        }

        public float SafetyMantleBaseSpeedMultiplier
        {
            get => safetyMantleBaseSpeedMultiplier;
            set => safetyMantleBaseSpeedMultiplier = Mathf.Max(0.1f, value);
        }

        public float SafetyMantlePerfectSpeedMultiplier
        {
            get => safetyMantlePerfectSpeedMultiplier;
            set => safetyMantlePerfectSpeedMultiplier = Mathf.Max(SafetyMantleBaseSpeedMultiplier, value);
        }

        public float FlowVaultMinimumVerticalBoost
        {
            get => flowVaultMinimumVerticalBoost;
            set => flowVaultMinimumVerticalBoost = Mathf.Clamp01(value);
        }

        public float FlowVaultPerfectVerticalBoost
        {
            get => flowVaultPerfectVerticalBoost;
            set => flowVaultPerfectVerticalBoost = Mathf.Max(FlowVaultMinimumVerticalBoost, Mathf.Clamp01(value));
        }

        public float SafetyMantleMinimumVerticalBoost
        {
            get => safetyMantleMinimumVerticalBoost;
            set => safetyMantleMinimumVerticalBoost = Mathf.Clamp01(value);
        }

        public float SafetyMantlePerfectVerticalBoost
        {
            get => safetyMantlePerfectVerticalBoost;
            set => safetyMantlePerfectVerticalBoost = Mathf.Max(SafetyMantleMinimumVerticalBoost, Mathf.Clamp01(value));
        }

        public float VaultBlendDurationSeconds
        {
            get => FlowVaultBlendDurationSeconds;
            set => FlowVaultBlendDurationSeconds = value;
        }

        public float WallJumpLookWeight
        {
            get => wallJumpLookWeight;
            set => wallJumpLookWeight = Mathf.Clamp(value, 0.2f, 0.9f);
        }

        public float WallJumpAwayWeight
        {
            get => wallJumpAwayWeight;
            set => wallJumpAwayWeight = Mathf.Clamp(value, 0.1f, 0.6f);
        }

        public float WallJumpCarryWeight
        {
            get => wallJumpCarryWeight;
            set => wallJumpCarryWeight = Mathf.Clamp(value, 0f, 0.5f);
        }

        public float WallJumpMinimumAwayDot
        {
            get => wallJumpMinimumAwayDot;
            set => wallJumpMinimumAwayDot = Mathf.Clamp(value, 0f, 0.75f);
        }

        public float WallrunSameWallLockoutSeconds
        {
            get => wallrunSameWallLockoutSeconds;
            set => wallrunSameWallLockoutSeconds = Mathf.Max(0f, value);
        }

        public bool WallrunDetachReattachDelayEnabled
        {
            get => wallrunDetachReattachDelayEnabled;
            set
            {
                wallrunDetachReattachDelayEnabled = value;
                if (!wallrunDetachReattachDelayEnabled)
                {
                    if (_session != null) _session.ClearDetachDelay();
                }
            }
        }

        public float WallrunDetachReattachDelaySeconds
        {
            get => wallrunDetachReattachDelaySeconds;
            set => wallrunDetachReattachDelaySeconds = Mathf.Max(0f, value);
        }

        public float WallrunReverseReentryDot
        {
            get => wallrunReverseReentryDot;
            set => wallrunReverseReentryDot = Mathf.Clamp(value, -1f, 0f);
        }

        public float WallrunMaximumSurfaceTurnAngle
        {
            get => wallrunMaximumSurfaceTurnAngle;
            set => wallrunMaximumSurfaceTurnAngle = Mathf.Clamp(value, 1f, 89f);
        }

        public float WallrunUpperProbeHeightFraction
        {
            get => wallrunUpperProbeHeightFraction;
            set => wallrunUpperProbeHeightFraction = Mathf.Clamp(value, 0.25f, 0.5f);
        }

        public float WallrunTopFlattenProbeHeightFraction
        {
            get => wallrunTopFlattenProbeHeightFraction;
            set => wallrunTopFlattenProbeHeightFraction = Mathf.Clamp(value, 0.43f, 0.65f);
        }

        public float WallrunTopUpwardVelocityMultiplier
        {
            get => wallrunTopUpwardVelocityMultiplier;
            set => wallrunTopUpwardVelocityMultiplier = Mathf.Clamp01(value);
        }

        public float WallrunCurvedSurfaceProbeForwardBias
        {
            get => wallrunCurvedSurfaceProbeForwardBias;
            set => wallrunCurvedSurfaceProbeForwardBias = Mathf.Clamp01(value);
        }

        public float WallrunHorizontalWallStickSpeed
        {
            get => wallrunHorizontalWallStickSpeed;
            set => wallrunHorizontalWallStickSpeed = Mathf.Clamp(value, 0f, 4f);
        }

    }
}
