using Extensions;
using JetBrains.Annotations;
using Models;

namespace Modules.LevelManaging.Editing.EventRecording.AssetCueManaging
{
    /// <summary>
    ///     Responsible for setup asset's activation cue for new recording event
    /// </summary>
    [UsedImplicitly]
    internal sealed class ActivationCueManager
    {
        private readonly IAssetCueManager[] _assetCueManagers;
        private readonly IAudioCueManager _audioCueManager;

        public ActivationCueManager(IAssetCueManager[] assetCueManagers, IAudioCueManager audioCueManager)
        {
            _assetCueManagers = assetCueManagers;
            _audioCueManager = audioCueManager;
        }

        public void SetupActivationCues(Event recordingEvent)
        {
            foreach (var specificAssetCueManager in _assetCueManagers)
            {
                specificAssetCueManager.SetupActivationCues(recordingEvent);
            }
        }

        public void SetupEndCues(Event recordedEvent)
        {
            foreach (var specificAssetCueManager in _assetCueManagers)
            {
                specificAssetCueManager.SetupEndCues(recordedEvent);
            }
        }

        public int GetMusicActivationCue(Event recordingEvent, Level level)
        {
            var previousEvent = level.GetEventBefore(recordingEvent);
            return _audioCueManager.GetMusicActivationCue(recordingEvent, previousEvent);
        }
    }
}