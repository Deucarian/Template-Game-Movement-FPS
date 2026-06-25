# Movement FPS Validation

Phase 2V through 2Y validation targets:

- Unity package import/compile
- EditMode tests for content defaults and XP/draft adapter logic
- PlayMode boot smoke
- PlayMode movement basics
- PlayMode sprint, slide, and double jump smoke
- PlayMode wallrun/wall jump smoke when geometry and timing permit
- PlayMode gun damage and enemy death
- PlayMode projectile launcher damage and enemy death
- PlayMode projectile upgrade effect
- PlayMode Orbit Pulse damage
- PlayMode Chain Bolt damage
- PlayMode Ground Rift damage
- PlayMode XP pickup/level-up draft
- PlayMode horde escalation
- PlayMode miniboss spawn
- PlayMode miniboss death and victory trigger
- EditMode content validation for IDs and references
- EditMode run summary tracking
- PlayMode victory summary and reward marker
- PlayMode defeat summary
- PlayMode restart/new-run summary reset
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
- Rift Launcher unlock/sample data exists and projectile shots damage enemies
- Orbit Pulse, Chain Bolt, and Ground Rift powers damage enemies through local cooldown logic
- horde pressure increases over time
- the Choir Ogre miniboss can spawn and trigger victory when killed
- victory summary shows kills, XP, upgrades, and local reward marker counts
- defeat summary shows elapsed time, kills, and XP gained
- XP pickup opens draft after collection
- 1/2/3 applies a draft choice
- defeat can restart with R
- victory can restart with R
