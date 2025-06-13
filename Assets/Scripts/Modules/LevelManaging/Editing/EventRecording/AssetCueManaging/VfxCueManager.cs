using Extensions;
using Models;
using Modules.LevelManaging.Editing.EventRecording.AssetCueManaging.CueProviders;

namespace Modules.LevelManaging.Editing.EventRecording.AssetCueManaging
{
    internal sealed class VfxCueManager: IAssetCueManager
    {
        private readonly VfxCueProvider _cueProvider;

        public VfxCueManager(VfxCueProvider cueProvider)
        {
            _cueProvider = cueProvider;
        }

        public void SetupActivationCues(Event recordingEvent)
        {
            var vfxController = recordingEvent.GetVfxController();
            var hasVfx = vfxController != null;
            if (!hasVfx) return;

            var vfxActivationCue = _cueProvider.GetActivationCue(recordingEvent);
            vfxController.ActivationCue = vfxActivationCue;
        }

        public void SetupEndCues(Event recorded)
        {
            var vfxController = recorded.GetVfxController();
            var hasVfx = vfxController != null;
            if (!hasVfx) return;
            
            var endCue = _cueProvider.GetEndCue(recorded);
            vfxController.EndCue = endCue;
        }
    }
}