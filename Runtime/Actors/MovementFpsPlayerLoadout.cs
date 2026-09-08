using System;
using System.Collections.Generic;
using Deucarian.Combat;
using Deucarian.TemplateGameMovementFps.Combat;
using Deucarian.TemplateGameMovementFps.Movement;
using Deucarian.TemplateGameMovementFps.Progression;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Deucarian.TemplateGameMovementFps.Actors
{
    internal sealed class MovementFpsPlayerLoadout
    {
        private readonly List<MovementFpsGunRuntimeState> _guns = new List<MovementFpsGunRuntimeState>();
        private readonly List<MovementFpsAutoPowerRuntimeState> _autoPowers = new List<MovementFpsAutoPowerRuntimeState>();
        private readonly List<MovementFpsGunDefinition> _startingGuns = new List<MovementFpsGunDefinition>();
        private readonly List<MovementFpsAutoPowerDefinition> _startingPowers = new List<MovementFpsAutoPowerDefinition>();
        private int _currentGunIndex;
        internal IReadOnlyList<MovementFpsGunRuntimeState> Guns => _guns;
        internal IReadOnlyList<MovementFpsAutoPowerRuntimeState> AutoPowers => _autoPowers;
        internal MovementFpsGunRuntimeState CurrentGun => _guns.Count == 0 ? null : _guns[Mathf.Clamp(_currentGunIndex, 0, _guns.Count - 1)];

        internal void Configure(IReadOnlyList<MovementFpsGunDefinition> guns, IReadOnlyList<MovementFpsAutoPowerDefinition> powers)
        {
            _startingGuns.Clear();
            _startingPowers.Clear();
            CopyStartingContent(guns, _startingGuns);
            CopyStartingContent(powers, _startingPowers);
        }

        internal bool AddGun(MovementFpsGunDefinition definition)
        {
            if (string.IsNullOrWhiteSpace(definition.Id) || HasGun(definition.Id))
            {
                return false;
            }

            _guns.Add(new MovementFpsGunRuntimeState(definition));
            return true;
        }

        internal bool AddAutoPower(MovementFpsAutoPowerDefinition definition)
        {
            if (string.IsNullOrWhiteSpace(definition.Id) || HasAutoPower(definition.Id))
            {
                return false;
            }

            _autoPowers.Add(new MovementFpsAutoPowerRuntimeState(definition));
            return true;
        }

        internal bool HasGun(string id)
        {
            for (int index = 0; index < _guns.Count; index++)
            {
                if (_guns[index].Definition.Id == id)
                {
                    return true;
                }
            }

            return false;
        }

        internal bool HasAutoPower(string id)
        {
            for (int index = 0; index < _autoPowers.Count; index++)
            {
                if (_autoPowers[index].Definition.Id == id)
                {
                    return true;
                }
            }

            return false;
        }

        internal void Reset()
        {
            _currentGunIndex = 0;
            _guns.Clear();
            _autoPowers.Clear();
            for (int index = 0; index < _startingGuns.Count; index++)
            {
                AddGun(_startingGuns[index]);
            }

            for (int index = 0; index < _startingPowers.Count; index++)
            {
                AddAutoPower(_startingPowers[index]);
            }
        }

        internal void TickGun(bool fireHeld, bool reloadPressed, bool nextGunPressed, float deltaTime, float cadenceMultiplier, IMovementFpsLoadoutEffects effects)
        {
            for (int index = 0; index < _guns.Count; index++)
            {
                _guns[index].Tick(deltaTime);
            }

            if (_guns.Count == 0)
            {
                return;
            }

            if (nextGunPressed)
            {
                _currentGunIndex = (_currentGunIndex + 1) % _guns.Count;
            }

            MovementFpsGunRuntimeState gun = CurrentGun;
            if (reloadPressed)
            {
                gun.StartReload(1f);
            }

            if (!fireHeld || !gun.TryFire(cadenceMultiplier))
            {
                return;
            }

            effects.FireGun(gun.Definition);
            effects.ApplyRecoil(gun.Definition.RecoilPitchDegrees);
        }

        internal void TickAutoPowers(float deltaTime, IMovementFpsLoadoutEffects effects)
        {
            for (int index = 0; index < _autoPowers.Count; index++)
            {
                MovementFpsAutoPowerRuntimeState power = _autoPowers[index];
                power.Tick(deltaTime);
                if (!power.Ready)
                {
                    continue;
                }

                effects.CastAutoPower(power.Definition);
                power.ResetCooldown(1f);
            }
        }

        internal MovementFpsGunRuntimeState FindGun(MovementFpsGunKind kind)
        {
            for (int index = 0; index < _guns.Count; index++)
            {
                if (_guns[index].Definition.Kind == kind)
                {
                    return _guns[index];
                }
            }

            return null;
        }

        private static void CopyStartingContent(IReadOnlyList<MovementFpsGunDefinition> source, List<MovementFpsGunDefinition> destination)
        {
            if (source == null)
            {
                return;
            }

            for (int index = 0; index < source.Count; index++)
            {
                if (!string.IsNullOrWhiteSpace(source[index].Id))
                {
                    destination.Add(source[index]);
                }
            }
        }

        private static void CopyStartingContent(IReadOnlyList<MovementFpsAutoPowerDefinition> source, List<MovementFpsAutoPowerDefinition> destination)
        {
            if (source == null)
            {
                return;
            }

            for (int index = 0; index < source.Count; index++)
            {
                if (!string.IsNullOrWhiteSpace(source[index].Id))
                {
                    destination.Add(source[index]);
                }
            }
        }

    }
}
