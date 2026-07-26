using System;
using System.Collections.Generic;
using UnityEngine;

namespace DeepDigger.Gameplay.World
{
    /// <summary>Outcome of applying damage to a cell.</summary>
    public enum MiningOutcome
    {
        /// <summary>Nothing there (empty cell), out of bounds, or an indestructible block.</summary>
        NoEffect,

        /// <summary>Block took damage and is still standing.</summary>
        Damaged,

        /// <summary>Block reached zero HP and was removed.</summary>
        Destroyed
    }

    /// <summary>Result of a single mining hit, including the block that was affected.</summary>
    public readonly struct MiningResult
    {
        public readonly MiningOutcome Outcome;
        public readonly BlockDefinition Block;
        public readonly int RemainingHealth;

        public MiningResult(MiningOutcome outcome, BlockDefinition block, int remainingHealth)
        {
            Outcome = outcome;
            Block = block;
            RemainingHealth = remainingHealth;
        }

        public static readonly MiningResult None = new(MiningOutcome.NoEffect, null, 0);
    }

    /// <summary>
    /// Pure data model of the mine: which <see cref="BlockDefinition"/> occupies each cell and its
    /// current HP. Deliberately free of Unity <c>MonoBehaviour</c>/Tilemap dependencies so it can be
    /// generated, mined and unit-tested in isolation; a separate view (see <c>IWorldView</c>) renders it.
    /// </summary>
    public sealed class WorldGrid
    {
        private readonly BlockDefinition[] _blocks;
        private readonly int[] _health;

        public int Width { get; }
        public int Height { get; }

        /// <summary>Cell where the player should start (carved out by the generator).</summary>
        public Vector2Int SpawnCell { get; set; }

        /// <summary>
        /// Points of interest recorded by the generator (chests, events, ruins…). Filled during
        /// generation; consumed by loot/event/NPC systems in later phases.
        /// </summary>
        public List<WorldFeature> Features { get; } = new();

        /// <summary>Raised when a single cell changes (dug or damaged). Payload is the cell coordinate.</summary>
        public event Action<Vector2Int> CellChanged;

        public WorldGrid(int width, int height)
        {
            if (width <= 0 || height <= 0)
                throw new ArgumentOutOfRangeException(nameof(width), "World dimensions must be positive.");

            Width = width;
            Height = height;
            _blocks = new BlockDefinition[width * height];
            _health = new int[width * height];
        }

        public bool InBounds(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;
        public bool InBounds(Vector2Int cell) => InBounds(cell.x, cell.y);

        /// <summary><c>true</c> when the cell holds a block (i.e. is not dug out).</summary>
        public bool IsSolid(int x, int y) => InBounds(x, y) && _blocks[Index(x, y)] != null;
        public bool IsSolid(Vector2Int cell) => IsSolid(cell.x, cell.y);

        public BlockDefinition GetBlock(int x, int y) => InBounds(x, y) ? _blocks[Index(x, y)] : null;
        public int GetHealth(int x, int y) => InBounds(x, y) ? _health[Index(x, y)] : 0;

        /// <summary>Places (or clears, when <paramref name="block"/> is null) a block and resets its HP.</summary>
        public void SetBlock(int x, int y, BlockDefinition block)
        {
            if (!InBounds(x, y)) return;

            int i = Index(x, y);
            _blocks[i] = block;
            _health[i] = block != null ? block.MaxHealth : 0;
            CellChanged?.Invoke(new Vector2Int(x, y));
        }

        /// <summary>Removes the block at the cell (digs it out) without producing a <see cref="MiningResult"/>.</summary>
        public void Clear(int x, int y) => SetBlock(x, y, null);

        /// <summary>
        /// Applies <paramref name="amount"/> damage to the block at the cell. Indestructible blocks and
        /// empty/out-of-bounds cells are ignored. Returns what happened plus the affected block.
        /// </summary>
        public MiningResult DamageBlock(int x, int y, int amount)
        {
            if (amount <= 0 || !IsSolid(x, y)) return MiningResult.None;

            int i = Index(x, y);
            BlockDefinition block = _blocks[i];
            if (block.IsIndestructible) return new MiningResult(MiningOutcome.NoEffect, block, _health[i]);

            _health[i] -= amount;
            if (_health[i] <= 0)
            {
                _blocks[i] = null;
                _health[i] = 0;
                CellChanged?.Invoke(new Vector2Int(x, y));
                return new MiningResult(MiningOutcome.Destroyed, block, 0);
            }

            CellChanged?.Invoke(new Vector2Int(x, y));
            return new MiningResult(MiningOutcome.Damaged, block, _health[i]);
        }

        private int Index(int x, int y) => y * Width + x;
    }
}
