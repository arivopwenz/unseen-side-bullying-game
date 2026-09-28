---
name: development-rules
description: >-
  Mandatory development rules and constraints for the Bullying Education Game.
  Use this skill to verify compliance before making any code changes or
  architectural decisions. These rules must never be violated.
---

# Development Rules — Bullying Education Game

## Architecture Rules

1. **Keep systems modular** — each system in its own namespace and folder
2. **Do NOT put all gameplay logic in a single script** — separate concerns
3. **Do NOT manipulate Main Camera directly** — use Cinemachine virtual cameras
4. **Use GameState for major global states** — no ad-hoc state tracking
5. **Use event/director systems for complex sequences** — bullying events, cinematics
6. **Keep QTE independent from specific input devices** — abstract the input layer
7. **Keep quest state separate from visual GameObjects** — data vs. presentation
8. **Keep item state separate from item visibility** — state machine drives visuals

## Input System Rules

9. **Do NOT manually edit generated Input System scripts** — `GameInput.cs`, `Player.cs`, `QTE.cs`, `UI.cs` in `Assets/_Game/Settings/`
10. **GameInputActions is the generated class** — not `Player` or `DefaultInputActions`
11. **DefaultInputActions is for UI only** — do not delete it, do not use it for gameplay
12. **Do NOT hard-code input to keyboard/mouse** — use Input System actions

## Data Rules

13. **Use ScriptableObjects for reusable data** — when relevant systems are built
14. **Use Addressables when asset loading benefits** — not prematurely

## Development Process Rules

15. **Build one working vertical slice before multiplying levels** — Level 01 first
16. **Graybox gameplay before final art** — prototyping over polish
17. **Desktop is the primary target** — do not sacrifice PC-first design
18. **Android is secondary** — adaptation later, not co-development now
19. **Narrative must support victim, bully, and witness perspectives**
20. **Bully background context explains but does NOT justify bullying**
21. **Bullying encounter occurs ONLY ONCE per applicable quest item/event**
22. **Bullying event is real-time 3D** — not pre-rendered video
23. **Do NOT skip to QTE before interaction/quest/event foundation exists**

## Code Style Rules

24. **Use `[SerializeField] private`** — not `public` for inspector fields
25. **Use namespaces** — `BullyingGame.{Category}`
26. **Use `[RequireComponent]`** — when component dependencies exist
27. **Use `[Header("...")]`** — for inspector field grouping
28. **Place scripts in the correct subfolder** — `Assets/_Game/Scripts/{Category}/`

## Unity Editor Rules

29. **Do NOT edit .unity or .prefab YAML files directly** — use Unity Editor or CLI
30. **Always verify compilation after script changes** — `unity cmd recompile` + `unity cmd get_console_logs`
31. **Bootstrap scene (00_Bootstrap) must be Build Index 0** — it initializes all persistent systems

## Step Progression Rules

32. **Confirm current step works before proceeding** — test then advance
33. **Follow the roadmap order** — dependencies exist between steps
34. **When resuming: verify current step status first** — don't assume completion
