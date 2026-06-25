using Deucarian.TemplateGameMovementFps;
using UnityEngine;

namespace Deucarian.TemplateGameMovementFps.BasicMovementFpsGame
{
    public sealed class BasicMovementFpsGameBootstrap : MonoBehaviour
    {
        [SerializeField]
        private MovementFpsTemplateController controller;

        private void Awake()
        {
            if (controller == null)
            {
                controller = GetComponent<MovementFpsTemplateController>();
            }

            if (controller == null)
            {
                controller = gameObject.AddComponent<MovementFpsTemplateController>();
            }

            controller.EnsureBootstrapped();
        }
    }
}
