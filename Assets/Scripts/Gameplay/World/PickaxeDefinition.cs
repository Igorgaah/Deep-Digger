using UnityEngine;

namespace DeepDigger.Gameplay.World
{
    /// <summary>
    /// Data for a mining tool (Durabilidade, Dano, Velocidade, Alcance, Sprite, Som). Also consumed
    /// later by the inventory and upgrade systems.
    /// </summary>
    [CreateAssetMenu(fileName = "Pickaxe_", menuName = "Deep Digger/World/Pickaxe Definition")]
    public sealed class PickaxeDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string id = "wood";
        [SerializeField] private string displayName = "Picareta de Madeira";
        [Tooltip("Nível da ferramenta. Comparado à Dureza do bloco para decidir a penalidade de velocidade.")]
        [SerializeField, Min(0)] private int tier;

        [Header("Mining (Dano / Velocidade / Alcance)")]
        [Tooltip("Dano por golpe.")]
        [SerializeField, Min(1)] private int damage = 1;
        [Tooltip("Intervalo entre golpes, em segundos (menor = mais rápido).")]
        [SerializeField, Min(0.05f)] private float swingInterval = 0.35f;
        [Tooltip("Alcance máximo de mineração, em unidades de mundo.")]
        [SerializeField, Min(0.5f)] private float range = 1.8f;

        [Header("Durabilidade")]
        [Tooltip("0 = indestrutível. Consumo de durabilidade será usado nas fases de crafting/upgrades.")]
        [SerializeField, Min(0)] private int durability;

        [Header("Apresentação")]
        [SerializeField] private Sprite sprite;
        [SerializeField] private AudioClip swingSound;

        public string Id => id;
        public string DisplayName => displayName;
        public int Tier => tier;
        public int Damage => damage;
        public float SwingInterval => swingInterval;
        public float Range => range;
        public int Durability => durability;
        public Sprite Sprite => sprite;
        public AudioClip SwingSound => swingSound;
    }
}
