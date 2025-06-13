using Extensions;
using Models;
using Modules.LevelManaging.Editing.EventRecording.AssetCueManaging.CueProviders;

namespace Modules.LevelManaging.Editing.EventRecording.AssetCueManaging
{
    internal sealed class SetLocationCueManager: IAssetCueManager
    {
        private readonly SetLocationCueProvider _cueProvider;

        public SetLocationCueManager(SetLocationCueProvider cueProvider)
        {
            _cueProvider = cueProvider;
        }

        public void SetupActivationCues(Event recording)
        {
            var setLocationController = recording.GetSetLocationController();
            setLocationController.ActivationCue = _cueProvider.GetActivationCue(recording);
        }

        public void SetupEndCues(Event recorded)
        {
            var setLocationController = recorded.GetSetLocationController();
            setLocationController.EndCue = _cueProvider.GetEndCue(recorded);
        }
    }
}