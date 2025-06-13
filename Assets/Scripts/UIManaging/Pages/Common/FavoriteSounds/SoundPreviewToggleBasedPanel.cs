using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.Common.FavoriteSounds
{
    public abstract class SoundPreviewToggleBasedPanel: SoundPreviewPanelBase
    {
        [SerializeField] private Toggle _playbackToggle;

        protected Toggle PlaybackToggle => _playbackToggle;
        
        protected override void OnInitialized()
        {
            base.OnInitialized();
            
            _playbackToggle.onValueChanged.AddListener(OnPlaybackChanged);
        }

        protected override void BeforeCleanUp()
        {
            base.BeforeCleanUp();

            _playbackToggle.isOn = false;
            
            _playbackToggle.onValueChanged.RemoveListener(OnPlaybackChanged);
        }

        protected abstract void OnPlaybackChanged(bool isOn);
    }
}