<div align="center">

# 🧮 Algorithm Notes

**Constant cost per tick. Zero allocation after setup.**

![C#](https://img.shields.io/badge/C%23-239120?logo=csharp&logoColor=white)
![Unity](https://img.shields.io/badge/Unity-6000.3.12f1-black?logo=unity&logoColor=white)
![Sections](https://img.shields.io/badge/Sections-5-6e7681)

</div>

Five places in this project deliberately walk away from the first solution that
comes to mind, so they are worth explaining on their own. The first two were
chosen for the same reason: **constant cost per tick and zero allocation once
the level is set up.** The third is not a data structure at all — it is a piece
of geometry that reproduces a hand-tuned camera in closed form. The fourth
closes the assumption the third leaves open: the screen does not stay still. The
fifth is the smallest of the five and the one a player feels most directly.

| Structure | Where | What it solves |
|---|---|---|
| [🔁 Circular array (ring buffer)](#-1-circular-array-ring-buffer--snakebody) | `Core/Snake/SnakeBody.cs` | Shifting the snake every step |
| [🎲 Sparse set](#-2-sparse-set--freecellset) | `Core/Grid/FreeCellSet.cs` | Picking a random empty cell |
| [📐 Support function](#-3-support-function--boardfit) | `Runtime/Board/BoardFit.cs` | Fitting the board to a tilted orthographic camera |
| [📱 Anchor ratios + polling](#-4-the-screen-is-not-fixed--safeareafit--reframing) | `Runtime/Presentation/SafeAreaFit.cs`, `Runtime/Board/BoardCamera.cs` | Surviving notches and a screen that rotates |
| [🎮 Bounded queue](#-5-bounded-queue--inputqueue) | `Core/Snake/InputQueue.cs` | Two presses inside one tick, without losing either |

---

## 🔁 1. Circular Array (Ring Buffer) — `SnakeBody`

### What it does

Keeps the snake's body in a fixed-capacity array. When the snake takes a step it
does not shift the array; it moves **the pointer that says where the head is**.

### Why this approach

Snake movement is a shift: a cell is added at the head and one falls off the
tail. The naive solution (`List.Insert(0, head)` plus `RemoveAt(last)`) moves
every element in memory on every tick — the cost of a step grows with the length
of the snake.

In a ring buffer nothing moves. The only thing that changes is which slot of the
array counts as the head.

### The key idea

There are two separate numberings, and keeping them apart is the whole trick:

- **slot** — the physical place in the array (`_cells[3]`)
- **index** — the logical position along the snake (0 = head, `Length - 1` = tail)

One line bridges them:

```csharp
public GridPosition this[int index]
{
    get { return _cells[(_headIndex + index) % _cells.Length]; }
}
```

`_headIndex` is **the head's physical slot**. Every access from outside —
`Head`, `Tail`, `this[i]`, `IndexOf` — goes through that mapping, which is why
changing `_headIndex` shifts the entire snake in one move.

### Step by step

Capacity 6, `Reset(head: (3,3), direction: Right, length: 3)`.

`Reset` writes backwards from the head (`backwards = direction.Opposite().ToOffset()`):

```
slot:      0      1      2      3      4      5
_cells [ (3,3) (2,3) (1,3)  ---    ---    ---  ]
           ^ _headIndex = 0                        Length = 3

this[0] = Head = (3,3)
this[1] =        (2,3)
this[2] = Tail = (1,3)
```

**`Advance((4,3), grow: false)`** — one step to the right

```csharp
_headIndex = (_headIndex - 1 + _cells.Length) % _cells.Length;  // 0 -> 5
_cells[_headIndex] = newHead;
```

```
slot:      0      1      2      3      4      5
_cells [ (3,3) (2,3) (1,3)  ---    ---   (4,3) ]
                                            ^ _headIndex = 5   Length = 3

this[0] = _cells[(5+0) % 6] = _cells[5] = (4,3)   <- new head
this[1] = _cells[(5+1) % 6] = _cells[0] = (3,3)
this[2] = _cells[(5+2) % 6] = _cells[1] = (2,3)   <- new tail
```

The old tail `(1,3)` is still physically sitting in slot 2. But `Length = 3`, so
`this[3]` cannot be asked for — it **counts as dropped**. Removing the tail cost
not a single write.

**`Advance((5,3), grow: false)`**

```
slot:      0      1      2      3      4      5
_cells [ (3,3) (2,3) (1,3)  ---   (5,3) (4,3) ]
                                     ^ _headIndex = 4   Length = 3

this[0] = (5,3)   this[1] = (4,3)   this[2] = (3,3)
```

**`Advance((5,4), grow: true)`** — food was eaten

Growing means "do not drop the tail". So the only thing that happens is `Length++`:

```
slot:      0      1      2      3      4      5
_cells [ (3,3) (2,3) (1,3) (5,4) (5,3) (4,3) ]
                             ^ _headIndex = 3   Length = 4

this[0] = (5,4)   this[1] = (5,3)   this[2] = (4,3)   this[3] = (3,3)
                                                          ^ would have gone, stayed
```

### Why `_headIndex` moves backwards

Logical index 0 is the head and indices **increase** towards the tail. Adding a
new head means adding to the **front** of the list, and adding to the front of a
ring means pulling the start pointer back by one.

The `- 1 + _cells.Length` is there because `%` returns a negative result for
negative operands in C#; the addition wraps to the last slot when walking back
past 0.

### Complexity

| Operation | Cost |
|---|---|
| `this[i]`, `Head`, `Tail` | O(1) |
| `Advance` (growing or not) | O(1) |
| `IndexOf` | O(Length) — at most once per tick |
| Memory | `capacity * sizeof(GridPosition)`, once in the constructor |

No allocation after setup. Capacity is the cell count of the grid — the snake
can never be longer than the board, so growing never reallocates.

### Rejected alternatives

- **`List<GridPosition>` with `Insert(0, …)`** — `Insert(0)` shifts every
  element every tick; the cost of a step grows with the snake.
- **`Queue<GridPosition>`** — Enqueue/Dequeue are O(1) but the *i*-th body part
  cannot be indexed. The view needs exactly that to derive a part's rotation
  from two consecutive cells.
- **Storing each part's direction separately** — storing something that can be
  derived creates a debt to keep the two in sync.

### Edge case: the cell the tail is vacating

The self-collision check has to happen **before** the step, because afterwards
the head is always at logical index 0 and finding itself is meaningless. The
cell the head is walking into is looked up on the map, and the one cell that
needs a second thought is the tail:

```csharp
if (_map[nextPos] != CellOccupantType.Snake)
{
    return DeathCause.None;
}

return nextPos != _snake.Tail || _growth.IsOwed ? DeathCause.Snake : DeathCause.None;
```

The head entering the cell the tail is **releasing this tick** is legal — the
tail is already being pulled out of it. Unless the snake is growing: then the
tail stays where it is, the cell is not released, and it is a collision. That is
what `_growth.IsOwed` answers.

---

## 🎲 2. Sparse Set — `FreeCellSet`

### What it does

Keeps track of which cells are empty and does all three of these in O(1): mark a
cell empty, mark it taken, and **pick a uniformly random one of the empty ones**.

### Why this approach

Spawning food is exactly this sentence: *"pick one of the empty cells,
uniformly."* The third operation — random selection — is the one ordinary set
structures cannot do. `HashSet<int>` adds and removes in O(1) but reaching its
*k*-th element means enumerating: O(n), plus enumerator boxing.

A sparse set is a set with indexed access. It costs two arrays.

### The key idea

The two arrays are **inverse maps** of each other:

- `_freeCells[slot] = cell` — "which cell is in this slot"
- `_slotOf[cell] = slot` — "which slot is this cell in"

**Invariant:** for every `slot < _count`, `_slotOf[_freeCells[slot]] == slot`.
The whole algorithm is nothing but preserving that equality.

`_count` is a boundary line: the `_count` elements to the left of it in
`_freeCells` are the empty cells, and everything to its right is **garbage** —
never cleaned up, because it is never read.

### Step by step

A grid of 6 cells (index 0..5).

**`Fill()` — everything is empty**

```
slot:        0  1  2  3  4  5
_freeCells [ 0  1  2  3  4  5 ]   _count = 6
_slotOf    [ 0  1  2  3  4  5 ]
cell:        0  1  2  3  4  5
```

**`Remove(2)` — a wall was placed on cell 2**

Move **the last element** on top of the one being removed, then pull the
boundary back by one:

```csharp
int slot = _slotOf[2];        // 2
_count--;                     // 5   <- boundary pulled back
int moved = _freeCells[5];    // 5   <- the element just outside the boundary
_freeCells[slot] = moved;     //     <- close the hole with it
_slotOf[moved] = slot;        //     <- update the inverse map
_slotOf[2] = -1;              //     <- 2 is no longer in the list
```

```
slot:        0  1  2  3  4 | 5
_freeCells [ 0  1  5  3  4 | 5 ]   _count = 5
_slotOf    [ 0  1 -1  3  4  2 ]
                    ^         ^
                  taken     new slot
```

The order is broken — 5 took 2's place. It does not matter: this is a **set**,
it carries no order.

**`Remove(0)`**

```
slot:        0  1  2  3 | 4  5
_freeCells [ 4  1  5  3 | 4  5 ]   _count = 4
_slotOf    [-1  1 -1  3  0  2 ]
```

Check: slot 0 holds cell 4, and `_slotOf[4] = 0`. The invariant holds.

**`Add(2)` — the snake's tail released cell 2**

Append at the end and push the boundary forward:

```csharp
_slotOf[2] = _count;      // 4
_freeCells[_count] = 2;
_count++;                 // 5
```

```
slot:        0  1  2  3  4 | 5
_freeCells [ 4  1  5  3  2 | 5 ]   _count = 5
_slotOf    [-1  1  4  3  0  2 ]
```

The old garbage (`_freeCells[4] = 4`) was overwritten. That is why nothing
outside the boundary ever has to be cleaned.

### The critical edge case

If the element being removed **is already the last one**, `moved` turns out to
be the element itself:

```
_freeCells = [4 1 5 3 2], _count = 5,  Remove(2):

slot  = 4
moved = _freeCells[4] = 2      <- itself
_freeCells[4] = 2              <- writes outside the boundary, harmless
_slotOf[2] = 4                 <- writes the wrong value
_slotOf[2] = -1                <- but the last line fixes it
```

This is why `_slotOf[cell] = -1` **has to be last**. Reordering breaks the
structure in exactly this one case.

### Complexity

| Operation | Sparse set | `List<int>` | `HashSet<int>` | Scan the grid |
|---|---|---|---|---|
| Pick a random empty cell | **O(1)** | O(1) | O(n) + boxing | O(n) |
| Empty → taken | **O(1)** | O(n) | O(1) | — |
| Taken → empty | **O(1)** | O(1) | O(1) | — |

Memory: `2 * 4n` bytes. `Fill()` is O(n) but is only called while a level loads,
never on the hot path. No allocation after setup.

### Why a `struct`, why `internal`

The free-cell ledger was pulled into its own type so that `GridMap` has one
responsibility: "what is in each cell" (SRP).

The field that holds it inside `GridMap` **is not, and cannot be, `readonly`**:
calling a mutating method on a `readonly` struct field makes C# take a defensive
copy, `Remove` writes into that copy and the change is silently lost. The type is
`internal` for that reason — it does not escape, so the risk of being copied is
confined to the inside of `GridMap`. If it ever has to become public it should
become a `sealed class`.

### A note on honesty

`_slotOf[cell] = -1` is never **read** by the algorithm — `Add` overwrites it
anyway. It is there only to answer "is this cell in the list" while debugging.
It costs one write.

---

## 📐 3. Support Function — `BoardFit`

### What it does

Given a grid size and the camera's rotation, it computes the **smallest
`orthographicSize`** at which the whole board fits on screen, and where to put
the camera. It never touches the rotation — the angle of the shot is an art
decision.

### Why this approach

The naive answer is `orthographicSize = Height / 2`. That is only true for a
camera looking straight down with yaw = 0. Turn the camera 135° and the board is
seen corner-on: what it occupies on screen is not its sides but its
**diagonal**. On top of that, neither the aspect ratio nor the space eaten by
the HUD and the arrow pad is in that formula.

The first attempt was written for a perspective camera: make the sphere around
the board tangent to the frustum. Correct, but loose — because the sphere is
based on the rectangle's diagonal, it leaves roughly 40% empty space on an
axis-aligned view. There is no closed form that fixes this in perspective:
nearby cells look bigger than distant ones, so the projection **depends on
depth**. Once the camera became orthographic the projection turned linear and
the exact solution fell out.

### The key idea

The half-extent of a body along a direction `a` is the projection of the point
that reaches furthest along it — in convex geometry this is called the **support
function**. For an axis-aligned box it can be written without walking the
corners; each half-edge contributes through an absolute value:

```
h(a) = |a.x| * halfX + |a.y| * halfY + |a.z| * halfZ
```

The board lies flat on the XZ plane, so `halfY = 0` and the middle term drops:

```csharp
private static float ExtentOn(Vector3 axis, float halfX, float halfZ)
{
    return Mathf.Abs(axis.x) * halfX + Mathf.Abs(axis.z) * halfZ;
}
```

There is no need to visit the corners, because the corner that gives the maximum
is always the one **both edges push in the same direction**. The absolute value
is precisely what says "pick that corner".

### Three axes, three questions

The camera's own axes are asked, not the world's:

| Axis | Question | Constraint |
|---|---|---|
| `right` | The half-width of the screen is `size * aspect` | `size >= h(right) / aspect` |
| `up` | What the panels leave behind is `band` | `size >= h(up) / band` |
| `forward` | The near plane must not cut into cells | `Depth = h(forward)` |

```
band = 1 - topFraction - bottomFraction
size = max(h(up) / band, h(right) / aspect)
```

`Depth` does **not** affect the image — an orthographic projection ignores
depth. It only says how far back the camera has to stand.

### Placing it in the middle of the band

If the panels are not equally deep, the empty band is not in the middle of the
screen:

```
bandCentre = bottomFraction + band / 2
UpShift    = (bandCentre - 0.5) * 2 * size
```

The camera slides that far **down** along its own `up` axis. When the bottom
panel is deeper, `bandCentre > 0.5`, the camera drops and the board moves up the
screen.

### Step by step — the actual shot in the scene

A 15×15 board, `Euler(30, 135, 0)`, 1080×1920 (aspect 0.5625), padding 0.5,
top 0.1, bottom 0.25.

```
halfX = halfZ = 15/2 + 0.5 = 8
```

Unity's Euler order is ZXY; with Z = 0 this is `R = Ry(135) * Rx(30)`:

```
right   = (-0.7071,  0.0000, -0.7071)
up      = ( 0.3536,  0.8660, -0.3536)
forward = ( 0.6124, -0.5000, -0.6124)
```

The support function on the three axes:

```
h(right)   = 0.7071*8 + 0.7071*8 = 11.3137
h(up)      = 0.3536*8 + 0.3536*8 =  5.6569
h(forward) = 0.6124*8 + 0.6124*8 =  9.7980
```

135° is the exact corner-on view, so `h(right)` takes the full contribution of
both sides: `2 * 8 * cos(45°) = 11.3137`. The 30° tilt, on the other hand,
leaves only `2 * 8 * sin(30°) * cos(45°) = 5.6569` going up — the board
flattens on screen.

```
band = 1 - 0.1 - 0.25 = 0.65

size >= 11.3137 / 0.5625 = 20.1133   <- width is the binding constraint
size >=  5.6569 / 0.65   =  8.7029

size = 20.1133
```

```
bandCentre = 0.25 + 0.325 = 0.575
UpShift    = 0.075 * 2 * 20.1133 = 3.0170
```

This shot had first been framed **by hand**, with `orthographicSize` typed in as
20. The formula gives 20.1133; that is the algorithm's own proof.

### Where the camera ends up

```
centre   = ((W-1)/2, 0, (H-1)/2)
position = centre
         - forward * (Depth + nearClipPlane + clearance)
         - up      * UpShift
```

`centre` is the middle of the cell centres, not the middle of the area: cells
sit on integer coordinates, so the middle of a 16-cell side is **7.5**, not 8.

### Complexity

**Independent of grid size**: three quaternion-vector products, six
multiplications, a handful of additions. O(1), no allocation. Called once per
level, never on the hot path.

### Edge cases

- **Straight down** (`Euler(90, 0, 0)`): `up = (0, 0, 1)` → `h(up) = halfZ`,
  `forward = (0, -1, 0)` → `Depth = 0`. A flat board reaches no distance towards
  the camera; the right answer.
- **Straight from the side**: the XZ component of `up` goes to zero, `h(up) = 0`
  and the vertical side never binds. The maths is right, but the board collapses
  to a line on screen — that is a camera mistake, not a fitting one.
- `band <= 0`, `aspect <= 0` and `padding < 0` are rejected.
- `Frame` is correct for one `aspect`. It has to be called again when the screen
  rotates or the window is resized — who does that is section 4.

### Rejected alternatives

- **`orthographicSize = Height / 2`** — only true looking straight down at yaw 0.
- **A bounding sphere** — rotation-independent and always safe, but because it is
  based on the diagonal it leaves roughly 40% empty on an axis-aligned view. It
  was the only sensible option in perspective; in orthographic it is waste.
- **Walking the four corners and taking min/max** — gives the same answer;
  `ExtentOn` is already its closed form. The loop adds nothing but noise.
- **Narrowing the viewport to the band with `Camera.rect`** — the area the camera
  does not draw is never cleared, so the rest of the screen smears; it would
  need a second background camera. Instead the camera stays full-screen and the
  band enters the maths as `band` and `UpShift`.

---

## 📱 4. The Screen Is Not Fixed — `SafeAreaFit` + Reframing

### What it does

Two separate jobs closing one assumption:

- `SafeAreaFit` — turns the rectangle the device leaves usable into the **anchor
  ratios** of a `RectTransform`.
- `BoardCamera.LateUpdate` — runs `Frame` again when the `aspect` it was framed
  for changes.

### Why this approach

Section 3's maths is correct for a single `aspect`, and its `band` for a single
screen height. Both assumed "the screen always stays the same". On Android that
is false: notches, camera cutouts, gesture bars, foldables, multi-window.

The naive safe-area solution writes **pixels** into `offsetMin` / `offsetMax`.
That is right once and wrong the moment the screen rotates — the offset is
absolute while the screen is not. An anchor is a **ratio**, so it moves itself
when the canvas is rescaled; the measurement happens once and the result holds
at every size.

### The key idea

`Screen.safeArea` arrives in pixels, an anchor lives in 0..1. One division
bridges them:

```
anchorMin = (safeArea.xMin / width, safeArea.yMin / height)
anchorMax = (safeArea.xMax / width, safeArea.yMax / height)
```

Because the rect is pinned to the edges with zero offsets (`offsetMin =
offsetMax = Vector2.zero`), the anchors alone decide both position and size.

### Step by step

1080×1920, a 132-pixel notch at the top, a 63-pixel gesture bar at the bottom.
`Screen.safeArea = (x: 0, y: 63, width: 1080, height: 1725)`.

```
anchorMin.y = 63   / 1920 = 0.0328
anchorMax.y = 1788 / 1920 = 0.9313      (1788 = 1920 - 132)
anchorMin.x = 0    / 1080 = 0
anchorMax.x = 1080 / 1080 = 1
```

The UI root now covers 89.9% of the screen vertically, clear of both the notch
and the gesture bar.

### Two narrowings overlap

Two independent things squeeze the same screen and **know nothing about each
other**:

| | What it narrows | Measured against |
|---|---|---|
| `band` (`topFraction` / `bottomFraction`) | The vertical strip the camera gives the board | The **whole** screen height |
| Safe area | The rectangle the UI root sits in | The insets the device reports |

The UI clings to the edge of the safe area; the camera reserves a percentage of
the full screen. One inequality keeps them from colliding:

```
topFraction    * height >= the top inset
bottomFraction * height >= the bottom inset
```

On the device above:

```
0.10 * 1920 = 192 >= 132   ✓   (60 pixels to spare)
0.25 * 1920 = 480 >=  63   ✓   (417 pixels to spare)
```

As long as it holds, the UI strip stays inside the band the camera reserved and
never covers the board. If it does not — a very aggressive notch, or a very
small `topFraction` — the UI overlaps the board. That is a design constraint the
code does not catch; `BoardCamera.Validate` only checks `top + bottom < 1`.

### Why polling

Unity raises **no event** when the screen rotates, the window is resized or the
safe area changes. `Screen.safeArea` is a property you read. So there are two
per-frame comparisons:

```csharp
// SafeAreaView.Update
if (Screen.width == _width && Screen.height == _height && Screen.safeArea == _safeArea)
{
    return;
}
```

```csharp
// BoardCamera.LateUpdate
if (!_framed || Mathf.Approximately(_camera.aspect, _aspect))
{
    return;
}
```

This is a deliberate departure from the project's "no work in `Update`" rule.
The reasoning: there is no `deltaTime` here and no game logic — only the reading
of a state the platform refuses to announce. The computation itself lives in
pure types (`SafeAreaFit`, `BoardFit`) and is tested in EditMode.

### Why `aspect`, not `Screen.width`

The camera watches `_camera.aspect`, not the screen size. Three reasons:

1. `BoardFit.For` consumes exactly `aspect` — what is watched is precisely what
   invalidates the result.
2. Narrowing the viewport with `Camera.rect` leaves `Screen` unchanged but
   changes `aspect`; watching the size would miss it.
3. A change that preserves the ratio, like 1080×1920 → 540×960, leaves `aspect`
   alone and no pointless `Frame` runs. Orthographic size is measured in world
   units, not pixels.

### Edge cases

| Input | Result | Why |
|---|---|---|
| `screenWidth <= 0` or `screenHeight <= 0` | The whole screen | The size may not be known on the first frame; narrow nothing rather than divide by zero |
| A safe area with zero width or height | The whole screen | Better to treat everything as usable than nothing |
| A safe area larger than the screen | Clamped to 0..1 | `Mathf.Clamp01`; some devices report values that overflow |
| `_area` not assigned | The view disables itself | Staying quiet beats throwing once per frame; `SceneViews.Validate` reports the reason |
| `Frame` never called (`_framed == false`) | `LateUpdate` returns early | It does not know what it would be framing |

### Complexity

`SafeAreaFit.For`: four divisions, four `Clamp01`s, two comparisons. O(1), no
allocation — it is a `readonly struct`.

Per-frame cost: three comparisons in `SafeAreaView` (one of them a `Rect`) and
one `Mathf.Approximately` in `BoardCamera`. That is all, as long as nothing
changes; when something does, `Frame` runs once and it too is O(1) in the grid
size.

### Rejected alternatives

- **Writing pixels into `offsetMin` / `offsetMax`** — right once, wrong after a
  rotation. An anchor ratio hands that conversion to the canvas.
- **One `ScreenWatcher` MonoBehaviour with two listeners** — it would create a
  shared event layer between the safe area and the camera. Two independent
  comparisons are cheaper and less coupled than an observer layer.
- **Calling `Frame` every frame** — it skips the comparison but performs a
  quaternion product and a transform write every frame. The change happens once
  in a blue moon.
- **Folding the safe area into the camera's `band`** — adding the inset to
  `topFraction` and narrowing the camera accordingly. It would shrink the board
  for nothing, because the UI is already pulled inside the inset; the allowance
  would be paid twice.

---

## 🎮 5. Bounded Queue — `InputQueue`

### What it does

Holds up to `Depth = 2` turns the player has asked for and hands exactly one of
them to each step.

### Why this approach

Input is sampled once a frame; the snake steps every 0.15 s. At 60 fps that is
about **nine frames between steps**. With a single slot for the pending turn, two
taps inside that window meant the second overwrote the first, and the first
simply never happened. The player pressed twice, watched one of them vanish, and
the game felt like it was not listening.

### The key idea

The queue is the easy half. The interesting half is that **the reversal rule
moves**.

With one slot, a second press *replaced* the first, so it had to be judged
against the direction the snake is actually going. Otherwise, heading Up,
`Left` then `Down` would collapse into `Up` → `Down`: the head turning straight
back into its own neck. That is why the old check read `direction.IsOpposite(_direction)`.

With a queue nothing is replaced. Each press is judged against **the one before
it in the chain**, and `Step` applies them in the order they arrived:

```csharp
Direction pending = PendingDirection;      // the back of the queue, or the
                                           // current direction when it is empty
if (direction.IsOpposite(pending))
{
    return false;
}
```

So consecutive *applied* directions can never be opposite, which is exactly the
guarantee the old check was there to give. The rule did not get looser; it moved
from one heading to every link of the chain. `Up`, `Left`, `Down` is now legal
because it is walked as three steps, not folded into one.

Two readings come off the queue:

- **back** (`PendingDirection`) — what a new press is measured against.
- **front** — what the next step will take, and what `PeekNextHead` looks at.

Both fall back to the current direction while the queue is empty.

### Step by step

Heading `Right` at `(4,4)`, queue empty.

```
frame 1   tap Up      Up vs Right    not opposite   -> [Up]
frame 2   tap Left    Left vs Up     not opposite   -> [Up, Left]
frame 3   tap Down    queue is full                 -> refused

step 1    dequeue Up      direction = Up     head (4,5)
step 2    dequeue Left    direction = Left   head (3,5)
step 3    queue empty     direction = Left   head (2,5)
```

A press that repeats the back of the chain queues nothing — it would change
where the snake ends up by nothing at all while eating a slot a real turn could
have used. It still answers `true`, because that press is what sets a waiting run
off, and because refusing it would report a failure the player did not make.

### Why two

A step lasts 0.15 s. Two turns is about the most a player can mean inside one
step; past that the queue starts playing the game for them, executing presses
from half a second ago against a board that has since moved. Two is deep enough
to stop losing a press and shallow enough to stay connected to the hand.

### Complexity

| Operation | Cost |
|---|---|
| `TryEnqueue` | O(1) |
| `TryDequeue` | O(Depth) — a shift of at most one element |
| `Front`, `Back`, `Clear` | O(1) |
| Memory | `Depth * sizeof(Direction)`, allocated once with the snake |

A ring buffer would make the dequeue O(1) as well. At a depth of two the shift is
a single assignment, and the array version is obviously correct at a glance —
which is worth more here than the constant.

### Edge cases

| Situation | Result | Why |
|---|---|---|
| A tunnel deflects the head | The queue is cleared and steering is locked for one step | The board has answered the question; a turn the player queued before it would silently undo the deflection |
| The run is reset | The queue is cleared | Presses from the level before are not the player asking for anything now |
| The queue is full | `TryTurn` returns `false` | Nothing is dropped, so the presses the player is watching for are the ones that happen |
| `Ready` → `Running` | Never blocked by a full queue | A run starts with an empty one |

### Rejected alternatives

- **One buffered direction** — what this replaced. Loses the first of two presses
  inside a tick, which is the whole complaint.
- **Reading input once per step instead of once per frame** — `GameSession`
  samples once a frame on purpose. On a catch-up frame that runs four steps,
  per-step reading would either apply the same held press four times or force
  every source to become edge-triggered.
- **An unbounded queue** — presses from seconds ago keep firing into a board that
  has moved on. The snake stops feeling attached to the hand.
- **Dropping the oldest when full** — the press the player made first is the one
  they are watching for; throwing it away is the bug this section is about.
- **Keeping the reversal check against the current heading** — it would reject
  `Up`, `Left`, `Down`, a sequence that is perfectly legal once both turns are
  actually walked, and bring the vanishing press back with it.

---

<sub>Made with Unity by <a href="https://github.com/Kutanarcan">Kutanarcan</a> · This document was written with Claude.</sub>
