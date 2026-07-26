using DeepDigger.Core.Events;
using UnityEngine;

namespace DeepDigger.Gameplay.World
{
    /// <summary>Raised when a block takes damage but survives. Feedback/audio systems react to it.</summary>
    public readonly struct BlockDamagedEvent : IEvent
    {
        public readonly Vector2Int Cell;
        public readonly Vector3 WorldPosition;
        public readonly BlockDefinition Block;
        public readonly int RemainingHealth;

        public BlockDamagedEvent(Vector2Int cell, Vector3 worldPosition, BlockDefinition block, int remainingHealth)
        {
            Cell = cell;
            WorldPosition = worldPosition;
            Block = block;
            RemainingHealth = remainingHealth;
        }
    }

    /// <summary>Raised when a block is fully mined out. Loot/feedback/quest systems react to it.</summary>
    public readonly struct BlockDestroyedEvent : IEvent
    {
        public readonly Vector2Int Cell;
        public readonly Vector3 WorldPosition;
        public readonly BlockDefinition Block;

        public BlockDestroyedEvent(Vector2Int cell, Vector3 worldPosition, BlockDefinition block)
        {
            Cell = cell;
            WorldPosition = worldPosition;
            Block = block;
        }
    }
}
