using System;

namespace Common.Pools
{
    public interface IPoolable<T>
    {
        event Action<T> Used;

        bool Visible { set; }

        void MarkAsUsed();
    }
}