# ✅ Pre-Training Checklist - CubeSat Agent

## 🚨 CRITICAL: Change Behavior Type Before Training!

### **⚠️ YOU MUST DO THIS:**

1. **Select CubeSat** in Hierarchy
2. **Find Behavior Parameters** component in Inspector
3. **Change Behavior Type:**
   ```
   Behavior Type: Default  ← Change from "Heuristic Only"!
   ```
4. **Save Scene** (Ctrl+S / Cmd+S)

**WHY:** 
- `Heuristic Only` = manual keyboard control (for testing)
- `Default` = AI training mode (required for ML-Agents)

---

## 📋 Complete Pre-Training Checklist

### **1. BehaviorParameters Settings** ⚠️ CRITICAL

```
Select: CubeSat → Inspector → Behavior Parameters

✓ Behavior Type: Default  ← MUST BE "Default" for training!
✓ Behavior Name: CubeSat
✓ Vector Observation
  ├── Space Size: 25
  └── Stacked Vectors: 1
✓ Actions
  ├── Continuous Actions: 4
  └── Discrete Branches: Size 0
```

**Verify:**
- [ ] Behavior Type is **Default** (not Heuristic Only!)
- [ ] Space Size is **25** (not 16)
- [ ] Continuous Actions is **4** (not 3)

---

### **2. CubeSat Agent Settings**

```
Select: CubeSat → Inspector → CubeSat Agent

Physics:
├── Max Thrust: 10
├── Max Torque: 2
└── Agent Mass: 1

Episode:
└── Max Episode Time: 30

Goal:
├── Goal Reach Distance: 2
└── Goal Transform: /Goal ✓

Boundary:
├── Enforce Boundary: ☑
├── Boundary Size: (80, 80, 80)
└── Boundary Penalty: -1

Spawn Settings:
├── Randomize Start Position: ☑
├── Start Spawn Area Size: (20, 20, 20)
├── Randomize Goal Position: ☑
├── Goal Spawn Area Size: (80, 80, 80)
└── Min Goal Distance: 25

References:
├── Asteroid Spawner: /AsteroidSpawner ✓
└── Telemetry UI: /TelemetryManager ✓
```

**Verify:**
- [ ] Goal Transform is assigned
- [ ] Asteroid Spawner is assigned
- [ ] Randomize Start/Goal are checked

---

### **3. Scene Setup**

```
Hierarchy Check:
├── CubeSat ✓
├── Goal ✓
├── AsteroidSpawner ✓
├── TelemetryManager ✓
├── Arena ✓
└── Main Camera ✓
```

**Verify:**
- [ ] All GameObjects exist in scene
- [ ] Scene is saved

---

### **4. Training Configuration**

**File:** `/Assets/ML-Agents/CubeSat.yaml`

```yaml
behaviors:
  CubeSat:
    trainer_type: ppo
    hyperparameters:
      batch_size: 2048         # Increased for better stability
      buffer_size: 20480       # Increased for complex task
      learning_rate: 3.0e-4
      beta: 5.0e-3
      epsilon: 0.2
      lambd: 0.95
      num_epoch: 3
      learning_rate_schedule: linear
    network_settings:
      normalize: true          # ✅ Enabled (important!)
      hidden_units: 256
      num_layers: 3            # Increased for rotation+thrust
      vis_encode_type: simple
    reward_signals:
      extrinsic:
        gamma: 0.99
        strength: 1.0
    max_steps: 3000000         # 3M steps (rotation is harder)
    time_horizon: 128          # Longer horizon for planning
    summary_freq: 10000
    keep_checkpoints: 5
    checkpoint_interval: 100000
```

**Key Changes for Realistic Physics:**
- ✅ **normalize: true** (helps with varied observations)
- ✅ **num_layers: 3** (deeper network for rotation coordination)
- ✅ **time_horizon: 128** (longer for multi-step maneuvers)
- ✅ **max_steps: 3M** (more training needed)
- ✅ **batch_size: 2048** (larger batches for stability)

**Verify:**
- [ ] File exists at `/Assets/ML-Agents/CubeSat.yaml`
- [ ] `normalize: true` is set

---

### **5. Environment Validation**

**Test in Play Mode:**

1. **Press Play** ▶️
2. **Check randomization:**
   - [ ] CubeSat starts in different positions
   - [ ] Goal appears in different locations
   - [ ] Asteroids are randomized
3. **Check telemetry:**
   - [ ] Velocity updates
   - [ ] Angular Velocity updates (shows values when rotating)
   - [ ] CubeSat Pos updates
   - [ ] Goal Pos displays correctly
4. **Check boundaries:**
   - [ ] CubeSat can move in full 3D (including -Y)
   - [ ] Episode ends if leaving boundary
5. **Stop Play**

---

### **6. Training Command**

**Open Terminal/Command Prompt:**

```bash
# Navigate to your project root
cd /path/to/pathfinding_1

# Start training
mlagents-learn Assets/ML-Agents/CubeSat.yaml --run-id=CubeSat_Realistic_v1

# When prompted "Start training by pressing...", click Play ▶️ in Unity
```

**Important:**
- Run command from **project root directory**
- Use `--run-id` to name this training run
- Press Play in Unity **after** seeing the prompt
- Keep Unity in focus initially to ensure connection

---

### **7. Expected Training Behavior**

#### **Early Training (0-100k steps):**
```
Behavior:
├── Random rotation
├── Random thrusting
├── Many crashes into asteroids
├── Many boundary violations
├── Rare goal reaching

Metrics:
├── Cumulative Reward: -50 to -200 (very negative)
├── Episode Length: 30-300 steps (many timeouts)
└── Success Rate: <5%
```

#### **Mid Training (100k-500k steps):**
```
Behavior:
├── Starting to rotate toward goal
├── Some thrust coordination
├── Fewer random crashes
├── Still many overshoots

Metrics:
├── Cumulative Reward: -20 to +20 (improving)
├── Episode Length: 100-400 steps
└── Success Rate: 10-30%
```

#### **Late Training (500k-2M+ steps):**
```
Behavior:
├── Smooth rotation toward goal
├── Coordinated thrust + rotation
├── Flip-and-burn deceleration
├── Obstacle avoidance
├── Efficient navigation

Metrics:
├── Cumulative Reward: +50 to +200 (positive!)
├── Episode Length: 200-500 steps (efficient)
└── Success Rate: 60-80%+
```

---

### **8. Monitoring Training**

#### **TensorBoard (Recommended):**
```bash
# In a new terminal
tensorboard --logdir results

# Open browser to: http://localhost:6006
```

**Key Metrics to Watch:**
```
Environment/Cumulative Reward
├── Should trend upward over time
└── Target: Eventually positive (+50 to +200)

Environment/Episode Length
├── Should stabilize
└── Target: 200-500 steps

Losses/Policy Loss
├── Should decrease initially
└── Then stabilize (small oscillations normal)

Policy/Learning Rate
└── Should decrease linearly to 0
```

#### **Console Output:**
```
Example good progress:
[INFO] Step: 50000.  Time Elapsed: 145.2 s.
Mean Reward: -45.3   ← Improving (was -100)
Std of Reward: 23.4
...
[INFO] Step: 500000. Time Elapsed: 2341.7 s.
Mean Reward: 15.2    ← Positive! Good sign
Std of Reward: 35.6
```

---

### **9. Common Issues & Solutions**

#### **Issue: "No Unity environment detected"**
```
Solution:
1. Make sure Unity is in Play mode
2. Check BehaviorParameters → Behavior Type is "Default"
3. Restart training command and press Play again
```

#### **Issue: "Dimension mismatch error"**
```
Solution:
1. Verify Vector Observation Space Size = 25
2. Verify Continuous Actions = 4
3. Delete old models in results/ folder
4. Restart training
```

#### **Issue: Reward stays very negative**
```
Possible causes:
├── Boundary too small (increase boundarySize)
├── Goal too far (decrease minGoalDistance)
├── Asteroids too dense (decrease asteroid count)
└── Episode too short (increase maxEpisodeTime)

Try easier settings first, then increase difficulty
```

#### **Issue: Training is very slow**
```
Solutions:
├── Reduce maxEpisodeTime (30 → 20 seconds)
├── Close other applications
├── Lower asteroid count in AsteroidSpawner
└── Reduce Time Scale in Unity (Edit → Project Settings → Time)
```

---

### **10. Training Configuration Variants**

#### **Fast Training (Easier, quicker results):**
```yaml
# CubeSat.yaml adjustments:
max_steps: 1500000        # Fewer steps needed
time_horizon: 64          # Shorter horizon
```

**In CubeSat Agent:**
```
Max Episode Time: 20      # Faster episodes
Min Goal Distance: 15     # Closer goals
Asteroid Count: 15        # Fewer obstacles (in AsteroidSpawner)
```

#### **Production Training (Harder, better policy):**
```yaml
# CubeSat.yaml (current settings):
max_steps: 3000000        # More steps for complex task
time_horizon: 128         # Longer planning horizon
```

**In CubeSat Agent:**
```
Max Episode Time: 30      # Longer episodes
Min Goal Distance: 25     # Farther goals
Asteroid Count: 30        # More obstacles
Add Initial Tumble: ☑     # Start with rotation
```

---

### **11. Success Criteria**

**Consider training successful when:**
- [ ] **Cumulative Reward** averaging **+50 or higher**
- [ ] **Success Rate** (goal reaching) above **60%**
- [ ] **Episode Length** averaging **200-400 steps**
- [ ] Agent shows **smooth rotation + thrust** coordination
- [ ] Agent demonstrates **flip-and-burn** deceleration
- [ ] Training has run for at least **1-2M steps**

**At this point:**
- Export the trained model
- Set Behavior Type back to "Inference Only"
- Assign the trained model to the Model field
- Test in different scenarios

---

## 🚀 Final Pre-Training Steps (DO THIS NOW!)

### **STEP 1: Change Behavior Type**
```
1. Select CubeSat in Hierarchy
2. Inspector → Behavior Parameters
3. Behavior Type: Default ◄── CHANGE THIS NOW!
4. Save Scene (Ctrl+S)
```

### **STEP 2: Verify Settings**
```
- [ ] Behavior Type = Default
- [ ] Space Size = 25
- [ ] Continuous Actions = 4
- [ ] Goal assigned
- [ ] Asteroids assigned
```

### **STEP 3: Test One Last Time**
```
1. Press Play
2. Check randomization works
3. Check telemetry displays correctly
4. Stop Play
```

### **STEP 4: Start Training**
```bash
# Terminal:
mlagents-learn Assets/ML-Agents/CubeSat.yaml --run-id=CubeSat_Realistic_v1

# Unity: Press Play when prompted
```

### **STEP 5: Open TensorBoard**
```bash
# New terminal:
tensorboard --logdir results

# Browser: http://localhost:6006
```

---

## 📊 Quick Reference Card

```
╔══════════════════════════════════════════════════════════╗
║            CUBESAT TRAINING - QUICK REFERENCE            ║
╠══════════════════════════════════════════════════════════╣
║                                                          ║
║  CRITICAL SETTINGS:                                      ║
║  ├── Behavior Type: Default (not Heuristic Only!)       ║
║  ├── Space Size: 25                                      ║
║  └── Continuous Actions: 4                               ║
║                                                          ║
║  TRAINING COMMAND:                                       ║
║  mlagents-learn Assets/ML-Agents/CubeSat.yaml \          ║
║    --run-id=CubeSat_Realistic_v1                         ║
║                                                          ║
║  EXPECTED DURATION:                                      ║
║  ├── Fast results: 30-60 minutes (1M steps)              ║
║  ├── Good results: 1-2 hours (2M steps)                  ║
║  └── Best results: 3-4 hours (3M steps)                  ║
║                                                          ║
║  SUCCESS METRICS:                                        ║
║  ├── Reward: +50 to +200                                 ║
║  ├── Success Rate: 60%+                                  ║
║  └── Episode Length: 200-400 steps                       ║
║                                                          ║
╚══════════════════════════════════════════════════════════╝
```

---

## ✅ You're Ready When:

- ✅ Behavior Type is **Default**
- ✅ Vector Observation Space Size is **25**
- ✅ Continuous Actions is **4**
- ✅ CubeSat.yaml exists and is configured
- ✅ Environment randomization works
- ✅ Telemetry displays correctly
- ✅ Scene is saved

**NOW GO TRAIN!** 🚀🤖✨

Good luck! Training a realistic spacecraft controller is challenging but rewarding!
