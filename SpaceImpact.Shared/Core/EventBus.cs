using System;
using System.Collections.Generic;

namespace SpaceImpact.Core
{
    /// <summary>
    /// Minimal publish/subscribe event bus (Observer pattern).
    /// Decouples gameplay systems: publishers never know their listeners.
    /// </summary>
    public sealed class EventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _handlers = new Dictionary<Type, List<Delegate>>();

        public void Subscribe<T>(Action<T> handler) where T : struct
        {
            if (!_handlers.TryGetValue(typeof(T), out var list))
            {
                list = new List<Delegate>();
                _handlers[typeof(T)] = list;
            }
            list.Add(handler);
        }

        public void Unsubscribe<T>(Action<T> handler) where T : struct
        {
            if (_handlers.TryGetValue(typeof(T), out var list))
                list.Remove(handler);
        }

        public void Publish<T>(T evt) where T : struct
        {
            if (!_handlers.TryGetValue(typeof(T), out var list))
                return;

            // Copy so handlers may subscribe/unsubscribe while dispatching.
            var snapshot = list.ToArray();
            foreach (var d in snapshot)
                ((Action<T>)d)(evt);
        }
    }
}
