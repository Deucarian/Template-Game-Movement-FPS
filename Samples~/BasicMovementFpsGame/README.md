# Basic Movement FPS Game Sample

Open `Scenes/BasicMovementFpsGame.unity` and enter Play Mode.

Controls:

- WASD: move
- Shift: sprint
- Ctrl or C: slide
- Space: jump / double jump
- Mouse: look
- Left mouse: fire
- Q: swap guns after the Rift Launcher is unlocked
- R: reload current gun, or restart after defeat
- 1/2/3: choose level-up draft options

The sample builds its arena at runtime. Its authored content files are intentionally small markers for the sample's default loadout and tuning; the first slice still uses local runtime defaults to preserve parity and avoid premature package extraction.

The sample run now includes local wave escalation, weighted horde enemies, and a Choir Ogre miniboss. Defeat and victory both restart with R. Spawn timing and miniboss state remain Movement-FPS template-kit code, not shared packages.

The HUD includes lightweight run summary feedback. The Choir Ogre victory reward is a local summary marker only; no persistent profile or meta system is included yet.
