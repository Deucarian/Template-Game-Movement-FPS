using System;
using System.Linq;
using System.Reflection;
using Deucarian.TemplateGameMovementFps.Movement;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Deucarian.TemplateGameMovementFps.Tests
{
    public sealed class WallrunnerCompatibilityTests
    {
        [Test]
        public void LegacySerializedFieldNamesAndComponentGuidRemainUnchanged()
        {
            // Frozen against origin/develop 09c3219, before the composition refactor.
            string expected =
                "airAcceleration|airborneSlideBufferSeconds|bunnyHopEnabled|bunnyHopLandingWindowSeconds|" +
                "bunnyHopSpeedMultiplier|bunnyHopSteerStrength|collisionMask|downhillGroundStickExtraLookAhead|" +
                "downhillGroundStickExtraProbeDistance|downhillGroundStickFadeEndAngleDegrees|downhillGroundStickFadeStartAngleDegrees|downhillGroundStickMinimumAngleDegrees|" +
                "extraAirJumps|flowVaultBaseSpeedMultiplier|flowVaultBlendDurationSeconds|flowVaultMinimumVerticalBoost|" +
                "flowVaultOverDistanceMeters|flowVaultPerfectSpeedMultiplier|flowVaultPerfectVerticalBoost|gravity|" +
                "groundAcceleration|groundContactProbeDistance|groundContactRadiusMultiplier|groundSnapBreakAngleDegrees|" +
                "groundSnapDistance|groundSnapReleaseDropAngleDegrees|groundSnapReleaseMinimumDrop|groundedJumpGraceSeconds|" +
                "jumpGroundLockoutSeconds|jumpVelocity|mantleCheckDistance|mantleHeight|" +
                "mantleMaximumWallNormalY|mantleMinimumApproachDot|mantleMinimumTopNormalY|mantleVaultOverDistance|" +
                "maxVelocityEnabled|maxVelocityHardCap|maxVelocityMetersPerSecond|maxVelocitySoftPullPerSecond|" +
                "safetyMantleBaseSpeedMultiplier|safetyMantleBlendDurationSeconds|safetyMantleMinimumVerticalBoost|safetyMantleOverDistanceMeters|" +
                "safetyMantlePerfectSpeedMultiplier|safetyMantlePerfectVerticalBoost|skinWidth|slideDownhillEnterSpeedMultiplier|" +
                "slideDownhillSpeedCapMultiplier|slideDownhillSpeedGainPerSecond|slideDragDelayFraction|slideDragRampExponent|" +
                "slideDurationSeconds|slideEnterSpeedMultiplier|slideEntrySpeedDecayResistanceExponent|slideEntrySpeedDelayExponent|" +
                "slideGroundSnapBreakAngleDegrees|slideGroundSnapDistance|slideGroundSnapReleaseDropAngleDegrees|slideJumpMinimumSpeedMultiplier|" +
                "slideJumpPeakSpeedMultiplier|slideJumpSpeedCapMultiplier|slideMinimumEntrySpeedFraction|slideSlopeEffectMinimumAngleDegrees|" +
                "slideSpeed|slideSpeedDecayPerSecond|slideUphillSpeedLossPerSecond|sprintSpeed|" +
                "standableGroundMaxAngleDegrees|vaultBaseSpeedMultiplier|vaultBlendDurationSeconds|vaultFlowMaxHeightMeters|" +
                "vaultHoldAssistEnabled|vaultMantleMaxHeightMeters|vaultMinimumSpeedFraction|vaultMinimumVerticalBoost|" +
                "vaultMode|vaultPerfectSpeedMultiplier|vaultPerfectVerticalBoost|verticalWallrunDurationSeconds|" +
                "verticalWallrunEnabled|verticalWallrunEndSpeedRetention|verticalWallrunLateDecelExponent|verticalWallrunLateDecelStart|" +
                "verticalWallrunLookMaxAngleDegrees|verticalWallrunMinimumEntrySpeedFraction|verticalWallrunMinimumLookUpAngleDegrees|verticalWallrunMoveMaxAngleDegrees|" +
                "verticalWallrunUpSpeed|verticalWallrunWallStickSpeed|walkSpeed|wallJumpApexTimingWindow|" +
                "wallJumpAwayWeight|wallJumpCarryWeight|wallJumpLookWeight|wallJumpMinimumAwayDot|" +
                "wallJumpMinimumSpeedFraction|wallJumpPeakTimingMultiplier|wallJumpSpeedCapMultiplier|wallJumpSpeedMultiplier|" +
                "wallJumpTurnaroundSpeedMultiplier|wallJumpVerticalVelocity|wallrunApexHeight|wallrunApexSpeedMultiplier|" +
                "wallrunApexTime|wallrunCurvedSurfaceProbeForwardBias|wallrunDetachReattachDelayEnabled|wallrunDetachReattachDelaySeconds|" +
                "wallrunDurationSeconds|wallrunEndSpeedRetention|wallrunFatigueGravityMultiplier|wallrunGravity|" +
                "wallrunHorizontalWallStickSpeed|wallrunLateDecelExponent|wallrunLateDecelStart|wallrunMaxDownwardVelocity|" +
                "wallrunMaximumSurfaceTurnAngle|wallrunPostApexRetention|wallrunReverseReentryDot|wallrunSameWallLockoutSeconds|" +
                "wallrunSpeed|wallrunTopFlattenProbeHeightFraction|wallrunTopUpwardVelocityMultiplier|wallrunUpperProbeHeightFraction";
            string[] actual = typeof(WallrunnerMotor).GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                .Where(field => field.IsDefined(typeof(SerializeField), false))
                .Select(field => field.Name).OrderBy(name => name, StringComparer.Ordinal).ToArray();
            CollectionAssert.AreEqual(expected.Split('|'), actual);
            var player = new GameObject("Wallrunner serialization compatibility");
            try
            {
                var motor = player.AddComponent<WallrunnerMotor>();
                string path = AssetDatabase.GetAssetPath(MonoScript.FromMonoBehaviour(motor));
                Assert.That(AssetDatabase.AssetPathToGUID(path), Is.EqualTo("8eedfc15c59b420bb912580260833ed3"));
                var serialized = new SerializedObject(motor);
                SerializedProperty probe = serialized.FindProperty("groundContactProbeDistance");
                SerializedProperty assist = serialized.FindProperty("vaultHoldAssistEnabled");
                SerializedProperty speed = serialized.FindProperty("maxVelocityMetersPerSecond");
                Assert.That(probe, Is.Not.Null, "Ground tuning must remain visible to Unity serialization.");
                Assert.That(assist, Is.Not.Null, "Vault tuning must remain visible to Unity serialization.");
                Assert.That(speed, Is.Not.Null, "Velocity tuning must remain visible to Unity serialization.");
                probe.floatValue = 0.3f;
                assist.boolValue = false;
                speed.floatValue = 25f;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                string checkpoint = EditorJsonUtility.ToJson(motor);
                Assert.That(checkpoint, Does.Contain("groundContactProbeDistance"));
                Assert.That(motor.GroundContactProbeDistance, Is.EqualTo(0.3f));
                probe.floatValue = 0.12f;
                assist.boolValue = true;
                speed.floatValue = 36f;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                Assert.That(motor.GroundContactProbeDistance, Is.EqualTo(0.12f));
                // Editor JSON carries Unity's native object envelope and script
                // identity. Restore its actual saved form rather than a partial
                // hand-written payload with no native envelope.
                EditorJsonUtility.FromJsonOverwrite(checkpoint, motor);
                Assert.That(motor.GroundContactProbeDistance, Is.EqualTo(0.3f));
                Assert.That(motor.VaultHoldAssistEnabled, Is.False);
                Assert.That(motor.MaxVelocityMetersPerSecond, Is.EqualTo(25f));
            }
            finally { UnityEngine.Object.DestroyImmediate(player); }
        }

        [Test]
        public void LegacySnapshotForwardsAllValuesAndPoseToSingleSession()
        {
            var player = new GameObject("Wallrunner snapshot compatibility");
            try
            {
                var motor = player.AddComponent<WallrunnerMotor>();
                motor.ResetMotor(new Vector3(2f, 20f, 3f));
                motor.SetVelocity(new Vector3(4f, 5f, 6f));
                motor.SetLookDirection(Vector3.forward + Vector3.up);
                motor.SetWallrunGuidanceDirection(Vector3.right);
                var expected = motor.CaptureRuntimeSnapshot();
                motor.ResetMotor(Vector3.zero);
                motor.SetVelocity(Vector3.zero);
                motor.RestoreRuntimeSnapshot(expected);
                Assert.That(motor.CaptureRuntimeSnapshot(), Is.EqualTo(expected));
                Assert.That(motor.Velocity, Is.EqualTo(new Vector3(4f, 5f, 6f)));
                Assert.That(player.transform.position, Is.EqualTo(expected.Position));
            }
            finally { UnityEngine.Object.DestroyImmediate(player); }
        }

        [TestCase(true)]
        [TestCase(false)]
        public void CollisionQueriesPreserveCapsuleEnabledState(bool enabled)
        {
            var player = new GameObject("Wallrunner query lifetime");
            try
            {
                var motor = player.AddComponent<WallrunnerMotor>();
                CapsuleCollider capsule = player.GetComponent<CapsuleCollider>();
                capsule.enabled = enabled;
                motor.ResetMotor(Vector3.up * 30f);
                motor.SetVelocity(Vector3.forward * 8f);
                motor.Tick(Vector2.up, true, false, false, true, true, 0.02f);
                Assert.That(capsule.enabled, Is.EqualTo(enabled));
            }
            finally { UnityEngine.Object.DestroyImmediate(player); }
        }

        [Test]
        public void PolicyAndStateTypesDoNotOwnUnityObjectsOrComponentInheritance()
        {
            Type[] types = typeof(WallrunnerSession).Assembly.GetTypes()
                .Where(type => type.Namespace == typeof(WallrunnerSession).Namespace
                    && type.Name.StartsWith("Wallrunner", StringComparison.Ordinal)
                    && !type.Name.StartsWith("WallrunnerMotor", StringComparison.Ordinal)).ToArray();
            Assert.That(types.Length, Is.GreaterThan(10));
            foreach (Type type in types)
            {
                Assert.That(typeof(UnityEngine.Object).IsAssignableFrom(type), Is.False, type.Name);
                foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                    Assert.That(typeof(UnityEngine.Object).IsAssignableFrom(field.FieldType), Is.False, type.Name + "." + field.Name);
            }
        }
    }
}
