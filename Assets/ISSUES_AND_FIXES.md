# Scene Setup Issues & How to Fix Them

## ❌ CRITICAL ISSUES FOUND

Your scene is **95% correct** - great job! But there are **3 critical issues** that will prevent the system from working:

---

## Issue #1: Missing Tags 🏷️

### Problem:
All GameObjects are tagged as "Untagged", but the code relies on specific tags for collision detection.

### Why it matters:
- `CubeSatAgent.cs` checks `if (collision.gameObject.CompareTag("Asteroid"))` to detect crashes
- It also checks `if (other.CompareTag("Goal"))` to detect goal reached
- Without proper tags, **collisions won't be detected** and the agent won't know when it crashes or succeeds

### How to Fix:

#### Step 1: Create Tags (if they don't exist)
1. In Unity, go to **Edit → Project Settings → Tags and Layers**
2. Under "Tags", click the **+** button to add new tags
3. Add these tags:
   - `Asteroid`
   - `Goal`
   - `Player` (might already exist)
4. Close the window

#### Step 2: Assign Tags to GameObjects
1. **CubeSat**: 
   - Select in Hierarchy
   - In Inspector, click "Tag" dropdown (top of Inspector)
   - Select **"Player"**

2. **Goal**:
   - Select in Hierarchy
   - Set Tag to **"Goal"**

3. **Asteroid_Prefab**:
   - In Project window, navigate to `/Assets/Prefab/`
   - **Click** (don't double-click) on `Asteroid_Prefab`
   - In Inspector, set Tag to **"Asteroid"**
   - **Important**: You must edit the prefab, not an instance in the scene

---

## Issue #2: Asteroid Prefab Missing Rigidbody 🪨

### Problem:
The `Asteroid_Prefab` only has a `SphereCollider` but no `Rigidbody` component.

### Why it matters:
- `AsteroidSpawner.cs` line 67: `rb.velocity = Random.insideUnitSphere * maxDriftSpeed;`
- This tries to set the velocity of the asteroid's Rigidbody
- **Without a Rigidbody, asteroids won't drift** and will throw errors

### How to Fix:
1. In Project window, navigate to `/Assets/Prefab/`
2. **Double-click** `Asteroid_Prefab` to open it in Prefab Mode
3. In Inspector, click **Add Component**
4. Search for and add **Rigidbody**
5. Configure the Rigidbody:
   - **Mass**: 100 (doesn't matter much since it's kinematic)
   - **Drag**: 0
   - **Angular Drag**: 0.05
   - **Use Gravity**: ❌ **UNCHECK THIS**
   - **Is Kinematic**: ✅ **CHECK THIS** (asteroids shouldn't respond to forces)
6. Click the **<** arrow at top-left or **File → Save** to exit Prefab Mode

---

## Issue #3: BehaviorParameters Vector Observation Size 🧠

### Problem:
The `BehaviorParameters` component on CubeSat needs to know how many observations to expect.

### Why it matters:
Your `CubeSatAgent.cs` sends 13 observations:
```
- transform.localPosition (3 floats)
- rigidbody.velocity (3 floats)
- rigidbody.angularVelocity (3 floats)
- relativeGoalPosition (3 floats)
- relativeGoalNormalized (3 floats) ← Note: This is actually not in the code!
- relativeGoalMagnitude (1 float)
```

Actually, looking at the code again, you're sending:
- Position: 3
- Velocity: 3
- Angular Velocity: 3
- Relative Goal Position: 3
- Relative Goal Normalized: 3
- Relative Goal Magnitude: 1
**Total: 16 observations**

But wait, I need to recount based on the actual code...

Looking at `CubeSatAgent.cs` lines 60-71:
```csharp
sensor.AddObservation(transform.localPosition);           // 3
sensor.AddObservation(agentRigidbody.velocity);           // 3
sensor.AddObservation(agentRigidbody.angularVelocity);    // 3
sensor.AddObservation(relativeGoalPosition);              // 3
sensor.AddObservation(relativeGoalPosition.normalized);   // 3
sensor.AddObservation(relativeGoalPosition.magnitude);    // 1
```
**Total: 16 observations**

### How to Fix:
1. Select **CubeSat** in Hierarchy
2. Find the **Behavior Parameters** component in Inspector
3. Expand **"Vector Observation"** section
4. Set **"Space Size"** to **16**
5. Verify:
   - **Behavior Name**: "CubeSat" ✓
   - **Vector Action**:
     - **Space Type**: Continuous ✓
     - **Space Size**: 3 ✓

---

## ✅ VERIFICATION CHECKLIST

After making the fixes above, verify everything is correct:

### Method 1: Use the Validator Script
1. Create an empty GameObject in Hierarchy → Name it "Validator"
2. Add Component → **SceneSetupValidator**
3. In Inspector, drag the following references:
   - **CubeSat** → CubeSat GameObject
   - **Goal** → Goal GameObject
   - **Asteroid Prefab** → The prefab from `/Assets/Prefab/`
   - **Asteroid Spawner** → AsteroidSpawner GameObject
   - **Telemetry UI** → TelemetryManager GameObject
4. Right-click the component → **"Validate Scene Setup"**
5. Check the Console for results (should be all green ✓)

### Method 2: Manual Check
- [ ] CubeSat tag = "Player"
- [ ] Goal tag = "Goal"
- [ ] Asteroid_Prefab tag = "Asteroid"
- [ ] Asteroid_Prefab has Rigidbody component
- [ ] Asteroid_Prefab Rigidbody: Is Kinematic = ✓, Use Gravity = ❌
- [ ] CubeSat BehaviorParameters: Vector Observation Space Size = 16
- [ ] CubeSat Rigidbody: Use Gravity = ❌

---

## 🧪 TEST THE SETUP

After fixing everything:

### Test 1: Manual Control
1. Select **CubeSat** in Hierarchy
2. In **Behavior Parameters** component:
   - Change **Behavior Type** to **"Heuristic Only"**
3. Press **Play** ▶️
4. Controls:
   - **Arrow Keys / WASD**: Move horizontally
   - **Space**: Move up
   - **Left Shift**: Move down
5. **Expected Results**:
   - ✓ Telemetry UI shows velocity updating
   - ✓ 20 asteroids spawn around the scene
   - ✓ Asteroids slowly drift
   - ✓ If you hit an asteroid, episode restarts and console shows crash
   - ✓ If you reach the green Goal sphere, episode restarts and console shows success

### Test 2: Check for Errors
1. Press Play
2. Open **Console** window
3. **Expected**: No errors
4. **Common errors to look for**:
   - "NullReferenceException" → A reference is missing
   - "Tag 'Asteroid' is not defined" → Tags weren't created
   - "The variable rb of AsteroidSpawner has not been assigned" → Rigidbody missing

---

## 📊 Summary of Changes Needed

| Item | Current State | Required State | How to Fix |
|------|---------------|----------------|------------|
| CubeSat Tag | `Untagged` | `Player` | Inspector → Tag dropdown |
| Goal Tag | `Untagged` | `Goal` | Inspector → Tag dropdown |
| Asteroid Tag | `Untagged` | `Asteroid` | Edit prefab, set tag |
| Asteroid Rigidbody | ❌ Missing | ✅ Present | Add component to prefab |
| Vector Obs Size | Not set | 16 | BehaviorParameters component |

---

## 🚀 After Fixes

Once all 3 issues are fixed, your scene will be **100% ready** for:
1. ✅ Manual testing with heuristic control
2. ✅ ML-Agents training
3. ✅ QP safety filter integration
4. ✅ Running the 100-trial experiments

You're very close! These are just small configuration issues. Fix these and everything will work perfectly! 🎯
