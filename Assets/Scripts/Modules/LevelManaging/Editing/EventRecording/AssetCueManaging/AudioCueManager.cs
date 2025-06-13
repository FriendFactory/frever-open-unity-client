using System;
using Extensions;
using JetBrains.Annotations;
using Models;
using Modules.LevelManaging.Editing.EventRecording.AssetCueManaging.CueProviders;

namespace Modules.LevelManaging.Editing.EventRecording.AssetCueManaging
{
    internal interface IAudioCueManager : IAssetCueManager
    {
        int GetMusicActivationCue(Event recordingEvent, Event previousEvent);
    }

    [UsedImplicitly]
    internal sealed class AudioCueManager: IAudioCueManager
    {
        private readonly MusicCueProvider _cueProvider;

        public AudioCueManager(MusicCueProvider cueProvider)
        {
            _cueProvider = cueProvider;
        }

        public void SetupActivationCues(Event recordingEvent)
        {
            if (!recordingEvent.HasMusic()) return;
            recordingEvent.SetMusicActivationCue(_cueProvider.GetActivationCue(recordingEvent));
        }

        public void SetupEndCues(Event recorded)
        {
            if (!recorded.HasMusic()) return;
            recorded.SetMusicEndCue(_cueProvider.GetEndCue(recorded));
        }

        public int GetMusicActivationCue(Event recordingEvent, Event previousEvent)
        {
            if (!recordingEvent.HasMusic()) throw new InvalidOperationException();
            return _cueProvider.GetActivationCue(recordingEvent);
        }
    }
}
