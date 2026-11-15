using System;
using UnityEngine.InputSystem;

namespace BullyingGame.UI
{
    public sealed class ActiveInputDeviceTracker : IDisposable
    {
        public InputDevice Device { get; private set; }
        public event Action Changed;

        public ActiveInputDeviceTracker()
        {
            InputSystem.onActionChange += OnActionChange;
            InputSystem.onDeviceChange += OnDeviceChange;
        }

        void OnActionChange(object source, InputActionChange change)
        {
            if (change == InputActionChange.BoundControlsChanged)
            {
                Changed?.Invoke();
                return;
            }
            if (change != InputActionChange.ActionPerformed || !(source is InputAction action)) return;
            var control = action.activeControl;
            if (control == null || control.device == Device ||
                !(control.device is Keyboard || control.device is Mouse || control.device is Gamepad)) return;
            if (control.device is Mouse && control.name != "delta" && !(control is UnityEngine.InputSystem.Controls.ButtonControl)) return;
            if (control.EvaluateMagnitude() < (control.device is Mouse && control.name == "delta" ? 2f : .25f)) return;
            Device = control.device;
            Changed?.Invoke();
        }

        void OnDeviceChange(InputDevice device, InputDeviceChange change)
        {
            if (device != Device || (change != InputDeviceChange.Removed &&
                change != InputDeviceChange.Disconnected && change != InputDeviceChange.Disabled)) return;
            Device = null;
            Changed?.Invoke();
        }

        public void Dispose()
        {
            InputSystem.onActionChange -= OnActionChange;
            InputSystem.onDeviceChange -= OnDeviceChange;
        }
    }

    public static class InputPromptResolver
    {
        public static string Label(InputAction action, InputDevice device)
        {
            if (action == null) return "";
            bool gamepad = device is Gamepad;
            for (int i = 0; i < action.bindings.Count; i++)
            {
                var binding = action.bindings[i];
                if (binding.isPartOfComposite || string.IsNullOrEmpty(binding.effectivePath)) continue;
                string path = binding.isComposite && i + 1 < action.bindings.Count
                    ? action.bindings[i + 1].effectivePath : binding.effectivePath;
                if (string.IsNullOrEmpty(path)) continue;
                string layout = InputControlPath.TryGetDeviceLayout(path);
                if (string.IsNullOrEmpty(layout)) continue;
                bool matches = gamepad
                    ? InputSystem.IsFirstLayoutBasedOnSecond(device.layout, layout)
                    : layout == "Keyboard" || layout == "Mouse";
                if (!matches) continue;
                string label = action.GetBindingDisplayString(i, out _, out var controlPath);
                return gamepad ? GamepadLabel(device.layout, controlPath, label) : label;
            }

            foreach (var control in action.controls)
                if (gamepad ? control.device == device : control.device is Keyboard || control.device is Mouse)
                    return gamepad ? GamepadLabel(device.layout, control.name, control.displayName) :
                        control.shortDisplayName ?? control.displayName;
            return "";
        }

        static string GamepadLabel(string layout, string path, string fallback)
        {
            bool playStation = InputSystem.IsFirstLayoutBasedOnSecond(layout, "DualShockGamepad");
            bool xbox = layout.IndexOf("XInput", StringComparison.OrdinalIgnoreCase) >= 0 ||
                layout.IndexOf("Xbox", StringComparison.OrdinalIgnoreCase) >= 0;
            switch (path)
            {
                case "buttonSouth": return playStation ? "Silang" : xbox ? "A" : "Tombol bawah";
                case "buttonNorth": return playStation ? "Segitiga" : xbox ? "Y" : "Tombol atas";
                case "buttonWest": return playStation ? "Kotak" : xbox ? "X" : "Tombol kiri";
                case "buttonEast": return playStation ? "Lingkaran" : xbox ? "B" : "Tombol kanan";
                case "start": return playStation ? "Options" : "Start";
                case "select": return playStation ? "Share / Create" : xbox ? "View" : "Select";
                case "leftShoulder": return playStation ? "L1" : xbox ? "LB" : "Bahu kiri";
                case "rightShoulder": return playStation ? "R1" : xbox ? "RB" : "Bahu kanan";
                case "leftStick": return "Stik kiri";
                case "rightStick":
                case "rightStick/y": return "Stik kanan";
                default: return fallback;
            }
        }
    }
}
