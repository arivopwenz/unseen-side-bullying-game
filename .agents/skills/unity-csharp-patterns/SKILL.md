---
name: unity-csharp-patterns
description: >-
  Reference for C# coding patterns, conventions, and architecture decisions used
  in this Unity project. Use this skill when writing new C# scripts, reviewing
  code, or ensuring consistency with established patterns. Covers namespaces,
  component architecture, singleton pattern, event system, and Input System usage.
---

# Unity C# Patterns — Bullying Education Game

## Namespace Convention

All project scripts follow this namespace hierarchy:

```csharp
namespace BullyingGame.Core      { }  // Core systems
namespace BullyingGame.Player    { }  // Player scripts
namespace BullyingGame.NPC       { }  // NPC scripts (future)
namespace BullyingGame.Quest     { }  // Quest system (future)
namespace BullyingGame.Dialogue  { }  // Dialogue system (future)
namespace BullyingGame.Inventory { }  // Inventory system (future)
namespace BullyingGame.Events    { }  // Event systems (future)
namespace BullyingGame.Camera    { }  // Camera scripts (future)
namespace BullyingGame.Animation { }  // Animation scripts (future)
namespace BullyingGame.UI        { }  // UI scripts (future)
namespace BullyingGame.Audio     { }  // Audio scripts (future)
namespace BullyingGame.Save      { }  // Save system (future)
```

## Singleton Pattern (Persistent Managers)

For global managers that persist across scenes:

```csharp
using UnityEngine;

namespace BullyingGame.Core
{
    public class ExampleManager : MonoBehaviour
    {
        public static ExampleManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}
```

**Usage**: GameStateManager, SceneLoader, AudioManager (future), etc.
**Location**: 00_Bootstrap scene under `_Systems` hierarchy.

## Event-Driven State Changes

GameStateManager uses `System.Action` events:

```csharp
public event Action<GameState, GameState> OnStateChanged;

public void SetState(GameState newState)
{
    if (newState == CurrentState) return;
    GameState previousState = CurrentState;
    CurrentState = newState;
    OnStateChanged?.Invoke(previousState, newState);
}
```

Subscribe in other scripts:
```csharp
GameStateManager.Instance.OnStateChanged += OnGameStateChanged;

private void OnGameStateChanged(GameState previous, GameState current)
{
    // React to state change
}
```

## Component Architecture (Player)

Player uses **separated responsibility** pattern:

```text
Player GameObject
├── CharacterController (Unity built-in)
├── PlayerMovement      (handles physics/movement calculation)
└── PlayerInputHandler  (reads input, feeds to PlayerMovement)
```

Key rules:
- PlayerMovement exposes `SetMoveInput(Vector2)` and `SetSprintInput(bool)`
- PlayerInputHandler reads input and calls those methods
- Movement is currently player-orientation-based (will become camera-relative at Step 9)
- Uses CharacterController, NOT Rigidbody

## Input System Usage

### GameInputActions (Generated Class)

```csharp
private GameInputActions inputActions;

private void Awake()
{
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
    Vector2 moveInput = inputActions.Movement.Move.ReadValue<Vector2>();
    bool sprintInput = inputActions.Movement.Sprint.IsPressed();
}

private void OnDestroy()
{
    inputActions.Dispose();
}
```

### Input Maps

```text
Movement (gameplay)
├── Move     — Vector2 (WASD)
├── Look     — Vector2 (Mouse)
├── Interact — Button
├── Sprint   — Button (Left Shift)
└── Pause    — Button (Escape)

QTE (future)
└── Tap      — Button
```

### Critical Rules
- **DO NOT** manually edit generated files: `GameInput.cs`, `Player.cs`, `QTE.cs`, `UI.cs` in `Assets/_Game/Settings/`
- `GameInputActions` is the generated C# class name
- `DefaultInputActions` is separate — used for UI/EventSystem only
- Do not delete DefaultInputActions

## SerializeField Pattern

Use `[SerializeField] private` instead of `public` for inspector-exposed fields:

```csharp
[Header("Movement")]
[SerializeField] private float moveSpeed = 4f;
[SerializeField] private float sprintSpeed = 6f;

[Header("Gravity")]
[SerializeField] private float gravity = -20f;
```

## RequireComponent Pattern

Use `[RequireComponent]` to enforce dependencies:

```csharp
[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour { }

[RequireComponent(typeof(PlayerMovement))]
public class PlayerInputHandler : MonoBehaviour { }
```

## Script File Organization

Every new script:
1. Place in appropriate subfolder: `Assets/_Game/Scripts/{Category}/`
2. Use the correct namespace: `BullyingGame.{Category}`
3. Add `[Header("...")]` groups for inspector organization
4. Use `[SerializeField] private` for inspector fields
5. Use `[RequireComponent]` when a component dependency exists
