using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Deucarian.TemplateGameMovementFps.Actors;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Deucarian.TemplateGameMovementFps.PlayModeTests
{
    public sealed class MovementFpsCompositionPlayModeTests
    {
        [UnityTest]
        public IEnumerator GeneratedMaterialsAndFeedbackClipsFollowTheirOwnersLifetime()
        {
            var priorClips = new HashSet<AudioClip>(Resources.FindObjectsOfTypeAll<AudioClip>());
            GameObject root = new GameObject("FPS composition lifetime test");
            try
            {
                var controller = root.AddComponent<MovementFpsTemplateController>();
                controller.EnsureBootstrapped();
                yield return null;
                AudioClip[] clips = Resources.FindObjectsOfTypeAll<AudioClip>()
                    .Where(clip => !priorClips.Contains(clip) && clip.name.StartsWith("movement-fps-")).ToArray();
                Assert.That(clips.Length, Is.EqualTo(6));
                var enemy = controller.SpawnEnemyForTest(controller.Player.transform.position + Vector3.forward * 5f);
                var projectile = controller.FireProjectileAtEnemyForTest(enemy);
                Assert.That(projectile, Is.Not.Null);
                Material[] projectileMaterials = projectile.GetComponents<Renderer>().Select(renderer => renderer.sharedMaterial).ToArray();
                Assert.That(projectileMaterials.Length, Is.GreaterThanOrEqualTo(2));
                Object.Destroy(projectile.gameObject);
                yield return null;
                yield return null;
                Assert.That(projectileMaterials.All(material => material == null), Is.True, "Projectile materials must not remain until the entire run ends.");
                Assert.That(clips.All(clip => clip != null), Is.True, "Feedback resources remain owned by the live template.");

                Material[] worldMaterials = root.GetComponentsInChildren<MovementFpsMaterialOwner>()
                    .SelectMany(owner => owner.GetComponents<Renderer>()).Select(renderer => renderer.sharedMaterial).ToArray();
                Assert.That(worldMaterials.Length, Is.GreaterThanOrEqualTo(16));
                Object.Destroy(root);
                yield return null;
                yield return null;
                Assert.That(worldMaterials.All(material => material == null), Is.True);
                Assert.That(clips.All(clip => clip == null), Is.True);
            }
            finally { if (root != null) Object.DestroyImmediate(root); }
        }

        [UnityTest]
        public IEnumerator PublicPlayerResetRestoresCameraHealthAndStartingLoadout()
        {
            GameObject root = new GameObject("FPS player reset composition test");
            try
            {
                var controller = root.AddComponent<MovementFpsTemplateController>();
                controller.EnsureBootstrapped();
                yield return null;
                MovementFpsPlayerController player = controller.Player;
                Camera camera = player.ViewCamera;
                Vector3 stand = camera.transform.localPosition;
                var guns = player.Guns;
                camera.fieldOfView = 105f;
                camera.transform.localPosition = new Vector3(1f, -1f, 2f);
                camera.transform.localRotation = Quaternion.Euler(30f, 10f, 12f);
                player.ApplyDamage(25d);
                player.CurrentGun.TryFire(1f);
                player.ResetPlayer(new Vector3(3f, 4f, 5f), Quaternion.Euler(0f, 90f, 0f));
                Assert.That(player.transform.position, Is.EqualTo(new Vector3(3f, 4f, 5f)));
                Assert.That(camera.transform.localPosition, Is.EqualTo(stand));
                Assert.That(Quaternion.Angle(camera.transform.localRotation, Quaternion.identity), Is.LessThan(0.001f));
                Assert.That(camera.fieldOfView, Is.EqualTo(76f));
                Assert.That(player.CurrentHealth, Is.EqualTo(player.MaximumHealth));
                Assert.That(player.Guns, Is.SameAs(guns));
                Assert.That(player.Guns.Count, Is.EqualTo(2));
                Assert.That(player.AutoPowers.Count, Is.EqualTo(3));
                Assert.That(player.CurrentGun.Ammo, Is.EqualTo(player.CurrentGun.Definition.MagazineSize));
            }
            finally { Object.DestroyImmediate(root); }
        }
    }
}
