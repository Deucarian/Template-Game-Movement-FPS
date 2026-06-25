# Movement FPS Validation

Phase 2V validation targets:

- Unity package import/compile
- EditMode tests for content defaults and XP/draft adapter logic
- PlayMode boot smoke
- PlayMode movement basics
- PlayMode sprint, slide, and double jump smoke
- PlayMode wallrun/wall jump smoke when geometry and timing permit
- PlayMode gun damage and enemy death
- PlayMode XP pickup/level-up draft
- PlayMode restart after defeat

Fresh publishing and Package Registry validation are intentionally out of scope for Phase 2V. This template must not be published or registered until a later publish-readiness gate.

## Manual Play Smoke

Open the sample scene and verify:

- WASD movement responds
- Shift sprint increases speed
- Ctrl or C enters slide when moving fast enough
- Space jumps and allows an air jump
- wallrun walls can be traversed
- mouse look and fire work
- enemy dies from carbine hits
- XP pickup opens draft after collection
- 1/2/3 applies a draft choice
- defeat can restart with R
