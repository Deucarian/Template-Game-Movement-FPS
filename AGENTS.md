# Deucarian Template Game Movement FPS Agent Notes

Package ID: `com.deucarian.template.game.movement-fps`
Repository: `Deucarian/Template-Game-Movement-FPS`

Follow the canonical Deucarian governance docs in [Package Registry](https://github.com/Deucarian/Package-Registry/blob/develop/ARCHITECTURE.md), especially capability ownership and dependency rules.

## Ownership

This package owns:

- A playable Movement FPS horde-arena template slice, local wallrunner movement, local gunplay/autonomous power adapters, local horde escalation, local pickup/restart/victory flow, sample scene bootstrap code, and template-specific content validation.

Registered capabilities:
- None.

This package must not own:

- Reusable FPS framework infrastructure, generic input systems, generic movement/pathfinding packages, Combat rules, Run Upgrades core logic, Game Content Authoring framework behavior, Package Installer behavior, or registry governance.

## Dependencies

Allowed dependency shape:

- Template package may depend on lower reusable gameplay packages and Unity Input System needed by its playable sample.

Required dependencies and why:

- `com.deucarian.common`: approved transient Unity object cleanup for local template runtime objects.
- `com.deucarian.combat`: local gunplay and damage flow.
- `com.deucarian.game-content-authoring`: editor validation/content authoring hooks.
- `com.deucarian.gameplay-foundation`: shared IDs and deterministic primitives.
- `com.deucarian.run-upgrades`: level-up draft and upgrade effect adapters.
- `com.unity.inputsystem`: FPS input actions and keyboard/mouse input.

Optional/version-defined dependencies:

- None.

Architecture exceptions:

- Editor content validation writes validation summaries to the Unity console for template developer visibility.

## Policies

- Keep reusable systems in lower packages; template code may compose and demonstrate them but should not become a generic framework.
- Local gameplay code can be pragmatic, but reusable behavior should be proposed to the owning package before extraction.
- Do not add persistence, progression, monetization, UI, or networking dependencies unless the template actually ships those flows.
- Logging: Direct Unity Debug calls are limited to editor/template validation diagnostics listed in `deucarian-package.json`.
- Unity object lifetime: Use Common's `UnityObjectUtility.DestroySafely` for local template runtime cleanup.
- Testing: Test fixture teardown may use Unity `Destroy`/`DestroyImmediate` directly.

## Validation

Run the shared validator before committing:

```powershell
python C:/Repositories/Package-Registry/Tools/deucarian_package_validator.py --registry-root C:/Repositories/Package-Registry --repository-root . --config deucarian-package.json
```

Also run existing repository tests when changing code or asmdefs. Documentation-only updates should still run `git diff --check`.

## Codex Guidance

- Inspect current files before changing anything.
- Work on `develop`; do not edit or merge `main` unless the task is promotion-only.
- Do not edit `Library/PackageCache`.
- Do not guess package versions or dependency versions.
- Do not add package dependencies casually; update asmdefs, `package.json`, `deucarian-package.json`, Package Registry, Package Installer fallback, and Bootstrap fallback together when a dependency is truly required.
- Do not create local copies of shared helpers.
- Keep commits focused and report exactly what changed and what was validated.
