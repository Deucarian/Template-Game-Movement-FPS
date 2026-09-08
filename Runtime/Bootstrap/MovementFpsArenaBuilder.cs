using System;
using System.Collections.Generic;
using Deucarian.Common;
using Deucarian.Combat;
using Deucarian.RunUpgrades;
using Deucarian.TemplateGameMovementFps.Actors;
using Deucarian.TemplateGameMovementFps.Combat;
using Deucarian.TemplateGameMovementFps.Movement;
using Deucarian.TemplateGameMovementFps.Progression;
using Deucarian.TemplateGameMovementFps.Run;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Deucarian.TemplateGameMovementFps
{
    internal sealed class MovementFpsArenaBuilder
    {
        private readonly Transform _runtimeRoot;
        internal MovementFpsArenaBuilder(Transform root) { _runtimeRoot = root; }

        internal void Build()
        {
            CreateSolid("Arena Floor", new Vector3(0f, -0.55f, 0f), new Vector3(42f, 1f, 42f), new Color(0.24f, 0.25f, 0.27f, 1f));
            CreateSolid("Left Wallrun Wall", new Vector3(-7f, 2f, 0f), new Vector3(0.45f, 4f, 22f), new Color(0.16f, 0.22f, 0.31f, 1f));
            CreateSolid("Right Wallrun Wall", new Vector3(7f, 2f, 0f), new Vector3(0.45f, 4f, 22f), new Color(0.16f, 0.22f, 0.31f, 1f));
            CreateSolid("Forward Wallrun Gate Left", new Vector3(-3.2f, 2.4f, 13f), new Vector3(0.42f, 4.8f, 8f), new Color(0.18f, 0.2f, 0.34f, 1f));
            CreateSolid("Forward Wallrun Gate Right", new Vector3(3.2f, 2.4f, 13f), new Vector3(0.42f, 4.8f, 8f), new Color(0.18f, 0.2f, 0.34f, 1f));
            CreateSolid("Back Slide Ramp", new Vector3(0f, 0.35f, -14f), new Vector3(6.5f, 0.7f, 5.4f), Quaternion.Euler(-12f, 0f, 0f), new Color(0.3f, 0.28f, 0.2f, 1f));
            CreateSolid("Side Transfer Ramp Left", new Vector3(-10.5f, 0.45f, 4f), new Vector3(5.6f, 0.75f, 3.2f), Quaternion.Euler(0f, 0f, -10f), new Color(0.28f, 0.25f, 0.18f, 1f));
            CreateSolid("Side Transfer Ramp Right", new Vector3(10.5f, 0.45f, 4f), new Vector3(5.6f, 0.75f, 3.2f), Quaternion.Euler(0f, 0f, 10f), new Color(0.28f, 0.25f, 0.18f, 1f));
            CreateSolid("Low Flow Vault", new Vector3(0f, 0.55f, -1f), new Vector3(2.2f, 1.1f, 0.7f), new Color(0.32f, 0.28f, 0.18f, 1f));
            CreateSolid("Tall Safety Mantle", new Vector3(3.8f, 1.05f, 3.5f), new Vector3(2.2f, 2.1f, 0.7f), new Color(0.28f, 0.24f, 0.34f, 1f));
            CreateSolid("Enemy Spawn Read North", new Vector3(0f, 0.05f, 17.5f), new Vector3(8f, 0.1f, 0.35f), new Color(0.68f, 0.16f, 0.2f, 1f));
            CreateSolid("Enemy Spawn Read East", new Vector3(17.5f, 0.05f, 0f), new Vector3(0.35f, 0.1f, 8f), new Color(0.68f, 0.16f, 0.2f, 1f));
            CreateSolid("Enemy Spawn Read West", new Vector3(-17.5f, 0.05f, 0f), new Vector3(0.35f, 0.1f, 8f), new Color(0.68f, 0.16f, 0.2f, 1f));

            GameObject lightObject = new GameObject("Arena Directional Light");
            lightObject.transform.SetParent(_runtimeRoot, false);
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.15f;
        }

        private void CreateSolid(string name, Vector3 position, Vector3 scale, Color color)
        {
            CreateSolid(name, position, scale, Quaternion.identity, color);
        }

        private void CreateSolid(string name, Vector3 position, Vector3 scale, Quaternion rotation, Color color)
        {
            GameObject solid = GameObject.CreatePrimitive(PrimitiveType.Cube);
            solid.name = name;
            solid.transform.SetParent(_runtimeRoot, false);
            solid.transform.SetPositionAndRotation(position, rotation);
            solid.transform.localScale = scale;
            Renderer renderer = solid.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = color;
                MovementFpsMaterialOwner.Own(renderer);
            }
        }

    }
}
