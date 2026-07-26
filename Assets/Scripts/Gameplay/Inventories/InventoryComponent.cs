using DeepDigger.Core.Events;
using DeepDigger.Gameplay.Items;
using UnityEngine;

namespace DeepDigger.Gameplay.Inventories
{
    /// <summary>
    /// Scene-facing wrapper around the pure <see cref="Inventory"/> model: exposes grid size/weight in
    /// the inspector, builds the model, and is the single place that publishes <see cref="ItemCollectedEvent"/>
    /// when items are gained.
    /// </summary>
    public sealed class InventoryComponent : MonoBehaviour
    {
        [Header("Grid")]
        [SerializeField, Min(1)] private int columns = 8;
        [SerializeField, Min(1)] private int rows = 5;

        [Header("Peso")]
        [Tooltip("Capacidade de peso total. 0 = sem limite de peso.")]
        [SerializeField, Min(0f)] private float maxWeight = 100f;

        /// <summary>The underlying model. Never null after <c>Awake</c>.</summary>
        public Inventory Inventory { get; private set; }

        private void Awake() => Inventory = new Inventory(columns, rows, maxWeight);

        /// <summary>
        /// Adds items and announces the pickup. Returns the leftover that did not fit (0 when all added).
        /// </summary>
        public int Add(ItemDefinition item, int amount)
        {
            if (item == null || amount <= 0) return amount;

            int leftover = Inventory.TryAdd(item, amount);
            int added = amount - leftover;
            if (added > 0)
                EventBus.Publish(new ItemCollectedEvent(item, added));

            return leftover;
        }
    }
}
