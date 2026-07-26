using UnityEngine;

namespace DeepDigger.Gameplay.World
{
    /// <summary>
    /// Renders a <see cref="WorldGrid"/> and maps between world space and grid cells. Keeps the grid
    /// and mining logic independent from the concrete rendering backend (Tilemap today, something
    /// else tomorrow), so both stay testable and swappable.
    /// </summary>
    public interface IWorldView
    {
        /// <summary>Binds the view to a grid and performs a full initial render.</summary>
        void Initialize(WorldGrid grid);

        /// <summary>Re-renders every cell from the current grid state.</summary>
        void RenderAll();

        /// <summary>Re-renders a single cell (called on dig/damage).</summary>
        void RenderCell(int x, int y);

        /// <summary>Converts a world-space position to a grid cell coordinate.</summary>
        Vector2Int WorldToCell(Vector3 worldPosition);

        /// <summary>Returns the world-space center of a grid cell.</summary>
        Vector3 CellCenterToWorld(int x, int y);
    }
}
