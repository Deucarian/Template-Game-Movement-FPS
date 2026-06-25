# Deucarian Template Game - Movement FPS

Starter Unity package for a movement-FPS / wallrunner horde arena template. This first playable slice keeps the custom movement, camera, input, and gun feel local to the template kit while using existing Deucarian packages only where the fit is already clean.

## What Is Included

- Local `WallrunnerMotor` and `FpsInputReader` adapted from the Movement-FPS reference repo.
- Runtime sample controller that builds a simple wallrun arena, player, enemy, XP pickup, and draft loop.
- Basic hitscan carbine and projectile launcher combat using `com.deucarian.combat`.
- Autonomous Orbit Pulse, Chain Bolt, and Ground Rift powers with local cooldown/targeting rules.
- Local horde wave escalation, weighted enemy pressure, miniboss spawn, victory, defeat, and restart flow.
- Per-run summary feedback for kills, XP gained, chosen upgrades, victory/defeat outcome, and local reward markers.
- Pure local content validation for weapons, powers, enemies, upgrades, loadout, wave references, and miniboss references.
- XP and three-choice upgrade draft adapter using `com.deucarian.run-upgrades`.
- `Samples~/BasicMovementFpsGame` with a minimal boot scene.
- EditMode and PlayMode smoke tests for the first slice.

## Sample

Import the `Basic Movement FPS Game` sample, then open:

`Samples/BasicMovementFpsGame/Scenes/BasicMovementFpsGame.unity`

The scene contains a tiny bootstrap object. At runtime it creates the sample arena, player controller, camera, starter enemy, XP pickup flow, and draft UI.

The default sample run escalates toward a Choir Ogre miniboss at 270 seconds. Killing that miniboss triggers victory; player death enters a defeat state that can restart with R.

Victory and defeat display summary data. Defeating the Choir Ogre records a local run reward marker; it is intentionally not persisted.

## Local Kit Code

Keep these systems local to this template until a concrete second FPS-style game proves reusable shape:

- `WallrunnerMotor`
- first-person input
- camera feedback
- projectile weapon feel
- recoil/spread/ammo/reload feel
- autonomous power targeting and cooldown timing
- horde wave timing and weighted enemy selection
- miniboss/victory/defeat state
- run summary and local reward marker tracking
- content validation around sample definitions
- movement probes
- wallrun, slide, double-jump, mantle/vault tuning
- arena traversal geometry
- movement validation helpers

## Deucarian Package Use

This slice uses:

- `com.deucarian.gameplay-foundation` for stable ID and deterministic primitive types surfaced by Deucarian runtime packages.
- `com.deucarian.combat` for health and damage resolution.
- `com.deucarian.run-upgrades` for stable upgrade IDs, weighted draft choices, ranks, and selection state.

It deliberately does not use Deucarian Session for gameplay run state and does not route player movement through World Navigation.

## Deferred

Richer spawn-pocket rules, save/meta progression, persistent rewards, reward drafts, full authored ScriptableObject pipelines, pooling, and deeper debug tooling are deferred to later Movement-FPS phases.
