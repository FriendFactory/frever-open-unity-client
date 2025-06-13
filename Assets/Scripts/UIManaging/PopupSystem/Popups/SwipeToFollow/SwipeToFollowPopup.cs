using System.Collections.Generic;
using Bridge;
using Bridge.Models.VideoServer;
using BrunoMikoski.AnimationSequencer;
using Navigation.Core;
using TMPro;
using UIManaging.Pages.Common.FollowersManagement;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.PopupSystem.Popups.SwipeToFollow
{
    public class SwipeToFollowPopup : BasePopup<SwipeToFollowPopupConfiguration>
    {
        [SerializeField] private TMP_Text _headerText;
        [SerializeField] private SwipeToFollowCardsController _cardsController;
        [Header("Buttons")]
        [SerializeField] private Button _closeButton;
        [Header("Animations")]
        [SerializeField] private AnimationSequencerController _swipeAnimationSequencer;

        [Inject] private PageManager _pageManager;

        private List<GroupInfo> _users;
        private int _usersCount;
        private int _currentUserIndex;
            
        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void OnEnable()
        {
            _closeButton.onClick.AddListener(OnCloseButtonClick);
        }

        private void OnDisable()
        {
            _closeButton.onClick.RemoveListener(OnCloseButtonClick);
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        public override void Show()
        {
            base.Show();
            
            _swipeAnimationSequencer.PlayForward();
        }

        public override void Hide(object result)
        {
            // we need to perform page switch before closing the popup
            Configs.OnClose?.Invoke(null);
            Configs.OnClose = null;
            
            _pageManager.PageDisplayed += OnPageDisplayed;
            
            void OnPageDisplayed(PageData pageData)
            {
                _pageManager.PageDisplayed -= OnPageDisplayed;
                
                _swipeAnimationSequencer.PlayBackwards(false, () => base.Hide(result));
            }
        }

        protected override void OnConfigure(SwipeToFollowPopupConfiguration config)
        {
            _usersCount = config.Profiles.Count;
            _cardsController.Initialize(config.Profiles);

            _cardsController.CurrentUserIndex.OnChanged += OnProgressChanged;
            _cardsController.Completed += OnCloseButtonClick;
            
            UpdateTitleText();
            OnProgressChanged();
        }

        protected override void OnHidden()
        {
            _cardsController.CurrentUserIndex.OnChanged -= OnProgressChanged;
            _cardsController.Completed -= OnCloseButtonClick;
            
            _cardsController.CleanUp();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void OnProgressChanged()
        {
            _currentUserIndex = _cardsController.CurrentUserIndex.Value;
            
            UpdateTitleText();
        }

        private void UpdateTitleText()
        {
            var progressCount = Mathf.Min(_currentUserIndex + 1, _usersCount);
            _headerText.text = $"{progressCount} of {_usersCount}";
        }

        private void OnCloseButtonClick()
        {
            Hide(true);
        }
    }
}