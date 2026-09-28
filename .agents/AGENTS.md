# Unity Development Guidelines & Knowledge Base

This project is configured with **Unity CLI** and live **Unity Editor Pipeline** connectivity.

## Unity CLI & Editor Integration

When working with this Unity project:
1. **Always Check Editor Status First**:
   Run `unity status` to check if the Unity Editor is running and in `ready` state.
2. **Interact via Editor Commands Instead of Direct File Mutation**:
   - **DO NOT** edit `.unity` (scene) or `.prefab` YAML files directly by hand unless absolutely necessary. Raw editing frequently corrupts YAML identifiers or meta GUIDs.
   - Use `unity cmd <command>` to interact with the live Editor. For example:
     - `unity cmd get_scene_hierarchy` — Inspect GameObjects and components in the active scene.
     - `unity cmd list_open_scenes` — List currently open scenes.
     - `unity cmd create_gameobject '{"name": "MyObject"}'` — Create GameObjects safely.
     - `unity cmd set_transform '{"target": "/MyObject", "position": [0, 1, 0]}'` — Adjust transforms.
     - `unity cmd add_component '{"target": "/MyObject", "component": "BoxCollider"}'` — Add components.
     - `unity cmd recompile` — Trigger domain reload and compilation after modifying C# scripts.
     - `unity cmd get_console_logs` — Retrieve console errors and warnings.
     - `unity cmd editor_play` / `editor_stop` — Control Play Mode.
3. **Handling Script Errors (Safe Mode Prevention)**:
   - When introducing or modifying C# scripts in `Assets/Scripts/`, verify with `unity cmd recompile` and `unity cmd get_console_logs`.
   - If compile errors occur, the Editor may drop into Safe Mode. Fix script syntax errors immediately to restore pipeline communication.
4. **Skill Reference**:
   The full reference manual for the Unity CLI is available under `.agents/skills/unity-cli/`.
