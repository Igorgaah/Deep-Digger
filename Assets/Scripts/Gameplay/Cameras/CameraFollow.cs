using UnityEngine;

namespace DeepDigger.Gameplay.Cameras
{
    /// <summary>
    /// Smoothly follows a target in 2D and applies optional screen-shake from a sibling
    /// <see cref="CameraShake"/>. Implemented directly (rather than via Cinemachine) to keep the
    /// early project dependency-free and fully code-driven; it can be swapped for a Cinemachine
    /// virtual camera later without touching gameplay code.
    /// </summary>
    public sealed class CameraFollow : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new(0f, 0f, -10f);

        [Header("Smoothing")]
        [SerializeField, Min(0f)] private float smoothTime = 0.15f;

        [Header("Bounds (optional)")]
        [SerializeField] private bool useBounds;
        [SerializeField] private Vector2 minBounds;
        [SerializeField] private Vector2 maxBounds;

        [SerializeField] private CameraShake shake;

        private Vector3 _velocity;

        /// <summary>Reassigns the follow target at runtime (e.g. after spawning the player).</summary>
        public void SetTarget(Transform newTarget) => target = newTarget;

        private void Awake()
        {
            if (shake == null) shake = GetComponent<CameraShake>();
        }

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 desired = target.position + offset;
            Vector3 smoothed = Vector3.SmoothDamp(transform.position, desired, ref _velocity, smoothTime);

            if (useBounds)
            {
                smoothed.x = Mathf.Clamp(smoothed.x, minBounds.x, maxBounds.x);
                smoothed.y = Mathf.Clamp(smoothed.y, minBounds.y, maxBounds.y);
            }

            if (shake != null)
                smoothed += shake.CurrentOffset;

            transform.position = smoothed;
        }
    }
}
