using System;
using UnityEngine;

namespace Deucarian.TemplateGameMovementFps.Combat
{
    public sealed class MovementFpsAutoPowerRuntimeState
    {
        private float _cooldown;

        public MovementFpsAutoPowerRuntimeState(MovementFpsAutoPowerDefinition definition)
        {
            Definition = string.IsNullOrWhiteSpace(definition.Id) ? throw new ArgumentException("Auto power definition is required.", nameof(definition)) : definition;
            _cooldown = definition.CooldownSeconds;
        }

        public MovementFpsAutoPowerDefinition Definition { get; }
        public bool Ready => _cooldown <= 0f;
        public float CooldownRemaining => Mathf.Max(0f, _cooldown);

        public void Tick(float deltaTime)
        {
            _cooldown = Mathf.Max(0f, _cooldown - deltaTime);
        }

        public void ResetCooldown(float cooldownMultiplier)
        {
            _cooldown = Definition.CooldownSeconds / Mathf.Max(0.1f, cooldownMultiplier);
        }
    }
}

