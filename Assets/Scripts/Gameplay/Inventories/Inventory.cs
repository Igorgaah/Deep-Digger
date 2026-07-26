using System;
using DeepDigger.Gameplay.Items;
using UnityEngine;

namespace DeepDigger.Gameplay.Inventories
{
    /// <summary>
    /// Pure C# model of a grid inventory: a fixed set of slots (Columns × Rows), stacking, and an
    /// optional weight limit. Deliberately free of <c>MonoBehaviour</c>/UI so it can be unit-tested and
    /// serialized to save files; the UI observes it through <see cref="SlotChanged"/>/<see cref="Changed"/>.
    /// </summary>
    public sealed class Inventory
    {
        private readonly ItemStack[] _slots;

        public int Columns { get; }
        public int Rows { get; }
        public int SlotCount => _slots.Length;

        /// <summary>0 = no weight limit.</summary>
        public float MaxWeight { get; }
        public float CurrentWeight { get; private set; }

        /// <summary>Raised for a single slot index whenever its contents change.</summary>
        public event Action<int> SlotChanged;

        /// <summary>Raised after any change, for listeners that just need a full refresh.</summary>
        public event Action Changed;

        public Inventory(int columns, int rows, float maxWeight = 0f)
        {
            Columns = Mathf.Max(1, columns);
            Rows = Mathf.Max(1, rows);
            MaxWeight = Mathf.Max(0f, maxWeight);
            _slots = new ItemStack[Columns * Rows];
        }

        public bool IsValidIndex(int index) => index >= 0 && index < _slots.Length;
        public ItemStack GetSlot(int index) => IsValidIndex(index) ? _slots[index] : ItemStack.Empty;

        /// <summary>
        /// Adds up to <paramref name="amount"/> of <paramref name="item"/>, filling existing stacks
        /// first and then empty slots, bounded by stack sizes and the weight limit. Returns how many
        /// units could NOT be added (0 when everything fit).
        /// </summary>
        public int TryAdd(ItemDefinition item, int amount)
        {
            if (item == null || amount <= 0) return amount;

            int acceptable = LimitByWeight(item, amount);
            int toPlace = acceptable;

            // 1) top up existing stacks of the same item
            for (int i = 0; i < _slots.Length && toPlace > 0; i++)
            {
                ItemStack slot = _slots[i];
                if (slot.IsEmpty || slot.Item != item || slot.RemainingSpace == 0) continue;

                int add = Mathf.Min(slot.RemainingSpace, toPlace);
                SetSlotInternal(i, slot.WithQuantity(slot.Quantity + add));
                toPlace -= add;
            }

            // 2) fill empty slots
            for (int i = 0; i < _slots.Length && toPlace > 0; i++)
            {
                if (!_slots[i].IsEmpty) continue;

                int add = Mathf.Min(item.MaxStack, toPlace);
                SetSlotInternal(i, new ItemStack(item, add));
                toPlace -= add;
            }

            int added = acceptable - toPlace;
            return amount - added;
        }

        /// <summary>Removes up to <paramref name="amount"/> from a slot; returns the amount actually removed.</summary>
        public int RemoveFromSlot(int index, int amount)
        {
            if (!IsValidIndex(index) || amount <= 0) return 0;

            ItemStack slot = _slots[index];
            if (slot.IsEmpty) return 0;

            int removed = Mathf.Min(amount, slot.Quantity);
            SetSlotInternal(index, slot.WithQuantity(slot.Quantity - removed));
            return removed;
        }

        /// <summary>
        /// Drag-and-drop primitive: moves/merges the stack in <paramref name="from"/> onto
        /// <paramref name="to"/>. Merges when both hold the same item, otherwise swaps them.
        /// </summary>
        public void MoveOrSwap(int from, int to)
        {
            if (from == to || !IsValidIndex(from) || !IsValidIndex(to)) return;

            ItemStack source = _slots[from];
            if (source.IsEmpty) return;

            ItemStack target = _slots[to];

            if (target.CanStackWith(source) && target.RemainingSpace > 0)
            {
                int moved = Mathf.Min(target.RemainingSpace, source.Quantity);
                SetSlotInternal(to, target.WithQuantity(target.Quantity + moved));
                SetSlotInternal(from, source.WithQuantity(source.Quantity - moved));
            }
            else
            {
                SetSlotInternal(from, target);
                SetSlotInternal(to, source);
            }
        }

        /// <summary>Total quantity of a given item across all slots.</summary>
        public int CountOf(ItemDefinition item)
        {
            if (item == null) return 0;

            int total = 0;
            foreach (ItemStack slot in _slots)
                if (!slot.IsEmpty && slot.Item == item) total += slot.Quantity;
            return total;
        }

        public void Clear()
        {
            for (int i = 0; i < _slots.Length; i++)
                if (!_slots[i].IsEmpty) SetSlotInternal(i, ItemStack.Empty);
        }

        private int LimitByWeight(ItemDefinition item, int amount)
        {
            if (MaxWeight <= 0f || item.Weight <= 0f) return amount;

            int maxByWeight = Mathf.FloorToInt((MaxWeight - CurrentWeight) / item.Weight);
            return Mathf.Clamp(maxByWeight, 0, amount);
        }

        private void SetSlotInternal(int index, ItemStack stack)
        {
            CurrentWeight += stack.TotalWeight - _slots[index].TotalWeight;
            if (CurrentWeight < 0f) CurrentWeight = 0f; // guard against float drift
            _slots[index] = stack;

            SlotChanged?.Invoke(index);
            Changed?.Invoke();
        }
    }
}
