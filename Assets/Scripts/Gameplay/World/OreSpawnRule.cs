using System;
using UnityEngine;

namespace DeepDigger.Gameplay.World
{
    /// <summary>
    /// One depth-banded ore distribution rule. The generator places <see cref="Block"/> in solid rock
    /// wherever the cell's normalized depth is inside [<see cref="minDepth"/>, <see cref="maxDepth"/>]
    /// and a Perlin sample exceeds <see cref="threshold"/> (higher threshold ⇒ rarer, smaller veins).
    /// Rules are evaluated in list order, so put the rarest/deepest ores first.
    /// </summary>
    [Serializable]
    public sealed class OreSpawnRule
    {
        [Tooltip("Apenas para leitura no inspector.")]
        public string label = "ore";

        public BlockDefinition block;

        [Header("Faixa de profundidade (0 = topo, 1 = fundo)")]
        [Range(0f, 1f)] public float minDepth;
        [Range(0f, 1f)] public float maxDepth = 1f;

        [Header("Ruído (veios)")]
        [Tooltip("Escala do ruído: menor = veios maiores.")]
        [Min(0.001f)] public float noiseScale = 0.14f;
        [Tooltip("Limiar de aparição: maior = mais raro.")]
        [Range(0f, 1f)] public float threshold = 0.62f;

        public bool DepthInBand(float depth01) => depth01 >= minDepth && depth01 <= maxDepth;
    }
}
