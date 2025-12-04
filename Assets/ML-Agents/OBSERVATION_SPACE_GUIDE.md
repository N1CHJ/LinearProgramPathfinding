# 🧠 Observation Space Guide - What the Agent Knows

## 📊 Complete Observation Vector (21 observations)

The agent receives **21 numerical observations** every step, giving it complete awareness of its state, orientation, and goal.

---

## 🎯 Observation Breakdown

### **1. Agent Position (3 observations)**
```csharp
sensor.AddObservation(transform.localPosition);  // Vector3 (x, y, z)
```

**What it is:**
- CubeSat's current position in the arena
- Relative to arena center (0, 0, 0)

**Values:**
- X: -25 to +25 (left/right)
- Y: -25 to +25 (down/up)
- Z: -25 to +25 (back/forward)

**Why it's useful:**
- Agent knows where it is in the arena
- Can detect proximity to boundaries
- Helps understand spatial context

**Example:**
```
Position: (5.2, -10.3, 15.8)
Agent is:
├── 5.2m right of center
├── 10.3m below center
└── 15.8m forward of center
```

---

### **2. Linear Velocity (3 observations)**
```csharp
sensor.AddObservation(agentRigidbody.linearVelocity);  // Vector3 (vx, vy, vz)
```

**What it is:**
- How fast CubeSat is moving in each direction (m/s)
- World space velocities

**Values:**
- Typical: -20 to +20 m/s per axis
- Can be higher if thrusting hard

**Why it's useful:**
- Agent knows its current momentum
- Critical for deceleration (flip-and-burn)
- Helps predict future position
- Needed for safe arrival

**Example:**
```
Velocity: (2.5, -1.0, 8.3)
Agent is:
├── Moving 2.5 m/s to the right
├── Moving 1.0 m/s downward
└── Moving 8.3 m/s forward
Total speed: sqrt(2.5² + 1² + 8.3²) ≈ 8.7 m/s
```

---

### **3. Angular Velocity (3 observations)**
```csharp
sensor.AddObservation(agentRigidbody.angularVelocity);  // Vector3 (ωx, ωy, ωz)
```

**What it is:**
- How fast CubeSat is spinning around each axis (rad/s)

**Values:**
- Typical: -5 to +5 rad/s per axis
- Higher if tumbling

**Why it's useful:**
- Agent knows its rotation rate
- Can stabilize tumbling
- Needed for precise pointing
- Essential for controlled thrust direction

**Example:**
```
Angular velocity: (0.1, -0.5, 0.2)
Agent is:
├── Slowly rolling (0.1 rad/s)
├── Yawing left (0.5 rad/s)
└── Slightly pitching up (0.2 rad/s)
```

---

### **4. Forward Direction (3 observations)**
```csharp
sensor.AddObservation(transform.forward);  // Vector3 (normalized)
```

**What it is:**
- CubeSat's forward direction in world space
- Where thrust will be applied
- Normalized vector (length = 1)

**Values:**
- Each component: -1 to +1
- Total magnitude: always 1.0

**Why it's useful:**
- Agent knows where it's pointing
- Critical for thrust planning
- Helps align with goal direction
- Needed for flip-and-burn maneuvers

**Example:**
```
Forward: (0.707, 0, 0.707)
CubeSat is pointing:
├── 45° to the right
├── Level (no up/down tilt)
└── 45° forward
Thrust will push in this direction!
```

---

### **5. Up Direction (3 observations)**
```csharp
sensor.AddObservation(transform.up);  // Vector3 (normalized)
```

**What it is:**
- CubeSat's up direction in world space
- Defines roll orientation
- Normalized vector (length = 1)

**Values:**
- Each component: -1 to +1
- Total magnitude: always 1.0

**Why it's useful:**
- Completes orientation awareness (with forward)
- Helps understand rotation state
- Can detect if upside-down
- Useful for stabilization

**Example:**
```
Up: (0, 1, 0)
CubeSat is:
├── Right-side up
└── No roll rotation

Up: (0, -1, 0)
CubeSat is:
├── Upside-down
└── 180° roll
```

---

### **6. Relative Goal Position - Individual Components (3 observations)**
```csharp
Vector3 relativeGoalPosition = goalTransform.position - transform.position;
sensor.AddObservation(relativeGoalPosition.x);  // ← NEW!
sensor.AddObservation(relativeGoalPosition.y);  // ← NEW!
sensor.AddObservation(relativeGoalPosition.z);  // ← NEW!
```

**What it is:**
- **Explicit X, Y, Z distances to goal**
- World space offset
- **This is what you wanted!** ✅

**Values:**
- Each component: -50 to +50 (max arena diagonal)
- Positive X: goal is to the right
- Positive Y: goal is above
- Positive Z: goal is forward

**Why it's critical:**
```
Agent can now clearly see:
├── "Goal is 15m to my right" (X = +15)
├── "Goal is 8m above me" (Y = +8)
└── "Goal is 22m forward" (Z = +22)

This makes planning MUCH easier!
```

**Example:**
```
Relative goal position: (12.5, -5.0, 30.0)

Agent knows:
├── Goal is 12.5m to the right
├── Goal is 5.0m below
├── Goal is 30.0m forward
└── To reach it:
    ├── Need to move +X (right)
    ├── Need to move -Y (down)
    └── Need to move +Z (forward)

Agent can reason:
"I need to thrust primarily forward (+Z),
slightly right (+X), and a bit down (-Y)"
```

---

### **7. Distance to Goal (1 observation)**
```csharp
sensor.AddObservation(relativeGoalPosition.magnitude);
```

**What it is:**
- Euclidean distance to goal
- Scalar value (single number)

**Values:**
- 0 to ~86 (max arena diagonal: sqrt(50² + 50² + 50²))
- Typically: 0 to 60

**Why it's useful:**
- Agent knows "how far" in total
- Can judge progress
- Useful for arrival speed planning
- Complements X, Y, Z components

**Example:**
```
Distance: 35.5

Agent knows:
├── Goal is 35.5m away
├── If moving 2.5 m/s, ~14 seconds to reach
└── Can plan deceleration timing
```

---

### **8. Local Goal Direction (3 observations)**
```csharp
Vector3 localGoalDirection = transform.InverseTransformDirection(relativeGoalPosition.normalized);
sensor.AddObservation(localGoalDirection);
```

**What it is:**
- Goal direction in **agent's local space**
- "Where is goal relative to my current orientation?"
- Normalized vector (length = 1)

**Values:**
- Each component: -1 to +1
- Magnitude: always 1.0

**Components:**
- **X:** How much to the right (+) or left (-) of CubeSat's forward
- **Y:** How much above (+) or below (-) CubeSat's forward
- **Z:** How much in front (+) or behind (-) CubeSat

**Why it's critical:**
```
This tells the agent:
├── "If I thrust now, am I pointing at the goal?"
├── "How much do I need to rotate?"
└── "Which direction should I turn?"

VERY useful for rotation control!
```

**Example 1: Goal directly ahead**
```
Local goal direction: (0, 0, 1)

Agent knows:
├── X = 0: Goal is straight ahead (no left/right)
├── Y = 0: Goal is level (no up/down)
├── Z = 1: Goal is in front
└── Action: Thrust forward! Already aligned ✅
```

**Example 2: Goal to the right and above**
```
Local goal direction: (0.6, 0.4, 0.69)

Agent knows:
├── X = 0.6: Goal is significantly to my right
├── Y = 0.4: Goal is moderately above me
├── Z = 0.69: Goal is mostly in front
└── Action: Rotate right and up, then thrust
```

**Example 3: Goal behind and below**
```
Local goal direction: (0, -0.5, -0.87)

Agent knows:
├── X = 0: Goal is straight behind (no left/right)
├── Y = -0.5: Goal is below me
├── Z = -0.87: Goal is mostly behind me
└── Action: Flip 180°, then thrust (flip-and-burn!)
```

---

## 🧮 **Total Observation Count: 21**

```
Position:                3  (x, y, z)
Linear velocity:         3  (vx, vy, vz)
Angular velocity:        3  (ωx, ωy, ωz)
Forward direction:       3  (fx, fy, fz)
Up direction:            3  (ux, uy, uz)
Relative goal X:         1  ← Explicit!
Relative goal Y:         1  ← Explicit!
Relative goal Z:         1  ← Explicit!
Distance to goal:        1  (magnitude)
Local goal direction:    3  (lx, ly, lz)
                        ───
TOTAL:                  21 observations
```

---

## 🎯 **Key Improvements (Your Request!)**

### **Before (implicit):**
```csharp
sensor.AddObservation(relativeGoalPosition);  // Vector3 as a whole
```
- Agent had goal offset, but less explicit
- Harder for neural network to parse individual components

### **After (explicit):**
```csharp
sensor.AddObservation(relativeGoalPosition.x);  // ← Explicit X
sensor.AddObservation(relativeGoalPosition.y);  // ← Explicit Y
sensor.AddObservation(relativeGoalPosition.z);  // ← Explicit Z
```
- Agent clearly sees X, Y, Z components separately
- **Much easier for network to learn**: "I need to move +12 in X, -5 in Y, +30 in Z"
- Matches your intuition about "knowing the change in position"

---

## 🧠 **What the Agent Can Reason:**

With these observations, the agent can answer:

### **"Where am I?"**
```
Position: (5, -10, 15)
"I'm 5m right, 10m below, 15m forward of center"
```

### **"How am I moving?"**
```
Velocity: (2, -1, 8)
"Moving 2 m/s right, 1 m/s down, 8 m/s forward"
"Total speed: 8.4 m/s"
```

### **"Am I spinning?"**
```
Angular velocity: (0.1, -0.5, 0.2)
"Slowly tumbling, mostly yawing left"
```

### **"Where am I pointing?"**
```
Forward: (0.707, 0, 0.707)
"Pointing 45° right and forward"

Up: (0, 1, 0)
"Right-side up, no roll"
```

### **"Where is the goal?" ← YOUR KEY QUESTION**
```
Relative goal X: +12.5
Relative goal Y: -5.0
Relative goal Z: +30.0

"Goal is 12.5m to my right, 5m below, 30m forward"

Distance: 33.5
"Total distance: 33.5m"

Local direction: (0.37, -0.15, 0.90)
"If I thrust now, I'll go mostly forward (0.90),
slightly right (0.37), and slightly down (-0.15)"
"Pretty well aligned! Maybe rotate a bit more right/down"
```

### **"What should I do?"**
```
Agent can plan:
├── "Goal is mostly forward (+Z = 30)"
├── "Some to the right (+X = 12.5)"
├── "Little bit down (-Y = 5)"
│
├── "I'm pointing 90% toward goal (local Z = 0.90)"
├── "I should rotate 10-15° right and down"
├── "Then thrust forward"
│
├── "Current speed: 8.4 m/s"
├── "Distance: 33.5m"
├── "At 8.4 m/s, ~4 seconds to reach"
├── "Need to decelerate soon!"
└── "Should flip and burn in ~2 seconds"
```

---

## 📊 **Observation Examples in Action**

### **Scenario 1: Agent spawns, sees goal for first time**

```
Observations:
├── Position: (0, 0, 0)  ← Spawned at center
├── Velocity: (0, 0, 0)  ← Stationary
├── Angular vel: (0.2, -0.1, 0.3)  ← Initial tumble
├── Forward: (1, 0, 0)  ← Pointing right
├── Up: (0, 1, 0)  ← Right-side up
├── Goal X: +20  ← Goal is 20m to the right
├── Goal Y: +10  ← Goal is 10m above
├── Goal Z: +20  ← Goal is 20m forward
├── Distance: 30  ← sqrt(20² + 10² + 20²)
└── Local goal: (0, 0.33, 0.94)
    └── "Goal is mostly in front, slightly above"

Agent should:
├── Stabilize tumble (counter angular velocity)
├── Rotate to point more at goal
└── Start thrusting
```

---

### **Scenario 2: Mid-flight, approaching goal**

```
Observations:
├── Position: (12, 5, 18)  ← Halfway there
├── Velocity: (5, 3, 8)  ← Moving toward goal
├── Angular vel: (0, 0, 0.1)  ← Mostly stable
├── Forward: (0.5, 0.3, 0.81)  ← Pointing toward goal
├── Up: (0, 0.95, 0.3)  ← Slight roll
├── Goal X: +8  ← 8m to the right
├── Goal Y: +5  ← 5m above
├── Goal Z: +2  ← 2m forward
├── Distance: 9.9  ← Close!
└── Local goal: (0.2, 0.1, 0.98)
    └── "Goal almost straight ahead!"

Agent should:
├── Current speed: 10 m/s (sqrt(5² + 3² + 8²))
├── Distance: 9.9m
├── Need to decelerate NOW!
├── Flip 180° (flip-and-burn)
└── Thrust to slow down to <3 m/s
```

---

### **Scenario 3: Final approach, need precision**

```
Observations:
├── Position: (19, 9.5, 19.5)  ← Very close
├── Velocity: (0.5, 0.2, 0.3)  ← Slow approach
├── Angular vel: (0, 0, 0)  ← Stable
├── Forward: (-0.5, -0.3, -0.81)  ← Pointing away (decelerating)
├── Up: (0, 1, 0)  ← Right-side up
├── Goal X: +1  ← Just 1m to the right
├── Goal Y: +0.5  ← 0.5m above
├── Goal Z: +0.5  ← 0.5m forward
├── Distance: 1.2  ← Almost there!
└── Local goal: (-0.7, -0.4, -0.59)
    └── "Goal is behind me (decelerating correctly)"

Agent should:
├── Current speed: 0.6 m/s (very slow!)
├── Distance: 1.2m
├── Perfect! Continue gentle approach
├── Small correction thrust
└── Should reach goal in ~2 seconds at 2.5 m/s ✅
```

---

## 🎓 **Why Individual X, Y, Z Components Matter**

### **Neural Network Perspective:**

**With explicit components:**
```python
# Network can learn simple relationships:
"If goal_x > 5 and local_z > 0.9:
    apply_right_torque()"

"If goal_z > 20 and velocity_z < 2:
    thrust_forward()"

"If goal_y < -10 and forward_y > -0.5:
    pitch_down()"
```

**With only vectors:**
```python
# Network has to learn to extract components internally:
"If goal_vector[0] > 5 and ..."  # Harder!
```

### **Learning Efficiency:**
```
Explicit components:
├── Faster learning (fewer layers needed)
├── Clearer gradients
├── Better interpretability
└── More stable training

Vector bundles:
├── Slower learning (network must learn to decompose)
├── Noisier gradients
├── Harder to debug
└── More training steps required
```

---

## 🎯 **Action Planning Examples**

### **Example 1: "I need to move right and up"**

```
Observations:
├── Goal X: +15  ← 15m to the right
├── Goal Y: +10  ← 10m up
├── Goal Z: +5   ← 5m forward

Agent reasons:
├── "Primary direction: right (+X = 15)"
├── "Secondary direction: up (+Y = 10)"
├── "Tertiary direction: forward (+Z = 5)"

Current orientation:
├── Forward: (1, 0, 0)  ← Pointing right
└── Need to pitch up ~45° to point toward goal

Actions:
├── Pitch up: +0.6 torque
├── Then thrust: 0.8
└── Net effect: Move right and up!
```

---

### **Example 2: "I need to decelerate"**

```
Observations:
├── Goal X: +2   ← Close!
├── Goal Y: +1   ← Close!
├── Goal Z: +3   ← Close!
├── Distance: 3.7m
├── Velocity: (5, 3, 8)  ← Speed = 9.9 m/s, too fast!

Agent reasons:
├── "Distance: 3.7m"
├── "Speed: 9.9 m/s"
├── "Will overshoot!"
├── "Need to flip and burn"

Velocity direction: (5, 3, 8) normalized = (0.5, 0.3, 0.8)

Actions:
├── Rotate to point at (-0.5, -0.3, -0.8)  ← Opposite velocity
├── Thrust: 1.0  ← Max deceleration
└── Net effect: Slow down to ~3 m/s before goal
```

---

## ✅ **Summary: Agent Now Knows**

### **About Itself:**
- ✅ Position in arena (3D)
- ✅ Velocity (3D vector)
- ✅ Angular velocity (rotation rate)
- ✅ Orientation (forward + up)

### **About the Goal (YOUR REQUEST!):**
- ✅ **Explicit X distance** (how far right/left)
- ✅ **Explicit Y distance** (how far up/down)
- ✅ **Explicit Z distance** (how far forward/back)
- ✅ Total distance (magnitude)
- ✅ Direction in local space (rotation needed)

### **What Agent Can Do:**
- ✅ Plan precise maneuvers in 3D
- ✅ Know exactly which direction to thrust
- ✅ Calculate deceleration timing
- ✅ Understand rotation needed
- ✅ Navigate methodically and intelligently

---

## 🚀 **Training Impact**

With explicit X, Y, Z components, expect:

```
✅ Faster learning (clearer observations)
✅ More direct paths (knows exactly where to go)
✅ Better rotation control (understands orientation vs. goal)
✅ Smoother arrivals (can plan deceleration per axis)
✅ Higher success rate (clear action → reward relationship)
```

---

## 📊 **Observation Vector Visualization**

```
Index | Observation          | Example Value | Meaning
------|----------------------|---------------|------------------------
0     | Position X           | 5.2           | 5.2m right of center
1     | Position Y           | -10.3         | 10.3m below center
2     | Position Z           | 15.8          | 15.8m forward
3     | Velocity X           | 2.5           | 2.5 m/s to the right
4     | Velocity Y           | -1.0          | 1.0 m/s downward
5     | Velocity Z           | 8.3           | 8.3 m/s forward
6     | Angular vel X        | 0.1           | Slow roll
7     | Angular vel Y        | -0.5          | Yawing left
8     | Angular vel Z        | 0.2           | Slight pitch
9     | Forward X            | 0.707         | Pointing 45° right
10    | Forward Y            | 0.0           | Level
11    | Forward Z            | 0.707         | 45° forward
12    | Up X                 | 0.0           | No roll
13    | Up Y                 | 1.0           | Right-side up
14    | Up Z                 | 0.0           | No tilt
15    | Goal offset X        | 12.5          | Goal 12.5m to right ← NEW!
16    | Goal offset Y        | -5.0          | Goal 5m below ← NEW!
17    | Goal offset Z        | 30.0          | Goal 30m forward ← NEW!
18    | Distance to goal     | 33.5          | Total distance
19    | Local goal X         | 0.37          | Goal somewhat right
20    | Local goal Y         | -0.15         | Goal slightly below
21    | Local goal Z         | 0.90          | Goal mostly ahead
```

**Total: 21 observations** ✅

---

**The agent now has complete, explicit, and actionable knowledge of where the goal is and how to reach it!** 🧠🎯✨
