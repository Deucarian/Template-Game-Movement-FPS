# Phase 2W Projectile And Power Parity

Reference repo inspected:

`OccultWallrunnerHordeFPS`

## Reference Behavior Captured

- `Rift Launcher` is a slow projectile sidearm: `gun.rift-launcher`, damage `38`, interval `0.72`, magazine `6`, reload `1.55`, projectile speed `30`, lifetime `3`.
- Projectiles move as simple actors with remaining lifetime and sphere-cast hit detection. They ignore the player owner and damage the first enemy hit.
- Guns own ammo and reload timing locally.
- `Orbit Pulse` periodically damages enemies around the player.
- `Chain Bolt` periodically damages the nearest targets in range.
- `Ground Rift` periodically damages an area around a nearby target, or a forward fallback point.
- Upgrades can unlock the launcher and powers or modify projectile/power stats.

## Template Behavior Added

- Local `MovementFpsGunKind.Projectile` support.
- Local `MovementFpsAutoPowerKind` support for Orbit Pulse, Chain Bolt, and Ground Rift.
- Local reloadable gun runtime state.
- Local autonomous power runtime state and cooldown handling.
- Local `MovementFpsProjectileActor` with sphere-cast movement and lifetime.
- Sample content markers for projectile and power definitions.
- Run-upgrade effects for projectile speed, auto-power damage, and unlocks.

## Package Boundaries

Existing packages used:

- `com.deucarian.combat` for damage types, damage requests, and health state.
- `com.deucarian.run-upgrades` for stable upgrade IDs, weighted drafts, ranks, and effect descriptors.
- `com.deucarian.gameplay-foundation` as an existing transitive ID/random foundation dependency.

Kept local to this template:

- projectile gun feel
- ammo/reload behavior
- recoil/spread behavior
- autonomous power target selection
- power cooldown timing
- Ground Rift placement rules
- concrete unlock and stat effects

No package extraction happened in Phase 2W.

## Deferred

- projectile pooling
- richer projectile impact VFX/audio hooks
- grenade/alt-fire style payloads
- full spawn-pocket line-of-sight rules
- horde escalation and miniboss/victory flow
- authored ScriptableObject content workflow
- save/meta progression
- movement debug visualizers
