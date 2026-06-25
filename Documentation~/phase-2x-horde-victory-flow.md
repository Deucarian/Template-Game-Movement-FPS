# Phase 2X - Horde Escalation and Victory Flow

Phase 2X ports the reference run-structure shape into local Movement-FPS template-kit code. No shared package extraction happened.

## Reference Behavior Captured

- The run timer advances only while gameplay is not paused by draft, victory, or defeat state.
- Horde pressure comes from timed wave segments.
- Escalation uses pressure based on elapsed minutes and `escalationPerMinute`.
- Spawn interval, batch size, and max-alive pressure increase over time.
- Weighted enemy pools add Leaping Runner and Bone Bulwark pressure after the opening segment.
- Choir Ogre spawns once as the miniboss at 270 seconds.
- Killing the miniboss triggers victory.
- If a wave has no miniboss, survival to the wave victory time can trigger victory.
- Player death enters a defeat state and restart resets the run.

## Local Template-Kit Code

The following remain local to `com.deucarian.template.game.movement-fps`:

- `MovementFpsWaveDefinition`
- `MovementFpsWaveSegmentDefinition`
- `MovementFpsWaveDirector`
- weighted horde enemy selection
- arena-specific spawn position choice
- miniboss timing and victory state
- victory/defeat HUD state
- concrete sample enemy and wave tuning

## Deucarian Package Use

Existing packages remain used only where the fit is clean:

- Combat resolves health and damage for player, horde enemies, and miniboss.
- Run Upgrades continues to own draft choices and upgrade ranks.
- Gameplay Foundation remains an indirect dependency through Deucarian runtime packages.

This phase deliberately does not route first-person movement through World Navigation and does not use Deucarian Session for gameplay run state.

## Deferred

- Authored ScriptableObject wave pipelines.
- Spawn-pocket authoring and occlusion preference matching the reference scene exactly.
- Pooling for horde enemies and pickups.
- Boss rewards and run summary UI.
- Deeper movement validation tools.
- Any new Movement-FPS shared package.

