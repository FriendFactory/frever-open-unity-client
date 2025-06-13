using Bridge.Models.VideoServer;
using Common.Abstract;
using Extensions;
using UIManaging.Common.Args.Views.Profile;
using UIManaging.Pages.Common.VideoDetails;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.RatingFeed.Rating
{
    internal sealed class RatingVideoDetailsPanel : BaseContextPanel<Video>
    {
        [SerializeField] private RectTransform _userDetailsRectTransform;
        [SerializeField] private NicknameView _nicknameView;
        [SerializeField] private RatingFeedFollowButton _followButton;
        [SerializeField] private VideoSongPanel _musicPanel;

        protected override void OnInitialized()
        {
            _nicknameView.Initialize(new NicknameModel
            {
                Nickname = ContextData.Owner.Nickname
            });
            _followButton.Initialize(ContextData);

            InitializeMusicPanel();

            LayoutRebuilder.ForceRebuildLayoutImmediate(_userDetailsRectTransform);
        }

        protected override void BeforeCleanUp()
        {
            _nicknameView.CleanUp();
            _followButton.CleanUp();

            if (_musicPanel.IsInitialized)
            {
                _musicPanel.CleanUp();
            }
        }

        private void InitializeMusicPanel()
        {
            var containsMusic = !string.IsNullOrEmpty(ContextData.SongName);

            _musicPanel.SetActive(containsMusic);

            if (!containsMusic) return;

            _musicPanel.Initialize(ContextData);
            _musicPanel.StartScrolling();
        }
    }
}