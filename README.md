# Deucarian Template Game - Movement FPS

Playable Unity template package for a movement-FPS wallrunner horde arena. The sample boots into a small runtime-built arena with first-person movement, gunplay, autonomous powers, XP pickups, a three-choice upgrade draft, miniboss victory, defeat, and restart flow.

The template is intentionally a game slice, not a reusable FPS framework. Genre-specific feel stays local here until a second concrete FPS product proves a shared package boundary.

## When To Use This

Use this package when you want:

- A Deucarian-shaped movement-FPS starter scene that can be opened and played quickly.
- A reference for composing Combat, Run Upgrades, Game Content Authoring, Gameplay Foundation, Common, and Unity Input System in a game template.
- Local wallrun, slide, double-jump, first-person camera, horde escalation, and gun/power adapters that are safe to customize per game.

Do not use this as a generic movement controller, reusable FPS framework, registry source of truth, or package installer surface. Those capabilities belong to lower reusable packages or other Deucarian owners.

## Install

Unity compatibility: `6000.3` or newer.

Install from Unity Package Manager with one of these Git URLs:

```json
"com.deucarian.template.game.movement-fps": "https://github.com/Deucarian/Template-Game-Movement-FPS.git#main"
```

```json
"com.deucarian.template.game.movement-fps": "https://github.com/Deucarian/Template-Game-Movement-FPS.git#develop"
```

Use `#main` for stable package consumption and `#develop` when testing active package work.

## Play In 3 Minutes

1. Add the package through Package Manager.
2. Import the `Basic Movement FPS Game` sample.
3. Open the imported sample scene, or in this repository open `Samples~/BasicMovementFpsGame/Scenes/BasicMovementFpsGame.unity`.
4. Press Play.
5. Survive until the Choir Ogre miniboss appears at about 115 seconds, then defeat it for victory.

The scene contains a tiny bootstrap object. At runtime it creates the arena, player controller, camera, starter enemies, XP pickup flow, draft UI, victory summary, and restart state.

## Controls

- WASD: move
- Shift: sprint
- Ctrl or C: slide
- Space: jump or double jump
- Mouse: look
- Left mouse: fire
- Q: swap between carbine and rift launcher
- R: reload, or restart after defeat or victory
- 1/2/3: choose level-up draft options

## What To Customize First

- Weapons and powers: start in `Runtime/BasicMovementFpsGame.cs` with `CreateCarbineDefinition`, `CreateRiftLauncherDefinition`, `CreateOrbitPulseDefinition`, `CreateChainBoltDefinition`, and `CreateGroundRiftDefinition`.
- Enemy pressure: tune `CreateEnemyDefinition`, `CreateLeapingRunnerDefinition`, `CreateBoneBulwarkDefinition`, `CreateChoirOgreDefinition`, and `CreatePrototypeWaveDefinition`.
- Loadout: adjust `CreateStartingLoadoutDefinition` or the sample marker at `Samples~/BasicMovementFpsGame/Content/DefaultLoadout/loadout.json`.
- Upgrades: update `CreateUpgradeCatalog`, then run content validation before committing.
- Movement feel: inspect `Runtime/Definitions/MovementTuningDefinition.cs` and `Runtime/Movement/WallrunnerMotor.cs`; keep reusable movement extraction out of this template until there is a second product need.

## Sample And API Map

- Sample scene: `Samples~/BasicMovementFpsGame/Scenes/BasicMovementFpsGame.unity`
- Sample bootstrap: `Samples~/BasicMovementFpsGame/Scripts/BasicMovementFpsGameBootstrap.cs`
- Main runtime catalog: `Runtime/BasicMovementFpsGame.cs`
- Runtime controller: `Runtime/MovementFpsTemplateController.cs`
- Movement kit: `Runtime/Movement`
- Gun and projectile kit: `Runtime/Combat`
- Content validation: `Runtime/Content/MovementFpsContentValidation.cs`
- Editor validation menu: `Editor/MovementFpsEditorContentValidation.cs`
- Structure notes: `Documentation~/template-structure.md`
- Validation notes: `Documentation~/validation.md`

## Integrations

This slice uses:

- `com.deucarian.common` for approved transient Unity object cleanup.
- `com.deucarian.gameplay-foundation` for stable ID and deterministic primitive types surfaced by Deucarian runtime packages.
- `com.deucarian.combat` for health and damage resolution.
- `com.deucarian.run-upgrades` for stable upgrade IDs, weighted draft choices, ranks, and selection state.
- `com.deucarian.game-content-authoring` from editor validation code for report formatting.
- `com.unity.inputsystem` for the sample first-person input path.

It deliberately does not use Deucarian Session for gameplay run state and does not route player movement through World Navigation.

## Local Template Code

Keep these systems local to this template until reuse is proven across another movement-FPS game:

- `WallrunnerMotor`, first-person input, and camera feedback.
- Recoil, spread, ammo, reload, and projectile weapon feel.
- Autonomous power targeting and cooldown timing.
- Horde wave timing, weighted enemy selection, miniboss, victory, defeat, and restart state.
- Run summary and local reward marker tracking.
- Content validation rules around sample definitions.
- Movement probes, wallrun, slide, double-jump, mantle/vault tuning, arena traversal geometry, and movement validation helpers.

## Content Validation

From the Unity editor, run:

`Tools > Deucarian > Templates > Movement FPS > Validate Content`

The menu validates sample files and runtime catalogs for IDs, references, loadout entries, wave enemies, upgrade targets, and miniboss references. The report is written to the Unity console for template developer visibility.

Before committing package changes, run:

```powershell
python C:/Repositories/Package-Registry/Tools/deucarian_package_validator.py --registry-root C:/Repositories/Package-Registry --repository-root . --config deucarian-package.json
git diff --check
```

Run existing Unity EditMode and PlayMode tests when changing code, asmdefs, package dependencies, or sample behavior.

## Screenshots And GIFs

No screenshot or GIF assets are committed yet. Add `Documentation~/media/` captures once the sample has stable visual direction, then link the first gameplay GIF and one arena screenshot from this section.

## Troubleshooting

- Sample scene is missing: import `Basic Movement FPS Game` from Package Manager, or open the repository copy under `Samples~`.
- Input does not respond: confirm `com.unity.inputsystem` is installed and Unity has completed a compile after adding the package.
- Validation reports missing sample content: re-import the sample and check `Samples~/BasicMovementFpsGame/Content/DefaultLoadout/loadout.json`.
- Draft choices do not appear: collect XP gems until the required XP threshold is reached, then choose with `1`, `2`, or `3`.
- Defeat or victory is stuck: press `R` to restart the run.

## Deferred

Richer spawn-pocket rules, save/meta progression, persistent rewards, reward drafts, full authored ScriptableObject pipelines, pooling, and deeper debug tooling are deferred to later Movement-FPS phases.

## License

MIT. See `LICENSE.md`.
