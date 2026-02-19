# 🎯 Kinematic Objects - Prefab Setup Templates

## Template 1: Rotating Door (90° Opening)

### GameObject Structure:
```
RotatingDoor_90
│
├─ DoorModel (MeshRenderer + MeshFilter)
│  └─ [Your door mesh with materials]
│
├─ GrabPosition (Empty Transform)
│  Position: Relative (-1.5, 0, 0) from door center
│  Rotation: Facing the door
│
├─ PivotAnchor (Empty Transform)
│  Position: At the HINGE edge of door
│  ⚠️ Must be on the edge, not center!
│
└─ Collider (BoxCollider)
   Is Trigger: ✅ TRUE
   Size: Cover the door + small margin
```

### Component Settings:
**KinematicObject:**
- Movement Type: `Pivot`
- Target Transform: `null` (not used for pivot)
- Speed: `45` (degrees/second)
- Grab Position: → `GrabPosition`
- Pivot Anchor: → `PivotAnchor`
- Max Rotation Angle: `90`

### Transform Values:
- **RotatingDoor_90:** Scale (1, 1, 1) ⚠️ CRITICAL
- **DoorModel:** Scale your mesh here (e.g., 2, 3, 0.1)
- **GrabPosition:** Local Position (-1.5, 0, 0), Rotation facing door
- **PivotAnchor:** Local Position at hinge edge (e.g., -1, 0, 0)

---

## Template 2: Sliding Platform (Horizontal)

### GameObject Structure:
```
SlidingPlatform_Horizontal
│
├─ PlatformModel (MeshRenderer + MeshFilter)
│  └─ [Your platform mesh]
│
├─ GrabPosition (Empty Transform)
│  Position: On the front edge
│  Rotation: Facing forward
│
└─ Collider (BoxCollider)
   Is Trigger: ✅ TRUE
   Size: Cover the platform

[SEPARATE OBJECT IN SCENE]
PlatformTarget_End (Empty Transform)
   Position: End point of slide path
```

### Component Settings:
**KinematicObject:**
- Movement Type: `Slide`
- Target Transform: → `PlatformTarget_End` (from scene)
- Speed: `2.0` (units/second)
- Grab Position: → `GrabPosition`
- Pivot Anchor: `null` (not used for slide)
- Max Rotation Angle: `0` (not used)

### Transform Values:
- **SlidingPlatform_Horizontal:** Scale (1, 1, 1) ⚠️ CRITICAL
- **PlatformModel:** Scale your mesh here
- **GrabPosition:** Local Position (0, 0, front edge)
- **PlatformTarget_End:** World Position where platform should end

---

## Template 3: Rotating Wall (120° Arc)

### GameObject Structure:
```
RotatingWall_120
│
├─ WallModel (MeshRenderer + MeshFilter)
│
├─ GrabPosition (Empty Transform)
│  Position: Front of wall, slightly offset
│
├─ PivotAnchor (Empty Transform)
│  Position: One corner/edge of wall
│
└─ Collider (BoxCollider)
   Is Trigger: ✅ TRUE
```

### Component Settings:
**KinematicObject:**
- Movement Type: `Pivot`
- Target Transform: `null`
- Speed: `30` (slower for heavy wall feel)
- Grab Position: → `GrabPosition`
- Pivot Anchor: → `PivotAnchor`
- Max Rotation Angle: `120`

---

## Template 4: Vertical Sliding Door

### GameObject Structure:
```
SlidingDoor_Vertical
│
├─ DoorModel (MeshRenderer + MeshFilter)
│
├─ GrabPosition (Empty Transform)
│  Position: Ground level, in front of door
│
└─ Collider (BoxCollider)
   Is Trigger: ✅ TRUE

[SEPARATE OBJECT]
DoorTarget_Up (Empty Transform)
   Position: Raised position (e.g., +4 units Y)
```

### Component Settings:
**KinematicObject:**
- Movement Type: `Slide`
- Target Transform: → `DoorTarget_Up`
- Speed: `1.5` (slower for heavy door)
- Grab Position: → `GrabPosition`

---

## 🎨 Quick Setup Checklist

### For ANY Kinematic Object:

1. **Parent Object:**
   - [ ] Name it descriptively (e.g., "RotatingDoor_East")
   - [ ] Set Scale to **(1, 1, 1)** ⚠️ MANDATORY
   - [ ] Add `KinematicObject` component
   - [ ] Add Collider with `Is Trigger = true`

2. **Child: Model**
   - [ ] Add your mesh renderer
   - [ ] Scale mesh HERE (not on parent)
   - [ ] Apply materials

3. **Child: GrabPosition**
   - [ ] Create empty Transform
   - [ ] Position where player should stand
   - [ ] Rotation = facing toward interaction point
   - [ ] Assign to `grabPosition` field

4. **For Pivot Type:**
   - [ ] Create child `PivotAnchor`
   - [ ] Position at hinge/edge (NOT center)
   - [ ] Must be vertical (Y-axis aligned)
   - [ ] Assign to `pivotAnchor` field
   - [ ] Set `maxRotationAngle` (usually 90-120)

5. **For Slide Type:**
   - [ ] Create separate GameObject in scene as target
   - [ ] Name it clearly (e.g., "PlatformTarget_End")
   - [ ] Position at end point
   - [ ] Assign to `targetTransform` field

6. **Testing:**
   - [ ] Enter Play Mode
   - [ ] Approach object (trigger range)
   - [ ] Press [X] to grab
   - [ ] Hold [W] to move
   - [ ] Check Gizmos in Scene view
   - [ ] Verify player doesn't deform (scale check)

---

## 🎨 Gizmo Color Guide

When you select a KinematicObject in editor, you'll see:

- 🔵 **Cyan Sphere** = Grab Position (where player stands)
- 🟢 **Green Cube** = Target Transform (end point for Slide)
- 🟡 **Yellow Line** = Movement path (Slide mode)
- 🔴 **Red Sphere** = Pivot Anchor (hinge point)
- 🟡 **Yellow Arc** = Rotation range (Pivot mode, when selected)

---

## ⚠️ Common Mistakes to Avoid

### 1. Scale Problem
❌ **Wrong:**
```
RotatingDoor (Scale: 2, 3, 0.5)
└─ Model
```

✅ **Correct:**
```
RotatingDoor (Scale: 1, 1, 1)
└─ Model (Scale: 2, 3, 0.5)
```

### 2. Pivot Position Problem
❌ **Wrong:** PivotAnchor at center of door
✅ **Correct:** PivotAnchor at edge/hinge

### 3. Collider Problem
❌ **Wrong:** Is Trigger = false (blocks player)
✅ **Correct:** Is Trigger = true (detects interaction)

### 4. Target Transform Problem (Slide)
❌ **Wrong:** Target as child of platform (moves with it!)
✅ **Correct:** Target as separate object in scene

---

## 📐 Example World Positions

### Rotating Door Example:
```
Door Object: Position (0, 0, 0), Rotation (0, 0, 0), Scale (1, 1, 1)
├─ Model: Local Scale (2, 3, 0.1) → 2m wide, 3m tall, 0.1m thick
├─ GrabPosition: Local Position (-1.5, 0, 0) → 1.5m to the left
└─ PivotAnchor: Local Position (-1, 0, 0) → Left edge (hinge)
```

When player presses [W], door rotates 90° around left edge (like real door).

### Sliding Platform Example:
```
Platform: Position (0, 0, 0), Scale (1, 1, 1)
├─ Model: Local Scale (3, 0.5, 2) → 3m wide, 0.5m tall, 2m deep
└─ GrabPosition: Local Position (0, 0, 1) → Front edge

PlatformTarget: Position (10, 0, 0) → 10 units to the right
```

When player holds [W], platform slides from X=0 to X=10.

---

## 🎬 Animation Timing Examples

| Object Type | Speed Value | Result |
|-------------|-------------|--------|
| Heavy Stone Door | 20 deg/sec | 90° in 4.5 seconds (slow, massive feel) |
| Normal Door | 45 deg/sec | 90° in 2 seconds (standard) |
| Quick Gate | 90 deg/sec | 90° in 1 second (fast, light) |
| Slow Platform | 1.0 unit/sec | Deliberate, puzzle-solving pace |
| Normal Platform | 2.0 unit/sec | Standard movement speed |
| Fast Platform | 4.0 unit/sec | Quick traversal |

**Tip:** Lower speed = heavier/more deliberate feel. Adjust to match object's perceived weight!

---

## 🔧 Prefab Workflow

### Creating Reusable Prefabs:

1. Set up GameObject with all children
2. Configure KinematicObject component
3. Test in Play Mode
4. Drag to Project window to create prefab
5. **Important:** Target Transforms won't be saved in prefab
   - You'll need to assign them per-instance in each scene

### Using Prefabs in Scenes:

1. Drag prefab into scene
2. If Slide type: Create target GameObject and assign
3. Position appropriately
4. Test!

---

**Note:** These templates are starting points. Adjust values based on your specific game feel and level design needs!

**File:** Prefab_Setup_Guide.md  
**Version:** 1.0  
**Related:** KINEMATIC_OBJECTS_README.md, IMPLEMENTATION_SUMMARY.md

