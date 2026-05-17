# GrafPack — 2D Shape Editor

A Windows Forms application for interactively drawing, selecting, moving, rotating, and deleting 2D geometric shapes, built in C# using GDI+.

## Features

- **Draw shapes** — Square, Triangle, and Circle, each created with two mouse clicks
- **Select shapes** — click any shape to select it; selected shapes are highlighted in red
- **Move** — drag a selected shape anywhere on the canvas
- **Rotate** — drag horizontally to rotate a selected shape; one pixel of drag equals one degree
- **Delete** — remove the selected shape from the canvas
- **Flicker-free rendering** — double-buffering via `ControlStyles.DoubleBuffer` for smooth redraws
- **Z-order aware selection** — clicking selects the topmost shape when shapes overlap


### Requirements

- Windows 10 or later
- Visual Studio 2019 or later (Community edition is free)
- .NET Framework 4.7.2 or later

### Running the project

1. Clone or download this repository
2. Open `GrafPack.sln` in Visual Studio
3. Press **F5** to build and run

No external dependencies or NuGet packages required.

## How to use

### Drawing shapes

1. Click **Create** in the menu bar
2. Select **Square**, **Triangle**, or **Circle**
3. Click once on the canvas to set the first point, then click again to complete the shape

| Shape | First click | Second click |
|---|---|---|
| Square | One corner (key point) | Opposite corner |
| Triangle | Apex | Bottom-right corner |
| Circle | Centre | Any point on the edge |

### Selecting a shape

1. Click **Select** in the menu bar
2. Click on any shape — it turns **red** to show it is selected
3. The **Transform** and **Delete** menu items become enabled

### Moving a shape

1. Select a shape
2. Click **Transform → Move**
3. Hold the left mouse button and drag

### Rotating a shape

1. Select a shape
2. Click **Transform → Rotate**
3. Hold the left mouse button and drag **left** (counter-clockwise) or **right** (clockwise)
4. One pixel of horizontal drag equals one degree of rotation

### Deleting a shape

1. Select a shape
2. Click **Delete** in the menu bar

## Code architecture

The project uses an abstract base class with concrete subclasses — a textbook application of polymorphism.

```
Shape (abstract)
├── draw(Graphics g, Pen pen)   — abstract
├── Contains(Point p)           — abstract
├── Move(int dx, int dy)        — abstract
├── Rotate(float angleDegrees)  — abstract
└── RotatePoint(...)            — shared helper
    ├── Square
    ├── Triangle
    └── Circle
```

### Shape construction

**Square** — defined by a key point and an opposite point. The four corners are calculated geometrically using the midpoint and perpendicular offsets, so the square stays true regardless of the angle between the two clicks.

**Triangle** — defined by an apex and a bottom-right point. The bottom-left point is derived symmetrically so the base is always horizontal at construction time.

**Circle** — defined by a centre and an edge point. The radius is the Euclidean distance between them.

### Circle rendering

The `Circle` class draws using **Bresenham's midpoint circle algorithm** rather than `Graphics.DrawEllipse`. This computes all eight symmetric octants using only integer arithmetic (no floating point per pixel), producing a pixel-accurate circle with minimal computation.

```csharp
int x = 0, y = radius, d = 3 - 2 * radius;
while (y >= x)
{
    // plot 8 symmetric points ...
    x++;
    d = d > 0 ? d + 4 * (x - y--) + 10 : d + 4 * x + 6;
}
```

### Rotation

All shapes rotate around their geometric centroid using standard 2D rotation:

```
x' = cos(θ)(x - cx) - sin(θ)(y - cy) + cx
y' = sin(θ)(x - cx) + cos(θ)(y - cy) + cy
```

Circles are rotationally symmetric, so their `Rotate` method is intentionally a no-op.

### Hit testing

| Shape | Method |
|---|---|
| Square | Bounding-circle check — point within circumradius of the square |
| Triangle | Axis-aligned bounding box check |
| Circle | Euclidean distance from centre ≤ radius |

### Double buffering

Flickering during redraws is eliminated by setting three control styles on the form:

```csharp
this.SetStyle(
    ControlStyles.DoubleBuffer |
    ControlStyles.UserPaint |
    ControlStyles.AllPaintingInWmPaint,
    true
);
```

All drawing happens in `OnPaint`, which is called by `this.Invalidate()` whenever state changes.

## Project structure

```
GrafPack/
├── GrafPack.cs          # Main form + all shape classes
├── GrafPack.csproj
├── GrafPack.sln
└── README.md
```

## Known limitations

- Hit testing for squares uses a circumscribed circle, so clicks near the corners of a rotated square may not register
- Triangle hit testing uses an axis-aligned bounding box, so the corners of the bounding box outside the triangle will still count as a hit
- No save/load functionality — shapes exist only for the current session
- Single file architecture (all classes in `GrafPack.cs`) — would benefit from splitting into separate files in a larger project

## Possible extensions

- Polygon-accurate hit testing for squares and triangles
- Fill colour picker per shape
- Undo/redo stack
- Export canvas to PNG or SVG
- Additional shapes (pentagon, star, freehand path)
