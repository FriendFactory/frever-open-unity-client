using Bridge.Models.Common;
using StansAssets.Foundation.Patterns;

namespace UIManaging.Pages.Common.SongOption.Common
{
    public class SongSelectedEvent: IEvent
    {
        public IPlayableMusic Playable { get; }
        public float ActivationCue { get; }

        public SongSelectedEvent(IPlayableMusic playable, float activationCue = 0f)
        {
            Playable = playable;
            ActivationCue = activationCue;
        }
    }
}