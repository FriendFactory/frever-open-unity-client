using Bridge.Models.Common;
using Common.Abstract;
using Extensions;
using TMPro;
using UnityEngine;

namespace UIManaging.Pages.Common.FavoriteSounds
{
    public class SoundInfoPanel: BaseContextPanel<IPlayableMusic>
    {
        [SerializeField] private TMP_Text _songName;
        [SerializeField] private TMP_Text _artistName;
        [SerializeField] private TMP_Text _duration;
        
        protected override void OnInitialized()
        {
            UpdateSoundInfo(ContextData);
        }

        protected override void BeforeCleanUp()
        {
            _artistName.text = string.Empty;
            _songName.text = string.Empty;
            _duration.text = string.Empty;
        }

        private void UpdateSoundInfo(IPlayableMusic sound)
        {
            _artistName.text = sound.GetArtistName();
            _songName.text = sound.GetSoundName();
            _duration.text = sound.GetDurationFormatted();
        }
    }
}