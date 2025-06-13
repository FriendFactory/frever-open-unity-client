using System;

namespace Modules.LevelManaging.Assets
{
    public interface IUnloadableAsset
    {
        void OnUnloadStarted();
        event Action UnloadStarted;
    }
}