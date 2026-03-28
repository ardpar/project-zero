// Implements: ADR-002 — ObjectPool interface contract (Tier 0)
// All pooled objects must implement this interface. Called by ObjectPool<T> on
// Get() and Return(). Implementors must reset all runtime state in OnPoolGet().

namespace Synthborn.Core
{
    /// <summary>
    /// Contract for any object managed by ObjectPool&lt;T&gt;.
    /// </summary>
    public interface IPoolable
    {
        /// <summary>Called immediately after the object is taken from the pool.</summary>
        void OnPoolGet();
        /// <summary>Called immediately before the object is returned to the pool.</summary>
        void OnPoolReturn();
    }
}
