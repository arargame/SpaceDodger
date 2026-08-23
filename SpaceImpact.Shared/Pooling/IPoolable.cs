namespace SpaceImpact.Pooling
{
    /// <summary>Contract for objects managed by an <see cref="ObjectPool{T}"/>.</summary>
    public interface IPoolable
    {
        /// <summary>True while the object is in use (checked out of the pool).</summary>
        bool Active { get; }

        /// <summary>Called when the object is checked out; reset state here.</summary>
        void OnObtain();

        /// <summary>Called when the object is returned to the pool.</summary>
        void OnRelease();
    }
}
