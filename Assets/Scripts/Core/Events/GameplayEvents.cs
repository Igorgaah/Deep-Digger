namespace DeepDigger.Core.Events
{
    /// <summary>Raised whenever the player's energy changes, so UI/audio can react without a hard reference.</summary>
    public readonly struct EnergyChangedEvent : IEvent
    {
        public readonly float Current;
        public readonly float Max;

        public EnergyChangedEvent(float current, float max)
        {
            Current = current;
            Max = max;
        }

        /// <summary>Normalized value in the [0, 1] range, safe against a zero maximum.</summary>
        public float Normalized => Max > 0f ? Current / Max : 0f;
    }

    /// <summary>
    /// Fire-and-forget request for a camera shake. Any system (combat, mining, explosions)
    /// can publish it without referencing the camera.
    /// </summary>
    public readonly struct CameraShakeRequest : IEvent
    {
        public readonly float Duration;
        public readonly float Magnitude;

        public CameraShakeRequest(float duration, float magnitude)
        {
            Duration = duration;
            Magnitude = magnitude;
        }
    }
}
