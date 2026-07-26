namespace DeepDigger.Gameplay.World
{
    /// <summary>Behavioural class of a block, orthogonal to its material identity (id/name).</summary>
    public enum BlockCategory
    {
        /// <summary>Plain diggable terrain (stone, dirt).</summary>
        Rock,

        /// <summary>Diggable block that yields a valuable resource.</summary>
        Ore,

        /// <summary>Diggable but harmful to touch (e.g. surrounds lava).</summary>
        Hazard,

        /// <summary>Cannot be mined by any tool (world border / bedrock).</summary>
        Indestructible
    }
}
