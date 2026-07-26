using UnityEngine;

namespace DeepDigger.Core.Interfaces
{
    /// <summary>
    /// Anything that can receive damage: players, enemies and destructible props alike.
    /// Combat code depends on this abstraction instead of concrete types.
    /// </summary>
    public interface IDamageable
    {
        bool IsAlive { get; }

        /// <summary>Applies <paramref name="amount"/> of damage coming from <paramref name="hitDirection"/> (used for knockback).</summary>
        void TakeDamage(float amount, Vector2 hitDirection);
    }
}
