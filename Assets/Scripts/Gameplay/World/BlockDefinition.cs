using DeepDigger.Gameplay.Items;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace DeepDigger.Gameplay.World
{
    /// <summary>
    /// Immutable data describing one kind of block (Vida, Dureza, Tipo, Drop, Som, Partículas).
    /// Runtime state (current HP) lives in <see cref="WorldGrid"/>, so a single asset is shared by
    /// every cell of that type — the flyweight pattern.
    /// </summary>
    [CreateAssetMenu(fileName = "Block_", menuName = "Deep Digger/World/Block Definition")]
    public sealed class BlockDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string id = "stone";
        [SerializeField] private string displayName = "Pedra";
        [SerializeField] private BlockCategory category = BlockCategory.Rock;

        [Header("Mining (Vida / Dureza)")]
        [Tooltip("Pontos de vida do bloco (Vida).")]
        [SerializeField, Min(1)] private int maxHealth = 2;
        [Tooltip("Nível mínimo de picareta para minerar em velocidade normal (Dureza). Abaixo disso, muito lento.")]
        [SerializeField, Min(0)] private int hardnessTier;

        [Header("Rendering")]
        [Tooltip("Tile autorado. Se vazio, o renderer usa um tile colorido gerado em runtime.")]
        [SerializeField] private TileBase tile;
        [SerializeField] private Color color = new(0.5f, 0.5f, 0.5f, 1f);

        [Header("Drop (Loot)")]
        [Tooltip("Item de recurso adicionado direto ao inventário ao minerar (ex.: minério). Preferido sobre o prefab.")]
        [SerializeField] private ItemDefinition dropItem;
        [Tooltip("Prefab físico solto ao quebrar (fallback / drops no chão em fases futuras).")]
        [SerializeField] private GameObject dropPrefab;
        [Tooltip("Quantidade mínima e máxima de drops (x = min, y = max).")]
        [SerializeField] private Vector2Int dropAmount = new(1, 1);

        [Header("Feedback (Som / Partículas)")]
        [SerializeField] private GameObject hitEffect;
        [SerializeField] private GameObject breakEffect;
        [SerializeField] private AudioClip hitSound;
        [SerializeField] private AudioClip breakSound;

        public string Id => id;
        public string DisplayName => displayName;
        public BlockCategory Category => category;
        public int MaxHealth => maxHealth;
        public int HardnessTier => hardnessTier;
        public TileBase Tile => tile;
        public Color Color => color;
        public ItemDefinition DropItem => dropItem;
        public GameObject DropPrefab => dropPrefab;
        public GameObject HitEffect => hitEffect;
        public GameObject BreakEffect => breakEffect;
        public AudioClip HitSound => hitSound;
        public AudioClip BreakSound => breakSound;

        /// <summary><c>true</c> when the block can never be destroyed.</summary>
        public bool IsIndestructible => category == BlockCategory.Indestructible;

        /// <summary>Rolls a drop quantity in the inclusive [min, max] range.</summary>
        public int RollDropAmount() => Random.Range(dropAmount.x, dropAmount.y + 1);
    }
}
