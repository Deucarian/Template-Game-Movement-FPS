using System.Collections;
using System.Reflection;
using Deucarian.TemplateGameMovementFps.Actors;
using Deucarian.TemplateGameMovementFps.Combat;
using Deucarian.TemplateGameMovementFps.Movement;
using Deucarian.TemplateGameMovementFps.Run;
using Deucarian.RunUpgrades;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Deucarian.TemplateGameMovementFps.PlayModeTests
{
    public sealed class MovementFpsTemplatePlayModeTests
    {
        private const string PresentationRootName = "Movement FPS Presentation";
        private const string WeaponPulseName = "Movement FPS Weapon Pulse";
        private const string PowerPulseName = "Movement FPS Power Pulse";
        private const string EnemyPulseName = "Movement FPS Enemy Pulse";
        private const string PickupPulseName = "Movement FPS Pickup Pulse";
        private const string RunPulseName = "Movement FPS Run State Pulse";
        private const string PresentationAudioName = "Movement FPS Feedback Audio";

        [UnityTest]
        public IEnumerator TemplateBootsWithPlayerArenaAndEnemy()
        {
            MovementFpsTemplateController controller = CreateController();
            yield return null;

            Assert.IsNotNull(controller.Player);
            Assert.IsNotNull(controller.Player.ViewCamera);
            Assert.That(controller.EnemyCount, Is.GreaterThanOrEqualTo(1));
            Assert.IsFalse(controller.DraftOpen);
            Assert.IsFalse(controller.Defeated);

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator TemplateCreatesPresentationFeedbackAndFullStarterLoadout()
        {
            MovementFpsTemplateController controller = CreateController();
            yield return null;

            Assert.IsNotNull(GameObject.Find(PresentationRootName));
            Assert.IsNotNull(GameObject.Find(WeaponPulseName));
            Assert.IsNotNull(GameObject.Find(PowerPulseName));
            Assert.IsNotNull(GameObject.Find(EnemyPulseName));
            Assert.IsNotNull(GameObject.Find(PickupPulseName));
            Assert.IsNotNull(GameObject.Find(RunPulseName));
            Assert.IsNotNull(GameObject.Find(PresentationAudioName));
            Assert.That(Object.FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None).Length, Is.GreaterThanOrEqualTo(5));
            Assert.That(Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None).Length, Is.GreaterThanOrEqualTo(1));
            Assert.That(controller.Player.Guns.Count, Is.GreaterThanOrEqualTo(2));
            Assert.That(controller.Player.AutoPowers.Count, Is.GreaterThanOrEqualTo(3));

            MovementFpsEnemyActor enemy = controller.SpawnEnemyForTest(controller.Player.transform.position + Vector3.forward * 5f);
            Assert.IsNotNull(controller.FireProjectileAtEnemyForTest(enemy));
            yield return null;

            Assert.That(Object.FindObjectsByType<TrailRenderer>(FindObjectsSortMode.None).Length, Is.GreaterThanOrEqualTo(1));

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator WallrunnerMotorMovesForwardWhenTicked()
        {
            GameObject ground = CreateGround();
            GameObject player = CreateMotorPlayer(new Vector3(0f, 1.1f, 0f), out WallrunnerMotor motor);
            yield return null;

            Vector3 start = player.transform.position;
            motor.Tick(Vector2.up, true, false, false, 0.1f);

            Assert.That(player.transform.position.z, Is.GreaterThan(start.z));

            Object.Destroy(player);
            Object.Destroy(ground);
        }

        [UnityTest]
        public IEnumerator SprintSlideAndDoubleJumpSmoke()
        {
            GameObject ground = CreateGround();
            GameObject player = CreateMotorPlayer(new Vector3(0f, 1.1f, 0f), out WallrunnerMotor motor);
            yield return null;

            motor.Tick(Vector2.up, true, false, false, 0.12f);
            float sprintSpeed = Vector3.ProjectOnPlane(motor.Velocity, Vector3.up).magnitude;

            SetPrivateField(motor, "_velocity", Vector3.forward * 12f);
            motor.Tick(Vector2.up, true, true, true, false, false, 0.02f);
            Assert.That(sprintSpeed, Is.GreaterThan(0f));
            Assert.That(motor.State, Is.EqualTo(WallrunnerMovementState.Sliding));

            Object.Destroy(player);
            Object.Destroy(ground);
            yield return null;

            GameObject airbornePlayer = CreateMotorPlayer(new Vector3(0f, 2f, 0f), out WallrunnerMotor airborneMotor);
            SetPrivateField(airborneMotor, "<State>k__BackingField", WallrunnerMovementState.Airborne);
            SetPrivateField(airborneMotor, "_grounded", false);
            SetPrivateField(airborneMotor, "_groundedJumpGraceTimer", 0f);
            SetPrivateField(airborneMotor, "_airJumpsRemaining", 1);
            SetPrivateField(airborneMotor, "_velocity", Vector3.forward * 4f);
            airborneMotor.Tick(Vector2.up, true, false, false, true, false, 0.02f);

            Assert.That(airborneMotor.Velocity.y, Is.GreaterThan(0f));

            Object.Destroy(airbornePlayer);
        }

        [UnityTest]
        public IEnumerator WallrunAndWallJumpSmoke()
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.transform.position = new Vector3(0.9f, 2f, 0f);
            wall.transform.localScale = new Vector3(0.2f, 4f, 10f);
            GameObject player = CreateMotorPlayer(new Vector3(0f, 2f, 0f), out WallrunnerMotor motor);
            SetPrivateField(motor, "_groundedJumpGraceTimer", 0f);
            SetPrivateField(motor, "_velocity", Vector3.forward * 8f);
            Physics.SyncTransforms();
            yield return null;

            motor.Tick(Vector2.up, true, false, false, false, false, 0.02f);
            Assert.That(motor.State, Is.EqualTo(WallrunnerMovementState.Wallrunning));

            SetPrivateField(motor, "_airJumpsRemaining", 0);
            motor.Tick(Vector2.up, true, false, false, true, false, 0.02f);

            Assert.That(motor.State, Is.EqualTo(WallrunnerMovementState.Airborne));
            Assert.That(motor.AirJumpsRemaining, Is.EqualTo(1));

            Object.Destroy(player);
            Object.Destroy(wall);
        }

        [UnityTest]
        public IEnumerator GunDamageCanKillEnemy()
        {
            MovementFpsTemplateController controller = CreateController();
            yield return null;

            int startingEnemies = controller.EnemyCount;
            MovementFpsEnemyActor enemy = controller.SpawnEnemyForTest(controller.Player.transform.position + Vector3.forward * 4f);
            controller.FireAtEnemyForTest(enemy);
            controller.FireAtEnemyForTest(enemy);
            yield return null;

            Assert.IsFalse(enemy != null && enemy.IsAlive);
            Assert.That(controller.EnemyCount, Is.LessThanOrEqualTo(startingEnemies));

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator ProjectileLauncherDamageCanKillEnemy()
        {
            MovementFpsTemplateController controller = CreateController();
            yield return null;

            MovementFpsEnemyDefinition bulwark = controller.GetEnemyDefinitionForTest("enemy.bone-bulwark");
            MovementFpsEnemyActor enemy = controller.SpawnEnemyForTest(bulwark, controller.Player.transform.position + Vector3.forward * 5f);
            MovementFpsProjectileActor first = controller.FireProjectileAtEnemyForTest(enemy);
            Assert.IsNotNull(first);
            yield return new WaitForSeconds(0.25f);

            MovementFpsProjectileActor second = controller.FireProjectileAtEnemyForTest(enemy);
            Assert.IsNotNull(second);
            yield return new WaitForSeconds(0.25f);

            MovementFpsProjectileActor third = controller.FireProjectileAtEnemyForTest(enemy);
            Assert.IsNotNull(third);
            yield return new WaitForSeconds(0.25f);

            Assert.IsFalse(enemy != null && enemy.IsAlive);

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator ProjectileUpgradeAffectsRuntimeStats()
        {
            MovementFpsTemplateController controller = CreateController();
            yield return null;

            double previousSpeed = controller.Progression.ProjectileSpeedMultiplier;
            Assert.IsTrue(controller.ApplyUpgradeByIdForTest(BasicMovementFpsGame.ProjectileSpeedUpgradeId).Succeeded);

            Assert.That(controller.Progression.ProjectileSpeedMultiplier, Is.GreaterThan(previousSpeed));

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator OrbitPulseDamageCanKillEnemy()
        {
            MovementFpsTemplateController controller = CreateController();
            yield return null;

            MovementFpsEnemyActor enemy = controller.SpawnEnemyForTest(controller.Player.transform.position + Vector3.forward * 2f);
            for (int index = 0; index < 6; index++)
            {
                controller.TickPlayerPowersForTest(BasicMovementFpsGame.CreateOrbitPulseDefinition().CooldownSeconds + 0.1f);
            }

            yield return null;

            Assert.IsFalse(enemy != null && enemy.IsAlive);

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator ChainBoltDamageCanKillEnemy()
        {
            MovementFpsTemplateController controller = CreateController();
            yield return null;

            MovementFpsEnemyActor enemy = controller.SpawnEnemyForTest(controller.Player.transform.position + Vector3.forward * 8f);
            controller.AddPowerForTest(BasicMovementFpsGame.CreateChainBoltDefinition());
            for (int index = 0; index < 4; index++)
            {
                controller.TickPlayerPowersForTest(BasicMovementFpsGame.CreateChainBoltDefinition().CooldownSeconds + 0.1f);
            }

            yield return null;

            Assert.IsFalse(enemy != null && enemy.IsAlive);

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator GroundRiftDamageCanKillEnemy()
        {
            MovementFpsTemplateController controller = CreateController();
            yield return null;

            MovementFpsEnemyActor enemy = controller.SpawnEnemyForTest(controller.Player.transform.position + Vector3.forward * 7f);
            controller.AddPowerForTest(BasicMovementFpsGame.CreateGroundRiftDefinition());
            for (int index = 0; index < 3; index++)
            {
                controller.TickPlayerPowersForTest(BasicMovementFpsGame.CreateGroundRiftDefinition().CooldownSeconds + 0.1f);
            }

            yield return null;

            Assert.IsFalse(enemy != null && enemy.IsAlive);

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator ExperienceCollectionOpensDraftAndUpgradeApplies()
        {
            MovementFpsTemplateController controller = CreateController();
            yield return null;

            controller.CollectExperienceForTest(controller.Progression.RequiredExperience);
            Assert.IsTrue(controller.DraftOpen);
            Assert.AreEqual(3, controller.Progression.CurrentDraft.Count);

            double previousDamage = controller.Progression.GunDamageBonus;
            double previousCadence = controller.Progression.GunCadenceMultiplier;
            double previousPickup = controller.Progression.PickupRadiusBonus;
            int previousGunCount = controller.Player.Guns.Count;
            int previousPowerCount = controller.Player.AutoPowers.Count;
            RunUpgradeSelectionResult result = controller.ChooseDraftForTest(0);
            Assert.IsTrue(result.Succeeded);

            Assert.IsFalse(controller.DraftOpen);
            Assert.AreEqual(1, controller.CurrentRunSummary.UpgradesChosen.Count);
            bool starterUnlockWasAlreadyOnline =
                result.Id.Equals(BasicMovementFpsGame.RiftLauncherUnlockUpgradeId) ||
                result.Id.Equals(BasicMovementFpsGame.ChainBoltUnlockUpgradeId) ||
                result.Id.Equals(BasicMovementFpsGame.GroundRiftUnlockUpgradeId);
            Assert.IsTrue(
                controller.Progression.GunDamageBonus > previousDamage ||
                controller.Progression.GunCadenceMultiplier > previousCadence ||
                controller.Progression.PickupRadiusBonus > previousPickup ||
                controller.Progression.ProjectileSpeedMultiplier > 0d ||
                controller.Progression.AutoPowerDamageMultiplier > 0d ||
                controller.Player.Guns.Count > previousGunCount ||
                controller.Player.AutoPowers.Count > previousPowerCount ||
                starterUnlockWasAlreadyOnline);

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator HordeEscalationSpawnsPressureEnemies()
        {
            MovementFpsTemplateController controller = CreateController();
            yield return null;

            int startingEnemies = controller.EnemyCount;
            controller.TickRunForTest(76f);
            yield return null;

            Assert.That(controller.RunElapsedSeconds, Is.GreaterThanOrEqualTo(76f));
            Assert.That(controller.CurrentSpawnSnapshot.BatchSize, Is.GreaterThanOrEqualTo(3));
            Assert.That(controller.CurrentSpawnSnapshot.MaxAlive, Is.GreaterThanOrEqualTo(34));
            Assert.That(controller.EnemyCount, Is.GreaterThan(startingEnemies));

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator MinibossSpawnsAndDeathTriggersVictory()
        {
            MovementFpsTemplateController controller = CreateController();
            yield return null;

            controller.TickRunForTest(115.1f);
            yield return null;

            MovementFpsEnemyActor miniboss = null;
            for (int index = 0; index < controller.Enemies.Count; index++)
            {
                if (controller.Enemies[index] != null && controller.Enemies[index].IsMiniBoss)
                {
                    miniboss = controller.Enemies[index];
                    break;
                }
            }

            Assert.IsTrue(controller.MiniBossSpawned);
            Assert.IsNotNull(miniboss);
            Assert.IsFalse(controller.Victory);

            controller.KillEnemyForTest(miniboss);
            yield return null;

            Assert.IsTrue(controller.MiniBossDefeated);
            Assert.IsTrue(controller.Victory);
            Assert.AreEqual(MovementFpsRunState.Victory, controller.RunState);
            MovementFpsRunSummary summary = controller.CurrentRunSummary;
            Assert.AreEqual(MovementFpsRunOutcome.Victory, summary.Outcome);
            Assert.That(summary.KillCount, Is.GreaterThanOrEqualTo(1));
            Assert.AreEqual(1, summary.MiniBossKills);
            Assert.AreEqual(1, summary.Rewards.Count);
            Assert.AreEqual(BasicMovementFpsGame.ChoirOgreRewardId, summary.Rewards[0].Id);

            controller.RestartRunForTest();
            yield return null;

            MovementFpsRunSummary reset = controller.CurrentRunSummary;
            Assert.AreEqual(MovementFpsRunOutcome.Running, reset.Outcome);
            Assert.AreEqual(0, reset.KillCount);
            Assert.AreEqual(0, reset.Rewards.Count);

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator DefeatSummaryTracksExperienceAndRestartResets()
        {
            MovementFpsTemplateController controller = CreateController();
            yield return null;

            controller.CollectExperienceForTest(6);
            controller.ApplyPlayerDamage(999d);
            yield return null;

            MovementFpsRunSummary summary = controller.CurrentRunSummary;
            Assert.IsTrue(controller.Defeated);
            Assert.AreEqual(MovementFpsRunOutcome.Defeat, summary.Outcome);
            Assert.AreEqual(6, summary.ExperienceGained);
            Assert.IsTrue(summary.Completed);

            controller.RestartRunForTest();
            yield return null;

            MovementFpsRunSummary reset = controller.CurrentRunSummary;
            Assert.AreEqual(MovementFpsRunOutcome.Running, reset.Outcome);
            Assert.AreEqual(0, reset.ExperienceGained);
            Assert.AreEqual(0, reset.UpgradesChosen.Count);

            Object.Destroy(controller.gameObject);
        }

        [UnityTest]
        public IEnumerator PlayerCanDieAndRestart()
        {
            MovementFpsTemplateController controller = CreateController();
            yield return null;

            controller.ApplyPlayerDamage(999d);
            yield return null;

            Assert.IsTrue(controller.Defeated);
            controller.RestartRunForTest();
            yield return null;

            Assert.IsFalse(controller.Defeated);
            Assert.IsFalse(controller.Victory);
            Assert.AreEqual(MovementFpsRunState.Running, controller.RunState);
            Assert.That(controller.Player.CurrentHealth, Is.GreaterThan(0d));
            Assert.That(controller.EnemyCount, Is.GreaterThanOrEqualTo(1));

            Object.Destroy(controller.gameObject);
        }

        private static MovementFpsTemplateController CreateController()
        {
            var root = new GameObject("Movement FPS Template PlayMode Test");
            MovementFpsTemplateController controller = root.AddComponent<MovementFpsTemplateController>();
            controller.EnsureBootstrapped();
            return controller;
        }

        private static GameObject CreateGround()
        {
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.transform.position = new Vector3(0f, -0.6f, 0f);
            ground.transform.localScale = new Vector3(20f, 1f, 20f);
            Physics.SyncTransforms();
            return ground;
        }

        private static GameObject CreateMotorPlayer(Vector3 position, out WallrunnerMotor motor)
        {
            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.transform.position = position;
            motor = player.AddComponent<WallrunnerMotor>();
            motor.ResetMotor(player.transform.position);
            Physics.SyncTransforms();
            return player;
        }

        private static void SetPrivateField<T>(WallrunnerMotor motor, string fieldName, T value)
        {
            FieldInfo field = typeof(WallrunnerMotor).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null);
            field.SetValue(motor, value);
        }
    }
}
