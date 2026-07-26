using DeepDigger.Gameplay.Input;
using DeepDigger.Gameplay.World;
using UnityEngine;

namespace DeepDigger.Gameplay.Player
{
    /// <summary>
    /// The player's mining ability: while the attack button is held, it swings at the block under the
    /// pointer (within the pickaxe's reach), spending energy per hit and driving damage through the
    /// <see cref="WorldController"/>. Swing cadence comes from the equipped pickaxe and slows down when
    /// the tool tier is below the block's hardness (the "muito lenta" rule).
    /// </summary>
    public sealed class MiningSystem : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private InputReader input;
        [SerializeField] private PlayerAim aim;
        [SerializeField] private EnergySystem energy;
        [SerializeField] private WorldController world;

        [Header("Equipment")]
        [SerializeField] private PickaxeDefinition pickaxe;

        [Header("Tuning")]
        [Tooltip("Energia gasta por golpe (Minerar → -2 no design).")]
        [SerializeField, Min(0f)] private float energyPerHit = 2f;
        [Tooltip("Multiplicador de lentidão quando a picareta é de nível abaixo da Dureza do bloco.")]
        [SerializeField, Min(1f)] private float underTierSlowMultiplier = 3f;

        private float _swingCooldown;

        /// <summary>Currently equipped pickaxe. Swapped by inventory/upgrade systems later.</summary>
        public PickaxeDefinition Pickaxe => pickaxe;

        private void Awake()
        {
            if (aim == null) aim = GetComponent<PlayerAim>();
            if (energy == null) energy = GetComponent<EnergySystem>();
            if (world == null) world = FindFirstObjectByType<WorldController>();
        }

        /// <summary>Equips a different pickaxe at runtime.</summary>
        public void SetPickaxe(PickaxeDefinition newPickaxe) => pickaxe = newPickaxe;

        private void Update()
        {
            if (_swingCooldown > 0f)
            {
                _swingCooldown -= Time.deltaTime;
                return;
            }

            if (input == null || !input.IsAttackHeld) return;

            TrySwing();
        }

        private void TrySwing()
        {
            if (pickaxe == null || world == null || aim == null) return;

            Vector3 target = aim.AimWorldPoint;
            if (Vector2.Distance(transform.position, target) > pickaxe.Range) return;

            BlockDefinition block = world.GetBlockAtWorld(target);
            if (block == null || block.IsIndestructible) return;

            // Only spend energy once we know there is something valid to hit.
            if (energy != null && !energy.TryConsume(energyPerHit)) return;

            MiningResult result = world.DamageAtWorld(target, pickaxe.Damage);

            float interval = pickaxe.SwingInterval;
            if (pickaxe.Tier < block.HardnessTier) interval *= underTierSlowMultiplier;
            _swingCooldown = interval;

            if (result.Outcome == MiningOutcome.Destroyed)
                SpawnDrop(result.Block, target);
        }

        // Minimal drop spawn for Fase 3; Fase 10 (Loot) moves this to a LootSpawner listening to
        // BlockDestroyedEvent, with proper loot tables and item stacks.
        private void SpawnDrop(BlockDefinition block, Vector3 position)
        {
            if (block == null || block.DropPrefab == null) return;

            int amount = block.RollDropAmount();
            for (int i = 0; i < amount; i++)
            {
                Vector2 jitter = Random.insideUnitCircle * 0.15f;
                Instantiate(block.DropPrefab, position + (Vector3)jitter, Quaternion.identity);
            }
        }
    }
}
