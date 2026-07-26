using DeepDigger.Core.Events;
using UnityEngine;

namespace DeepDigger.Gameplay.Player
{
    /// <summary>
    /// Reusable stamina/energy pool. Sprint, dash, mining and attacks all draw from it.
    /// Regeneration pauses for a short window after any consumption so spending energy has weight.
    /// Notifies the rest of the game through <see cref="EnergyChangedEvent"/> on the <see cref="EventBus"/>.
    /// </summary>
    public sealed class EnergySystem : MonoBehaviour
    {
        [Header("Capacity")]
        [SerializeField, Min(1f)] private float maxEnergy = 100f;

        [Header("Regeneration")]
        [SerializeField, Min(0f)] private float regenPerSecond = 12f;
        [Tooltip("Seconds without spending energy before regeneration resumes.")]
        [SerializeField, Min(0f)] private float regenDelay = 0.8f;

        private float _current;
        private float _regenCooldown;

        public float Current => _current;
        public float Max => maxEnergy;
        public bool IsEmpty => _current <= 0f;

        private void Awake()
        {
            _current = maxEnergy;
        }

        private void Start()
        {
            // Broadcast the initial value once listeners (UI) have had a chance to subscribe.
            Publish();
        }

        private void Update()
        {
            if (_regenCooldown > 0f)
            {
                _regenCooldown -= Time.deltaTime;
                return;
            }

            if (_current >= maxEnergy) return;

            _current = Mathf.Min(maxEnergy, _current + regenPerSecond * Time.deltaTime);
            Publish();
        }

        /// <summary>Returns <c>true</c> if at least <paramref name="amount"/> energy is available.</summary>
        public bool HasEnergy(float amount) => _current >= amount;

        /// <summary>
        /// Atomically spends <paramref name="amount"/> energy. Returns <c>false</c> and spends nothing
        /// when the pool is insufficient. Use for discrete costs (dash, attack, a mining hit).
        /// </summary>
        public bool TryConsume(float amount)
        {
            if (amount <= 0f) return true;
            if (_current < amount) return false;

            _current -= amount;
            _regenCooldown = regenDelay;
            Publish();
            return true;
        }

        /// <summary>
        /// Drains up to <paramref name="amount"/> energy, clamping at zero. Use for continuous
        /// costs such as sprinting, where partial spend is acceptable.
        /// </summary>
        public void Drain(float amount)
        {
            if (amount <= 0f || _current <= 0f) return;

            _current = Mathf.Max(0f, _current - amount);
            _regenCooldown = regenDelay;
            Publish();
        }

        private void Publish() => EventBus.Publish(new EnergyChangedEvent(_current, maxEnergy));
    }
}
