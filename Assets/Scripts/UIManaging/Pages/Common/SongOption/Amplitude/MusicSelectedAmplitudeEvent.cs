using System;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.Common;
using Modules.Amplitude;
using Modules.Amplitude.Events.Core;

namespace UIManaging.Pages.Common.SongOption.Amplitude
{
    internal sealed class MusicSelectedAmplitudeEvent: BaseAmplitudeEvent
    {
        public override string Name { get; }
        
        public MusicSelectedAmplitudeEvent(IPlayableMusic playableMusic, float loadingTime)
        {
            Name = GetName(playableMusic);
            
            _eventProperties.Add(GetIdPropertyName(playableMusic), playableMusic.Id);
            _eventProperties.Add(AmplitudeEventConstants.EventProperties.LOADING_TIME, loadingTime);
        }
        
        private string GetName(IPlayableMusic playableMusic)
        {
            return playableMusic switch
            {
                SongInfo => AmplitudeEventConstants.EventNames.SONG_SELECTED,
                UserSoundFullInfo => "sound_selected",
                // external track has its own event
                _ => throw new NotImplementedException()
            };
        }
        
        private string GetIdPropertyName(IPlayableMusic playableMusic)
        {
            return playableMusic switch
            {
                SongInfo => AmplitudeEventConstants.EventProperties.SONG_ID,
                UserSoundFullInfo => "Sound ID",
                _ => throw new NotImplementedException()
            };
        }
    }
}