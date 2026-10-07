---
name: project-context
description: >-
  Master reference for the Bullying Education Game project. Use this skill when
  starting any new conversation about this project, when needing to understand
  the overall architecture, current development progress, or project scope.
  Activate this before any other project-specific skill.
---

# The Unseen Side — Bullying Education Game: Project Context

## Active Design Update — 7 October 2026

Latest presentation/routing revision: read `document/Presentation_SceneSplit_Revision.md` first. Level01 now hosts ONLY Chapter 1; Level02–Level05 host the corresponding chapter, with async scene transitions and menu Continue routing. `chapters` remains a full catalog for restoring prerequisite/quest records, not an in-scene campaign. UI uses a compact right quest panel with a NavMesh direction arrow, no Chapter/POV header or duplicated bottom instructions; gameplay HUD hides during dialogue/cinematics/QTE/quiz/result. Question cards concern story events instead of QTE failure. Each level has a local Cinemachine/Signal Timeline for the opening; Chapter 1 introduces the cast individually. Deoccluder is a deferred manual learning step; DialogueFocus target is ready but the extension has not been added automatically. Read the latest test results, not earlier counts.

Latest revision: read `document/Chapter01_02_Narrative_Revision.md` before the older five-chapter document. Chapter 1 uses Aji (new Protagonist POV) with exactly three dialogue responses and mutually exclusive objective routes; choices commit only after normal completion and persist into Chapter 2 opening. Chapter 2 now uses Denis (Victim): untimed arithmetic with bullying after either answer, coerced food loss after either QTE effort result, money/extortion threats, Ari witness support and two reflections. Names Billy and Pak Bambang replace Raka and Bu Nadia. The user explicitly chose an indoor classroom exception; remaining campus/story areas are predominantly outdoors. Expanded campus is 64 x 56 m. There are 26 quest definitions including three exclusive routes, 24 playable per campaign. Chapters 3–5 remain earlier playable prototypes, with renamed characters, awaiting deeper rewriting. Do not run historical tests assuming 22 quests or old actor paths; use `Chapter12Smoke` and the latest documented results. Final character models are deferred to user-supplied assets. Use only the already-open Editor.

Read `document/Five_Outdoor_Chapters_Implementation.md` for the current campaign, authoring workflow and verification results. The user now explicitly authorizes Codex to configure Unity directly, handle code and continue the roadmap autonomously. Use the existing Editor only: the machine has limited RAM. Never open a second Editor against this project. Five chapters must all remain outdoors, with multiple story-supporting quests per chapter. The original Step 16 roadmap below is historical. The saved Level01 contains a functional prototype campaign with 22 ordered quests, 10 reflections, three playable character roles, opening Timeline signals, escort, notebook QTE/random rehide, checkpoints, menu and audio. Names Arga/Dimas/Raka/Nara/Bima/Fajar/Bu Nadia are prototype names and remain editable. Final character art, produced After Effects media and physical Android validation are separate release work, not automatically completed by configuring their runtime hooks.

## Project Identity

- **Type**: 3D Educational Bullying Game
- **Engine**: Unity 6.6.3f1
- **Primary Platform**: Windows Desktop / PC
- **Secondary Platform**: Android (later)
- **Purpose**: Tugas akhir & lomba nasional
- **Core Concept**: Multi-POV narrative (Victim, Bully, Witness)

## Multi-POV Narrative Design

### Victim POV
Covers verbal bullying, physical bullying, and cyberbullying. Shows consequences
through cinematic scenes, interactive gameplay, dialogue, and educational feedback.

### Bully POV
Background story includes family environment, social environment, peer pressure,
previous experiences, learned behavior, social status, conflict at home.
**Principle**: Background explains but does NOT justify bullying.

### Witness / Bystander POV
Explores fear, uncertainty, peer pressure, fear of becoming a target, not knowing
what to do, group influence, deciding whether to seek help.

## Architecture Overview

```text
00_Bootstrap (persistent systems)
├── GameStateManager (singleton, DontDestroyOnLoad)
└── SceneLoader (async scene loading)

Core Systems
├── GameState enum (Boot, Loading, MainMenu, Playing, Dialogue, Quest, Cinematic, QTE, Quiz, Result, Paused, GameOver)
├── Input System (GameInputActions — generated from GameInput.inputactions)
└── Camera System (Cinemachine — CameraDirector pattern planned)

Player Stack
├── CharacterController (NOT Rigidbody)
├── PlayerMovement (speed, sprint, gravity)
└── PlayerInputHandler (bridges GameInputActions → PlayerMovement)
```

## Installed Packages

- Cinemachine
- Addressables
- Input System
- Animation Rigging
- AI Navigation

## Folder Structure Convention

```text
Assets/_Game/
├── Art/ (Characters, Environment, Props, UI, Cutscenes)
├── Audio/ (BGM, SFX, Voice, Mixer)
├── Data/ (Characters, Dialogues, Items, Quests, Levels, Questions, Events)
├── Prefabs/ (Player, NPC, Items, Interactables, Cameras, UI)
├── Scenes/ (00_Bootstrap, 01_MainMenu, Levels, Test)
├── Scripts/ (Core, Player, NPC, Interaction, Quest, Dialogue, Inventory, Events, Camera, Animation, UI, Audio, Save, Editor)
├── Settings/
└── Resources/
```

## Namespace Convention

- `BullyingGame.Core` — for core systems (GameState, GameStateManager, SceneLoader)
- `BullyingGame.Player` — for player scripts (PlayerMovement, PlayerInputHandler)
- Future: `BullyingGame.NPC`, `BullyingGame.Quest`, `BullyingGame.Dialogue`, etc.

## Development Roadmap

```text
STEP 1  Input Actions                         ✓
STEP 2  Bootstrap Scene                       ✓
STEP 3  GameState System                      ✓
STEP 4  Bootstrap → Main Menu Loading         ✓
STEP 5  Main Menu UI Foundation               ✓ / paused
STEP 6  Level_01 + Player Prototype           ✓
STEP 7  PlayerInputHandler + WASD             ✓
STEP 8  Third-Person Camera + Cinemachine     ✓
STEP 9  Camera-Relative Movement              ✓
STEP 10 Interaction System                    ✓
STEP 11 NPC Prototype                         ✓
STEP 12 Dialogue System                       ✓
STEP 13 Quest System                          ✓
STEP 14 Quest Item System                     ✓
STEP 15 Quest State Management + HUD          ✓
STEP 16 Real-Time Cinematic Event Director    ← CURRENT
STEP 17 Bully NPC Group System
STEP 18 Bullying Encounter
STEP 19 QTE System
STEP 20 QTE Success / Failure
STEP 21 Random Item Rehide System
STEP 22 Cinematic Camera Sequence
STEP 23 Animation / IK
STEP 24 Multi-POV Narrative System
STEP 25 After Effects Cutscene Integration
STEP 26 Save System
STEP 27 Educational Feedback / Quiz
STEP 28 Level Progression
STEP 29 Audio System
STEP 30 HUD / Gameplay UI
STEP 31 Addressables Integration
STEP 32 Polish
STEP 33 Desktop Optimization
STEP 34 Android Compatibility Pass
```

## Vertical Slice Goal

Level 01 must prove the complete loop before scaling:

```text
2D Intro → 3D Gameplay → Player Movement → Camera → NPC Interaction →
Dialogue → Main Quest → Quest Item → Bullying Cinematic → Bully Group →
QTE → Success/Failure → Item Secured/Rehidden → Quest Continues
```

## Key Technical Files

| File | Path |
|------|------|
| GameState.cs | Assets/_Game/Scripts/Core/GameState.cs |
| GameStateManager.cs | Assets/_Game/Scripts/Core/GameStateManager.cs |
| PlayerMovement.cs | Assets/_Game/Scripts/Player/PlayerMovement.cs |
| GameInput.inputactions | Assets/_Game/Settings/GameInput.inputactions |
| GameInput.cs | Assets/_Game/Settings/GameInput.cs (GENERATED — DO NOT EDIT) |
