using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace DeepDigger.Gameplay.World
{
    /// <summary>
    /// Renders a <see cref="WorldGrid"/> onto a Unity <see cref="Tilemap"/>. When a block has no
    /// authored tile, a colored tile is generated at runtime, so the mine is fully visible (and
    /// collidable) without any art — ideal for iterating on gameplay before the pixel-art pipeline.
    /// </summary>
    [RequireComponent(typeof(Tilemap))]
    public sealed class TilemapWorldRenderer : MonoBehaviour, IWorldView
    {
        private Tilemap _tilemap;
        private WorldGrid _grid;

        // One reusable tile per block definition (flyweight), built lazily.
        private readonly Dictionary<BlockDefinition, TileBase> _tileCache = new();
        private Sprite _unitSprite;

        private void Awake() => _tilemap = GetComponent<Tilemap>();

        public void Initialize(WorldGrid grid)
        {
            _grid = grid;
            RenderAll();
        }

        public void RenderAll()
        {
            if (_grid == null) return;

            _tilemap.ClearAllTiles();
            for (int y = 0; y < _grid.Height; y++)
            for (int x = 0; x < _grid.Width; x++)
                ApplyCell(x, y);
        }

        public void RenderCell(int x, int y)
        {
            if (_grid == null || !_grid.InBounds(x, y)) return;
            ApplyCell(x, y);
        }

        public Vector2Int WorldToCell(Vector3 worldPosition)
        {
            Vector3Int cell = _tilemap.WorldToCell(worldPosition);
            return new Vector2Int(cell.x, cell.y);
        }

        public Vector3 CellCenterToWorld(int x, int y) => _tilemap.GetCellCenterWorld(new Vector3Int(x, y, 0));

        private void ApplyCell(int x, int y)
        {
            BlockDefinition block = _grid.GetBlock(x, y);
            _tilemap.SetTile(new Vector3Int(x, y, 0), block != null ? GetTileFor(block) : null);
        }

        private TileBase GetTileFor(BlockDefinition block)
        {
            if (block.Tile != null) return block.Tile;

            if (_tileCache.TryGetValue(block, out TileBase cached)) return cached;

            var tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = GetUnitSprite();
            tile.color = block.Color;
            tile.colliderType = Tile.ColliderType.Grid; // solid cells collide with the player
            _tileCache[block] = tile;
            return tile;
        }

        private Sprite GetUnitSprite()
        {
            if (_unitSprite != null) return _unitSprite;

            Texture2D tex = Texture2D.whiteTexture;
            // pixelsPerUnit = texture width => the sprite is exactly one world unit (one cell).
            _unitSprite = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), tex.width);
            return _unitSprite;
        }
    }
}
