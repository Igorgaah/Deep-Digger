using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DeepDigger.Gameplay.Input
{
    /// <summary>
    /// Central, engine-agnostic-ish input facade. Gameplay code depends only on this asset,
    /// never on the Input System directly, which keeps rebinding and control-scheme changes
    /// contained to a single file. Actions are declared in code so the project compiles and
    /// runs without any authored <c>.inputactions</c> asset.
    /// </summary>
    [CreateAssetMenu(fileName = "InputReader", menuName = "Deep Digger/Input Reader")]
    public sealed class InputReader : ScriptableObject
    {
        /// <summary>Latest movement vector (already clamped by the caller when needed).</summary>
        public Vector2 MoveInput { get; private set; }

        /// <summary>Screen-space pointer position, forwarded for aim calculations.</summary>
        public Vector2 PointerPosition { get; private set; }

        /// <summary><c>true</c> while the attack/mine button is held (used for continuous mining).</summary>
        public bool IsAttackHeld { get; private set; }

        public event Action DashPerformed;
        public event Action AttackPerformed;
        public event Action InteractPerformed;
        /// <summary><c>true</c> when sprint is held, <c>false</c> when released.</summary>
        public event Action<bool> SprintToggled;

        private InputActionMap _gameplayMap;
        private InputAction _move;
        private InputAction _look;
        private InputAction _dash;
        private InputAction _sprint;
        private InputAction _attack;
        private InputAction _interact;

        private void OnEnable() => BuildActions();

        private void OnDisable()
        {
            if (_gameplayMap == null) return;

            _move.performed -= OnMove;
            _move.canceled -= OnMove;
            _look.performed -= OnLook;
            _dash.performed -= OnDash;
            _sprint.performed -= OnSprintStarted;
            _sprint.canceled -= OnSprintCanceled;
            _attack.performed -= OnAttack;
            _attack.canceled -= OnAttackReleased;
            _interact.performed -= OnInteract;

            _gameplayMap.Disable();
            _gameplayMap.Dispose();
            _gameplayMap = null;
        }

        /// <summary>Enables gameplay input. Called by the owning system when it becomes active.</summary>
        public void EnableGameplayInput()
        {
            if (_gameplayMap == null) BuildActions();
            _gameplayMap.Enable();
        }

        /// <summary>Disables gameplay input (e.g. while paused or in menus).</summary>
        public void DisableGameplayInput() => _gameplayMap?.Disable();

        private void BuildActions()
        {
            if (_gameplayMap != null) return;

            _gameplayMap = new InputActionMap("Gameplay");

            _move = _gameplayMap.AddAction("Move", InputActionType.Value, expectedControlLayout: "Vector2");
            _move.AddCompositeBinding("2DVector(mode=2)") // 2 = Digital Normalized
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");

            _look = _gameplayMap.AddAction("Look", InputActionType.Value, "<Pointer>/position", expectedControlLayout: "Vector2");
            _dash = _gameplayMap.AddAction("Dash", InputActionType.Button, "<Keyboard>/space");
            _sprint = _gameplayMap.AddAction("Sprint", InputActionType.Button, "<Keyboard>/leftShift");
            _attack = _gameplayMap.AddAction("Attack", InputActionType.Button, "<Mouse>/leftButton");
            _interact = _gameplayMap.AddAction("Interact", InputActionType.Button, "<Keyboard>/e");

            _move.performed += OnMove;
            _move.canceled += OnMove;
            _look.performed += OnLook;
            _dash.performed += OnDash;
            _sprint.performed += OnSprintStarted;
            _sprint.canceled += OnSprintCanceled;
            _attack.performed += OnAttack;
            _attack.canceled += OnAttackReleased;
            _interact.performed += OnInteract;
        }

        private void OnMove(InputAction.CallbackContext ctx) => MoveInput = ctx.ReadValue<Vector2>();
        private void OnLook(InputAction.CallbackContext ctx) => PointerPosition = ctx.ReadValue<Vector2>();
        private void OnDash(InputAction.CallbackContext ctx) => DashPerformed?.Invoke();

        private void OnAttack(InputAction.CallbackContext ctx)
        {
            IsAttackHeld = true;
            AttackPerformed?.Invoke();
        }

        private void OnAttackReleased(InputAction.CallbackContext ctx) => IsAttackHeld = false;
        private void OnInteract(InputAction.CallbackContext ctx) => InteractPerformed?.Invoke();
        private void OnSprintStarted(InputAction.CallbackContext ctx) => SprintToggled?.Invoke(true);
        private void OnSprintCanceled(InputAction.CallbackContext ctx) => SprintToggled?.Invoke(false);
    }
}
