// Implements: ADR-002 — Generic ObjectPool<T> (Tier 0)
// Provides allocation-free object reuse for ProjectileController, XPGem, etc.
// Factory delegate supplied at construction; pool grows on demand (no fixed cap).
// Callers must call Return() when done — pool does not auto-reclaim.

using System;
using System.Collections.Generic;
using UnityEngine;
namespace Synthborn.Core
{
    /// <summary>
    /// Generic object pool that calls <see cref="IPoolable.OnPoolGet"/> and
    /// <see cref="IPoolable.OnPoolReturn"/> on each borrow/return cycle.
    /// </summary>
    /// <typeparam name="T">A <see cref="MonoBehaviour"/> that implements <see cref="IPoolable"/>.</typeparam>
    public class ObjectPool<T> where T : MonoBehaviour, IPoolable
    {
        private readonly Stack<T> _pool;
        private readonly Func<T> _factory;
        /// <param name="factory">Called when the pool is empty and a new instance is needed.</param>
        /// <param name="initialCapacity">Pre-warms the pool with this many instances.</param>
        public ObjectPool(Func<T> factory, int initialCapacity = 0)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _pool = new Stack<T>(initialCapacity);
            for (int i = 0; i < initialCapacity; i++)
            {
                T instance = _factory();
                instance.gameObject.SetActive(false);
                _pool.Push(instance);
            }
        }
        /// <summary>
        /// Returns an active instance from the pool, creating one if necessary.
        /// <see cref="IPoolable.OnPoolGet"/> is called before returning.
        /// </summary>
        public T Get()
            T instance = _pool.Count > 0 ? _pool.Pop() : _factory();
            instance.gameObject.SetActive(true);
            instance.OnPoolGet();
            return instance;
        /// Returns an instance to the pool and deactivates its GameObject.
        /// <see cref="IPoolable.OnPoolReturn"/> is called before deactivation.
        public void Return(T instance)
            if (instance == null) return;
            instance.OnPoolReturn();
            instance.gameObject.SetActive(false);
            _pool.Push(instance);
        /// <summary>Number of available (inactive) instances.</summary>
        public int AvailableCount => _pool.Count;
    }
}
