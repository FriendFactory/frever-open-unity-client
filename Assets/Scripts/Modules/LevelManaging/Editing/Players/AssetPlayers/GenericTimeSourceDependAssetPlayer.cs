using Common.TimeManaging;
using Modules.LevelManaging.Assets;

namespace Modules.LevelManaging.Editing.Players.AssetPlayers
{
    internal abstract class GenericTimeSourceDependAssetPlayer<T> : AssetPlayerBase<T> where T: IAsset
    {
        protected ITimeSource TimeSource;
        
        public void SetTimeSource(ITimeSource source)
        {
            TimeSource = source;
        }
    }
}
