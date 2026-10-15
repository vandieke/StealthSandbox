# Stealth sandbox foundation

Open `Assets/Scenes/StealthSandbox.unity` and press Play. Move with WASD,
arrow keys, or a gamepad's left stick. The camera follows the cyan player.
Orange is the guard; the white marker shows each actor's facing direction.

The scene is generated once on the first script import. If needed, use
**Stealth Sandbox > Create Foundation Scene**. Existing sandbox scenes are
opened rather than overwritten by this menu command.

## Your AI starts here

Add your own scripts to Guard. `GuardReferences` provides the player,
patrol-point transforms, and a wall layer mask. It implements no AI.

`ActorMotor2D.SetMoveInput(Vector2)` accepts a world-space direction with
magnitude 0–1. Call `Stop()` when movement should stop and `Face(Vector2)`
to turn while stationary. `FacingDirection` is available for your vision
cone; local `transform.up` does not rotate. The motor handles physics and
normalizes diagonal movement. Player and guard use zero-gravity rigidbodies.

Walls have BoxCollider2D components on the Walls layer. Floor tiles have
no colliders. Use the wall mask for your own line-of-sight raycasts.
The patrol points are positions only. There are no states, transitions,
perception checks, memory, search logic, or pathfinding yet.

Player and Guard prefabs live in `Assets/Prefabs`. Scene-specific player
and patrol references are assigned to the scene guard. Assign those again
if you place a fresh guard prefab into another scene.

## Reusing Bound assets

In Bound select the desired assets, then **Assets > Export Package** with
**Include dependencies** enabled. Import the `.unitypackage` here through
**Assets > Import Package > Custom Package**. This is useful for tilemaps,
tile palettes, animations, and prefabs which reference other assets.

For individual sprites you can copy the image AND its matching `.meta`
file into this project's Assets folder while Unity is closed. The meta
preserves sprite slicing, pixel size, and the GUID. Avoid GUID collisions
and copying Bound's entire ProjectSettings, Library, or Packages folders.
Bound character prefabs may include gameplay scripts and side-view physics;
for this top-down sandbox, reuse their art on the Body child and keep the
foundation movement components. No Bound assets have been copied yet.
