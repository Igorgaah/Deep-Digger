namespace DeepDigger.Core.Events
{
    /// <summary>
    /// Marker interface for every message that travels through the <see cref="EventBus"/>.
    /// Implement it on small, immutable <c>readonly struct</c> payloads to keep the bus allocation-free.
    /// </summary>
    public interface IEvent { }
}
