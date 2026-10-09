
---

## 4. The Screen Is Not Fixed — `SafeAreaFit` + Reframing

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
