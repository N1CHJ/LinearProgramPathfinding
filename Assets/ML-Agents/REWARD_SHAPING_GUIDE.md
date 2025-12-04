# 🎯 Reward Shaping Guide - CubeSat Navigation

## 🚨 Problem You Identified (270k steps)

**Issue:** Agent learns to "stay alive" instead of "reach goal"

**Why:**
```
Time penalty over 30s: -0.001 × ~1500 steps = -1.5
Collision penalty: -1.0
Boundary penalty: -1.0
Timeout penalty: -0.5
Goal reward: +1.0

Agent thinks:
"Avoiding crashes (-1.0) is as good as reaching goal (+1.0)!"
"Staying alive longer = less penalty per second"
```

**Result:** Agent circles around, avoiding asteroids, not pursuing goal ❌

---

## ✅ NEW REWARD STRUCTURE (Balanced & Smart)

### **Configurable Reward Settings (NEW!):**

```
Select: CubeSat → Inspector → Reward Settings

Goal Reward: 2.0                    ← DOUBLED! Goal is now 2× better
Collision Penalty: -1.0             ← Same (crash = bad)
Timeout Penalty: -1.0               ← CHANGED from -0.5
Time Step Penalty: -0.001           ← Same (small per-step cost)
Max Speed At Goal: 3.0              ← NEW! Safe arrival speed
Speed Penalty Multiplier: 0.1       ← NEW! Penalty for fast arrival
Excessive Speed Penalty: 0.0001     ← NEW! Tiny penalty for going too fast
```

---

## 📊 Reward Breakdown

### **1. Goal Reward (Success!)**
```
Base: +2.0

Speed at arrival:
├── Speed ≤ 3.0 m/s: Full +2.0 reward ✅ (safe landing!)
├── Speed = 5.0 m/s: +2.0 - (2.0 × 0.1) = +1.8
├── Speed = 10.0 m/s: +2.0 - (7.0 × 0.1) = +1.3 (too fast!)
└── Speed = 20.0 m/s: +2.0 - (17.0 × 0.1) = +0.3 (crash speed!)

Formula:
reward = goalReward - (excessSpeed × speedPenaltyMultiplier)
where excessSpeed = max(0, currentSpeed - maxSpeedAtGoal)
```

**Effect:** Encourages controlled, safe arrival! 🛰️

---

### **2. Time-Based Penalties**

#### **Per-Step Penalty:**
```
-0.001 per FixedUpdate step

Over 30 seconds (~1500 steps):
Total = -0.001 × 1500 = -1.5

Over 60 seconds (~3000 steps):
Total = -0.001 × 3000 = -3.0  ← Way worse than timeout!
```

#### **Timeout Penalty (NEW!):**
```
If episode reaches maxEpisodeTime (30s):
Penalty = -1.0  ← NOW EQUALS collision/boundary penalties!

Total if timing out:
= timeStepPenalty × steps + timeoutPenalty
= (-0.001 × 1500) + (-1.0)
= -1.5 + -1.0
= -2.5  ← Worse than collision (-1.0)!
```

**Effect:** Timing out is now worse than crashing → Agent motivated to take action!

---

### **3. Failure Penalties (Equal Weight)**

```
Collision with asteroid: -1.0
Leaving boundary: -1.0
Timeout (30s): -1.0 (via timeoutPenalty)

All failures now equal!
Agent doesn't prefer "survival" over "action"
```

---

### **4. Excessive Speed Penalty (NEW!)**

```
If speed > 15 m/s during flight:
penalty = -0.0001 × (speed - 15)

Examples:
├── Speed = 10 m/s: No penalty ✓
├── Speed = 15 m/s: No penalty ✓
├── Speed = 20 m/s: -0.0001 × 5 = -0.0005 per step
├── Speed = 30 m/s: -0.0001 × 15 = -0.0015 per step
└── Speed = 50 m/s: -0.0001 × 35 = -0.0035 per step

If going 30 m/s for 100 steps:
Total = -0.0015 × 100 = -0.15  (noticeable but small)
```

**Effect:** Gentle discouragement of reckless speed (harder to control = more crashes)

---

## 🎯 Complete Reward Examples

### **Example 1: Perfect Navigation**
```
Scenario:
├── Reach goal in 15 seconds
├── Arrival speed: 2.5 m/s (safe!)
├── No crashes, smooth flight

Rewards:
├── Goal: +2.0 (speed ≤ 3.0 m/s, no penalty)
├── Time: -0.001 × ~750 steps = -0.75
├── Speed: ~0 (stayed under 15 m/s)
└── TOTAL: +2.0 - 0.75 = +1.25 ✅ Positive!
```

### **Example 2: Fast But Controlled**
```
Scenario:
├── Reach goal in 10 seconds (fast!)
├── Arrival speed: 8.0 m/s (too fast)
├── Went 20 m/s for ~200 steps mid-flight

Rewards:
├── Goal base: +2.0
├── Speed penalty: (8.0 - 3.0) × 0.1 = -0.5
├── Goal total: +2.0 - 0.5 = +1.5
├── Time: -0.001 × ~500 = -0.5
├── Excessive speed: -0.0001 × 5 × 200 = -0.1
└── TOTAL: +1.5 - 0.5 - 0.1 = +0.9 ✅ Still positive but less
```

### **Example 3: Crashed**
```
Scenario:
├── Crashed into asteroid at 8 seconds
├── Was going 25 m/s when crashed

Rewards:
├── Collision: -1.0
├── Time: -0.001 × ~400 = -0.4
├── Excessive speed: -0.0001 × 10 × 400 = -0.4
└── TOTAL: -1.0 - 0.4 - 0.4 = -1.8 ❌ Negative
```

### **Example 4: Timeout (Circling)**
```
Scenario:
├── Timeout at 30 seconds
├── Stayed under 15 m/s (safe but slow)
├── Never reached goal

Rewards:
├── Timeout: -1.0
├── Time: -0.001 × ~1500 = -1.5
└── TOTAL: -1.0 - 1.5 = -2.5 ❌ Very negative!

Worse than crashing! Agent learns: "Do something, don't circle!"
```

### **Example 5: Left Boundary**
```
Scenario:
├── Left boundary at 12 seconds
├── Was thrusting hard, went 35 m/s

Rewards:
├── Boundary: -1.0
├── Time: -0.001 × ~600 = -0.6
├── Excessive speed: -0.0001 × 20 × 600 = -1.2
└── TOTAL: -1.0 - 0.6 - 1.2 = -2.8 ❌ Very bad
```

---

## 🎯 Reward Comparison Table

| Outcome | Goal | Time | Speed | Timeout | Failure | **TOTAL** |
|---------|------|------|-------|---------|---------|-----------|
| **Perfect (slow, safe)** | +2.0 | -0.75 | 0 | 0 | 0 | **+1.25** ✅ |
| **Perfect (fast, safe)** | +2.0 | -0.5 | -0.1 | 0 | 0 | **+1.4** ✅ Best! |
| **Goal (too fast)** | +1.5 | -0.5 | -0.1 | 0 | 0 | **+0.9** ✅ OK |
| **Crashed** | 0 | -0.4 | -0.4 | 0 | -1.0 | **-1.8** ❌ |
| **Timeout (circling)** | 0 | -1.5 | 0 | -1.0 | 0 | **-2.5** ❌ Worst! |
| **Boundary** | 0 | -0.6 | -1.2 | 0 | -1.0 | **-2.8** ❌ |

**Key Insight:** Timeout is now worse than crashing! Agent must take action.

---

## 🧠 Expected Learning Behavior

### **Phase 1: Random Exploration (0-50k)**
```
Behavior:
├── Random thrusting/rotation
├── Many crashes and timeouts
└── Occasional lucky goal reaches

Rewards:
├── Mean: -2.0 to -3.0 (very negative)
└── Std: High (inconsistent)
```

### **Phase 2: Basic Navigation (50k-200k)**
```
Behavior:
├── Starts rotating toward goal
├── Still crashes often
├── Some overshoots (too fast)
└── Fewer timeouts (taking action!)

Rewards:
├── Mean: -1.0 to 0 (improving)
└── Occasional positive spikes (goal reaches)
```

### **Phase 3: Controlled Movement (200k-500k)**
```
Behavior:
├── Smoother rotation
├── Thrust + rotation coordination
├── Some speed control
└── 20-40% success rate

Rewards:
├── Mean: 0 to +0.5 (positive trend!)
└── More consistent positive rewards
```

### **Phase 4: Optimized Navigation (500k-2M+)**
```
Behavior:
├── Efficient flip-and-burn
├── Smooth deceleration
├── Safe arrival speeds (<3 m/s)
├── Good obstacle avoidance
└── 60-80%+ success rate

Rewards:
├── Mean: +0.5 to +1.0 (consistently positive!)
└── Low std (stable policy)
```

---

## ⚙️ Tuning the Rewards

### **If Agent Still Circles (Not Goal-Seeking):**

**Increase goal reward:**
```
Goal Reward: 3.0  ← Make goal even more attractive
Timeout Penalty: -1.5  ← Make timeout even worse
```

**Or decrease time penalty:**
```
Time Step Penalty: -0.0005  ← Less penalty for taking time
```

---

### **If Agent Crashes Too Often (Too Aggressive):**

**Increase collision penalty:**
```
Collision Penalty: -1.5  ← Make crashes more painful
```

**Or add distance-to-goal shaping (advanced):**
```csharp
// In OnActionReceived, after applying forces:
float distanceToGoal = Vector3.Distance(transform.position, goalTransform.position);
float previousDistance = ... // Store from last step
if (distanceToGoal < previousDistance)
{
    AddReward(0.0001f);  // Small reward for getting closer
}
```

---

### **If Agent Arrives Too Fast (Unsafe):**

**Increase speed penalties:**
```
Speed Penalty Multiplier: 0.2  ← Double the penalty
Max Speed At Goal: 2.0  ← Lower safe speed threshold
```

**Or add stronger excessive speed penalty:**
```
Excessive Speed Penalty: 0.0005  ← 5× stronger during flight
```

---

### **If Agent Is Too Slow (Conservative):**

**Reduce speed penalties:**
```
Speed Penalty Multiplier: 0.05  ← Halve the penalty
Max Speed At Goal: 5.0  ← Allow faster arrivals
Excessive Speed Penalty: 0.00005  ← Weaken flight penalty
```

**Or increase timeout pressure:**
```
Max Episode Time: 20  ← Shorter episodes
Timeout Penalty: -1.5  ← Harsher timeout
```

---

## 🎮 Recommended Starting Values (Current Setup)

```
CubeSat Agent → Inspector → Reward Settings:

✓ Goal Reward: 2.0                 ← 2× better than any failure
✓ Collision Penalty: -1.0          ← Equal to other failures
✓ Timeout Penalty: -1.0            ← Equal to collision/boundary
✓ Time Step Penalty: -0.001        ← Small per-step cost
✓ Max Speed At Goal: 3.0           ← Reasonable safe speed
✓ Speed Penalty Multiplier: 0.1    ← Moderate speed penalty
✓ Excessive Speed Penalty: 0.0001  ← Tiny flight speed penalty

Expected total rewards:
├── Perfect navigation: +1.0 to +1.5 ✅
├── Fast navigation: +0.5 to +1.0 ✅
├── Crash: -1.5 to -2.0 ❌
├── Timeout: -2.0 to -2.5 ❌
└── Boundary: -2.0 to -3.0 ❌
```

---

## 📊 Monitoring in TensorBoard

### **Key Metrics:**

**Environment/Cumulative Reward:**
```
Target progression:
├── 0-100k steps: -2.0 to -1.0 (learning basics)
├── 100k-500k: -1.0 to 0 (getting better)
├── 500k-1M: 0 to +0.5 (consistent success)
└── 1M+: +0.5 to +1.0 (optimized) ✅
```

**Environment/Episode Length:**
```
Target:
├── Early: 300-1000 steps (many timeouts)
├── Mid: 200-500 steps (taking action)
└── Late: 200-400 steps (efficient navigation)
```

**Policy/Learning Rate:**
```
Should decrease linearly from 3e-4 to 0
```

---

## 🧪 Testing Your Changes

### **Test 1: Quick Manual Test**
```
1. Set Behavior Type: Heuristic Only (for testing)
2. Press Play
3. Reach goal manually (W + rotation)
4. Check Console for final reward
5. Should see: "Reward: +1.5 to +2.0" if slow/safe ✓
```

### **Test 2: Monitor First Episode**
```
1. Set Behavior Type: Default
2. Start training
3. Press Play in Unity
4. Watch first episode in Scene view
5. Check Console: Early rewards should be very negative (-2 to -3)
```

### **Test 3: Compare to Previous Run**
```
After 100k steps:
├── Old mean reward: -1.5 to -2.0 (circling)
└── New mean reward: Should be better (-1.0 to -0.5) ✓

After 500k steps:
├── Old: Still negative (not goal-seeking)
└── New: Should be positive or near 0 (goal-seeking!) ✓
```

---

## 🎯 Success Criteria

**Training is working well when:**

1. **Mean Reward Trends Upward**
   - 100k: -1.5 to -1.0
   - 300k: -0.5 to 0
   - 500k: 0 to +0.5
   - 1M+: +0.5 to +1.0 ✅

2. **Agent Behavior Shows:**
   - ✅ Active goal-seeking (not circling)
   - ✅ Rotation toward goal
   - ✅ Thrust + rotation coordination
   - ✅ Deceleration near goal (flip-and-burn)
   - ✅ Safe arrival speeds (<3-5 m/s)

3. **Episode Outcomes:**
   - ✅ Timeout rate decreases (<20% by 500k steps)
   - ✅ Goal reach rate increases (>50% by 1M steps)
   - ✅ Crash rate moderate (20-30% acceptable)

---

## 🚀 Summary of Changes

### **What Changed:**

```
OLD Reward Structure:
├── Goal: +1.0
├── Collision: -1.0
├── Boundary: -1.0
├── Timeout: -0.5  ← Too lenient!
├── Time: -0.001 per step
└── Problem: Circling = less bad than trying

NEW Reward Structure:
├── Goal: +2.0  ← DOUBLED!
├── Collision: -1.0
├── Boundary: -1.0
├── Timeout: -1.0  ← Equal to failures!
├── Time: -0.001 per step
├── Speed at goal: 0 to -1.7  ← Encourages safe arrival!
└── Excessive speed: Small penalty  ← Discourages recklessness
```

### **Expected Effect:**

```
Before:
├── Agent circles, avoids asteroids
├── Rarely reaches goal
└── Learns "survival > success"

After:
├── Agent seeks goal actively
├── Balances speed vs. safety
├── Learns "controlled success > survival"
└── Develops flip-and-burn maneuvers ✅
```

---

## 🎓 Advanced: Curriculum Learning (Optional)

**If training is still difficult, try progressive difficulty:**

### **Stage 1: Easy (0-500k steps)**
```
CubeSat Agent:
├── Min Goal Distance: 15  ← Closer goals
├── Max Episode Time: 40  ← More time

AsteroidSpawner:
├── Asteroid Count: 10  ← Fewer obstacles
├── Max Drift Speed: 1.0  ← Slower asteroids
```

### **Stage 2: Medium (500k-1.5M steps)**
```
CubeSat Agent:
├── Min Goal Distance: 20
├── Max Episode Time: 30

AsteroidSpawner:
├── Asteroid Count: 20
├── Max Drift Speed: 2.0
```

### **Stage 3: Hard (1.5M+ steps)**
```
CubeSat Agent:
├── Min Goal Distance: 25  ← Current setting
├── Max Episode Time: 30

AsteroidSpawner:
├── Asteroid Count: 30
├── Max Drift Speed: 3.0
```

---

**Your reward structure is now balanced! Time to retrain and see goal-seeking behavior!** 🚀✨

**Key improvement:** Timeout penalty now equals failure penalties → Agent learns to ACT instead of SURVIVE!
