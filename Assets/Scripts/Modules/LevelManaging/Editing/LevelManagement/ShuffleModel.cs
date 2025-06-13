using Bridge.Models.ClientServer.Level.Shuffle;

namespace Modules.LevelManaging.Editing.LevelManagement
{
    public class ShuffleModel
    {
        public ShuffleModel(ShuffleAssets assets)
        {
            Assets = assets;
        }

        public ShuffleAssets Assets { get; }
    }
}