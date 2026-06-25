using UnityEngine;
using UnityEngine.InputSystem;

namespace Deucarian.TemplateGameMovementFps.Movement
{
    public sealed class FpsInputReader
    {
        private readonly InputAction _moveAction;
        private readonly InputAction _lookAction;
        private readonly InputAction _sprintAction;
        private readonly InputAction _slideAction;
        private readonly InputAction _jumpAction;
        private readonly InputAction _fireAction;
        private readonly InputAction _reloadAction;
        private readonly InputAction _nextGunAction;
        private readonly InputAction _pauseAction;
        private bool _enabled;

        public FpsInputReader()
        {
            _moveAction = new InputAction("Move", InputActionType.Value, expectedControlType: "Vector2");
            _moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");
            _moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/upArrow")
                .With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow")
                .With("Right", "<Keyboard>/rightArrow");

            _lookAction = new InputAction("Look", InputActionType.Value, "<Mouse>/delta", expectedControlType: "Vector2");

            _sprintAction = new InputAction("Sprint", InputActionType.Button, "<Keyboard>/leftShift");
            _sprintAction.AddBinding("<Keyboard>/rightShift");

            _slideAction = new InputAction("Slide", InputActionType.Button, "<Keyboard>/leftCtrl");
            _slideAction.AddBinding("<Keyboard>/c");

            _jumpAction = new InputAction("Jump", InputActionType.Button, "<Keyboard>/space");

            _fireAction = new InputAction("Fire", InputActionType.Button, "<Mouse>/leftButton");

            _reloadAction = new InputAction("Reload", InputActionType.Button, "<Keyboard>/r");

            _nextGunAction = new InputAction("NextGun", InputActionType.Button, "<Keyboard>/q");
            _nextGunAction.AddBinding("<Keyboard>/1");
            _nextGunAction.AddBinding("<Keyboard>/2");

            _pauseAction = new InputAction("Pause", InputActionType.Button, "<Keyboard>/escape");
            _pauseAction.AddBinding("<Keyboard>/p");
        }

        public Vector2 Move { get; private set; }

        public Vector2 Look { get; private set; }

        public bool SprintHeld { get; private set; }

        public bool SlidePressed { get; private set; }

        public bool SlideHeld { get; private set; }

        public bool JumpPressed { get; private set; }

        public bool JumpHeld { get; private set; }

        public bool FireHeld { get; private set; }

        public bool ReloadPressed { get; private set; }

        public bool NextGunPressed { get; private set; }

        public bool PausePressed { get; private set; }

        public void Enable()
        {
            if (_enabled)
            {
                return;
            }

            _moveAction.Enable();
            _lookAction.Enable();
            _sprintAction.Enable();
            _slideAction.Enable();
            _jumpAction.Enable();
            _fireAction.Enable();
            _reloadAction.Enable();
            _nextGunAction.Enable();
            _pauseAction.Enable();
            _enabled = true;
        }

        public void Disable()
        {
            if (!_enabled)
            {
                return;
            }

            _moveAction.Disable();
            _lookAction.Disable();
            _sprintAction.Disable();
            _slideAction.Disable();
            _jumpAction.Disable();
            _fireAction.Disable();
            _reloadAction.Disable();
            _nextGunAction.Disable();
            _pauseAction.Disable();
            ResetFrameValues();
            _enabled = false;
        }

        public void Read()
        {
            Enable();

            Vector2 move = _moveAction.ReadValue<Vector2>();
            Move = move.sqrMagnitude > 1f ? move.normalized : move;
            Look = _lookAction.ReadValue<Vector2>();
            SprintHeld = _sprintAction.IsPressed();
            SlidePressed = _slideAction.WasPressedThisFrame();
            SlideHeld = _slideAction.IsPressed();
            JumpPressed = _jumpAction.WasPressedThisFrame();
            JumpHeld = _jumpAction.IsPressed();
            FireHeld = _fireAction.IsPressed();
            ReloadPressed = _reloadAction.WasPressedThisFrame();
            NextGunPressed = _nextGunAction.WasPressedThisFrame();
            PausePressed = _pauseAction.WasPressedThisFrame();
        }

        private void ResetFrameValues()
        {
            Move = Vector2.zero;
            Look = Vector2.zero;
            SprintHeld = false;
            SlidePressed = false;
            SlideHeld = false;
            JumpPressed = false;
            JumpHeld = false;
            FireHeld = false;
            ReloadPressed = false;
            NextGunPressed = false;
            PausePressed = false;
        }
    }
}
