using System;
using System.Collections.Generic;
using UnityEngine;

namespace Synthborn.Core
{
    /// <summary>
    /// Generic object pool. Calls IPoolable.OnPoolGet/OnPoolReturn on each cycle.
    /// </summary>
    public class ObjectPool<T> where T : MonoBehaviour, IPoolable
    {
        private readonly Stack<T> _pool;
        private readonly Func<T> _factory;

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

        /// <summary>Get an active instance from the pool, creating one if needed.</summary>
        public T Get(Vector2 position)
        {
            T instance = _pool.Count > 0 ? _pool.Pop() : _factory();
            instance.transform.position = position;
            instance.gameObject.SetActive(true);
            instance.OnPoolGet();
            return instance;
        }

        /// <summary>Get without position.</summary>
        public T Get()
        {
            T instance = _pool.Count > 0 ? _pool.Pop() : _factory();
            instance.gameObject.SetActive(true);
            instance.OnPoolGet();
            return instance;
        }

        /// <summary>Return an instance to the pool.</summary>
        public void Return(T instance)
        {
            if (instance == null) return;
            instance.OnPoolReturn();
            instance.gameObject.SetActive(false);
            _pool.Push(instance);
        }

        /// <summary>Number of available instances.</summary>
        public int AvailableCount => _pool.Count;
    }
}
