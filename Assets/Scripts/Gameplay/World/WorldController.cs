using DeepDigger.Core.Events;
using UnityEngine;

namespace DeepDigger.Gameplay.World
{
    /// <summary>
    /// Owns the runtime <see cref="WorldGrid"/> and is the single authority on terrain changes: it
    /// generates the mine, keeps the <see cref="IWorldView"/> in sync, converts between world and cell
    /// space, and publishes <see cref="BlockDamagedEvent"/>/<see cref="BlockDestroyedEvent"/> so other
    /// systems (feedback, loot, audio) react without touching the terrain directly.
    /// </summary>
    public sealed class WorldController : MonoBehaviour
    {
        [Header("Generation")]
        [SerializeField] private WorldGeneratorSO generator;
        [SerializeField] private bool randomizeSeed = true;
        [SerializeField] private int seed;

        [Header("Spawn")]
        [Tooltip("Se atribuído, é reposicionado no bôlsão inicial após gerar a mina.")]
        [SerializeField] private Transform playerToPlace;

        private IWorldView _view;
        private WorldGrid _grid;

        public WorldGrid Grid => _grid;

        private void Awake()
        {
            _view = GetComponentInChildren<IWorldView>();
            if (_view == null)
                Debug.LogError($"{nameof(WorldController)} não encontrou um {nameof(IWorldView)} nos filhos (ex.: {nameof(TilemapWorldRenderer)}).", this);
        }

        private void Start() => BuildWorld();

        private void OnDestroy()
        {
            if (_grid != null) _grid.CellChanged -= OnCellChanged;
        }

        /// <summary>Generates a fresh mine and renders it. Safe to call again to regenerate.</summary>
        public void BuildWorld()
        {
            if (generator == null)
            {
                Debug.LogError($"{nameof(WorldController)}: nenhum {nameof(WorldGeneratorSO)} atribuído.", this);
                return;
            }

            if (_grid != null) _grid.CellChanged -= OnCellChanged;

            int usedSeed = randomizeSeed ? Random.Range(int.MinValue, int.MaxValue) : seed;
            _grid = generator.Generate(usedSeed);
            _grid.CellChanged += OnCellChanged;

            _view?.Initialize(_grid);

            if (playerToPlace != null)
                playerToPlace.position = GetSpawnWorldPosition();
        }

        /// <summary>World-space center of the carved starting pocket.</summary>
        public Vector3 GetSpawnWorldPosition()
        {
            if (_grid == null || _view == null) return transform.position;
            return _view.CellCenterToWorld(_grid.SpawnCell.x, _grid.SpawnCell.y);
        }

        public bool IsSolidAtWorld(Vector3 worldPosition)
        {
            if (_grid == null || _view == null) return false;
            Vector2Int cell = _view.WorldToCell(worldPosition);
            return _grid.IsSolid(cell);
        }

        /// <summary>Returns the block at a world position, or <c>null</c> if empty/out of bounds.</summary>
        public BlockDefinition GetBlockAtWorld(Vector3 worldPosition)
        {
            if (_grid == null || _view == null) return null;
            Vector2Int cell = _view.WorldToCell(worldPosition);
            return _grid.GetBlock(cell.x, cell.y);
        }

        /// <summary>
        /// Applies <paramref name="amount"/> mining damage to whatever block sits under
        /// <paramref name="worldPosition"/> and reports the outcome. Terrain events are published here.
        /// </summary>
        public MiningResult DamageAtWorld(Vector3 worldPosition, int amount)
        {
            if (_grid == null || _view == null) return MiningResult.None;

            Vector2Int cell = _view.WorldToCell(worldPosition);
            MiningResult result = _grid.DamageBlock(cell.x, cell.y, amount);
            Vector3 center = _view.CellCenterToWorld(cell.x, cell.y);

            switch (result.Outcome)
            {
                case MiningOutcome.Damaged:
                    EventBus.Publish(new BlockDamagedEvent(cell, center, result.Block, result.RemainingHealth));
                    break;
                case MiningOutcome.Destroyed:
                    EventBus.Publish(new BlockDestroyedEvent(cell, center, result.Block));
                    break;
            }

            return result;
        }

        private void OnCellChanged(Vector2Int cell) => _view?.RenderCell(cell.x, cell.y);
    }
}
