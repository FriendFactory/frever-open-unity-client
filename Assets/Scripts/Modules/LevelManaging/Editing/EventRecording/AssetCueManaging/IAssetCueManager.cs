using Models;

namespace Modules.LevelManaging.Editing.EventRecording.AssetCueManaging
{
    internal interface IAssetCueManager
    {
        void SetupActivationCues(Event recording);
        void SetupEndCues(Event recorded);
    }
}