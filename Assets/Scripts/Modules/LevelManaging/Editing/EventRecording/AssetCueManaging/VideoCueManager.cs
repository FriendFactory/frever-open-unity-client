using Extensions;
using Models;
using Modules.LevelManaging.Editing.EventRecording.AssetCueManaging.CueProviders;

namespace Modules.LevelManaging.Editing.EventRecording.AssetCueManaging
{
    internal sealed class VideoCueManager: IAssetCueManager
    {
        private readonly VideoCueProvider _cueProvider;

        public VideoCueManager(VideoCueProvider cueProvider)
        {
            _cueProvider = cueProvider;
        }

        public void SetupActivationCues(Event recordingEvent)
        {
            var cue = _cueProvider.GetActivationCue(recordingEvent);
            var controller = recordingEvent.GetSetLocationController();
            controller.VideoActivationCue = cue;
        }

        public void SetupEndCues(Event recorded)
        {
            var endCue = _cueProvider.GetEndCue(recorded);
           
            var setLocationController = recorded.GetSetLocationController();
            setLocationController.VideoEndCue = endCue;
        }
    }
}