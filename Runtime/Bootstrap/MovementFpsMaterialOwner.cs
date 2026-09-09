using System.Collections.Generic;
using Deucarian.Common;
using UnityEngine;

namespace Deucarian.TemplateGameMovementFps
{
    // Generated visual materials follow the generated object's lifetime, including
    // projectiles and pickups removed between run restarts.
    internal sealed class MovementFpsMaterialOwner : MonoBehaviour
    {
        private readonly List<Material> _materials = new List<Material>();

        internal static void Own(Renderer renderer)
        {
            if (renderer == null) return;
            MovementFpsMaterialOwner owner = renderer.GetComponent<MovementFpsMaterialOwner>();
            if (owner == null) owner = renderer.gameObject.AddComponent<MovementFpsMaterialOwner>();
            Material material = renderer.material;
            if (!owner._materials.Contains(material)) owner._materials.Add(material);
        }

        private void OnDestroy()
        {
            foreach (Material material in _materials) UnityObjectUtility.DestroySafely(material);
            _materials.Clear();
        }
    }
}
