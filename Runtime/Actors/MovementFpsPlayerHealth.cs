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
    internal sealed class MovementFpsPlayerHealth
    {
        private CombatCatalog _catalog;
        private double _maximumHealth;
        private HealthState _health;

        internal double CurrentHealth => _health == null ? 0d : _health.CurrentHealth;
        internal double MaximumHealth => _health == null ? 0d : _health.MaximumHealth;
        internal bool IsAlive => _health != null && _health.IsAlive;

        internal void Configure(CombatCatalog catalog, double maximumHealth)
        {
            _catalog = catalog;
            _maximumHealth = maximumHealth;
        }

        internal void Reset()
        {
            _health = new HealthState(new CombatantId("combatant.player"), _maximumHealth, _maximumHealth);
        }

        // Returns only the transition caused by this command. The Unity bridge
        // forwards it to the run owner; repeated damage after death is a no-op.
        internal bool ApplyDamage(double amount)
        {
            if (_health == null || !_health.IsAlive) return false;
            DamageRequest request = new DamageRequest(
                _health.Id,
                new[] { new DamageComponent(BasicMovementFpsGame.KineticDamageType, amount) },
                sourceId: new CombatantId("combatant.enemy"));
            DamageResolver.Apply(_catalog, _health, null, request);
            return !_health.IsAlive;
        }

    }
}
