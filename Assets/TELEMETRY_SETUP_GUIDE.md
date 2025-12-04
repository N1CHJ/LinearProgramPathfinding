# 📊 Telemetry UI - Setup Guide

## ✅ What Changed

Updated `TelemetryUI.cs` to display **CubeSat Position** and **Goal Position** instead of Speed and Status.

---

## 🎯 New UI Fields

### **Before:**
```
├── Velocity Text
├── Angular Velocity Text
├── Speed Text          ❌ (removed)
└── Status Text         ❌ (removed)
```

### **After:**
```
├── Velocity Text
├── Angular Velocity Text
├── CubeSat Position Text   ✅ (NEW!)
└── Goal Position Text      ✅ (NEW!)
```

---

## ⚙️ Setup Instructions

### **Step 1: Select TelemetryManager GameObject**
1. In **Hierarchy**, click **TelemetryManager**

### **Step 2: Assign References in Inspector**

In the **TelemetryUI** component, assign the following:

```
References:
├── CubeSat Rigidbody: Drag "CubeSat" GameObject
└── Goal Transform: Drag "Goal" GameObject  ◄── NEW!

UI Text Elements:
├── Velocity Text: Drag "VelocityText" from Hierarchy
├── Angular Velocity Text: Drag "AngularVelocityText" from Hierarchy
├── CubeSat Position Text: Drag your new position text  ◄── NEW!
└── Goal Position Text: Drag your new goal text          ◄── NEW!
```

---

## 🔍 Finding UI Text Elements

Your UI text elements are located here:

```
Hierarchy:
└── TelemetryCanvas
    └── TelemetryPanel
        ├── VelocityText
        ├── AngularVelocityText
        ├── [Your CubeSat Position Text]  ◄── Drag this to CubeSat Position Text
        └── [Your Goal Position Text]     ◄── Drag this to Goal Position Text
```

---

## 📋 Step-by-Step Assignment

### **Assigning CubeSat Position Text:**
1. Select **TelemetryManager** in Hierarchy
2. In Inspector, find **TelemetryUI** component
3. Locate **CubeSat Position Text** field (should be empty)
4. In Hierarchy, expand **TelemetryCanvas → TelemetryPanel**
5. **Drag** your CubeSat position text element → **CubeSat Position Text** field
6. ✅ Should now show the text component

### **Assigning Goal Position Text:**
1. Still in **TelemetryManager** Inspector
2. Locate **Goal Position Text** field (should be empty)
3. **Drag** your goal position text element → **Goal Position Text** field
4. ✅ Should now show the text component

### **Assigning Goal Transform (NEW!):**
1. Still in **TelemetryManager** Inspector
2. Locate **Goal Transform** field under **References**
3. In Hierarchy, find **Goal** GameObject
4. **Drag** the **Goal** GameObject → **Goal Transform** field
5. ✅ Should now show "Goal (Transform)"

---

## 🎨 Expected Display Format

### **CubeSat Position Text:**
```
CubeSat Pos: (5.23, -12.45, 8.91)
```

### **Goal Position Text:**
```
Goal Pos: (25.00, -15.32, 30.12)
```

### **Velocity Text (unchanged):**
```
Velocity: (3.45, -1.23, 2.67) m/s
```

### **Angular Velocity Text (unchanged):**
```
Angular Velocity: (0.12, -0.34, 0.05) rad/s
```

---

## 🧪 Testing

### **Test 1: CubeSat Position Updates**
1. Press Play ▶️
2. Look at the **CubeSat Pos** text
3. Press **W** to move
4. ✅ Position values should change as CubeSat moves

### **Test 2: Goal Position Updates**
1. Press Play ▶️
2. Look at the **Goal Pos** text
3. Note the position
4. Crash or reach goal → Episode resets
5. ✅ Goal position should change (randomized)

### **Test 3: All Telemetry Working**
1. Press Play ▶️
2. Check all four text displays:
   - ✅ Velocity updates (changes when moving)
   - ✅ Angular Velocity updates (changes when rotating)
   - ✅ CubeSat Pos updates (changes when moving)
   - ✅ Goal Pos displays (static until episode resets)

---

## 🔧 Troubleshooting

### **"CubeSat Pos shows (0.00, 0.00, 0.00)"**
- Check that **CubeSat Rigidbody** is assigned in TelemetryUI
- Make sure CubeSat is actually moving (press W)

### **"Goal Pos shows (0.00, 0.00, 0.00)"**
- Check that **Goal Transform** is assigned in TelemetryUI
- Make sure the Goal GameObject exists in the scene

### **"Text doesn't update"**
- Make sure you assigned the **TextMeshProUGUI** components, not the GameObject
- Check that the text elements are under TelemetryPanel

### **"I don't see the new fields in Inspector"**
- Make sure the script compiled successfully (no errors in Console)
- Try selecting a different GameObject, then select TelemetryManager again
- Check that TelemetryUI.cs has the new code

---

## 📊 Telemetry Manager Inspector Layout

After setup, your Inspector should look like this:

```
TelemetryUI (Script)
├─ References
│  ├── CubeSat Rigidbody: CubeSat (Rigidbody)        ✅
│  └── Goal Transform: Goal (Transform)               ✅ NEW!
│
├─ UI Text Elements
│  ├── Velocity Text: VelocityText (TextMeshProUGUI)         ✅
│  ├── Angular Velocity Text: AngularVelocityText (TMP)      ✅
│  ├── CubeSat Position Text: [Your text] (TMP)              ✅ NEW!
│  └── Goal Position Text: [Your text] (TMP)                 ✅ NEW!
│
└─ Display Settings
   ├── Show Debug Info: ☑
   └── Decimal Places: 2
```

---

## 💡 Usage Tips

### **Decimal Places:**
- Default: **2** (shows `5.23`)
- For more precision: **3** or **4** (shows `5.234` or `5.2345`)
- For cleaner display: **1** (shows `5.2`)

### **Monitoring Navigation:**
```
Watch these values during flight:
├── CubeSat Pos: Where you are
├── Goal Pos: Where you need to go
├── Velocity: How fast you're moving
└── Angular Velocity: How fast you're rotating

Calculate distance in your head:
Goal Pos (25, -15, 30) - CubeSat Pos (5, -12, 8)
= Distance ≈ √[(20)² + (-3)² + (22)²] ≈ 30 units
```

---

## 🎯 Why Position Display Is Better

### **Before (Speed + Status):**
- ❌ Speed = just magnitude of velocity (redundant)
- ❌ Status = only shows when reaching goal or crashing

### **After (Positions):**
- ✅ See exact location in 3D space
- ✅ See goal location (helps with manual control)
- ✅ Can mentally calculate distance to goal
- ✅ Better for debugging navigation issues
- ✅ More useful information

---

## 📐 Coordinate Reference

Remember the coordinate system:

```
         +Y (up)
          ▲
          │
          │
          ├─────────► +X (right)
         /
        /
       ▼ +Z (forward)

Example positions:
├── CubeSat: (5.2, -3.1, 8.7)
│   └── 5.2 to the right, 3.1 down, 8.7 forward
│
└── Goal: (25.0, -15.3, 30.1)
    └── 25.0 to the right, 15.3 down, 30.1 forward
```

---

## 🚀 Quick Checklist

Setup checklist:
- [ ] Select TelemetryManager in Hierarchy
- [ ] Assign **CubeSat Rigidbody** (CubeSat GameObject)
- [ ] Assign **Goal Transform** (Goal GameObject)
- [ ] Assign **Velocity Text** (UI text element)
- [ ] Assign **Angular Velocity Text** (UI text element)
- [ ] Assign **CubeSat Position Text** (your new UI text)
- [ ] Assign **Goal Position Text** (your new UI text)
- [ ] Press Play to test
- [ ] Check all four displays update correctly

---

## 📊 Example Output During Flight

```
Frame 0 (Start):
├── Velocity: (0.00, 0.00, 0.00) m/s
├── Angular Velocity: (0.00, 0.00, 0.00) rad/s
├── CubeSat Pos: (3.12, -5.23, 1.45)
└── Goal Pos: (28.34, -18.92, 25.67)

Frame 100 (Moving):
├── Velocity: (4.23, -1.12, 3.45) m/s
├── Angular Velocity: (0.15, -0.23, 0.08) rad/s
├── CubeSat Pos: (8.45, -6.01, 5.23)
└── Goal Pos: (28.34, -18.92, 25.67)

Frame 500 (Near Goal):
├── Velocity: (1.23, -0.45, 0.89) m/s
├── Angular Velocity: (0.02, -0.05, 0.01) rad/s
├── CubeSat Pos: (27.12, -17.89, 24.56)
└── Goal Pos: (28.34, -18.92, 25.67)
```

---

## 🎯 Summary

**New Features:**
- ✅ Real-time CubeSat position display
- ✅ Real-time Goal position display
- ✅ Goal Transform reference (updates when goal moves)
- ✅ Better situational awareness

**Setup Required:**
- Assign Goal Transform in TelemetryManager
- Assign your new UI text elements
- Test in Play mode

**Ready to fly with full position telemetry!** 🛰️📊✨
