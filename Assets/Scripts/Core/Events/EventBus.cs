using System;
using System.Collections.Generic;
using UnityEngine;

namespace DeepDigger.Core.Events
{
    /// <summary>
    /// Global, type-safe publish/subscribe hub that decouples systems: publishers never
    /// reference subscribers. Events are strongly typed via <see cref="IEvent"/>, so there is
    /// no string matching and no boxing when the payload is a <c>readonly struct</c>.
    /// </summary>
    /// <remarks>
    /// Intentionally static for zero-friction global access. State is reset on every play
    /// session through <see cref="ResetState"/> so it behaves correctly even when
    /// "Enter Play Mode Options" disables domain reload.
    /// </remarks>
    public static class EventBus
    {
        private static readonly Dictionary<Type, Delegate> Handlers = new();

        /// <summary>Registers <paramref name="handler"/> to receive events of type <typeparamref name="T"/>.</summary>
        public static void Subscribe<T>(Action<T> handler) where T : IEvent
        {
            if (handler == null) return;

            Handlers.TryGetValue(typeof(T), out Delegate existing);
            Handlers[typeof(T)] = (Action<T>)existing + handler;
        }

        /// <summary>Removes a previously registered <paramref name="handler"/>.</summary>
        public static void Unsubscribe<T>(Action<T> handler) where T : IEvent
        {
            if (handler == null) return;
            if (!Handlers.TryGetValue(typeof(T), out Delegate existing)) return;

            Delegate updated = (Action<T>)existing - handler;
            if (updated == null)
                Handlers.Remove(typeof(T));
            else
                Handlers[typeof(T)] = updated;
        }

        /// <summary>Dispatches <paramref name="evt"/> to every current subscriber of type <typeparamref name="T"/>.</summary>
        public static void Publish<T>(T evt) where T : IEvent
        {
            if (Handlers.TryGetValue(typeof(T), out Delegate existing))
                ((Action<T>)existing)?.Invoke(evt);
        }

        /// <summary>Removes every subscription. Exposed mainly for tests and scene teardown.</summary>
        public static void Clear() => Handlers.Clear();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetState() => Handlers.Clear();
    }
}
