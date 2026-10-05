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

### Step 7 — PlayerInputHandler + WASD (COMPLETED ✓)

**Location**: `Assets/_Game/Scripts/Player/PlayerInputHandler.cs`
**Namespace**: `BullyingGame.Player`
- PlayerInputHandler bridges GameInputActions with PlayerMovement.
- Includes EnsureInputActions guard to prevent NullReference on domain reload.
- Movement, Sprint, and cursor lock states handled.

### Step 8 — Third-Person Camera + Cinemachine (COMPLETED ✓)

**Location**: `Cameras/CinemachineCamera`
- Cinemachine Orbital Follow + Rotation Composer + Deoccluder.
- CameraTarget followed smoothly, cursor locked during gameplay.

### Step 9 — Camera-Relative Movement (COMPLETED ✓)

**Location**: `Assets/_Game/Scripts/Player/PlayerMovement.cs`
- Camera-relative direction calculation on horizontal plane.
- Character rotates to face movement direction.
- CharacterController null & enabled guarded against inactive calls.

### Step 10 — Interaction System (COMPLETED ✓)

**Location**: `Assets/_Game/Scripts/Interaction/`
- `IInteractable` interface: `GetPromptText()`, `CanInteract()`, `Interact(GameObject)`.
- `InteractionDetector`: trigger-based detection with closest distance sorting.
- `InteractionPromptUI`: World Space Canvas with automatic billboard rotation facing camera. Automatically positions prompt directly above whichever object is being targeted using `col.bounds.center + Vector3.up * (extents.y + offset)`. Auto-hides during dialogue.

### Step 11 — NPC Prototype (COMPLETED ✓)

**Location**: `Assets/_Game/Scripts/NPC/`
- `BaseNPC`: implements `IInteractable`. Supports `dialogueSpawnPoint` for exact player positioning during dialogue, automatic mutual rotation (NPC faces player, player faces NPC), and gentle rotation reset after dialogue ends. Gizmo visualization in Scene view.

### Step 12 — Dialogue System (COMPLETED ✓)

**Location**: `Assets/_Game/Scripts/Dialogue/`
- `DialogueData` & `DialogueLine`: ScriptableObject structure with `speakerName`, `text`, `voiceClip`.
- `DialogueManager`: singleton event dispatcher (`OnDialogueStarted`, `OnLineDisplayed`, `OnDialogueEnded`).
- `DialogueUI`: Screen Space Overlay with typewriter effect, voice clip playback, continue button delayed until typing finishes, advance dialogue on Enter/E or button click.
- `DialogueCamera`: Cinemachine camera with priority blending (Priority 0 -> 20 on dialogue start, back to 0 on end). Uses cinematic Two-Shot 45° angle with `PlayerDialogueSpawnPoint` for consistent framing.
- `DialogueInputHandler`: locks player input and unlocks cursor safely during dialogue.

### Step 13-15 — Quest System & HUD (COMPLETED ✓)

**Location**: `Assets/_Game/Scripts/Quest/`, `Assets/_Game/Scripts/UI/`
- `QuestData` & `QuestObjective`: ScriptableObject definition with objectives and progress tracking.
- `QuestManager`: manages quest states (Locked, Available, Active, Completed, Failed).
- `QuestGiverNPC`: inherits from `BaseNPC`. Handles branching dialogue states (Intro/Available, Active, Completed).
- `QuestItem`: implements `IInteractable`. Collects item, updates objective via `QuestManager.Instance.UpdateObjective()`, and triggers bullying event if linked.
- `QuestHUDUI`: top-left modern dark glass HUD displaying active quest title and live objective counters `[V] / - (0/1)`. Auto-wires child elements in `Awake()`.
- Data Assets: `Quest_Level01_LostNotebook.asset`, `Dialogue_GuruBK_Intro.asset`, `Dialogue_GuruBK_QuestActive.asset`, `Dialogue_GuruBK_QuestCompleted.asset`.

### Step 16-18 — Bullying Event System (CURRENT ←)

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
