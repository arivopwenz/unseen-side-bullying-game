---
name: gameplay-systems
description: >-
  Reference for gameplay systems architecture in the Bullying Education Game.
  Use this skill when implementing quest system, item system, bullying events,
  QTE system, dialogue system, NPC system, or any gameplay mechanic. Covers
  state machines, event flows, and system interactions.
---

# Gameplay Systems Architecture — Bullying Education Game

## Main Gameplay Flow

```text
Game Launch → 00_Bootstrap → Core Systems → Main Menu → Level →
2D After Effects Intro → 3D Gameplay → NPC Interaction → Main Quest →
Quest Item Obtained → Real-Time 3D Bullying Cinematic → Bullies Approach →
Confrontation/Mocking → QTE →
  ├── Success → Bullies leave → Item safe → Quest continues
  └── Failure → Item stolen → Item hidden randomly → Quest continues
```

## Game State Machine

States defined in `GameState.cs`:

```text
Boot → Loading → MainMenu → Playing → Dialogue → Quest → Cinematic →
QTE → Quiz → Result → Paused → GameOver
```

Key transitions:
```text
Playing → Cinematic → QTE → Playing
Playing → Paused → Playing
Playing → Dialogue → Playing
```

## Item State System

```text
States: Hidden | Available | Collected | Stolen | Rehidden | Secured

Flow A (QTE Success):
  Available → Collected → QTE Success → Secured

Flow B (QTE Failure):
  Available → Collected → QTE Failure → Stolen → Rehidden → Collected Again → Secured
```

Rules:
- Item state is separate from item visibility
- Quest state is separate from visual GameObjects
- Use ScriptableObjects for item data when system is built

## Bullying Event State Machine

```text
States: Inactive | Triggered | Approaching | Confrontation | QTE |
        Success | Failure | ItemHidden | Completed

Flow:
  Inactive → Triggered (when quest item obtained) →
  Approaching (bully group walks toward player) →
  Confrontation (mocking/dialogue) →
  QTE (player challenge) →
    ├── Success → Completed
    └── Failure → ItemHidden → Completed
```

Rules:
- Bullying encounter for a relevant quest item happens ONLY ONCE
- This is a real-time 3D event, NOT a pre-rendered video
- Lock/restrict normal gameplay during the event
- Use cinematic camera during the event

## Real-Time Bullying Event Sequence

```text
Quest Item Obtained
  ↓
Check Event State (has this event fired before?)
  ↓
Trigger Bullying Encounter
  ↓
Lock/restrict normal gameplay (disable player input)
  ↓
Cinematic Camera (switch via CameraDirector)
  ↓
Bully Group Approaches (AI Navigation)
  ↓
Mocking / Confrontation (Dialogue System)
  ↓
QTE Challenge
  ↓
Result handling
```

## QTE System Architecture

```text
Input Device → Input System → QTE System
```

Design principles:
- QTE uses a timer/needle + progress bar + repeated player input
- Desktop: mouse/keyboard input
- Android (future): touch input
- Do NOT hard-code QTE logic to one device
- QTE map in GameInput.inputactions has a `Tap` action

## Camera Architecture

Cinemachine-based with director pattern:

```text
CameraDirector
├── Gameplay       (third-person follow — Step 8)
├── Dialogue       (close-up — future)
├── Quest          (reveal — future)
├── Cinematic      (bullying event — future)
└── QTE            (challenge view — future)
```

Camera roles match game states:
```text
Playing    → Gameplay camera
Dialogue   → Dialogue camera
Cinematic  → Cinematic camera
QTE        → QTE camera
```

Rules:
- Do NOT manipulate Main Camera directly from gameplay scripts
- Use Cinemachine virtual cameras and priority switching
- Step 8 implements only the basic third-person gameplay camera

## Level Intro System

```text
After Effects → 2D Intro Video → Unity VideoPlayer → Intro Complete → 3D Gameplay
```

Rules:
- Disable player input during intro
- Enable input after intro completes
- Each level can have its own 2D cutscene

## Interaction System (Future — Step 10)

Design intent:
- Raycasting or trigger-based detection
- Show interaction prompt UI
- Context-sensitive actions
- Works with NPC dialogue and item pickup

## Dialogue System (Step 12)

Design intent:
- Support narrative dialogue (multi-POV)
- NPC conversations
- Bullying confrontation dialogue
- Educational feedback text
- Use ScriptableObjects for dialogue data

### Dynamic Camera System for Dialogue & Action Events:
- **Cinemachine Dynamic Dialogue Camera**:
  - Automatically blends from player follow camera to dialogue framing when dialogue starts (`OnDialogueStarted`).
  - Framing options: Over-the-shoulder (OTS) shot, speaker close-up, rule-of-thirds framing between Player and NPC.
  - Dynamic Zoom: Zoom-in during serious/critical dialogue moments, zoom-out when multiple characters participate.
  - Smooth blend transition back to gameplay follow camera when dialogue ends (`OnDialogueEnded`).
- **Cinematic Event Action Camera (Bullying Encounter & QTE)**:
  - Dynamic angle transitions to emphasize tension (Dutch angle, low angle confrontation, dramatic zoom during QTE timers).
  - High emotional impact without feeling static or monotonous.

## Interaction & World-Space Prompt Architecture (Implemented — Step 10)

- **IInteractable Pattern**: Decoupled interface implemented by NPCs (`BaseNPC`), Quest Items (`QuestItem`), and world props.
- **Dynamic World-Space Placement**:
  - `InteractionCanvas` is in `RenderMode.WorldSpace` with `(0.004, 0.004, 0.004)` scale.
  - Automatically calculates physical bounds via `col.bounds.center + Vector3.up * (extents.y + offset)` so the prompt dynamically hovers directly above the target (tall NPCs at head height, small items on ground right above the prop).
  - Billboard rotation: `parentCanvas.transform.rotation = mainCamera.transform.rotation` keeping it readable from any camera angle.
  - Automatically hides when no interactable is detected or during dialogue.

## Dialogue & Camera Framing Architecture (Implemented — Step 12)

- **Dialogue Flow**: `DialogueData` (ScriptableObject) -> `DialogueManager` event dispatcher -> `DialogueUI` with typewriter effect, voice audio, and delayed continue button.
- **Dialogue Camera & PlayerDialogueSpawnPoint**:
  - Two-Shot 45° angle Cinemachine camera (`DialogueCamera`, Priority 0 -> 20 on dialogue start, resets to 0 on dialogue end).
  - NPCs support `PlayerDialogueSpawnPoint` child GameObject:
    - On dialogue start: Player is placed at `PlayerDialogueSpawnPoint` on the left of the camera frame, facing the NPC.
    - NPC rotates to face Player on the right of the camera frame.
    - Creates a balanced, cinematic two-shot composition from any approach angle.
    - On dialogue end: NPC smoothly rotates back to original orientation over 0.5s; player is free to walk away.
    - Scene View gizmo (cyan sphere and line) renders on NPC selection for visual stand-point adjustment.

## Quest System Architecture (Implemented — Step 13-15)

- **Data Models**: `QuestData` (ScriptableObject) with arrays of `QuestObjective` (`objectiveId`, `description`, `requiredAmount`, `currentAmount`).
- **State Machine**: `QuestState` (`Locked`, `Available`, `Active`, `Completed`, `Failed`).
- **QuestManager**: Singleton event hub dispatching `OnQuestStateChanged` and `OnObjectiveProgress`.
- **QuestGiverNPC**: Inherits from `BaseNPC`. Branches dialogue based on quest state (Intro/Giver, Active reminder, Completed appreciation).
- **QuestItem**: Implements `IInteractable`. Collects item, advances quest objective, and triggers linked bullying event.
- **QuestHUDUI**: Screen Space overlay in top-left with dark glass background, displaying active quest title and live objective counters `[V] / - (0/1)`.

## Multi-POV Narrative System (Future — Step 24)

Design intent:
- Support victim, bully, and witness perspectives
- Different gameplay experiences per POV
- Shared world, different story beats
- Bully background context explains but does NOT justify bullying

## Development Sequence

Systems should be built in this order (dependencies flow downward):

```text
Player Movement (✓) → Camera (✓) → Camera-Relative Movement (✓) →
Interaction (✓) → NPC (✓) → Dialogue (✓) → Quest (✓) → Quest Items (✓) →
Quest State + HUD (✓) → Event Director (Step 16) → Bully NPC → Bullying Encounter →
QTE → QTE Results → Item Rehide → Cinematic Camera →
Animation → Multi-POV → Cutscenes → Save → Quiz →
Level Progression → Audio → HUD → Addressables → Polish
```

Do NOT skip ahead — each system depends on the ones before it.
