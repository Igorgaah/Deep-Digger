using DeepDigger.Gameplay.Input;
using UnityEngine;

namespace DeepDigger.Gameplay.Player
{
    /// <summary>
    /// Drives top-down player locomotion: walking, energy-gated sprinting and a short dash with
    /// cooldown. Reads intent from an <see cref="InputReader"/> and moves a physics
    /// <see cref="Rigidbody2D"/>, keeping input, energy and physics concerns separated.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerController : MonoBehaviour
    {
        private enum LocomotionState { Normal, Dashing }

        [Header("Dependencies")]
        [SerializeField] private InputReader input;
        [SerializeField] private EnergySystem energy;

        [Header("Movement")]
        [SerializeField, Min(0f)] private float moveSpeed = 5f;
        [SerializeField, Min(1f)] private float sprintMultiplier = 1.6f;
        [SerializeField, Min(0f)] private float sprintEnergyPerSecond = 18f;

        [Header("Dash")]
        [SerializeField, Min(0f)] private float dashSpeed = 18f;
        [SerializeField, Min(0.01f)] private float dashDuration = 0.15f;
        [SerializeField, Min(0f)] private float dashCooldown = 0.6f;
        [SerializeField, Min(0f)] private float dashEnergyCost = 20f;

        private Rigidbody2D _rb;
        private Vector2 _moveInput;
        private Vector2 _facing = Vector2.down;
        private bool _sprintHeld;

        private LocomotionState _state = LocomotionState.Normal;
        private Vector2 _dashDirection;
        private float _dashTimeLeft;
        private float _dashCooldownLeft;

        /// <summary>Last non-zero movement direction; useful for aiming a dash or attack.</summary>
        public Vector2 Facing => _facing;
        public bool IsDashing => _state == LocomotionState.Dashing;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            if (energy == null) energy = GetComponent<EnergySystem>();
        }

        private void OnEnable()
        {
            if (input == null)
            {
                Debug.LogError($"{nameof(PlayerController)} is missing an {nameof(InputReader)} reference.", this);
                enabled = false;
                return;
            }

            input.EnableGameplayInput();
            input.DashPerformed += OnDashRequested;
            input.SprintToggled += OnSprintToggled;
        }

        private void OnDisable()
        {
            if (input == null) return;

            input.DashPerformed -= OnDashRequested;
            input.SprintToggled -= OnSprintToggled;
            input.DisableGameplayInput();
        }

        private void Update()
        {
            _moveInput = Vector2.ClampMagnitude(input.MoveInput, 1f);
            if (_moveInput.sqrMagnitude > 0.01f) _facing = _moveInput.normalized;

            if (_dashCooldownLeft > 0f) _dashCooldownLeft -= Time.deltaTime;
        }

        private void FixedUpdate()
        {
            if (_state == LocomotionState.Dashing)
            {
                TickDash();
                return;
            }

            bool sprinting = _sprintHeld
                             && _moveInput.sqrMagnitude > 0.01f
                             && energy != null
                             && !energy.IsEmpty;

            float speed = sprinting ? moveSpeed * sprintMultiplier : moveSpeed;
            _rb.linearVelocity = _moveInput * speed;

            if (sprinting)
                energy.Drain(sprintEnergyPerSecond * Time.fixedDeltaTime);
        }

        private void OnSprintToggled(bool held) => _sprintHeld = held;

        private void OnDashRequested()
        {
            if (_state == LocomotionState.Dashing || _dashCooldownLeft > 0f) return;
            if (energy != null && !energy.TryConsume(dashEnergyCost)) return;

            _dashDirection = _moveInput.sqrMagnitude > 0.01f ? _moveInput.normalized : _facing;
            _state = LocomotionState.Dashing;
            _dashTimeLeft = dashDuration;
            _dashCooldownLeft = dashCooldown;
        }

        private void TickDash()
        {
            _rb.linearVelocity = _dashDirection * dashSpeed;
            _dashTimeLeft -= Time.fixedDeltaTime;
            if (_dashTimeLeft <= 0f)
                _state = LocomotionState.Normal;
        }
    }
}
