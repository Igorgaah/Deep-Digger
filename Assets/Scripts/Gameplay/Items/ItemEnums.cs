namespace DeepDigger.Gameplay.Items
{
    /// <summary>Broad gameplay class of an item, used for filtering, sorting and UI grouping.</summary>
    public enum ItemCategory
    {
        Resource,
        Gem,
        Tool,
        Consumable,
        Relic,
        Misc
    }

    /// <summary>Rarity tier — drives tooltip color and drop/value balancing.</summary>
    public enum ItemRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }
}
