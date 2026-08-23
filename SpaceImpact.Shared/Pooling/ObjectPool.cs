using System;
using System.Collections.Generic;

namespace SpaceImpact.Pooling
{
    /// <summary>
    /// Generic object pool (Object Pool pattern). Instances are created once
    /// and recycled, so gameplay produces zero garbage per frame.
    /// A free-set guards against double-release, which would otherwise hand the
    /// same instance out twice.
    /// </summary>
    public class ObjectPool<T> where T : class, IPoolable
    {
        private readonly Func<T> _factory;
        private readonly List<T> _all = new List<T>();
        private readonly Stack<T> _free = new Stack<T>();
        private readonly HashSet<T> _freeSet = new HashSet<T>();

        /// <summary>Every instance ever created (active and inactive).</summary>
        public IReadOnlyList<T> Items => _all;

        public ObjectPool(Func<T> factory, int preallocate = 0)
        {
            _factory = factory;
            for (int i = 0; i < preallocate; i++)
            {
                var item = _factory();
                _all.Add(item);
                PushFree(item);
            }
        }

        /// <summary>Check an instance out of the pool (creates one if empty).</summary>
        public T Obtain()
        {
            T item;
            if (_free.Count > 0)
            {
                item = _free.Pop();
                _freeSet.Remove(item);
            }
            else
            {
                item = _factory();
                _all.Add(item);
            }

            item.OnObtain();
            return item;
        }

        /// <summary>
        /// Return an instance to the pool. Safe to call on an instance that is
        /// already free — the redundant call is ignored.
        /// </summary>
        public void Release(T item)
        {
            if (item == null || _freeSet.Contains(item))
                return;

            // The free-set makes this exactly-once, so OnRelease can clean up
            // unconditionally (an entity that already deactivated itself still
            // needs its event handlers cleared).
            item.OnRelease();
            PushFree(item);
        }

        /// <summary>True if the instance is currently sitting in the pool.</summary>
        public bool IsFree(T item) => _freeSet.Contains(item);

        public void ReleaseAll()
        {
            _free.Clear();
            _freeSet.Clear();

            foreach (var item in _all)
            {
                item.OnRelease();
                PushFree(item);
            }
        }

        private void PushFree(T item)
        {
            _free.Push(item);
            _freeSet.Add(item);
        }

        public int CountActive
        {
            get
            {
                int n = 0;
                foreach (var item in _all)
                    if (item.Active)
                        n++;
                return n;
            }
        }
    }
}
