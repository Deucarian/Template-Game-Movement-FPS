# Gameplay composition

The sample remains local template code. Its public Unity components retain their
serialized fields, script GUIDs and public methods, while ordinary C# owners hold
run and player state. The collaborators are internal to the runtime assembly;
they do not establish a reusable FPS framework or a new package contract.

## Run ownership

`MovementFpsRunSession` owns elapsed time, draft/defeat/victory state, progression,
the wave director and the run summary. It receives an explicit random-seed
function plus `IMovementFpsRunWorld` and `IMovementFpsRunFeedback` ports. Tests can
exercise run transitions and command ordering without creating a Unity scene.
The existing progression, wave and summary types retain their narrower policies.

`MovementFpsRunInput` captures a keyboard frame. The session gives terminal-state
restart priority, then draft choices in 1/2/3 order. Restart and draft selection
return before advancing the clock. Wave tuning is captured at run start, while
the spawning-enabled flag is read each frame. Killing an enemy removes it from
the world registry before recording the kill, creating XP and issuing feedback;
the scene bridge then destroys the actor.

| Owner | Responsibility |
| --- | --- |
| `MovementFpsTemplateController` | Serialized Unity entry point, composition, public compatibility forwarding and frame dispatch |
| `MovementFpsDefaultContent` | Default definitions and combat catalog |
| `MovementFpsUnityWorld` | Scene hierarchy, player/enemy registry, spawning, pickups, projectiles and spatial queries |
| `MovementFpsArenaBuilder` | Sample geometry and lighting |
| `MovementFpsFeedbackPresenter` | Feedback cues, generated audio clips and particle presentation |
| `MovementFpsHudSnapshot` | Captured values for one HUD draw |
| `MovementFpsHudPresenter` | GUI styles, layout and formatting |
| `MovementFpsMaterialOwner` | Generated renderer-material lifetime for each scene object |

The world adapter passes the public controller to existing actors through their
unchanged initialization API. Run policies receive only the explicit ports, so
they cannot inspect transforms, draw the HUD or retain audio resources.

Restart clears combat objects and resets run/player state while retaining the
arena and feedback resources. Destruction releases the feedback hierarchy and
its six generated audio clips, then the runtime hierarchy. Generated materials
follow their scene object's lifetime, including projectiles and pickups that
expire before the template is destroyed. Cleanup uses Common's approved
`UnityObjectUtility.DestroySafely`. A failed partial bootstrap releases resources
already created before propagating the exception.

## Player ownership

`MovementFpsPlayerController` retains its 15 serialized fields and the existing
public player API. It resolves Unity references, copies current camera settings,
dispatches one frame in the existing order, and forwards damage transitions to
the run controller. Its collaborators are composed objects:

- `MovementFpsPlayerLoadout` owns starting definitions, gun selection, runtime
  ammo/reload state and power cooldowns. Effects go through
  `IMovementFpsLoadoutEffects`; tests can observe dispatch ordering directly.
- `MovementFpsPlayerHealth` adapts Combat health/damage and reports only a newly
  caused death. Reset restores the configured maximum health.
- `MovementFpsPlayerStats` resolves gun damage, cadence, projectile speed and
  power-damage modifiers from the existing progression contract.
- `MovementFpsPlayerCombat` performs Unity hit scans, projectile creation and
  autonomous power targeting. It reads live camera/muzzle references.
- `MovementFpsCameraPresenter` owns pitch, recoil, roll, posture and field of view.
  Current serialized tuning is copied before each frame and reset.

Frame order remains look, motor tick/look direction, gun timers/selection/fire
and recoil, autonomous powers, then camera posture. A gun switch takes effect
before firing; reload prevents fire; a power's cooldown resets after dispatch.
Reset preserves the public loadout collection identities and restores starting
content, health, motor pose and camera view.

The player owns its `FpsInputReader`. Disable suspends its nine actions so a
component can be enabled again. Destruction disposes those actions exactly once.
The reader's existing bindings and keyboard/mouse behavior are unchanged.

## Motor compatibility boundary

The earlier [movement composition](movement-architecture.md) remains unchanged.
The 1,448-line aggregate `WallrunnerMotor` compatibility facade retains 124
serialized fields and public tuning/snapshot contracts across named partials.
Those files contain serialization and forwarding, while movement policy/state
already lives in composed owners. Splitting those contracts again would not
create a new responsibility boundary. Each production file is at most 500 lines.

## Regression coverage

`MovementFpsCompositionTests` covers scene-independent restart, draft priority,
miniboss victory ordering, paused clocks, wave suppression, loadout reset,
gun/reload dispatch, power cooldown ordering and health transitions.
`MovementFpsCompositionCompatibilityTests` protects both component field inventories
and GUIDs, scene-free state ownership and input-action disposal.
`MovementFpsCompositionPlayModeTests` verifies generated material/audio lifetimes
and public player reset against actual Unity objects. Existing run, combat,
movement and restart suites remain part of the integration gate.
