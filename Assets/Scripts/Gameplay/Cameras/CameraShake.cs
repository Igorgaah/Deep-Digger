using DeepDigger.Core.Events;
using UnityEngine;

namespace DeepDigger.Gameplay.Cameras
{
    /// <summary>
    /// Produces a decaying positional offset for screen-shake. It never touches the transform
    /// itself; <see cref="CameraFollow"/> reads <see cref="CurrentOffset"/> and applies it, avoiding
    /// two scripts fighting over the camera position. Listens on the <see cref="EventBus"/> so any
    /// system can request a shake by publishing <see cref="CameraShakeRequest"/>.
    /// </summary>
    public sealed class CameraShake : MonoBehaviour
    {
        /// <summary>Current shake offset to be added on top of the follow position.</summary>
        public Vector3 CurrentOffset { get; private set; }

        private float _duration;
        private float _elapsed;
        private float _magnitude;

        private void OnEnable() => EventBus.Subscribe<CameraShakeRequest>(OnShakeRequested);
        private void OnDisable() => EventBus.Unsubscribe<CameraShakeRequest>(OnShakeRequested);

        private void OnShakeRequested(CameraShakeRequest request) => Shake(request.Duration, request.Magnitude);

        /// <summary>
        /// Starts (or reinforces) a shake. Overlapping requests take the stronger magnitude and the
        /// longer remaining duration instead of stacking, which keeps the effect readable.
        /// </summary>
        public void Shake(float duration, float magnitude)
        {
            if (duration <= 0f || magnitude <= 0f) return;

            _magnitude = Mathf.Max(_magnitude, magnitude);
            _duration = Mathf.Max(_duration - _elapsed, duration);
            _elapsed = 0f;
        }

        private void LateUpdate()
        {
            if (_elapsed >= _duration)
            {
                CurrentOffset = Vector3.zero;
                _magnitude = 0f;
                return;
            }

            _elapsed += Time.deltaTime;
            float damper = 1f - Mathf.Clamp01(_elapsed / _duration);
            Vector2 random = Random.insideUnitCircle * (_magnitude * damper);
            CurrentOffset = new Vector3(random.x, random.y, 0f);
        }
    }
}
