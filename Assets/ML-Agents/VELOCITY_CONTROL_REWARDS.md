# 🎯 Velocity Control & Progress Rewards - Smooth & Goal-Seeking

## ✅ What Changed

### **1. Progress Reward DOUBLED (0.0005 → 0.001)**
```
Progress Reward: 0.001 per step  ← Doubled!

Effect: Now TWICE as valuable to move toward goal
```

### **2. Linear Velocity Penalty (NEW!)**
```
Linear Velocity Penalty: 0.0001 per (m/s) above threshold
Max Safe Linear Velocity: 10 m/s

Replaces: Old "excessive speed" penalty based on arbitrary 15 m/s
```

### **3. Angular Velocity Penalty (NEW!)**
```
Angular Velocity Penalty: 0.0002 per (rad/s) above threshold
Max Safe Angular Velocity: 2 rad/s

Effect: Discourages wild tumbling and spinning
```

---

## 📊 NEW Reward Structure

### **Per-Step Rewards:**
```
┌──────────────────────────────────────────────────────────┐
│ ALWAYS APPLIED:                                          │
├──────────────────────────────────────────────────────────┤
│ Time penalty: -0.0005                                    │
│                                                          │
│ CONDITIONAL (when moving toward goal):                  │
├──────────────────────────────────────────────────────────┤
│ Progress reward: +0.001  ← DOUBLED!                      │
│                                                          │
│ PENALTIES (when exceeding safe limits):                 │
├──────────────────────────────────────────────────────────┤
│ Linear velocity penalty:                                 │
│   if speed > 10 m/s:                                     │
│     -0.0001 × (speed - 10)                              │
│                                                          │
│ Angular velocity penalty:                                │
│   if angular speed > 2 rad/s:                            │
│     -0.0002 × (angular speed - 2)                        │
└──────────────────────────────────────────────────────────┘
```

### **Terminal Rewards (unchanged):**
```
Goal reached: +2.0 (minus speed penalty if > 3 m/s)
Collision: -1.0
Boundary: -1.0
Timeout: -1.0
```

---

## 🎯 Linear Velocity Penalty Details

### **How It Works:**
```csharp
float currentLinearSpeed = agentRigidbody.linearVelocity.magnitude;

if (currentLinearSpeed > maxSafeLinearVelocity)  // 10 m/s
{
    float excessLinearVelocity = currentLinearSpeed - maxSafeLinearVelocity;
    AddReward(-linearVelocityPenalty * excessLinearVelocity);
    // = -0.0001 × excess
}
```

### **Examples:**

**Speed = 5 m/s (safe):**
```
5 < 10  →  No penalty  ✓
```

**Speed = 12 m/s (slightly fast):**
```
12 - 10 = 2 m/s excess
Penalty = -0.0001 × 2 = -0.0002 per step

Over 100 steps: -0.02 (small penalty)
```

**Speed = 20 m/s (reckless):**
```
20 - 10 = 10 m/s excess
Penalty = -0.0001 × 10 = -0.001 per step

Over 100 steps: -0.1 (noticeable penalty)
Same as time penalty! Agent learns to slow down.
```

**Speed = 50 m/s (very reckless):**
```
50 - 10 = 40 m/s excess
Penalty = -0.0001 × 40 = -0.004 per step

Over 100 steps: -0.4 (significant penalty)
4× worse than time penalty! Strongly discourages.
```

### **Why 10 m/s Threshold?**
```
Arena diagonal: ~86m (sqrt(50² + 50² + 50²))

At 10 m/s:
├── Can cross arena in ~8.6 seconds
├── Reasonable travel speed
├── Still time to decelerate
└── Safe for maneuvering

Above 10 m/s:
├── Harder to control
├── Longer deceleration needed
├── Higher crash risk
└── Penalty discourages
```

---

## 🌀 Angular Velocity Penalty Details

### **How It Works:**
```csharp
float currentAngularSpeed = agentRigidbody.angularVelocity.magnitude;

if (currentAngularSpeed > maxSafeAngularVelocity)  // 2 rad/s
{
    float excessAngularVelocity = currentAngularSpeed - maxSafeAngularVelocity;
    AddReward(-angularVelocityPenalty * excessAngularVelocity);
    // = -0.0002 × excess
}
```

### **Examples:**

**Angular speed = 1 rad/s (controlled rotation):**
```
1 < 2  →  No penalty  ✓

Rotation rate: ~57°/second (moderate)
Effect: Smooth, controlled turning
```

**Angular speed = 3 rad/s (fast spinning):**
```
3 - 2 = 1 rad/s excess
Penalty = -0.0002 × 1 = -0.0002 per step

Over 100 steps: -0.02 (small penalty)
Rotation rate: ~172°/second (fast)
```

**Angular speed = 5 rad/s (tumbling):**
```
5 - 2 = 3 rad/s excess
Penalty = -0.0002 × 3 = -0.0006 per step

Over 100 steps: -0.06 (noticeable penalty)
Rotation rate: ~286°/second (wild tumbling)
Agent learns to stabilize!
```

**Angular speed = 10 rad/s (crazy tumbling):**
```
10 - 2 = 8 rad/s excess
Penalty = -0.0002 × 8 = -0.0016 per step

Over 100 steps: -0.16 (significant penalty)
Rotation rate: ~573°/second (out of control)
Strongly discourages wild spinning!
```

### **Why 2 rad/s Threshold?**
```
2 rad/s ≈ 115°/second

Allows:
├── Quick target acquisition
├── Flip-and-burn maneuvers
├── Obstacle avoidance rotations
└── Controlled re-orientation

Above 2 rad/s:
├── Harder to stabilize
├── Overshoot target orientation
├── Wasted control effort
└── Indicates loss of control
```

---

## 🎯 Progress Reward (DOUBLED!)

### **Before:**
```
Progress Reward: 0.0005 per step
Time Penalty: -0.0005 per step

Net when moving toward goal: 0 (balanced)
```

### **After:**
```
Progress Reward: 0.001 per step  ← DOUBLED!
Time Penalty: -0.0005 per step

Net when moving toward goal: +0.0005 ✅ POSITIVE!

Effect: Agent rewarded for goal-seeking behavior!
```

### **Impact:**

**Moving toward goal every step (1000 steps):**
```
Progress: +0.001 × 1000 = +1.0
Time: -0.0005 × 1000 = -0.5
Net: +0.5 before reaching goal! ✅

Strong incentive to seek goal!
```

**Moving toward goal 60% of steps (1000 steps):**
```
Progress: +0.001 × 600 = +0.6
Time: -0.0005 × 1000 = -0.5
Net: +0.1 (still positive!) ✅

Detours OK if generally progressing!
```

**Circling (no progress, 1000 steps):**
```
Progress: 0
Time: -0.5
Net: -0.5 (bad) ❌

Not moving toward goal = negative!
```

---

## 📊 Complete Reward Examples

### **Example 1: Perfect Smooth Navigation**
```
Scenario:
├── 30 seconds (1500 steps)
├── Linear velocity: 8 m/s (within safe limit)
├── Angular velocity: 1.5 rad/s (within safe limit)
├── Moving toward goal every step
├── Arrival speed: 2.8 m/s

Rewards:
├── Goal: +2.0
├── Time: -0.0005 × 1500 = -0.75
├── Progress: +0.001 × 1500 = +1.5  ← Big boost!
├── Linear penalty: 0 (within limit)
├── Angular penalty: 0 (within limit)
├── Arrival speed: 0 (safe)
└── TOTAL: +2.0 - 0.75 + 1.5 = +2.75 ✅ Excellent!
```

### **Example 2: Fast But Controlled**
```
Scenario:
├── 20 seconds (1000 steps)
├── Linear velocity: 15 m/s (5 m/s over limit)
├── Angular velocity: 2.5 rad/s (0.5 rad/s over limit)
├── Moving toward goal 90% of steps
├── Arrival speed: 3.5 m/s (slightly fast)

Rewards:
├── Goal base: +2.0
├── Arrival speed penalty: -0.5 × 0.1 = -0.05
├── Goal total: +1.95
├── Time: -0.0005 × 1000 = -0.5
├── Progress: +0.001 × 900 = +0.9
├── Linear penalty: -0.0001 × 5 × 1000 = -0.5
├── Angular penalty: -0.0002 × 0.5 × 1000 = -0.1
└── TOTAL: +1.95 - 0.5 + 0.9 - 0.5 - 0.1 = +1.75 ✅ Good!
```

### **Example 3: Methodical & Cautious**
```
Scenario:
├── 50 seconds (2500 steps)
├── Linear velocity: 6 m/s (very safe)
├── Angular velocity: 1.0 rad/s (very controlled)
├── Moving toward goal 70% of steps (detours around asteroids)
├── Arrival speed: 2.0 m/s (very safe)

Rewards:
├── Goal: +2.0
├── Time: -0.0005 × 2500 = -1.25
├── Progress: +0.001 × 1750 = +1.75  ← Progress > time!
├── Linear penalty: 0 (within limit)
├── Angular penalty: 0 (within limit)
├── Arrival speed: 0 (safe)
└── TOTAL: +2.0 - 1.25 + 1.75 = +2.5 ✅ Great!
```

### **Example 4: Reckless & Wild**
```
Scenario:
├── 15 seconds (750 steps)
├── Linear velocity: 25 m/s (15 m/s over limit!)
├── Angular velocity: 5 rad/s (3 rad/s over limit!)
├── Moving toward goal 80% of steps (mostly straight)
├── Crashed!

Rewards:
├── Collision: -1.0
├── Time: -0.0005 × 750 = -0.375
├── Progress: +0.001 × 600 = +0.6
├── Linear penalty: -0.0001 × 15 × 750 = -1.125
├── Angular penalty: -0.0002 × 3 × 750 = -0.45
└── TOTAL: -1.0 - 0.375 + 0.6 - 1.125 - 0.45 = -2.35 ❌ Very bad!

Agent learns: Fast progress doesn't justify reckless velocity!
```

### **Example 5: Tumbling & Lost**
```
Scenario:
├── 60 seconds timeout
├── Linear velocity: 12 m/s (2 m/s over)
├── Angular velocity: 8 rad/s (6 rad/s over - wild tumbling!)
├── Moving toward goal only 20% of steps (out of control)

Rewards:
├── Timeout: -1.0
├── Time: -0.0005 × 3000 = -1.5
├── Progress: +0.001 × 600 = +0.6
├── Linear penalty: -0.0001 × 2 × 3000 = -0.6
├── Angular penalty: -0.0002 × 6 × 3000 = -3.6  ← Huge!
└── TOTAL: -1.0 - 1.5 + 0.6 - 0.6 - 3.6 = -6.1 ❌ Terrible!

Agent learns: Must control tumbling! Angular velocity very expensive.
```

---

## 📊 Comparison Table

| Strategy | Progress | Velocities | Goal | **TOTAL** |
|----------|----------|------------|------|-----------|
| **Perfect smooth** | +1.5 | 0 | +2.0 | **+2.75** ✅ |
| **Methodical** | +1.75 | 0 | +2.0 | **+2.5** ✅ |
| **Fast controlled** | +0.9 | -0.6 | +1.95 | **+1.75** ✅ |
| **Reckless** | +0.6 | -1.575 | -1.0 | **-2.35** ❌ |
| **Tumbling** | +0.6 | -4.2 | -1.0 | **-6.1** ❌ |

**Key Insight:** Smooth, controlled navigation wins!

---

## 🎯 Why Separate Linear & Angular Penalties?

### **Before (single speed penalty):**
```
Problem:
├── Only penalized linear speed > 15 m/s
├── Angular velocity not penalized at all
└── Agent could tumble wildly without penalty

Result:
├── Agent sometimes spun out of control
├── Wasteful torque usage
└── Difficult to aim thrust precisely
```

### **After (separate penalties):**
```
Benefits:
├── Linear velocity penalty → smooth translation
├── Angular velocity penalty → controlled rotation
└── Agent learns both are important!

Result:
├── Smooth, controlled flight ✅
├── Precise thrust aiming ✅
├── Efficient fuel usage ✅
└── Professional-looking navigation ✅
```

### **Real CubeSat Analogy:**
```
Real spacecraft:
├── Limited reaction wheel capacity (angular control)
├── Limited fuel (linear control)
├── Must conserve BOTH
└── Smooth, efficient maneuvers preferred

Our agent now learns the same principles!
```

---

## ⚙️ Tuning Knobs (Inspector)

### **Current Settings:**
```
Reward Settings:
├── Goal Reward: 2.0
├── Collision Penalty: -1.0
├── Timeout Penalty: -1.0
├── Time Step Penalty: -0.0005
├── Progress Reward: 0.001  ← DOUBLED!
├── Max Speed At Goal: 3.0
├── Speed Penalty Multiplier: 0.1
├── Linear Velocity Penalty: 0.0001  ← NEW!
├── Angular Velocity Penalty: 0.0002  ← NEW!
├── Max Safe Linear Velocity: 10  ← NEW!
└── Max Safe Angular Velocity: 2  ← NEW!
```

### **If Agent Too Conservative:**
```
Increase safe thresholds:
├── Max Safe Linear Velocity: 15  ← Allow faster
└── Max Safe Angular Velocity: 3  ← Allow more rotation

Or reduce penalties:
├── Linear Velocity Penalty: 0.00005  ← Half
└── Angular Velocity Penalty: 0.0001  ← Half
```

### **If Agent Too Reckless:**
```
Decrease safe thresholds:
├── Max Safe Linear Velocity: 8  ← Stricter
└── Max Safe Angular Velocity: 1.5  ← Stricter

Or increase penalties:
├── Linear Velocity Penalty: 0.0002  ← Double
└── Angular Velocity Penalty: 0.0004  ← Double
```

### **If Agent Not Goal-Seeking:**
```
Increase progress reward:
├── Progress Reward: 0.002  ← Triple original!
└── Or increase goal reward: 3.0
```

### **If Agent Takes Too Many Detours:**
```
Reduce progress reward slightly:
├── Progress Reward: 0.0007
└── Makes direct paths more valuable
```

---

## 📈 Expected Training Behavior

### **Early Training (0-100k):**
```
Behavior:
├── Random exploration
├── Learning to control velocities
├── Discovering progress reward
└── Many crashes and tumbles

Velocities:
├── Often exceeds safe limits
├── Wild tumbling common
└── Learning to stabilize

Mean Reward: -2.0 to -1.0
```

### **Mid Training (100k-500k):**
```
Behavior:
├── More goal-seeking (progress reward working!)
├── Better velocity control
├── Less tumbling
├── Still learning optimal balance

Velocities:
├── Usually within safe limits
├── Occasional excursions when urgent
└── Learning smooth control

Mean Reward: -0.5 to +1.0
```

### **Late Training (500k-2M+):**
```
Behavior:
├── Smooth, controlled navigation ✅
├── Efficient goal-seeking ✅
├── Minimal tumbling ✅
├── Balanced speed vs. safety ✅

Velocities:
├── Mostly within safe limits (6-9 m/s linear)
├── Controlled rotation (1-1.8 rad/s angular)
├── Brief excursions when needed
└── Smooth deceleration near goal

Mean Reward: +1.5 to +2.5 ✅
```

---

## 🧪 Testing the Changes

### **Test 1: Progress Reward (Doubled)**
```
1. Set Behavior Type: Heuristic Only
2. Press Play
3. Fly toward goal (W + rotation)
4. Watch reward accumulation
5. Should see faster positive accumulation than before ✓

Expected:
├── Moving toward goal for 20 seconds
├── Progress: +0.001 × 1000 = +1.0
└── Much more than before (+0.5) ✅
```

### **Test 2: Linear Velocity Penalty**
```
1. Heuristic mode
2. Hold W (thrust) for 10+ seconds
3. Build up high speed (>10 m/s)
4. Watch Console for negative rewards
5. Should see penalty increasing with speed ✓

Expected:
├── Speed = 15 m/s → -0.0005 per step
├── Speed = 20 m/s → -0.001 per step
└── Speed = 30 m/s → -0.002 per step
```

### **Test 3: Angular Velocity Penalty**
```
1. Heuristic mode
2. Hold pitch/yaw/roll keys continuously
3. Build up angular velocity (>2 rad/s)
4. Watch Console for negative rewards
5. Should see penalty for tumbling ✓

Expected:
├── Angular vel = 3 rad/s → -0.0002 per step
├── Angular vel = 5 rad/s → -0.0006 per step
└── Angular vel = 10 rad/s → -0.0016 per step
```

### **Test 4: Smooth Navigation**
```
1. Heuristic mode
2. Navigate to goal carefully
3. Keep speed < 10 m/s
4. Keep rotation < 2 rad/s
5. Arrive safely

Expected final reward:
├── Goal: +2.0
├── Progress: +1.5 to +2.0
├── Time: -0.5 to -1.0
├── Velocity penalties: 0
└── Total: +2.0 to +2.5 ✅ Excellent!
```

---

## 🎯 Training Strategy Impact

### **What Agent Will Learn:**

**High Priority (strongly rewarded):**
```
✅ Move toward goal consistently (progress reward)
✅ Reach goal (terminal reward)
✅ Control velocities (avoid penalties)
✅ Smooth, efficient navigation
```

**Medium Priority (balanced):**
```
✅ Time efficiency (small penalty for taking too long)
✅ Safe arrival speed (moderate penalty for fast arrival)
```

**Penalized Behaviors:**
```
❌ High linear velocity (penalty scales with excess)
❌ High angular velocity (2× penalty, scales with excess)
❌ Circling/not progressing (no progress reward)
❌ Timeouts (terminal penalty)
❌ Collisions (terminal penalty)
```

### **Emergent Behaviors:**

**Agent will learn to:**
```
1. Accelerate to ~8-10 m/s (efficient but safe)
2. Maintain stable orientation (~1-2 rad/s rotation)
3. Navigate smoothly toward goal (maximize progress)
4. Plan deceleration early (avoid velocity penalties)
5. Arrive gently (avoid arrival speed penalty)
6. Balance speed vs. control (optimal efficiency)
```

---

## 📊 Reward Math Summary

### **Per Step (assuming 50 FPS = 0.02s per step):**

| Condition | Reward | Effect |
|-----------|--------|--------|
| **Time exists** | -0.0005 | Encourages efficiency |
| **Moving toward goal** | +0.001 | Encourages progress |
| **Linear vel = 15 m/s** | -0.0005 | Mild discouragement |
| **Linear vel = 25 m/s** | -0.0015 | Strong discouragement |
| **Angular vel = 4 rad/s** | -0.0004 | Discourage tumbling |
| **Angular vel = 8 rad/s** | -0.0012 | Strong anti-tumble |

### **Net Per Step Examples:**

**Perfect navigation (toward goal, smooth):**
```
-0.0005 (time) + 0.001 (progress) = +0.0005 ✅ Positive!
```

**Fast navigation (toward goal, 15 m/s):**
```
-0.0005 (time) + 0.001 (progress) - 0.0005 (speed) = 0 ⚖️ Neutral
```

**Reckless (toward goal, 25 m/s, 5 rad/s tumbling):**
```
-0.0005 (time) + 0.001 (progress) - 0.0015 (speed) - 0.0006 (angular)
= -0.0016 ❌ Negative!
```

---

## 🚀 Restart Training

Since reward structure changed significantly, **restart training**:

```bash
# Stop current training (Ctrl+C)

# Start fresh with new velocity rewards
mlagents-learn Assets/ML-Agents/CubeSat.yaml --run-id=CubeSat_VelocityControl_v5

# Press Play when prompted
```

---

## 📁 Summary of Changes

### **Code Changes:**
```
✅ Progress Reward: 0.0005 → 0.001 (doubled)
✅ Removed: excessiveSpeedPenalty (0.0001)
✅ Added: linearVelocityPenalty (0.0001)
✅ Added: angularVelocityPenalty (0.0002)
✅ Added: maxSafeLinearVelocity (10 m/s)
✅ Added: maxSafeAngularVelocity (2 rad/s)
✅ Updated: OnActionReceived() with new penalty logic
```

### **Expected Results:**
```
✅ Stronger goal-seeking (2× progress reward)
✅ Smoother flight (linear velocity control)
✅ Controlled rotation (angular velocity penalty)
✅ Professional-looking navigation
✅ Faster learning (clearer reward signals)
✅ Higher success rates
```

---

**Your agent will now learn smooth, controlled, goal-seeking navigation!** 🛰️🎯✨

**Key improvements:**
1. **2× stronger progress reward** → more goal-seeking
2. **Linear velocity penalty** → smooth translation
3. **Angular velocity penalty** → controlled rotation

**Result: Professional spacecraft navigation!** 🚀
