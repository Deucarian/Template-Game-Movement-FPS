# Movement composition

`WallrunnerMotor` remains the Unity component and compatibility boundary. Its
script GUID, 124 serialized field names, public tuning properties, `Tick`
overloads, command methods, and nested `RuntimeSnapshot` contract are retained.
Existing scenes and prefabs require no migration.

The component copies its serialized tuning values into `WallrunnerSettings` at
the command/tick boundary. Inspector changes and existing public setters therefore
take effect on the next tick. `ApplyMovementTuning` keeps its original clamping
and mapping rules. The component owns no second velocity or traversal state.

## State and tick ownership

`WallrunnerSession` explicitly composes an ordinary C# movement application.
`WallrunnerState` is its single internal mutable state owner. Callers issue
commands to the session and observe read-only properties or immutable
`WallrunnerSnapshot` values. Snapshots are explicit checkpoints; they are not a
second live state path.

The session preserves the original ordered tick:

1. Continue an active vault, otherwise advance lockout and input-buffer timers.
2. Probe/snap ground contact and process landing, grace windows, and air jumps.
3. Resolve slide, vault, and jump input in the original priority order.
4. Resolve wallrunning, gravity, lateral control, slope following, and velocity cap.
5. Move with collision, resolve post-move grounding, and publish the final state.

The collaborators have separate reasons to change:

| Collaborator | Responsibility |
| --- | --- |
| `WallrunnerDirections` | Wish/look directions, wall continuity, guidance and launch geometry |
| `WallrunnerVelocityPolicy` | Air steering, momentum boosts, bunny hopping and velocity limits |
| `WallrunnerSlidePolicy` | Slide entry, drag profile, slope speed and jump timing |
| `WallrunnerGroundProbes` | Ground-contact sampling and standable-normal classification |
| `WallrunnerGrounding` | Ground following, edge release, snapping and post-move grounding |
| `WallrunnerWallProbes` | Surface candidates, intent, continuity scoring and top contact |
| `WallrunnerWallrunPolicy` | Wallrun entry, speed/height profile, fatigue and reentry locks |
| `WallrunnerJumpPolicy` | Ground/slide/wall/air jump transitions and jump resource use |
| `WallrunnerVaultPolicy` | Ledge selection, flow/mantle policy, exit velocity and blend progress |
| `WallrunnerCollisionPolicy` | Ordered capsule movement and contact-plane velocity response |

The dependency graph is acyclic. Collaborators receive the same state and settings
through their constructors; there is no service locator, runtime reflection,
global movement state, or gameplay base class.

The new session, settings, snapshots, environment port and collaborators are
internal to this template. The existing EditMode test assembly has explicit friend
access. `WallrunnerMotor` remains the supported public movement boundary.

## Unity boundary

`IWallrunnerEnvironment` supplies pose, capsule geometry, collision query results,
and surface identity comparisons. `UnityWallrunnerEnvironment` owns all
`Transform`, `CapsuleCollider`, `Physics`, and `WallrunSurfaceGroup` access.
Each query excludes the local capsule and restores its previous enabled state in
`finally`, including a failed query. Unity's destroyed-object/null comparison
semantics remain in this adapter.

Policy uses Unity's vector/quaternion/math value types, without constructing
GameObjects or requiring a physics scene. The EditMode tests replace the
environment with analytic responses. This is a scene-independent policy boundary,
not a promise that every Unity math operation executes on an ordinary .NET host.

## Compatibility surface and review guardrails

The named `WallrunnerMotor` partial files contain only the existing serialization,
public tuning, tuning import, and legacy snapshot adapter responsibilities. Actual
movement behavior lives in separate composed types. Review the aggregate facade
size when adding API: the partial files are not permission to accumulate new
movement logic. New movement rules belong to the relevant collaborator.

Legacy snapshot surface references remain `Transform` for source compatibility;
session snapshots carry only an opaque surface identity. Snapshots containing a
Unity surface are local runtime checkpoints and are not persistence documents.
No serialized assets, scene/prefab references, package dependencies, or lower
package ownership changed.

## Regression coverage

`WallrunnerSessionTests` covers carried air speed, hard/soft lateral limits, air
jump consumption/refill, slide momentum and landing buffer, bunny hopping,
horizontal/vertical wall traversal, wall-jump reentry locks, fatigue/surface-loss
delay, flow/mantle selection, blocked vault fallback, blend completion, collision
projection, snapshot replay, and independent sessions.

`WallrunnerCompatibilityTests` protects the exact serialized field-name inventory,
original component GUID, representative serialized tuning import, legacy snapshot
roundtrip, and the absence of Unity-object ownership in session/policy/state types.
The existing PlayMode movement and full gameplay tests remain in place, using
public motor commands instead of writing private implementation fields.

Ordered real-physics comparison against the unchanged pre-refactor motor is also
run in the local validation host; that frozen implementation is a validation
artifact, not a second production motor shipped by this package. Its evidence
belongs in the task's validation report.
