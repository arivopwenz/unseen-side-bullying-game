---
name: step-implementation
description: >-
  Guide for implementing development steps in the Bullying Education Game roadmap.
  Use this skill when working on any specific step (Step 7 through Step 34),
  to understand what needs to be done, the correct order, dependencies,
  verification process, and how to properly advance to the next step.
---

# Step Implementation Guide — Bullying Education Game

## General Workflow for Any Step

1. **Read** the step requirements from the roadmap
2. **Verify** previous step is confirmed working
3. **Create** script files in the correct folder with correct namespace
4. **Implement** the feature following established patterns
5. **Compile** — run `unity cmd recompile`
6. **Check** — run `unity cmd get_console_logs` for errors
7. **Test** — verify in Play Mode (`unity cmd editor_play`)
8. **Confirm** — user verifies, then proceed to next step

## Step Details Reference

### Step 7 — PlayerInputHandler + WASD (CURRENT)

**Location**: `Assets/_Game/Scripts/Player/PlayerInputHandler.cs`
**Namespace**: `BullyingGame.Player`
**Dependencies**: PlayerMovement.cs, GameInputActions

Tasks:
- Create `PlayerInputHandler.cs`
- Add `[RequireComponent(typeof(PlayerMovement))]`
- Wire `GameInputActions.Movement` map
- Read `Move` (Vector2) and `Sprint` (bool) inputs
- Feed to `PlayerMovement.SetMoveInput()` and `SetSprintInput()`
- Dispose `inputActions` in `OnDestroy()`
- Test WASD movement and Shift sprint

### Step 8 — Third-Person Camera + Cinemachine

**Location**: `Assets/_Game/Scripts/Camera/` (new folder)
**Namespace**: `BullyingGame.Camera`
**Dependencies**: Cinemachine package (installed), Player

Tasks:
- Create Cinemachine FreeLook or Virtual Camera for third-person follow
- Set Player as follow/look-at target
- Configure basic orbit/distance settings
- Lock cursor during gameplay
- Read Look input from GameInputActions.Movement.Look

### Step 9 — Camera-Relative Movement

**Location**: Modify `PlayerMovement.cs`
**Dependencies**: Step 8 camera working

Tasks:
- Change movement calculation from player-orientation to camera-relative
- Movement direction based on camera forward/right (flattened to XZ plane)
- Player rotates to face movement direction

### Step 10 — Interaction System

**Location**: `Assets/_Game/Scripts/Interaction/`
**Namespace**: `BullyingGame.Interaction`

Tasks:
- Create interactable interface/base class
- Raycast or trigger-based detection from player
- Show/hide interaction prompt
- Handle Interact input action

### Step 11 — NPC Prototype

**Location**: `Assets/_Game/Scripts/NPC/`
**Namespace**: `BullyingGame.NPC`

Tasks:
- Create basic NPC MonoBehaviour
- Implement IInteractable
- NPC data via ScriptableObject (optional at this stage)

### Step 12 — Dialogue System

**Location**: `Assets/_Game/Scripts/Dialogue/`
**Namespace**: `BullyingGame.Dialogue`

Tasks:
- Dialogue data structure (ScriptableObject)
- Dialogue UI (text box, speaker name, continue button)
- Game state transition: Playing → Dialogue → Playing
- Disable player input during dialogue

### Step 13-15 — Quest System

**Location**: `Assets/_Game/Scripts/Quest/`
**Namespace**: `BullyingGame.Quest`

Tasks:
- Quest definition (ScriptableObject)
- Quest state management (separate from GameObjects)
- Quest item tracking
- Objective system with updates

### Step 16-18 — Bullying Event System

**Location**: `Assets/_Game/Scripts/Events/`
**Namespace**: `BullyingGame.Events`

Tasks:
- Event Director for cinematic sequences
- Bully NPC group behavior (AI Navigation)
- Bullying encounter trigger and flow
- State machine: Inactive → Triggered → ... → Completed

### Step 19-21 — QTE System

**Location**: `Assets/_Game/Scripts/QTE/` (new folder under Scripts)
**Namespace**: `BullyingGame.QTE`

Tasks:
- QTE core system (timer, progress bar, input detection)
- QTE UI elements
- Success/failure handling
- Random item rehide system (on failure)
- Use QTE input map, not hardcoded keys

### Step 22-23 — Cinematic & Animation

Tasks:
- Cinematic camera sequences via Cinemachine
- Character animation integration
- IK system (Animation Rigging package)

### Step 24 — Multi-POV Narrative

Tasks:
- Support switching between victim/bully/witness storylines
- Different gameplay experiences per POV
- Shared world, different narrative beats

### Step 25-34 — Polish & Integration

Tasks:
- After Effects cutscene integration (video playback)
- Save system
- Educational quiz/feedback system
- Level progression
- Audio system
- HUD/gameplay UI
- Addressables integration
- Polish, optimization, Android compatibility

## File Creation Template

When creating a new script for any step:

```csharp
using UnityEngine;

namespace BullyingGame.{Category}
{
    public class {ClassName} : MonoBehaviour
    {
        // Implementation
    }
}
```

## Verification Checklist

For every step completion:
- [ ] Script compiles without errors
- [ ] No warnings related to the new code
- [ ] Feature works in Play Mode
- [ ] Does not break existing functionality
- [ ] Follows namespace and folder conventions
- [ ] Uses established patterns (SerializeField, RequireComponent, etc.)
