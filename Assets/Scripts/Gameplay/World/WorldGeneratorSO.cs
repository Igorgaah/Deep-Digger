using UnityEngine;

namespace DeepDigger.Gameplay.World
{
    /// <summary>
    /// Strategy (as a ScriptableObject) that fills a <see cref="WorldGrid"/>. Swapping the asset
    /// referenced by the world swaps the whole generation algorithm — the procedural generator of
    /// Fase 4 will be just another subclass, with no change to the rest of the game.
    /// </summary>
    public abstract class WorldGeneratorSO : ScriptableObject
    {
        [Header("Dimensões da mina")]
        [SerializeField, Min(4)] protected int width = 96;
        [SerializeField, Min(4)] protected int height = 160;

        public int Width => width;
        public int Height => height;

        /// <summary>Builds and returns a freshly generated grid using an optional deterministic seed.</summary>
        public WorldGrid Generate(int seed)
        {
            var grid = new WorldGrid(width, height);
            var random = new System.Random(seed);
            Populate(grid, random);
            return grid;
        }

        /// <summary>Fills <paramref name="grid"/>. Implementations must guarantee a reachable spawn.</summary>
        protected abstract void Populate(WorldGrid grid, System.Random random);
    }
}
