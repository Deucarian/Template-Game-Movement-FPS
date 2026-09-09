using UnityEngine.InputSystem;

namespace Deucarian.TemplateGameMovementFps
{
    internal readonly struct MovementFpsRunInput
    {
        internal MovementFpsRunInput(bool restart, bool first, bool second, bool third)
        {
            Restart = restart;
            First = first;
            Second = second;
            Third = third;
        }
        internal bool Restart { get; }
        internal bool First { get; }
        internal bool Second { get; }
        internal bool Third { get; }

        internal static MovementFpsRunInput ReadKeyboard()
        {
            Keyboard keyboard = Keyboard.current;
            return keyboard == null ? default : new MovementFpsRunInput(
                keyboard[Key.R].wasPressedThisFrame,
                keyboard[Key.Digit1].wasPressedThisFrame,
                keyboard[Key.Digit2].wasPressedThisFrame,
                keyboard[Key.Digit3].wasPressedThisFrame);
        }
    }
}
