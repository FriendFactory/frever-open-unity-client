using Extensions;
using Models;
using Modules.LevelManaging.Assets;

namespace Modules.LevelManaging.Editing.Players.EventAssetsProviders
{
    internal interface IEventAssetsProvider
    {
        DbModelType TargetType { get; }
       
        IAsset[] GetLoadedAssets(Event ev);
    }
}