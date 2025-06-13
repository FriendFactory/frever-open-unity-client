using Bridge.Models.ClientServer.Level.Shuffle;

namespace Modules.LevelManaging.Editing.LevelManagement
{
    public class AIShuffleModel : ShuffleModel
    {
        public string Prompt { get; }

        public AIShuffleModel(ShuffleAssets assets, string prompt) : base(assets)
        {
            Prompt = prompt;
        }
    }
}