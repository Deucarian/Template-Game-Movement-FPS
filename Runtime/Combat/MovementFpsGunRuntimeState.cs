using System;
using UnityEngine;

namespace Deucarian.TemplateGameMovementFps.Combat
{
    public sealed class MovementFpsGunRuntimeState
    {
        private float _cooldown;
        private float _reloadTimer;

        public MovementFpsGunRuntimeState(MovementFpsGunDefinition definition)
        {
            Definition = string.IsNullOrWhiteSpace(definition.Id) ? throw new ArgumentException("Gun definition is required.", nameof(definition)) : definition;
            Ammo = definition.MagazineSize;
        }

        public MovementFpsGunDefinition Definition { get; }
        public int Ammo { get; private set; }
        public bool Reloading => _reloadTimer > 0f;
        public float ReloadRemaining => Mathf.Max(0f, _reloadTimer);

        public void Tick(float deltaTime)
        {
            _cooldown = Mathf.Max(0f, _cooldown - deltaTime);
            if (_reloadTimer <= 0f)
            {
                return;
            }

            _reloadTimer -= deltaTime;
            if (_reloadTimer <= 0f)
            {
                Ammo = Definition.MagazineSize;
            }
        }

        public bool TryFire(float fireRateMultiplier)
        {
            if (Reloading || _cooldown > 0f)
            {
                return false;
            }

            if (Ammo <= 0)
            {
                StartReload(1f);
                return false;
            }

            Ammo--;
            _cooldown = Definition.FireIntervalSeconds / Mathf.Max(0.1f, fireRateMultiplier);
            if (Ammo <= 0)
            {
                StartReload(1f);
            }

            return true;
        }

        public void StartReload(float reloadSpeedMultiplier)
        {
            if (Ammo >= Definition.MagazineSize)
            {
                return;
            }

            _reloadTimer = Definition.ReloadSeconds / Mathf.Max(0.1f, reloadSpeedMultiplier);
        }
    }
}

