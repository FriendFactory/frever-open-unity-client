using Modules.LevelManaging.Assets;

namespace Modules.LevelManaging.Editing.Players.AssetPlayers
{
    public abstract class GenericTimeDependAssetPlayer<T> : AssetPlayerBase<T>, ITimeDependAssetPlayer where T: IAsset
    {
        protected float StartTime;

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void SetStartTime(float startTime)
        {
            StartTime = startTime;
        }
    }
}
