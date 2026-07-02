# Basic Movement FPS Game Sample

Open `Scenes/BasicMovementFpsGame.unity` and enter Play Mode. The bootstrap object builds the arena, player, camera, horde loop, XP gems, draft UI, victory, defeat, and restart flow at runtime.

Controls:

- WASD: move
- Shift: sprint
- Ctrl or C: slide
- Space: jump / double jump
- Mouse: look
- Left mouse: fire
- Q: swap between the carbine and rift launcher
- R: reload current gun, or restart after defeat or victory
- 1/2/3: choose level-up draft options

First run target:

- Test sprint, slide, double jump, and wallrun movement around the generated arena.
- Fire the carbine, swap to the rift launcher, and watch Orbit Pulse, Chain Bolt, and Ground Rift trigger automatically.
- Collect XP gems until a draft opens, then choose an option with `1`, `2`, or `3`.
- Survive until the Choir Ogre miniboss spawns at about 115 seconds. Defeat it for victory, or press `R` to restart after defeat.

The sample builds its arena at runtime. Its authored content files are intentionally small markers for the sample's default loadout and tuning; the first slice still uses local runtime defaults to preserve parity and avoid premature package extraction. The first run starts with the launcher and all three autonomous powers online so Play mode demonstrates the full controls/combat surface quickly.

The sample run now includes local wave escalation, weighted horde enemies, and a Choir Ogre miniboss inside a two-minute demo cadence. Defeat and victory both restart with R. Spawn timing and miniboss state remain Movement-FPS template-kit code, not shared packages.

The HUD includes lightweight run summary feedback. The Choir Ogre victory reward is a local summary marker only; no persistent profile or meta system is included yet.

Run `Tools > Deucarian > Templates > Movement FPS > Validate Content` after editing sample loadout or runtime catalog definitions.
