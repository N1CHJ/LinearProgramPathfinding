# AsteroidField Scene Setup Guide

## Step-by-Step GameObject Creation

### 1. Arena (Boundary Box)
1. In Hierarchy, right-click → Create Empty → Name it "Arena"
2. Add Component → Box Collider
   - Set Size: X=100, Y=100, Z=100
   - Check "Is Trigger"
3. Optional: Add a visual boundary (Create → 3D Object → Cube, scale to match, remove collider, make semi-transparent material)

---

### 2. CubeSat (The Agent)
1. Create → 3D Object → Cube → Name it "CubeSat"
2. Position: (0, 0, 0)
3. Scale: (1, 1, 1)
4. Add Component → Rigidbody
   - Mass: 1
   - Drag: 0.1
   - Angular Drag: 0.5
   - **Uncheck "Use Gravity"**
5. Add Component → CubeSatAgent (script we just created)
6. Add Component → Decision Requester
   - Decision Period: 5
7. Add Component → Behavior Parameters
   - Behavior Name: "CubeSat"
   - Vector Observation Space Size: 13
   - Actions: Continuous Size = 3
8. Tag: Create and assign "Player" tag

**Create Visual Model (Child of CubeSat):**
1. Right-click CubeSat → 3D Object → Cube → Name "Model"
2. Position: (0, 0, 0), Scale: (0.8, 0.8, 0.8)
3. Create Material in /Assets/Materials → Name "CubeSat_Mat" → Set to bright color (cyan/blue)
4. Apply material to Model

---

### 3. Goal
1. Create → 3D Object → Sphere → Name it "Goal"
2. Position: (30, 10, 30) - or any position away from origin
3. Scale: (2, 2, 2)
4. Remove the Sphere Collider component
5. Add Component → Sphere Collider
   - **Check "Is Trigger"**
6. Tag: Create and assign "Goal" tag
7. Create Material → Name "Goal_Mat" → Set to green, emission enabled
8. Apply material

---

### 4. AsteroidSpawner
1. Create Empty → Name it "AsteroidSpawner"
2. Position: (0, 0, 0)
3. Add Component → AsteroidSpawner (script)
4. Configure in Inspector:
   - Asteroid Count: 20
   - Spawn Area Size: (60, 60, 60)
   - Min Distance Between Asteroids: 5
   - Max Drift Speed: 2

**Create Asteroid Prefab:**
1. Create → 3D Object → Sphere → Name "Asteroid_Prefab"
2. Scale: (2, 2, 2)
3. Create Material → Name "Asteroid_Mat" → Set to gray/brown
4. Apply material
5. Drag from Hierarchy to /Assets/Prefabs folder
6. Delete from Hierarchy
7. In AsteroidSpawner Inspector, drag prefab to "Asteroid Prefab" field

---

### 5. UI Canvas
1. Right-click Hierarchy → UI → Canvas → Name "TelemetryCanvas"
2. Canvas component:
   - Render Mode: Screen Space - Overlay
3. Canvas Scaler:
   - UI Scale Mode: Scale with Screen Size
   - Reference Resolution: 1920 x 1080

**Add Background Panel:**
1. Right-click TelemetryCanvas → UI → Panel → Name "TelemetryPanel"
2. Rect Transform:
   - Anchor: Top-Left
   - Position X: 200, Y: -150
   - Width: 400, Height: 250
3. Image component: Set color to semi-transparent black (A: 180)

**Add Text Elements (Using TextMeshPro):**
For each text element below, right-click TelemetryPanel → UI → Text - TextMeshPro:

**a) VelocityText**
- Name: "VelocityText"
- Rect Transform: Anchor Top-Left, Pos X: 10, Y: -20, Width: 380, Height: 40
- TextMeshPro:
  - Text: "Velocity: (0.00, 0.00, 0.00) m/s"
  - Font Size: 18
  - Color: White
  - Alignment: Left

**b) AngularVelocityText**
- Name: "AngularVelocityText"
- Rect Transform: Anchor Top-Left, Pos X: 10, Y: -60, Width: 380, Height: 40
- TextMeshPro:
  - Text: "Angular Velocity: (0.00, 0.00, 0.00) rad/s"
  - Font Size: 18
  - Color: Cyan

**c) SpeedText**
- Name: "SpeedText"
- Rect Transform: Anchor Top-Left, Pos X: 10, Y: -100, Width: 380, Height: 40
- TextMeshPro:
  - Text: "Speed: 0.00 m/s"
  - Font Size: 20
  - Color: Yellow
  - Style: Bold

**d) StatusText**
- Name: "StatusText"
- Rect Transform: Anchor Top-Left, Pos X: 10, Y: -150, Width: 380, Height: 50
- TextMeshPro:
  - Text: "Status: Ready"
  - Font Size: 22
  - Color: Green
  - Style: Bold

---

### 6. TelemetryUI Manager
1. Create Empty in Hierarchy → Name "TelemetryManager"
2. Add Component → TelemetryUI (script)
3. In Inspector, drag references:
   - CubeSat Rigidbody → Drag CubeSat to this field
   - Velocity Text → Drag VelocityText
   - Angular Velocity Text → Drag AngularVelocityText
   - Speed Text → Drag SpeedText
   - Status Text → Drag StatusText

---

### 7. QPSafetyFilter (Optional - for later)
1. Create Empty → Name "QPSafetyFilter"
2. Add Component → QPSafetyFilter (script)
3. Configure:
   - Safe Distance: 2.0
   - Gamma0: 1.0
   - Gamma1: 2.0
   - Agent Mass: 1.0

---

### 8. Connect References in CubeSatAgent
Select the CubeSat GameObject and in the Inspector:
1. Goal Transform → Drag "Goal" GameObject
2. Asteroid Spawner → Drag "AsteroidSpawner" GameObject
3. Telemetry UI → Drag "TelemetryManager" GameObject
4. Safety Filter → Drag "QPSafetyFilter" GameObject
5. Max Force: 10
6. Max Episode Time: 30
7. Goal Reach Distance: 2
8. **Uncheck "Use Safety Filter"** (for initial training)

---

### 9. Connect References in AsteroidSpawner
Select AsteroidSpawner and in Inspector:
1. Agent Transform → Drag "CubeSat" GameObject
2. Goal Transform → Drag "Goal" GameObject

---

## Final Scene Hierarchy Should Look Like:

```
AsteroidField
├── Main Camera
├── Directional Light
├── Arena (BoxCollider)
├── CubeSat (Agent)
│   └── Model (Cube - visual)
├── Goal (Sphere)
├── AsteroidSpawner
│   └── (Asteroids will spawn here at runtime)
├── TelemetryManager (TelemetryUI script)
├── QPSafetyFilter (QPSafetyFilter script)
└── TelemetryCanvas (UI)
    └── TelemetryPanel
        ├── VelocityText (TMP)
        ├── AngularVelocityText (TMP)
        ├── SpeedText (TMP)
        └── StatusText (TMP)
```

---

## Testing the Setup

### Manual Control Test:
1. Select CubeSat → In CubeSatAgent, check "Behavior Type: Heuristic Only"
2. Enter Play Mode
3. Use keyboard:
   - **Arrow Keys/WASD**: Move horizontally (X/Z)
   - **Space**: Move up (Y+)
   - **Left Shift**: Move down (Y-)
4. Watch the Telemetry UI update in real-time!

### Training Test:
1. Ensure Behavior Type is set to "Default"
2. Create a training config file (see ML-Agents documentation)
3. Run: `mlagents-learn config.yaml --run-id=SafePilot_Test`

---

## Troubleshooting

**UI not showing:**
- Check Canvas is set to Screen Space - Overlay
- Ensure TextMeshPro is imported (Window → TextMeshPro → Import TMP Essentials)

**Agent falling:**
- Make sure Rigidbody "Use Gravity" is unchecked

**Asteroids not spawning:**
- Check AsteroidSpawner has the prefab assigned
- Check "Asteroid" tag exists

**Tags needed:**
- "Asteroid" - for collision detection
- "Goal" - for goal detection
- "Player" - optional, for organization
