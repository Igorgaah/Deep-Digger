using DeepDigger.Gameplay.Input;
using UnityEngine;

namespace DeepDigger.Gameplay.Player
{
    /// <summary>
    /// Converts the pointer position (routed through the <see cref="InputReader"/>) into a world-space
    /// aim direction. Consumed by mining and combat to know where the player is pointing.
    /// </summary>
    public sealed class PlayerAim : MonoBehaviour
    {
        [SerializeField] private InputReader input;
        [SerializeField] private Camera targetCamera;

        /// <summary>Normalized direction from the player toward the pointer.</summary>
        public Vector2 AimDirection { get; private set; } = Vector2.down;

        /// <summary>World-space point under the pointer, on the player's plane.</summary>
        public Vector2 AimWorldPoint { get; private set; }

        private void Awake()
        {
            if (targetCamera == null) targetCamera = Camera.main;
        }

        private void Update()
        {
            if (input == null || targetCamera == null) return;

            Vector3 screen = new(input.PointerPosition.x, input.PointerPosition.y, -targetCamera.transform.position.z);
            Vector2 world = targetCamera.ScreenToWorldPoint(screen);
            AimWorldPoint = world;

            Vector2 delta = world - (Vector2)transform.position;
            if (delta.sqrMagnitude > 0.0001f)
                AimDirection = delta.normalized;
        }
    }
}
