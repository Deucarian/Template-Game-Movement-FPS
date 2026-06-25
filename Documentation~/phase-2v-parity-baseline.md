# Phase 2V Parity Baseline

Reference repo inspected:

`OccultWallrunnerHordeFPS`

Reference branch at audit time:

`develop` at `4d66927 Harden solver memory farming`

## Reference Systems

The reference game is a Movement-FPS horde arena prototype. It is built around a custom kinematic `WallrunnerMotor`, not Unity `CharacterController`, and not Deucarian World Navigation. Its core feel comes from local motor probes, state transitions, momentum retention, camera offsets, FOV kicks, wallrun roll, slide drop, and first-person input.

Observed baseline systems:

- Sprint, slide, double jump, wallrun, wall jump, vault/mantle, bunny-hop carry, velocity caps, and fall recovery live in `Runtime/Movement/WallrunnerMotor.cs`.
- First-person input lives in `Runtime/Movement/FpsInputReader.cs`.
- Camera feel and basic hitscan/projectile gun ownership live in `Runtime/Actors/PlayerCombatController.cs`.
- Enemy chase/contact damage and XP drop live in `Runtime/Actors/EnemyActor.cs`.
- XP pickups and magnet attraction live in `Runtime/Actors/PickupActor.cs`.
- Run boot/composition live in `Runtime/Bootstrap/PrototypeGameSession.cs`.
- Spawn pressure and spawn pockets live in `Runtime/Spawning/HordeWaveDirector.cs`.
- Upgrade drafts and horde progression are prototype-local gameplay systems.

## First Slice Ported

This package ports only a first playable vertical slice:

- first-person player boots
- movement motor and input reader are local template code
- sprint, slide, double jump, wallrun, wall jump, vault/mantle support are present through the preserved motor
- camera FOV/roll/slide drop values match the reference defaults used for feel
- basic hitscan carbine fires
- enemy spawns, chases, contact-damages, takes damage, and dies
- XP pickup drops and attracts to the player
- XP opens a three-choice level-up draft
- choosing an upgrade applies a local run effect
- player defeat and restart are present

## Deliberately Deferred

- projectile launcher parity
- autonomous powers
- richer horde escalation
- spawn-pocket line-of-sight rules
- miniboss/victory flow
- save/meta progression
- authored ScriptableObject content pipeline
- movement debug visualizers
- full prototype bootstrap/editor tools
- full movement gauntlet, route-planning, and solver-memory tooling

## Protected Boundaries

Do not route `WallrunnerMotor` through shared navigation systems. Do not replace the movement model for architecture style. Do not use Deucarian Session for gameplay run state. Movement feel is the first-class acceptance gate.
