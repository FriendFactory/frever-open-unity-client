using Extensions;
using UIManaging.Pages.Common.FavoriteSounds;

namespace UIManaging.PopupSystem.Popups.UsedVideoSounds
{
    class UsedVideoSoundPreviewPanel : SoundPreviewToggleBasedPanel
    {
        private void Awake()
        {
            PlaybackToggle.SetActive(false);
        }

        protected override void OnPlaybackChanged(bool isOn) { }
    }
}