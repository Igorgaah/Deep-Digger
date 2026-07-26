using UnityEngine;

namespace DeepDigger.Gameplay.Items
{
    /// <summary>
    /// Immutable data for one kind of item (Ícone, Descrição, Stack, Peso). A single asset is shared
    /// by every stack of that item (flyweight); runtime quantity lives in <c>ItemStack</c>/<c>Inventory</c>.
    /// </summary>
    [CreateAssetMenu(fileName = "Item_", menuName = "Deep Digger/Items/Item Definition")]
    public sealed class ItemDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string id = "ore_stone";
        [SerializeField] private string displayName = "Pedra";
        [SerializeField, TextArea(2, 4)] private string description = "";
        [SerializeField] private ItemCategory category = ItemCategory.Resource;
        [SerializeField] private ItemRarity rarity = ItemRarity.Common;

        [Header("Apresentação")]
        [SerializeField] private Sprite icon;

        [Header("Inventário")]
        [Tooltip("Quantidade máxima por slot (Stack).")]
        [SerializeField, Min(1)] private int maxStack = 99;
        [Tooltip("Peso por unidade.")]
        [SerializeField, Min(0f)] private float weight = 0.1f;

        [Header("Economia")]
        [Tooltip("Valor base de venda (usado na Loja).")]
        [SerializeField, Min(0)] private int baseValue = 1;

        public string Id => id;
        public string DisplayName => displayName;
        public string Description => description;
        public ItemCategory Category => category;
        public ItemRarity Rarity => rarity;
        public Sprite Icon => icon;
        public int MaxStack => maxStack;
        public float Weight => weight;
        public int BaseValue => baseValue;
    }
}
