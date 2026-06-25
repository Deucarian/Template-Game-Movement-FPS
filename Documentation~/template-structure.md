# Movement FPS Template Structure

## Runtime

`Runtime/Movement`

Local first-person movement and input:

- `WallrunnerMotor`
- `FpsInputReader`

`Runtime/Actors`

Local sample actors:

- player controller
- enemy actor
- XP pickup actor

`Runtime/Combat`

Local combat-feel adapters:

- reloadable gun runtime state
- autonomous power runtime state
- sphere-cast projectile actor

`Runtime/Progression`

Local adapter for XP, draft state, and applying run-upgrade effects including projectile speed and autonomous power damage modifiers.

`Runtime/Run`

Local run-state and horde-wave code:

- run state enum
- wave definition and segment models
- weighted enemy entries
- wave spawn snapshots
- horde wave director

`Runtime/Bootstrap`

Template composition for the sample run. This builds the runtime arena, player, enemies, pickups, miniboss/victory flow, and simple HUD without involving Deucarian Session.

## Samples

`Samples~/BasicMovementFpsGame`

Contains a minimal sample scene and small sample content marker files. The scene is intentionally tiny and lets runtime composition create the playable slice.

## Tests

`Tests/EditMode`

Pure data and progression tests.

`Tests/PlayMode`

Movement, boot, combat, XP/draft, and restart smoke tests.

## Package Usage

Existing Deucarian packages used in this phase:

- Gameplay Foundation: stable IDs and deterministic primitive types surfaced by Combat and Run Upgrades
- Combat: health and damage
- Run Upgrades: draft definitions, stable IDs, weighted selection, ranks

No package extraction happened in Phase 2X. Projectile launcher feel, ammo/reload timing, power cooldowns, power target selection, power placement rules, horde timing, weighted enemy pressure, miniboss flow, victory state, and arena spawn rules remain local Movement-FPS kit code.

Not used in this phase:

- World Navigation for first-person movement
- Session for run lifecycle
- new shared Movement-FPS package
- package extraction of motor, camera, recoil, or traversal logic
