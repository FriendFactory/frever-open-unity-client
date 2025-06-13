using Bridge.Models.Common;
using Extensions;
using UIManaging.Pages.Common.Text;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.Common.SongOption.SongTitlePanel
{
    public sealed class SongSelectedPanel : MonoBehaviour
    {
        [SerializeField] private GameObject _previewSongGameObject;
        [SerializeField] private Image _scrollingTextBackground;
        [SerializeField] private OpenSongsMenuButton _songsMenuButton;
        [SerializeField] private ScrollableSongNamePanel _songNamePanel;
        [Header("Layout Settings")]
        [SerializeField] private HorizontalLayoutGroup _layoutGroup;
        [SerializeField] private int _playingRightPadding = 60;
        [SerializeField] private int _selectionRightPadding;

        public void DisplayPlayingUI()
        {
            // tasteless, but does the job 
            _layoutGroup.padding.right = _playingRightPadding;
            
            _previewSongGameObject.SetActive(false);
            _scrollingTextBackground.enabled = false;
            _songsMenuButton.SetInteractable(false);
        }

        public void DisplaySelectionUI()
        {
            _layoutGroup.padding.right = _selectionRightPadding;
            
            _previewSongGameObject.SetActive(true);
            _scrollingTextBackground.enabled = true;
            _songsMenuButton.SetInteractable(true);
        }

        public void UpdateSongTitle(IPlayableMusic current)
        {
            var title = current.GetName();
            
            SetSongTitleText(title);
        }

        private void SetSongTitleText(string title)
        {
            _songNamePanel.Initialize(new SongTextModel(title));
        }
    }
}