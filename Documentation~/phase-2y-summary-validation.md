# Phase 2Y - Run Summary and Content Validation

Phase 2Y adds local run-summary feedback and authored-content validation around the Movement-FPS template. No package extraction happened.

## Run Summary

The template now tracks a lightweight per-run summary:

- outcome: running, victory, or defeat
- elapsed run time
- kill count
- miniboss kill count
- XP gained
- upgrade IDs chosen
- local reward markers

The summary resets on restart/new run. It is not persisted and does not introduce a meta-progression system.

## Reward Behavior

The current reference slice does not contain a larger reward economy. This phase keeps reward behavior minimal:

- defeating the Choir Ogre records a local `reward.choir-ogre-banished` marker
- the marker is visible through run summary data and the simple runtime HUD
- no save/profile/meta currency is created

Boss rewards, run-end reward drafts, and profile persistence are deferred until a later phase proves the shape.

## Content Validation

`MovementFpsContentValidator` validates local template content:

- unique weapon IDs
- unique power IDs
- unique enemy IDs
- upgrade IDs through the Run Upgrades catalog
- known upgrade target references
- unlock effects that reference existing guns/powers
- starting loadout gun and power references
- wave enemy references and weights
- miniboss references

The validator is pure runtime code so EditMode tests and publish-readiness gates can run it without an editor menu or package extraction.

## Local Kit Code

These systems remain local Movement-FPS template-kit code:

- run summary tracker
- local reward markers
- content library assembly
- template content validation
- wave and miniboss references
- HUD summary display

## Deferred

- persistent profile rewards
- save migration
- reward drafts
- boss reward economy
- authored ScriptableObject content pipelines
- editor validation menu
- Package Registry or publish-readiness work

