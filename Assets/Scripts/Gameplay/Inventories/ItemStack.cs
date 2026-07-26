using DeepDigger.Gameplay.Items;
using UnityEngine;

namespace DeepDigger.Gameplay.Inventories
{
    /// <summary>
    /// An immutable amount of one item in a slot. Being a small <c>readonly struct</c> keeps inventory
    /// storage allocation-free; mutating operations return a new stack instead of editing in place.
    /// </summary>
    public readonly struct ItemStack
    {
        public readonly ItemDefinition Item;
        public readonly int Quantity;

        public ItemStack(ItemDefinition item, int quantity)
        {
            Item = item;
            Quantity = Mathf.Max(0, quantity);
        }

        /// <summary>The canonical empty stack.</summary>
        public static readonly ItemStack Empty = default;

        public bool IsEmpty => Item == null || Quantity <= 0;

        /// <summary>Total weight this stack contributes to the inventory.</summary>
        public float TotalWeight => IsEmpty ? 0f : Item.Weight * Quantity;

        /// <summary>Free room left before hitting the item's max stack size.</summary>
        public int RemainingSpace => IsEmpty ? 0 : Mathf.Max(0, Item.MaxStack - Quantity);

        /// <summary>Returns a copy with a different quantity (empty when &lt;= 0).</summary>
        public ItemStack WithQuantity(int quantity) => quantity <= 0 ? Empty : new ItemStack(Item, quantity);

        /// <summary>Whether <paramref name="other"/> holds the same item type and could stack with this one.</summary>
        public bool CanStackWith(ItemStack other) => !IsEmpty && !other.IsEmpty && other.Item == Item;
    }
}
