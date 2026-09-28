# Bullying Education Game --- Development Handoff

**Unity:** 6.6.0f1\
**Primary platform:** Windows Desktop / PC\
**Secondary platform:** Android (optional/later)\
**Current progress:** **STEP 7 --- PlayerInputHandler + WASD Movement**\
**Next:** **STEP 8 --- Third-Person Camera + Cinemachine**

------------------------------------------------------------------------

## 1. Project Scope

This project is a 3D educational bullying game. The project supports a
narrative with three major perspectives:

1.  Victim / bullied person
2.  Bully / perpetrator
3.  Witness / bystander

The project should remain focused only on this bullying education game.

------------------------------------------------------------------------

## 2. Multi-POV Narrative

### Victim POV

The victim perspective can cover:

-   verbal bullying
-   physical bullying
-   cyberbullying

The game can show the consequences and context through cinematic scenes,
interactive gameplay, dialogue, and educational feedback.

### Bully POV

The bully can have a background story involving:

-   family environment
-   social environment
-   peer pressure
-   previous experiences
-   learned behavior
-   social status
-   conflict or pressure at home

Narrative principle: background circumstances may explain behavior but
do not justify bullying.

### Witness / Bystander POV

The witness perspective can explore why a witness does not immediately
intervene, including:

-   fear
-   uncertainty
-   peer pressure
-   fear of becoming a target
-   not knowing what to do
-   group influence
-   deciding whether to seek help

------------------------------------------------------------------------

## 3. Main Gameplay Flow

``` text
Game Launch
  ↓
00_Bootstrap
  ↓
Core Systems
  ↓
Main Menu
  ↓
Level
  ↓
2D After Effects Intro
  ↓
3D Gameplay
  ↓
NPC Interaction
  ↓
Main Quest
  ↓
Quest Item Obtained
  ↓
Real-Time 3D Bullying Cinematic
  ↓
Bullies Approach
  ↓
Confrontation / Mocking
  ↓
QTE
  ├── Success → Bullies leave → Item safe → Quest continues
  └── Failure → Item stolen → Item hidden randomly → Quest continues
```

The bullying encounter for a relevant quest item should happen only
once.

------------------------------------------------------------------------

## 4. Level Intro

Each level can begin with a 2D cutscene created in After Effects:

``` text
After Effects
  ↓
2D Intro Video
  ↓
Unity
  ↓
Intro Complete
  ↓
3D Gameplay Enabled
```

Gameplay input should be disabled during the intro and enabled after the
intro completes.

------------------------------------------------------------------------

## 5. Real-Time Bullying Event

When the player obtains a main quest item:

``` text
Quest Item Obtained
  ↓
Check Event State
  ↓
Trigger Bullying Encounter
  ↓
Lock/restrict normal gameplay
  ↓
Cinematic Camera
  ↓
Bully Group Approaches
  ↓
Mocking / Confrontation
  ↓
QTE
```

This is intended to be a real-time 3D event, not a pre-rendered video.

------------------------------------------------------------------------

## 6. QTE

Concept:

``` text
Timer / Needle
QTE Progress Bar
Repeated player input
```

Desktop can use mouse/keyboard. Android can later use touch.

Architecture:

``` text
Input Device
  ↓
Input System
  ↓
QTE System
```

Do not hard-code QTE logic directly to one device.

### Success

``` text
QTE Success
  ↓
Bullies leave
  ↓
Item remains safe
  ↓
Event completed
  ↓
Quest continues
```

### Failure

``` text
QTE Failure
  ↓
Bullies steal item
  ↓
Item becomes hidden
  ↓
Select random valid hiding point
  ↓
Quest objective updates
```

------------------------------------------------------------------------

## 7. Planned Item States

``` text
Hidden
Available
Collected
Stolen
Rehidden
Secured
```

Example:

``` text
Available → Collected → QTE → Success → Secured
```

or:

``` text
Available → Collected → QTE → Failure
→ Stolen → Rehidden → Collected Again → Secured
```

------------------------------------------------------------------------

## 8. Planned Bullying Event States

``` text
Inactive
Triggered
Approaching
Confrontation
QTE
Success
Failure
ItemHidden
Completed
```

------------------------------------------------------------------------

## 9. Camera Architecture

Cinemachine is installed.

Planned camera roles:

``` text
Gameplay
Dialogue
Quest
Item Reveal
Bullying Cinematic
QTE
Result
```

Long-term structure:

``` text
CameraDirector
  ├── Gameplay
  ├── Dialogue
  ├── Quest
  ├── Cinematic
  └── QTE
```

The immediate next camera task is only the basic third-person gameplay
camera.

------------------------------------------------------------------------

## 10. Platform Strategy

### Primary

Windows Desktop / PC.

Primary input:

-   Keyboard
-   Mouse

### Secondary

Android.

Android is a later compatibility/adaptation target. Do not sacrifice the
PC-first design prematurely.

------------------------------------------------------------------------

## 11. Input Architecture

Manual asset:

``` text
Assets/_Game/Settings/GameInput.inputactions
```

Generated class:

``` text
GameInputActions
```

The gameplay input map is intended to contain:

``` text
Movement
├── Move
├── Look
├── Interact
├── Sprint
└── Pause
```

WASD is corrected to:

``` text
W = Up
S = Down
A = Left
D = Right
```

QTE is planned as:

``` text
QTE
└── Tap
```

The Unity default UI input asset remains separate:

``` text
DefaultInputActions
  ↓
EventSystem / UI
```

while:

``` text
GameInput
  ↓
Gameplay / QTE
```

Do not delete the default UI input asset.

------------------------------------------------------------------------

## 12. Bootstrap

Startup scene:

``` text
00_Bootstrap
```

Current structure:

``` text
00_Bootstrap
├── _Systems
│   ├── GameStateManager
│   └── SceneLoader
└── Bootstrap
```

Bootstrap initializes persistent/global systems before gameplay scenes.

------------------------------------------------------------------------

## 13. Game State

File:

``` text
Assets/_Game/Scripts/Core/GameState.cs
```

States:

``` text
Boot
Loading
MainMenu
Playing
Dialogue
Quest
Cinematic
QTE
Quiz
Result
Paused
GameOver
```

Manager:

``` text
Assets/_Game/Scripts/Core/GameStateManager.cs
```

Important transitions:

``` text
Playing → Cinematic → QTE → Playing
Playing → Paused → Playing
```

------------------------------------------------------------------------

## 14. Scene Loader

File:

``` text
Assets/_Game/Scripts/Core/SceneLoader.cs
```

Uses asynchronous scene loading.

Concept:

``` text
LoadSceneAsync
  ↓
Loading State
  ↓
Scene Loaded
```

------------------------------------------------------------------------

## 15. Packages Already Set Up

Relevant packages already installed/configured:

-   Cinemachine
-   Addressables
-   Input System
-   Animation Rigging
-   AI Navigation

------------------------------------------------------------------------

## 16. Target Folder Structure

``` text
Assets/
└── _Game/
    ├── Art/
    │   ├── Characters/
    │   ├── Environment/
    │   ├── Props/
    │   ├── UI/
    │   └── Cutscenes/
    ├── Audio/
    │   ├── BGM/
    │   ├── SFX/
    │   ├── Voice/
    │   └── Mixer/
    ├── Data/
    │   ├── Characters/
    │   ├── Dialogues/
    │   ├── Items/
    │   ├── Quests/
    │   ├── Levels/
    │   ├── Questions/
    │   └── Events/
    ├── Prefabs/
    │   ├── Player/
    │   ├── NPC/
    │   ├── Items/
    │   ├── Interactables/
    │   ├── Cameras/
    │   └── UI/
    ├── Scenes/
    │   ├── 00_Bootstrap/
    │   ├── 01_MainMenu/
    │   ├── Levels/
    │   └── Test/
    ├── Scripts/
    │   ├── Core/
    │   ├── Player/
    │   ├── NPC/
    │   ├── Interaction/
    │   ├── Quest/
    │   ├── Dialogue/
    │   ├── Inventory/
    │   ├── Events/
    │   ├── Camera/
    │   ├── Animation/
    │   ├── UI/
    │   ├── Audio/
    │   ├── Save/
    │   └── Editor/
    ├── Settings/
    └── Resources/
```

------------------------------------------------------------------------

# 17. Main Menu Status

Main Menu UI foundation was created but development is intentionally
paused because gameplay is now the priority.

Target hierarchy:

``` text
01_MainMenu
├── Main Camera
├── EventSystem
│   ├── Event System
│   └── Input System UI Input Module
└── Canvas_MainMenu
    ├── Background
    ├── GameLogo
    └── MenuPanel
        ├── StartButton
        ├── ContinueButton
        ├── SettingsButton
        └── ExitButton
```

The EventSystem currently uses Unity's `DefaultInputActions` for UI.

------------------------------------------------------------------------

# 18. Level 01 Status

Scene:

``` text
Assets/_Game/Scenes/Levels/Level_01.unity
```

Current target hierarchy:

``` text
Level_01
├── _Level
│   ├── Environment
│   │   └── Ground
│   ├── SpawnPoints
│   └── EventPoints
├── Player
└── Cameras
```

The Ground is currently a primitive Plane used for grayboxing.

------------------------------------------------------------------------

# 19. Player Status

Current Player prototype:

``` text
Player
├── CharacterController
├── PlayerMovement
└── PlayerInputHandler
```

The Player uses CharacterController rather than Rigidbody movement.

------------------------------------------------------------------------

# 20. PlayerMovement

File:

``` text
Assets/_Game/Scripts/Player/PlayerMovement.cs
```

Current design:

``` csharp
[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float sprintSpeed = 6f;
    [SerializeField] private float gravity = -20f;

    private CharacterController characterController;
    private Vector2 moveInput;
    private bool isSprinting;
    private float verticalVelocity;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    public void SetMoveInput(Vector2 input)
    {
        moveInput = Vector2.ClampMagnitude(input, 1f);
    }

    public void SetSprintInput(bool sprinting)
    {
        isSprinting = sprinting;
    }

    private void Update()
    {
        Vector3 movement =
            transform.right * moveInput.x +
            transform.forward * moveInput.y;

        float speed = isSprinting ? sprintSpeed : moveSpeed;

        if (characterController.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity =
            movement * speed +
            Vector3.up * verticalVelocity;

        characterController.Move(velocity * Time.deltaTime);
    }
}
```

Current movement is based on Player orientation. This will later become
camera-relative.

------------------------------------------------------------------------

# 21. CURRENT STEP --- STEP 7

## PlayerInputHandler

File:

``` text
Assets/_Game/Scripts/Player/PlayerInputHandler.cs
```

Target code:

``` csharp
using UnityEngine;

namespace BullyingGame.Player
{
    [RequireComponent(typeof(PlayerMovement))]
    public class PlayerInputHandler : MonoBehaviour
    {
        private GameInputActions inputActions;
        private PlayerMovement playerMovement;

        private void Awake()
        {
            playerMovement = GetComponent<PlayerMovement>();
            inputActions = new GameInputActions();
        }

        private void OnEnable()
        {
            inputActions.Movement.Enable();
        }

        private void OnDisable()
        {
            inputActions.Movement.Disable();
        }

        private void Update()
        {
            Vector2 moveInput =
                inputActions.Movement.Move.ReadValue<Vector2>();

            bool sprintInput =
                inputActions.Movement.Sprint.IsPressed();

            playerMovement.SetMoveInput(moveInput);
            playerMovement.SetSprintInput(sprintInput);
        }

        private void OnDestroy()
        {
            inputActions.Dispose();
        }
    }
}
```

### Step 7 task

-   Create the script.
-   Add it to Player.
-   Confirm `GameInputActions` is recognized.
-   Test W/S/A/D.
-   Test Shift + movement for sprint.

### Current status

**STEP 7 is the current active step.**

The user has been instructed to implement and test it, but has not yet
explicitly confirmed that the test is complete.

Therefore, when continuing:

1.  First confirm Step 7.
2.  Fix any compile/input issue if present.
3.  Only then proceed to Step 8.

------------------------------------------------------------------------

# 22. PREVIOUS INPUT SYSTEM ERROR --- RESOLVED

There were two generated `Player.cs` classes.

One generated file existed at:

``` text
Assets/Player.cs
```

and another under:

``` text
Assets/_Game/Settings/Player.cs
```

They produced duplicate `Player` members such as `FindBinding` and
`IEnumerator.GetEnumerator`.

The manually created Input Actions generated class was renamed to:

``` text
GameInputActions
```

The duplicate root-level generated `Assets/Player.cs` was removed.

After correction, Unity Console returned to zero errors/warnings.

Do not manually edit generated Input System files.

------------------------------------------------------------------------

# 23. DEVELOPMENT ROADMAP

``` text
STEP 1  Input Actions                         ✓
STEP 2  Bootstrap Scene                      ✓
STEP 3  GameState System                     ✓
STEP 4  Bootstrap → Main Menu Loading       ✓
STEP 5  Main Menu UI Foundation              ✓ / paused
STEP 6  Level_01 + Player Prototype          ✓
STEP 7  PlayerInputHandler + WASD            ← CURRENT
STEP 8  Third-Person Camera + Cinemachine    ← NEXT
STEP 9  Camera-Relative Movement
STEP 10 Interaction System
STEP 11 NPC Prototype
STEP 12 Dialogue System
STEP 13 Quest System
STEP 14 Quest Item System
STEP 15 Quest State Management
STEP 16 Real-Time Cinematic Event Director
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

------------------------------------------------------------------------

# 24. VERTICAL SLICE GOAL

Before building many levels, Level 01 should eventually prove this
complete loop:

``` text
2D Intro
  ↓
3D Gameplay
  ↓
Player Movement
  ↓
Camera
  ↓
NPC Interaction
  ↓
Dialogue
  ↓
Main Quest
  ↓
Quest Item
  ↓
Real-Time Bullying Cinematic
  ↓
Bully Group
  ↓
QTE
  ↓
Success / Failure
  ↓
Item Secured / Rehidden
  ↓
Quest Continues
```

Only after this works reliably should the project scale to additional
levels and POV scenarios.

------------------------------------------------------------------------

# 25. DEVELOPMENT RULES

1.  Keep systems modular.
2.  Do not put all gameplay logic in Player.cs.
3.  Do not manipulate the Main Camera directly from every gameplay
    script.
4.  Use GameState for major global states.
5.  Use event/director systems for complex sequences.
6.  Keep QTE independent from a specific input device.
7.  Keep quest state separate from visual GameObjects.
8.  Keep item state separate from item visibility.
9.  Do not manually edit generated Input System scripts.
10. Use ScriptableObjects for reusable data when the relevant systems
    are built.
11. Use Addressables when asset lifecycle/loading benefits from them.
12. Build one working vertical slice before multiplying levels.
13. Graybox gameplay before final art.
14. Desktop is the primary target.
15. Android is secondary.
16. Narrative architecture must support victim, bully, and witness
    perspectives.
17. Bully background context explains circumstances but does not justify
    bullying.
18. The relevant bullying encounter occurs only once per applicable
    quest item/event.
19. The bullying event is a real-time 3D gameplay/cinematic event.
20. Do not jump to QTE implementation before the required
    interaction/quest/event foundation exists.

------------------------------------------------------------------------

# 26. NEXT CONTINUATION INSTRUCTION

When continuing from this document, use:

> **Continue from STEP 7. First verify PlayerInputHandler and WASD +
> Sprint. If Step 7 works, start STEP 8 --- Third-Person Camera +
> Cinemachine.**

The immediate technical chain is:

``` text
Finish Step 7
  ↓
Step 8 Camera
  ↓
Step 9 Camera-relative movement
  ↓
Step 10 Interaction
  ↓
Step 11 NPC
  ↓
Step 12 Dialogue
  ↓
Step 13 Quest
```
