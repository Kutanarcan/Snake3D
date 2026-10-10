<div align="center">

# 🐍 Snake3D

**Eat the apples. Dodge the walls. Clear the board.**

A Snake3D clone. Unity **6000.3.12f1**, URP,
built for Android.

![Unity](https://img.shields.io/badge/Unity-6000.3.12f1-black?logo=unity&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?logo=csharp&logoColor=white)
![Platform](https://img.shields.io/badge/Platform-Android-lightgrey)
![Pipeline](https://img.shields.io/badge/Pipeline-URP-6e7681)
![Tests](https://img.shields.io/badge/Tests-464-2ea44f)

</div>

> [!NOTE]
> Open `Assets/Scenes/Game.unity` and press Play. Levels are JSON files under
> `Assets/Levels/`, authored with a custom editor window at
> **Tools → Snake → Level Editor**.

| Section | |
|---|---|
| [🎬 Video](#-video) | Gameplay, portrait |
| [📋 Features](#-features) | Each feature and where it lives |
| [🏛 Architecture](#-architecture) | Assemblies, levels, editor, presentation, steering |
| [🧮 Algorithms](#-algorithms) | Ring buffer, sparse set, camera fit, polling |
| [🧪 Tests](#-tests) | EditMode and PlayMode coverage |
| [⚡ Performance decisions](#-performance-decisions) | Rendering, allocation, audio, frame rate |

---

## 🎬 Video

<div align="center">
  <video src="https://github.com/user-attachments/assets/41cbbd4e-d8d2-4622-98f8-13a7baa54c6f" width="300" controls muted></video>
</div>

## 📋 Features

| Feature | Where it lives |
|---|---|
| Grid size, layout, snake position and goals editable in-project; levels designable | `Assets/Levels/*.json`, authored through **Tools → Snake → Level Editor** (`Scripts/Editor`) |
| Grid size adjustable, rectangular | `GridSize(width, height)` — the two are independent; every level file carries its own `Width` and `Height` |
| UI supports different resolutions | `SafeAreaFit` turns `Screen.safeArea` into anchor ratios; `BoardFit` reframes the camera whenever the aspect changes |
| Tick rate adjustable | `Tick Interval` on `GameInstaller` (default 0.15 s), consumed by `TickTimer` |
| Food collected / goal shown at the top | `LevelInformationView` |
| Each part follows the one in front of it | `SnakeBody` — a ring buffer; one pointer move shifts the whole snake |
| Direction keys steer; head and tail art rotates to face the direction | On-screen arrow pad **and** swipe, both funnelled through `IInputSource`; `SnakeTrail` derives each part's rotation from the two cells around it |
| The snake grows by eating | `GrowthDebt` — food owes cells, paid off one per tick |
| Food spawns on a random empty cell, disappears when eaten and respawns | `FoodSpawner` over `FreeCellSet`, a sparse set that makes "pick a random empty cell" O(1) |
| Running into itself is fatal | `SnakeGame.CauseOfDeathAt` → `DeathCause.Snake` |
| Collecting every food clears the level | `GameStatus.Won`; `LevelSequence` advances and `PlayerPrefsLevelPersistent` remembers where the player got to |
| The player is told how the level ended | `LevelEndPopupView`, revealed by the ending presenter rather than the instant of death |
| Walls can be placed; hitting one loses the level | `CellGroundType.Wall` → `DeathCause.Ground` |
| Particles and sounds on eating and on other chosen events | Eat and golden-apple bursts; sounds for eating, food arriving, steering, entering a tunnel, wall and self impacts, win, lose and UI |
| Two extra items with mechanics of their own | **Tunnel** and **Golden Apple** — below |
| Runs on Android / iOS | Android build target, 60 fps cap, URP mobile pipeline asset |
| Unity version | 6000.3.12f1 |

### 🎁 The two items

**Tunnel** — a ground type the snake walks over. Arriving on it mirrors the
heading across the diagonal: Right→Up, Up→Right, Down→Left, Left→Down. Because
the enum is ordered clockwise the whole rule is one bit flip,
`(int)direction ^ 1`. It is its own inverse — two tunnels in a row return the
snake to where it was going — and it can never produce the opposite direction,
so a tunnel can never fold the snake onto its own neck. The forced turn also
locks the heading for one step: input is read every frame but a step happens
every 0.15 s, so without the lock any key pressed in the ~9 frames after the
turn would quietly undo it.

**Golden Apple** — a bonus that appears on its own clock, stays for a while and
leaves again. It grows the snake by three cells and counts three towards the
goal, and it does not consume the plain apple: eating one leaves the ordinary
food exactly where it was. Its timings are authored per level in seconds and
converted to whole ticks at load, so the simulation stays discrete.

---

## 🏛 Architecture

```
Assets/Scripts/
  Core/       Game.Core.asmdef          -> no UnityEngine reference at all
  Runtime/    Game.Runtime.asmdef       -> Core + UnityEngine
  Authoring/  Game.Authoring.asmdef     -> Core, editor-only, no engine
  Editor/     Game.Editor.asmdef        -> the level editor window
  Tests/EditMode, Tests/PlayMode
```

The rule that everything else follows from: **`Game.Core` cannot reference
`UnityEngine`.** It is not a convention, it is `noEngineReferences: true` in the
assembly definition — reaching for a Unity API from the game rules is a compile
error, and the whole simulation can be constructed and stepped without opening
Unity. That is why 389 of the tests run in EditMode.

- **One composition root.** `GameInstaller` is the only place anything is wired
  together; there are no singletons and nothing looks itself up.
- **MonoBehaviours are adapters.** They carry serialized data, forward lifecycle
  events and call Unity APIs. Rules, state machines and calculations live in
  plain C# classes that can be `new`ed.
- **Everything non-deterministic sits behind an interface** — `IInputSource`,
  `IRandomSource`, `ILevelPersistent`, `ISoundPlayer`, `IEffectPlayer` — so a
  test can pin the die, script the input or record what was played.
- **Two ways of saying no.** `Result` reports authored data that is wrong (a
  level file, an unassigned Inspector field) and the game says so and carries
  on; an exception reports code that is wired wrong, and belongs where it was
  wired.

### 🗺️ Levels

A level is a flat list of items, so a new ground type costs one
`CellGroundType` value and one line in the parser — no new field in `LevelData`,
no branch in the validator, no branch in the builder:

```json
{
  "Width": 7, "Height": 7,
  "StartX": 2, "StartY": 4, "StartDirection": "Down",
  "SnakeLength": 3, "FoodGoal": 7,
  "Items": [ { "Type": "Wall", "X": 0, "Y": 0 },
             { "Type": "Tunnel", "X": 4, "Y": 3 } ]
}
```

The loader only enforces what would crash or produce an unplayable level: an
item off the board, two items on one cell, an unknown type, a snake lying
inside a wall, a goal larger than the open cells can hold.

### 🛠️ The level editor

**Tools → Snake → Level Editor** opens a window that paints a level rather than
typing one. A palette picks the brush — Wall, Tunnel, Start, Erase — and the
grid is painted by dragging across it. The bar above holds everything that is
not a cell: board width and height, the snake's length and starting heading,
the food goal, and whether this level has a golden apple and on what timings.
Resizing keeps whatever still fits on the smaller board and drops the rest.
Open, New and Save work on the same JSON files the game ships.

A new ground type costs the palette nothing: the brush list is built from
`CellGroundType`, so the value that gives the game a wall also gives the tool a
button for it.

The grid is drawn the way the camera sees it — yaw 135°, so board `+X` runs up
and left on screen and `(0,0)` sits bottom-right — which makes the tool's
picture and the running game's picture the same picture.

The tool keeps **no copy of the rules**. `LevelDraft.Validate` calls
`Level.TryCreate`, the very function the game loads with, and shows the
`Result.Error` it comes back with. A test pins that down: what the draft
complains about is exactly what the game would complain about.

The model behind it lives in `Game.Authoring` — editor-only, no engine
reference, so `LevelDraft`, `ItemGrid` and `BoardToGrid` are covered by EditMode
tests like the rest of the pure code, and none of it ships in a player build.

### 🎨 Presentation

The board is mirrored every frame; **moments** are authored:

- **Sound** — a caller names a `SoundId`, never a file. `SoundCatalog` maps it to
  a clip and a volume.
- **Effects** — an `EffectId` and a cell. `EffectPlayer` keeps one pre-warmed
  pool per authored effect; a particle system returns itself when it stops.
- **Endings** — `EndingSettings` holds a win and one row per way of dying.
  `DeathPresenter` plays the hit, waits, then plays the verdict and hands the
  player back; `RunTimeline` picks the presenter and guarantees the handover
  fires even when nothing is authored, so a run can never stall on a missing
  row. Hitting a wall, biting yourself and walking off the board are three
  separate rows: the board edge is deliberately silent on impact, because there
  is nothing there to hit.

The board also stops being mirrored once a run is over, so the ending owns the
pieces instead of fighting a per-frame redraw.

### 📱 Steering on a phone

Two decisions come from the fact that the player is watching the snake, not
their thumb.

**The arrows are bigger than they look.** Each `ArrowRoot_*` carries the raycast
target and its `ArrowIcon` child carries the art, so the tap area was widened
well past the drawn arrow — enough that a glance-free thumb lands on it, and
stopping just short of the neighbouring arrow, because two overlapping targets
would turn a near miss into a confidently wrong turn.

**A press is never dropped.** Input is sampled once a frame but the snake steps
every 0.15 s — about nine frames apart. With one pending slot the second of two
quick taps overwrote the first and the player watched a press vanish, so turns
go into a two-deep queue and one is spent per step. The reversal rule moved with
it: a press is judged against the turn already waiting rather than the current
heading, which is what keeps *up, left, down* legal without ever letting the
head fold back into its own neck. The reasoning is in
[the algorithm notes](docs/Algorithm_Explanation.md).

---

## 🧮 Algorithms

Four places walk away from the obvious solution on purpose. Ring buffer, sparse
set, the support function that fits a tilted orthographic camera, and the
polling that survives a screen which will not announce that it changed:

**[docs/Algorithm_Explanation.md](docs/Algorithm_Explanation.md)** — with
step-by-step traces, complexity tables and the alternatives that were rejected.

---

## 🧪 Tests

464 tests: **389 EditMode** and **75 PlayMode**, over 48 files that mirror the
source layout. Run them from **Window → General → Test Runner**.

EditMode is the default because Core has no engine dependency. PlayMode is kept
for what genuinely needs the runtime: wiring, pooling, and the fact that a
missing Inspector field is reported as one error and the installer switches
itself off rather than running half built.

Fakes are hand-written rather than mocked — `ScriptedInputSource`,
`PinnedRandomSource`, a recording `ISoundPlayer` — because the fake doubles as
documentation of what the interface is for.

---

## ⚡ Performance decisions

Measured with the Frame Debugger, the Stats overlay and the in-game overlay
(`PerformanceOverlayView`, drop it on any GameObject: FPS, frame and CPU ms,
memory, per-frame GC allocation and collection count).

**Rendering**

- Both atlas materials were on the built-in `Unlit/Texture` shader, which the
  **SRP Batcher cannot batch** — so it was enabled and doing nothing, and every
  cell paid full setup. On `URP/Unlit` they look identical and batch: the opaque
  pass collapses to a single SRP Batcher batch.
- Nothing casts shadows and the materials are unlit, so the main light, its
  shadow pass, HDR and post-processing are off, and the renderers no longer ask
  for motion vectors or light probes.
- The background is a custom **screen-space gradient** in the skybox slot. The
  board camera is orthographic, so a sky keyed to view direction would render
  flat; and keeping it in the skybox slot means depth testing stops it from
  shading the pixels the board already covers.
- `Render Scale 0.8`, MSAA off.

**Allocation**

- `SnakeBody` (ring buffer) and `FreeCellSet` (sparse set) allocate once at
  setup and never again — a step is O(1) whatever the snake's length, and
  spawning food is O(1) whatever the board size.
- Cells and particles are pooled; the pools are sized from the hungriest level
  in the sequence (`CellStock`), so a run never stops to build one.
- Tween callbacks in the presenters are cached in the constructor rather than
  made per play.

**Audio**

- Every clip is a short 2D one-shot: forced to mono, sample rate overridden to
  22 050 Hz, ADPCM, preloaded. Mono halves the channels and the override halves
  the samples again.
- One `AudioSource` with `PlayOneShot` — it mixes any number of overlapping
  clips, so no voice pool is needed.

**Frame rate**

- `DisplaySetup` caps to 60 fps before the scene loads. With the cap in place,
  read headroom from the CPU line of the overlay rather than from FPS.

What was deliberately **not** done: combining the board into a single mesh,
merging the two atlases, and removing the second renderer on the organic prefab.
All three are real levers, but after the SRP Batcher fix the profiler stopped
pointing at rendering — so they stay unwritten until it points there again.

---

<sub>Made with Unity by <a href="https://github.com/Kutanarcan">Kutanarcan</a> · This README was written with Claude.</sub>
