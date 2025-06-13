using Bridge.Models.Common;

namespace UIManaging.Pages.Common.SongOption.MusicCue
{
    public sealed class SoundPreviewInfoPanelModel
    {
        public IPlayableMusic Sound { get; }

        public SoundPreviewInfoPanelModel(IPlayableMusic sound)
        {
            Sound = sound;
        }
    }
}