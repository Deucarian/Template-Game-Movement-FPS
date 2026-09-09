using System;
using Deucarian.TemplateGameMovementFps.Definitions;
using Deucarian.TemplateGameMovementFps.Movement;
using NUnit.Framework;
using UnityEngine;

namespace Deucarian.TemplateGameMovementFps.Tests
{
    public sealed class WallrunnerSessionTests
    {
        [Test]
        public void AirControlSteersWithoutDiscardingCarriedMomentum()
        {
            var environment = new WallrunnerTestEnvironment();
            var session = new WallrunnerSession(new WallrunnerSettings(), environment);
            session.SetVelocity(Vector3.forward * 20f + Vector3.up * 5f);
            session.Tick(Vector2.right, false, false, false, 0.1f);
            Assert.That(Vector3.ProjectOnPlane(session.Velocity, Vector3.up).magnitude, Is.EqualTo(20f).Within(0.0001f));
            Assert.That(session.Velocity.x, Is.GreaterThan(0f));
            Assert.That(session.Velocity.y, Is.EqualTo(2.6f).Within(0.0001f));
            Assert.That(session.State, Is.EqualTo(WallrunnerMovementState.Airborne));
        }

        [TestCase(true, 10f)]
        [TestCase(false, 19f)]
        public void VelocityLimitAffectsOnlyLateralSpeed(bool hardCap, float expected)
        {
            var settings = new WallrunnerSettings
            {
                Gravity = 0f, MaxVelocityEnabled = true, MaxVelocityMetersPerSecond = 10f,
                MaxVelocityHardCap = hardCap, MaxVelocitySoftPullPerSecond = 5f
            };
            var session = new WallrunnerSession(settings, new WallrunnerTestEnvironment());
            session.SetVelocity(Vector3.forward * 20f + Vector3.up * 3f);
            session.Tick(Vector2.zero, false, false, false, 0.2f);
            Assert.That(session.Velocity.z, Is.EqualTo(expected).Within(0.0001f));
            Assert.That(session.Velocity.y, Is.EqualTo(3f));
        }

        [Test]
        public void AirJumpIsConsumedOnceAndGroundContactRefillsIt()
        {
            var environment = new WallrunnerTestEnvironment();
            var session = new WallrunnerSession(new WallrunnerSettings(), environment);
            session.Tick(Vector2.zero, false, false, false, true, false, 0.02f);
            float firstJumpVelocity = session.Velocity.y;
            Assert.That(firstJumpVelocity, Is.EqualTo(8.3f * 0.94f - 24f * 0.02f).Within(0.0001f));
            Assert.That(session.AirJumpsRemaining, Is.Zero);
            session.Tick(Vector2.zero, false, false, false, true, false, 0.02f);
            Assert.That(session.Velocity.y, Is.LessThan(firstJumpVelocity));
            environment.Ground = true;
            session.SetVelocity(Vector3.down);
            session.Tick(Vector2.zero, false, false, false, 0.2f);
            Assert.That(session.IsGrounded, Is.True);
            Assert.That(session.AirJumpsRemaining, Is.EqualTo(1));
        }

        [Test]
        public void SlideUsesEntryMomentumAndJumpTransitionsBeforeMovement()
        {
            var environment = new WallrunnerTestEnvironment { Ground = true };
            var session = new WallrunnerSession(new WallrunnerSettings(), environment);
            session.SetVelocity(Vector3.forward * 12f);
            session.Tick(Vector2.up, true, true, true, false, false, 0.02f);
            Assert.That(session.State, Is.EqualTo(WallrunnerMovementState.Sliding));
            Assert.That(session.Velocity.z, Is.EqualTo(12f * 1.18f).Within(0.0001f));
            float slideSpeed = session.Velocity.z;
            session.Tick(Vector2.up, true, false, false, true, false, 0.02f);
            Assert.That(session.State, Is.EqualTo(WallrunnerMovementState.Airborne));
            Assert.That(session.Velocity.z, Is.GreaterThan(slideSpeed));
            Assert.That(session.Velocity.y, Is.EqualTo(8.3f - 24f * 0.02f).Within(0.0001f));
        }

        [Test]
        public void AirborneSlideBufferStartsSlideOnLanding()
        {
            var environment = new WallrunnerTestEnvironment();
            var session = new WallrunnerSession(new WallrunnerSettings(), environment);
            session.SetVelocity(Vector3.forward * 12f);
            session.Tick(Vector2.up, true, true, false, false, false, 0.02f);
            environment.Ground = true;
            session.Tick(Vector2.up, true, false, false, false, false, 0.02f);
            Assert.That(session.State, Is.EqualTo(WallrunnerMovementState.Sliding));
        }

        [TestCase(0.02f, true)]
        [TestCase(0.2f, false)]
        public void GroundJumpGraceExpiresWithoutGrantingAnAirJump(float airborneTime, bool canJump)
        {
            var environment = new WallrunnerTestEnvironment { Ground = true };
            var session = new WallrunnerSession(new WallrunnerSettings { ExtraAirJumps = 0 }, environment);
            session.Tick(Vector2.zero, false, false, false, 0.02f);
            environment.Ground = false;
            session.Tick(Vector2.zero, false, false, false, airborneTime);
            session.Tick(Vector2.zero, false, false, false, true, false, 0.01f);
            Assert.That(session.Velocity.y > 0f, Is.EqualTo(canJump));
            Assert.That(session.AirJumpsRemaining, Is.Zero);
        }

        [Test]
        public void SteepContactIsNotStandableGround()
        {
            var environment = new WallrunnerTestEnvironment
            {
                Ground = true, GroundNormal = new Vector3(0f, 0.25f, 1f).normalized
            };
            var session = new WallrunnerSession(new WallrunnerSettings(), environment);
            session.Tick(Vector2.zero, false, false, false, 0.02f);
            Assert.That(session.IsGrounded, Is.False);
            Assert.That(session.State, Is.EqualTo(WallrunnerMovementState.Airborne));
        }

        [Test]
        public void LandingBunnyHopPreservesFasterIncomingVelocity()
        {
            var environment = new WallrunnerTestEnvironment { Ground = true };
            var session = new WallrunnerSession(new WallrunnerSettings(), environment);
            session.SetVelocity(Vector3.forward * 20f + Vector3.down);
            session.Tick(Vector2.up, true, false, false, true, false, 0.02f);
            Assert.That(session.Velocity.z, Is.EqualTo(20f * 1.04f).Within(0.0001f));
            Assert.That(session.IsGrounded, Is.False);
        }

        [Test]
        public void WallJumpLocksSameDirectionReentryAndRestoresAirJump()
        {
            var environment = new WallrunnerTestEnvironment { Wall = true };
            var session = new WallrunnerSession(new WallrunnerSettings(), environment);
            session.SetVelocity(Vector3.forward * 8f);
            session.Tick(Vector2.up, true, false, false, 0.02f);
            Assert.That(session.State, Is.EqualTo(WallrunnerMovementState.Wallrunning));
            session.Tick(Vector2.up, true, false, false, true, false, 0.02f);
            Assert.That(session.State, Is.EqualTo(WallrunnerMovementState.Airborne));
            Assert.That(session.Velocity.x, Is.LessThan(0f));
            Assert.That(session.AirJumpsRemaining, Is.EqualTo(1));
            session.Tick(Vector2.up, true, false, false, 0.02f);
            Assert.That(session.State, Is.EqualTo(WallrunnerMovementState.Airborne));
            Assert.That(session.CaptureSnapshot().WallrunChainSameDirectionLocked, Is.True);
        }

        [Test]
        public void WallFatigueAndSurfaceLossHaveDistinctReattachDelay()
        {
            var settings = new WallrunnerSettings { WallrunDetachReattachDelayEnabled = true };
            var environment = new WallrunnerTestEnvironment { Wall = true };
            var session = new WallrunnerSession(settings, environment);
            session.SetVelocity(Vector3.forward * 8f);
            session.Tick(Vector2.up, true, false, false, 0.02f);
            environment.Wall = false;
            session.Tick(Vector2.up, true, false, false, 0.02f);
            Assert.That(session.CaptureSnapshot().WallrunDetachReattachTimer, Is.EqualTo(0.1f));
            Assert.That(session.State, Is.EqualTo(WallrunnerMovementState.Airborne));
            environment.Wall = true;
            var fatigued = new WallrunnerSession(settings, environment);
            fatigued.SetVelocity(Vector3.forward * 8f);
            fatigued.Tick(Vector2.up, true, false, false, 2f);
            Assert.That(fatigued.CaptureSnapshot().WallrunFatigued, Is.True);
            Assert.That(fatigued.CaptureSnapshot().WallrunDetachReattachTimer, Is.Zero);
        }

        [Test]
        public void VerticalWallrunRequiresUpwardLookAndRetainsItsStyle()
        {
            var environment = new WallrunnerTestEnvironment { Wall = true, WallNormal = Vector3.back };
            var session = new WallrunnerSession(new WallrunnerSettings(), environment);
            session.SetVelocity(Vector3.forward * 8f);
            session.SetLookDirection(Vector3.forward + Vector3.up * 0.5f);
            session.Tick(Vector2.up, true, false, false, 0.02f);
            Assert.That(session.ActiveWallrunStyle, Is.EqualTo(WallrunTraversalStyle.Vertical));
            Assert.That(session.Velocity.y, Is.GreaterThan(8f));
            session.SetLookDirection(Vector3.forward);
            session.Tick(Vector2.up, true, false, false, 0.02f);
            Assert.That(session.ActiveWallrunStyle, Is.EqualTo(WallrunTraversalStyle.Vertical));
        }

        [TestCase(VaultMode.Hybrid, 1f, VaultTraversalStyle.Flow)]
        [TestCase(VaultMode.Hybrid, 2f, VaultTraversalStyle.SafetyMantle)]
        [TestCase(VaultMode.SafetyMantle, 1f, VaultTraversalStyle.SafetyMantle)]
        [TestCase(VaultMode.Flow, 2f, VaultTraversalStyle.Flow)]
        public void VaultStyleAndBlendCompleteWithoutRetickingCollision(VaultMode mode, float height, VaultTraversalStyle style)
        {
            var environment = new WallrunnerTestEnvironment { Ledge = true, LedgeHeight = height };
            var session = new WallrunnerSession(new WallrunnerSettings { VaultMode = mode }, environment);
            session.SetVelocity(Vector3.forward * 8f);
            session.Tick(Vector2.up, true, false, false, true, true, 0.02f);
            Assert.That(session.ActiveVaultStyle, Is.EqualTo(style));
            WallrunnerSnapshot snapshot = session.CaptureSnapshot();
            int queries = environment.Queries;
            session.Tick(Vector2.zero, false, false, false, false, false, 1f);
            Assert.That(environment.Queries, Is.EqualTo(queries));
            Assert.That(environment.Position, Is.EqualTo(snapshot.VaultTargetPosition));
            Assert.That(session.State, Is.EqualTo(WallrunnerMovementState.Airborne));
            Assert.That(session.ActiveVaultStyle, Is.EqualTo(VaultTraversalStyle.None));
            Assert.That(session.LastVaultStyle, Is.EqualTo(style));
        }

        [Test]
        public void BlockedVaultFallsBackToJump()
        {
            var environment = new WallrunnerTestEnvironment { Ledge = true, BlockVault = true };
            var session = new WallrunnerSession(new WallrunnerSettings(), environment);
            session.Tick(Vector2.up, false, false, false, true, true, 0.02f);
            Assert.That(session.State, Is.EqualTo(WallrunnerMovementState.Airborne));
            Assert.That(session.LastVaultStyle, Is.EqualTo(VaultTraversalStyle.None));
            Assert.That(session.AirJumpsRemaining, Is.Zero);
        }

        [Test]
        public void CollisionProjectsVelocityOntoContactPlane()
        {
            var environment = new WallrunnerTestEnvironment
            {
                NextCollision = new WallrunnerHit(Vector3.zero, Vector3.back, 0.1f)
            };
            var session = new WallrunnerSession(new WallrunnerSettings { Gravity = 0f }, environment);
            session.SetVelocity(new Vector3(2f, 0f, 8f));
            session.Tick(Vector2.zero, false, false, false, 0.02f);
            Assert.That(session.Velocity, Is.EqualTo(Vector3.right * 2f));
        }

        [Test]
        public void SnapshotRestoreReplaysSameTickAndSessionsRemainIndependent()
        {
            var environment = new WallrunnerTestEnvironment { Wall = true };
            var session = new WallrunnerSession(new WallrunnerSettings(), environment);
            session.SetVelocity(Vector3.forward * 8f);
            session.Tick(Vector2.up, true, false, false, 0.02f);
            WallrunnerSnapshot before = session.CaptureSnapshot();
            session.Tick(Vector2.up, true, false, false, 0.1f);
            WallrunnerSnapshot after = session.CaptureSnapshot();
            session.RestoreSnapshot(before);
            session.Tick(Vector2.up, true, false, false, 0.1f);
            Assert.That(session.CaptureSnapshot(), Is.EqualTo(after));
            Assert.That(environment.Synchronizations, Is.EqualTo(1));
            var independent = new WallrunnerSession(new WallrunnerSettings(), new WallrunnerTestEnvironment());
            independent.Reset(Vector3.zero);
            Assert.That(session.CaptureSnapshot(), Is.EqualTo(after));
        }

        [Test]
        public void MissingDependenciesAreRejectedAtConstruction()
        {
            Assert.Throws<ArgumentNullException>(() => new WallrunnerSession(null, new WallrunnerTestEnvironment()));
            Assert.Throws<ArgumentNullException>(() => new WallrunnerSession(new WallrunnerSettings(), null));
        }
    }
}
