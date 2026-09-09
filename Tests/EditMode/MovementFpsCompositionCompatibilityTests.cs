using System;
using System.Linq;
using System.Reflection;
using Deucarian.TemplateGameMovementFps.Actors;
using Deucarian.TemplateGameMovementFps.Movement;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Deucarian.TemplateGameMovementFps.Tests
{
    public sealed class MovementFpsCompositionCompatibilityTests
    {
        [TestCase(typeof(MovementFpsTemplateController),
            "buildSampleArenaOnAwake|enemySpawningEnabled|escalationMultiplier|maxAliveOverride|miniBossSpawningEnabled|spawnBatchMultiplier|spawnIntervalMultiplier",
            "56b3eb70b02540c59ad444b56b8baf84")]
        [TestCase(typeof(MovementFpsPlayerController),
            "baseFieldOfView|cameraFeelFollowSpeed|mouseSensitivity|movementCameraFeelEnabled|muzzle|slideCameraDrop|slideCameraFollowSpeed|slideCameraRollDegrees|slideFieldOfViewKick|speedFieldOfViewKick|speedFieldOfViewReference|vaultFieldOfViewKick|viewCamera|wallrunCameraRollDegrees|wallrunFieldOfViewKick",
            "ad519239fa8d446f8c0e64dce8453ff8")]
        public void ComponentSerializedFieldsAndScriptGuidRemainStable(Type component, string expected, string guid)
        {
            string[] fields = component.GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                .Where(field => field.IsDefined(typeof(SerializeField), false))
                .Select(field => field.Name).OrderBy(name => name, StringComparer.Ordinal).ToArray();
            CollectionAssert.AreEqual(expected.Split('|'), fields);
            string path = AssetDatabase.GUIDToAssetPath(guid);
            MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
            Assert.That(script, Is.Not.Null);
            Assert.That(script.GetClass(), Is.EqualTo(component));
        }

        [Test]
        public void PublicPlayerQueriesAndNullTargetsRemainValidBeforeInitialization()
        {
            var root = new GameObject("Uninitialized player compatibility");
            try
            {
                root.AddComponent<CapsuleCollider>();
                var player = root.AddComponent<MovementFpsPlayerController>();
                Assert.That(player.GunDamage, Is.Zero);
                Assert.That(player.CurrentHealth, Is.Zero);
                Assert.That(player.IsAlive, Is.False);
                Assert.That(player.FireAt(null), Is.Null);
                Assert.That(player.FireProjectileAtForTest(null), Is.Null);
                var gun = BasicMovementFpsGame.CreateCarbineDefinition();
                Assert.That(player.AddGun(gun), Is.True);
                Assert.That(player.GunDamage, Is.EqualTo(gun.Damage));
                Assert.That(player.HasGun(gun.Id), Is.True);
                Assert.That(player.PickupRadius, Is.EqualTo(4.2f));
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
        }

        [Test]
        public void PublicPlayerInitializationWithoutSessionStillConfiguresHealthAndCamera()
        {
            var root = new GameObject("Sessionless player compatibility");
            try
            {
                root.AddComponent<CapsuleCollider>();
                var cameraObject = new GameObject("Player Camera");
                cameraObject.transform.SetParent(root.transform, false);
                cameraObject.transform.localPosition = Vector3.up * 0.62f;
                var view = cameraObject.AddComponent<Camera>();
                var player = root.AddComponent<MovementFpsPlayerController>();
                var definition = BasicMovementFpsGame.CreatePlayerDefinition();
                var gun = BasicMovementFpsGame.CreateCarbineDefinition();
                player.Initialize(null, definition, new[] { gun }, null);
                Assert.That(player.IsAlive, Is.True);
                Assert.That(player.CurrentHealth, Is.EqualTo(definition.MaximumHealth));
                Assert.That(player.GunDamage, Is.EqualTo(gun.Damage));
                Assert.That(player.ViewCamera, Is.SameAs(view));
                Assert.That(view.fieldOfView, Is.EqualTo(76f));
                Assert.That(root.transform.position, Is.EqualTo(Vector3.up * 1.2f));
                Assert.That(view.transform.Find("Muzzle"), Is.Not.Null);
                player.ResetPlayer(Vector3.up * 2f, Quaternion.identity);
                Assert.That(player.CurrentHealth, Is.EqualTo(definition.MaximumHealth));
                Assert.That(root.transform.position, Is.EqualTo(Vector3.up * 2f));
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
        }

        [Test]
        public void InputReaderDisableAndReenableThenDisposeReleasesOnlyItsActions()
        {
            var before = InputSystem.ListEnabledActions();
            var reader = new FpsInputReader();
            try
            {
                reader.Enable();
                InputAction[] owned = InputSystem.ListEnabledActions().Except(before).ToArray();
                Assert.That(owned.Length, Is.EqualTo(9));
                reader.Disable();
                Assert.That(owned.All(action => !action.enabled), Is.True);
                reader.Enable();
                Assert.That(owned.All(action => action.enabled), Is.True);
                reader.Dispose();
                reader.Dispose();
                Assert.That(InputSystem.ListEnabledActions().Intersect(owned), Is.Empty);
                Assert.That(InputSystem.ListEnabledActions(), Is.EquivalentTo(before));
                Assert.Throws<ObjectDisposedException>(() => reader.Read());
            }
            finally { reader.Dispose(); }
        }

        [Test]
        public void RunLoadoutAndHealthOwnersHaveNoSceneOrPresentationReferences()
        {
            foreach (Type type in new[] { typeof(MovementFpsRunSession), typeof(MovementFpsPlayerLoadout), typeof(MovementFpsPlayerHealth) })
            {
                Assert.That(typeof(UnityEngine.Object).IsAssignableFrom(type), Is.False, type.Name);
                foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    Assert.That(typeof(UnityEngine.Object).IsAssignableFrom(field.FieldType), Is.False, field.Name);
                    Assert.That(field.FieldType, Is.Not.EqualTo(typeof(MovementFpsFeedbackPresenter)), field.Name);
                    Assert.That(field.FieldType, Is.Not.EqualTo(typeof(MovementFpsHudPresenter)), field.Name);
                }
            }
        }
    }
}
