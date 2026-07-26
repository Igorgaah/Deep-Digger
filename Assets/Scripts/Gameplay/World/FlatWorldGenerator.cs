using UnityEngine;

namespace DeepDigger.Gameplay.World
{
    /// <summary>
    /// Simplest generator: fills the whole mine with a single solid block and carves a small starting
    /// pocket near the top so the player has room to stand and begin digging. Kept as a permanent,
    /// useful generator (test rooms, tutorial) and as the reference implementation of the seam that
    /// Fase 4's procedural generator will follow.
    /// </summary>
    [CreateAssetMenu(fileName = "FlatWorldGenerator", menuName = "Deep Digger/World/Generators/Flat")]
    public sealed class FlatWorldGenerator : WorldGeneratorSO
    {
        [Header("Blocos")]
        [SerializeField] private BlockDefinition fillBlock;
        [Tooltip("Borda indestrutível ao redor da mina (opcional).")]
        [SerializeField] private BlockDefinition borderBlock;

        [Header("Bôlsão inicial")]
        [SerializeField, Min(2)] private int pocketWidth = 6;
        [SerializeField, Min(2)] private int pocketHeight = 4;

        protected override void Populate(WorldGrid grid, System.Random random)
        {
            if (fillBlock == null)
            {
                Debug.LogError($"{name}: 'fillBlock' não atribuído — a mina ficará vazia.");
                return;
            }

            for (int y = 0; y < grid.Height; y++)
            for (int x = 0; x < grid.Width; x++)
            {
                bool isBorder = x == 0 || y == 0 || x == grid.Width - 1 || y == grid.Height - 1;
                grid.SetBlock(x, y, isBorder && borderBlock != null ? borderBlock : fillBlock);
            }

            CarveStartingPocket(grid);
        }

        private void CarveStartingPocket(WorldGrid grid)
        {
            int centerX = grid.Width / 2;
            int topY = grid.Height - 2; // just inside the top border

            int halfW = pocketWidth / 2;
            for (int y = topY; y > topY - pocketHeight && y > 0; y--)
            for (int x = centerX - halfW; x <= centerX + halfW; x++)
            {
                if (x <= 0 || x >= grid.Width - 1) continue;
                grid.Clear(x, y);
            }

            grid.SpawnCell = new Vector2Int(centerX, topY - 1);
        }
    }
}
