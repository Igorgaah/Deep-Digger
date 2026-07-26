using UnityEngine;

namespace DeepDigger.Gameplay.World
{
    /// <summary>Kind of point-of-interest placed by generation, spawned later by gameplay systems.</summary>
    public enum WorldFeatureType
    {
        Chest,
        Event,
        Ruin,
        Merchant,
        Altar
    }

    /// <summary>
    /// A marker left by the generator at a grid cell. The terrain generator only records *where*
    /// something interesting should be; the loot/event/NPC systems of later phases read this list and
    /// spawn the actual entities, keeping generation decoupled from gameplay content.
    /// </summary>
    public readonly struct WorldFeature
    {
        public readonly Vector2Int Cell;
        public readonly WorldFeatureType Type;

        public WorldFeature(Vector2Int cell, WorldFeatureType type)
        {
            Cell = cell;
            Type = type;
        }
    }
}
