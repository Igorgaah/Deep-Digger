using DeepDigger.Core.Events;
using DeepDigger.Gameplay.Items;

namespace DeepDigger.Gameplay.Inventories
{
    /// <summary>
    /// Raised when items enter the player's inventory. Quests, tutorials, achievements and audio can
    /// react without referencing the inventory or the mining system.
    /// </summary>
    public readonly struct ItemCollectedEvent : IEvent
    {
        public readonly ItemDefinition Item;
        public readonly int Amount;

        public ItemCollectedEvent(ItemDefinition item, int amount)
        {
            Item = item;
            Amount = amount;
        }
    }
}
