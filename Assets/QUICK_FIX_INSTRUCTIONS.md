# ⚠️ QUICK FIX - Update BehaviorParameters NOW!

## 🚨 **YOU MUST DO THIS BEFORE PRESSING PLAY:**

### **Step 1: Select CubeSat**
1. In **Hierarchy**, click **CubeSat**

### **Step 2: Update BehaviorParameters**
In the **Inspector**, find **Behavior Parameters** component:

1. **Expand** "Behavior Parameters" section
2. **Expand** "Model" → **Brain Parameters**
3. **Change these values:**

```
Vector Observation
├── Space Size: 25          ← Change from 16 to 25
└── Stacked Vectors: 1      ← Leave as is

Actions
├── Continuous Actions: 4   ← Change from 3 to 4
└── Discrete Branches: 0    ← Leave as is
```

### **Step 3: Save**
- Press **Ctrl+S** (Cmd+S on Mac) to save the scene

---

## ✅ **Expected Result:**

**Before (WRONG - causes errors):**
```
Vector Observation Space Size: 16  ❌
Continuous Actions: 3              ❌
```

**After (CORRECT):**
```
Vector Observation Space Size: 25  ✅
Continuous Actions: 4              ✅
```

---

## 🎮 **New Controls After Fix:**

| Key | Action |
|-----|--------|
| W | Thrust forward |
| S | Thrust backward |
| A | Yaw left (turn) |
| D | Yaw right (turn) |
| Q | Pitch down |
| E | Pitch up |
| Z | Roll left |
| X | Roll right |

---

## 🐛 **Why This Error Happened:**

The code was updated to use **4 actions** (thrust + 3 rotation axes), but BehaviorParameters still has **3 actions** configured.

When the code tries to write to `actionsOut[3]` (the 4th action), it's out of bounds because the array only has 3 elements.

---

## ✅ **After Fixing, You Should See:**

- ✅ No more IndexOutOfRangeException
- ✅ Angular velocity shows in telemetry
- ✅ CubeSat rotates when you press A/D/Q/E
- ✅ Camera locked to CubeSat (no lag)

---

## 📸 **Visual Guide:**

```
Inspector → CubeSat
├── Transform
├── Mesh Filter
├── Mesh Renderer
├── Box Collider
├── Rigidbody
├── Behavior Parameters  ← FIND THIS
│   ├── Behavior Name: CubeSat
│   ├── Vector Observation
│   │   ├── Space Size: 25      ← CHANGE THIS
│   │   └── Stacked Vectors: 1
│   ├── Actions
│   │   ├── Continuous Actions: 4  ← CHANGE THIS
│   │   └── Discrete Branches: 0
│   └── Model: None
├── CubeSat Agent
└── Decision Requester
```

---

## 🚀 **DO THIS NOW:**

1. ⬜ Select CubeSat in Hierarchy
2. ⬜ Find Behavior Parameters in Inspector
3. ⬜ Set Vector Observation Space Size to **25**
4. ⬜ Set Continuous Actions to **4**
5. ⬜ Save scene (Ctrl+S)
6. ⬜ Press Play and test!

---

**After fixing, delete this file - you won't need it anymore!** ✅
