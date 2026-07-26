using System.Collections.Generic;
using UnityEngine;

namespace DeepDigger.Gameplay.World
{
    /// <summary>
    /// Produces a unique, mostly-solid mine every run by combining three techniques (see ADR-008):
    /// <list type="bullet">
    /// <item>organic caverns via <b>Cellular Automata</b>, <b>gated by a low-frequency Perlin mask</b>
    /// so caves stay in pockets and the mine remains diggable;</item>
    /// <item>depth-banded <b>ore veins</b> from per-rule Perlin noise (<see cref="OreSpawnRule"/>);</item>
    /// <item>a <b>connectivity pass</b> (flood-fill + carved corridors) that guarantees every open
    /// region is reachable on foot — never an impossible map.</item>
    /// </list>
    /// Plugs into the existing <see cref="WorldGeneratorSO"/> seam, so nothing else in the game changes.
    /// </summary>
    [CreateAssetMenu(fileName = "ProceduralWorldGenerator", menuName = "Deep Digger/World/Generators/Procedural")]
    public sealed class ProceduralWorldGenerator : WorldGeneratorSO
    {
        [Header("Blocos base")]
        [SerializeField] private BlockDefinition baseRock;
        [Tooltip("Borda indestrutível ao redor da mina (opcional).")]
        [SerializeField] private BlockDefinition borderBlock;

        [Header("Cavernas (Cellular Automata + máscara Perlin)")]
        [Tooltip("Escala da máscara de região: menor = regiões de caverna maiores.")]
        [SerializeField, Min(0.001f)] private float caveRegionScale = 0.06f;
        [Tooltip("Acima deste valor da máscara, a região pode ter cavernas (maior = menos cavernas).")]
        [SerializeField, Range(0f, 1f)] private float caveRegionThreshold = 0.55f;
        [Tooltip("Chance inicial de célula aberta dentro de uma região de caverna.")]
        [SerializeField, Range(0f, 1f)] private float caveFillProbability = 0.46f;
        [SerializeField, Range(0, 8)] private int caveSmoothingSteps = 4;

        [Header("Bôlsão inicial")]
        [SerializeField, Min(2)] private int pocketWidth = 6;
        [SerializeField, Min(2)] private int pocketHeight = 4;

        [Header("Salas / Ruínas")]
        [SerializeField, Min(0)] private int roomCount = 6;
        [SerializeField, Min(2)] private int minRoomSize = 4;
        [SerializeField, Min(3)] private int maxRoomSize = 9;

        [Header("Minérios (avaliados em ordem: mais raros primeiro)")]
        [SerializeField] private List<OreSpawnRule> oreRules = new();

        [Header("Features (marcadores para fases futuras)")]
        [SerializeField, Min(0)] private int chestCount = 4;
        [SerializeField, Min(0)] private int eventCount = 3;

        // Noise offsets, randomized per generation for uniqueness.
        private Vector2 _caveOffset;
        private Vector2[] _oreOffsets;

        // 4-neighbour directions for flood fill.
        private static readonly int[] DirX = { 1, -1, 0, 0 };
        private static readonly int[] DirY = { 0, 0, 1, -1 };

        protected override void Populate(WorldGrid grid, System.Random random)
        {
            if (baseRock == null)
            {
                Debug.LogError($"{name}: 'baseRock' não atribuído — a mina não pode ser gerada.");
                return;
            }

            PrepareNoise(random);
            FillSolid(grid);
            GenerateCaves(grid, random);
            CarveStartingPocket(grid);
            CarveRooms(grid, random);
            EnsureConnectivity(grid);
            ScatterOres(grid);
            PlaceFeatures(grid, random);
        }

        private void PrepareNoise(System.Random random)
        {
            _caveOffset = RandomOffset(random);
            _oreOffsets = new Vector2[oreRules.Count];
            for (int i = 0; i < _oreOffsets.Length; i++)
                _oreOffsets[i] = RandomOffset(random);
        }

        private static Vector2 RandomOffset(System.Random random) =>
            new((float)(random.NextDouble() * 10000.0), (float)(random.NextDouble() * 10000.0));

        private void FillSolid(WorldGrid grid)
        {
            for (int y = 0; y < grid.Height; y++)
            for (int x = 0; x < grid.Width; x++)
            {
                bool isBorder = x == 0 || y == 0 || x == grid.Width - 1 || y == grid.Height - 1;
                grid.SetBlock(x, y, isBorder && borderBlock != null ? borderBlock : baseRock);
            }
        }

        // ----- Caverns: Perlin-gated Cellular Automata --------------------------------------------

        private void GenerateCaves(WorldGrid grid, System.Random random)
        {
            int w = grid.Width, h = grid.Height;
            bool[] open = new bool[w * h];

            for (int y = 1; y < h - 1; y++)
            for (int x = 1; x < w - 1; x++)
            {
                float mask = Mathf.PerlinNoise((x + _caveOffset.x) * caveRegionScale, (y + _caveOffset.y) * caveRegionScale);
                bool canCave = mask > caveRegionThreshold;
                open[y * w + x] = canCave && random.NextDouble() < caveFillProbability;
            }

            for (int step = 0; step < caveSmoothingSteps; step++)
                open = SmoothCaves(open, w, h);

            for (int y = 1; y < h - 1; y++)
            for (int x = 1; x < w - 1; x++)
                if (open[y * w + x]) grid.Clear(x, y);
        }

        private static bool[] SmoothCaves(bool[] open, int w, int h)
        {
            bool[] next = new bool[w * h];
            for (int y = 1; y < h - 1; y++)
            for (int x = 1; x < w - 1; x++)
            {
                int walls = CountSolidNeighbors(open, x, y, w, h);
                next[y * w + x] = walls >= 5 ? false : walls <= 3 ? true : open[y * w + x];
            }
            return next;
        }

        private static int CountSolidNeighbors(bool[] open, int x, int y, int w, int h)
        {
            int count = 0;
            for (int dy = -1; dy <= 1; dy++)
            for (int dx = -1; dx <= 1; dx++)
            {
                if (dx == 0 && dy == 0) continue;
                int nx = x + dx, ny = y + dy;
                if (nx < 0 || ny < 0 || nx >= w || ny >= h || !open[ny * w + nx]) count++;
            }
            return count;
        }

        // ----- Pocket & rooms ---------------------------------------------------------------------

        private void CarveStartingPocket(WorldGrid grid)
        {
            int centerX = grid.Width / 2;
            int topY = grid.Height - 2;
            int halfW = pocketWidth / 2;

            for (int y = topY; y > topY - pocketHeight && y > 0; y--)
            for (int x = centerX - halfW; x <= centerX + halfW; x++)
            {
                if (x <= 0 || x >= grid.Width - 1) continue;
                grid.Clear(x, y);
            }

            grid.SpawnCell = new Vector2Int(centerX, topY - 1);
        }

        private void CarveRooms(WorldGrid grid, System.Random random)
        {
            int placed = 0;
            int maxAttempts = roomCount * 4;
            int floorLimit = Mathf.RoundToInt(grid.Height * 0.75f); // ruins live in the lower part

            for (int attempt = 0; attempt < maxAttempts && placed < roomCount; attempt++)
            {
                int upper = Mathf.Max(minRoomSize, maxRoomSize) + 1;
                int rw = random.Next(minRoomSize, upper);
                int rh = random.Next(minRoomSize, upper);
                int x = random.Next(2, Mathf.Max(3, grid.Width - rw - 2));
                int y = random.Next(2, Mathf.Max(3, floorLimit - rh));

                for (int yy = y; yy < y + rh && yy < grid.Height - 1; yy++)
                for (int xx = x; xx < x + rw && xx < grid.Width - 1; xx++)
                    grid.Clear(xx, yy);

                grid.Features.Add(new WorldFeature(new Vector2Int(x + rw / 2, y + rh / 2), WorldFeatureType.Ruin));
                placed++;
            }
        }

        // ----- Connectivity: connect every open region to the spawn region ------------------------

        private void EnsureConnectivity(WorldGrid grid)
        {
            int w = grid.Width, h = grid.Height;
            int[] region = new int[w * h];
            for (int i = 0; i < region.Length; i++) region[i] = -1;

            var representatives = new List<Vector2Int>();
            int regionCount = 0;
            Vector2Int spawn = grid.SpawnCell;

            for (int y = 1; y < h - 1; y++)
            for (int x = 1; x < w - 1; x++)
            {
                if (grid.IsSolid(x, y) || region[y * w + x] != -1) continue;

                Vector2Int rep = FloodFillRegion(grid, region, new Vector2Int(x, y), regionCount, spawn);
                representatives.Add(rep);
                regionCount++;
            }

            if (regionCount <= 1) return;

            int spawnRegion = grid.IsSolid(spawn.x, spawn.y) ? -1 : region[spawn.y * w + spawn.x];

            for (int r = 0; r < regionCount; r++)
            {
                if (r == spawnRegion) continue;
                CarveCorridor(grid, representatives[r], spawn);
            }
        }

        /// <summary>BFS over open cells; returns the region cell closest to <paramref name="spawn"/>.</summary>
        private static Vector2Int FloodFillRegion(WorldGrid grid, int[] region, Vector2Int start, int id, Vector2Int spawn)
        {
            int w = grid.Width;
            var queue = new Queue<Vector2Int>();
            queue.Enqueue(start);
            region[start.y * w + start.x] = id;

            Vector2Int best = start;
            int bestDist = ManhattanDistance(start, spawn);

            while (queue.Count > 0)
            {
                Vector2Int c = queue.Dequeue();
                for (int k = 0; k < 4; k++)
                {
                    int nx = c.x + DirX[k], ny = c.y + DirY[k];
                    if (!grid.InBounds(nx, ny) || grid.IsSolid(nx, ny)) continue;
                    if (region[ny * w + nx] != -1) continue;

                    region[ny * w + nx] = id;
                    var n = new Vector2Int(nx, ny);
                    int dist = ManhattanDistance(n, spawn);
                    if (dist < bestDist) { bestDist = dist; best = n; }
                    queue.Enqueue(n);
                }
            }

            return best;
        }

        private static void CarveCorridor(WorldGrid grid, Vector2Int from, Vector2Int to)
        {
            int x = from.x, y = from.y;

            int stepX = to.x > x ? 1 : -1;
            while (x != to.x) { ClearInterior(grid, x, y); x += stepX; }

            int stepY = to.y > y ? 1 : -1;
            while (y != to.y) { ClearInterior(grid, x, y); y += stepY; }

            ClearInterior(grid, to.x, to.y);
        }

        private static void ClearInterior(WorldGrid grid, int x, int y)
        {
            if (x <= 0 || y <= 0 || x >= grid.Width - 1 || y >= grid.Height - 1) return;
            grid.Clear(x, y);
        }

        // ----- Ores & features --------------------------------------------------------------------

        private void ScatterOres(WorldGrid grid)
        {
            if (oreRules.Count == 0) return;

            int maxDepthIndex = Mathf.Max(1, grid.Height - 1);

            for (int y = 1; y < grid.Height - 1; y++)
            for (int x = 1; x < grid.Width - 1; x++)
            {
                if (!ReferenceEquals(grid.GetBlock(x, y), baseRock)) continue; // only plain rock

                float depth01 = (float)(maxDepthIndex - y) / maxDepthIndex;

                for (int i = 0; i < oreRules.Count; i++)
                {
                    OreSpawnRule rule = oreRules[i];
                    if (rule.block == null || !rule.DepthInBand(depth01)) continue;

                    float n = Mathf.PerlinNoise((x + _oreOffsets[i].x) * rule.noiseScale, (y + _oreOffsets[i].y) * rule.noiseScale);
                    if (n > rule.threshold)
                    {
                        grid.SetBlock(x, y, rule.block);
                        break;
                    }
                }
            }
        }

        private void PlaceFeatures(WorldGrid grid, System.Random random)
        {
            var openCells = new List<Vector2Int>();
            for (int y = 1; y < grid.Height - 1; y++)
            for (int x = 1; x < grid.Width - 1; x++)
                if (!grid.IsSolid(x, y)) openCells.Add(new Vector2Int(x, y));

            if (openCells.Count == 0) return;

            AddRandomFeatures(grid, random, openCells, chestCount, WorldFeatureType.Chest);
            AddRandomFeatures(grid, random, openCells, eventCount, WorldFeatureType.Event);
        }

        private static void AddRandomFeatures(WorldGrid grid, System.Random random, List<Vector2Int> openCells, int count, WorldFeatureType type)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2Int cell = openCells[random.Next(openCells.Count)];
                grid.Features.Add(new WorldFeature(cell, type));
            }
        }

        private static int ManhattanDistance(Vector2Int a, Vector2Int b) =>
            Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }
}
