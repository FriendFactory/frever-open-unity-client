using Extensions;
using UIManaging.Animated.Behaviours;
using UIManaging.Common.Buttons;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.PopupSystem.Popups.Share
{
    public sealed class ShareToPopup : BasePopup<ShareToPopupConfiguration>
    {
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _outsideButton;
        [SerializeField] private ShareVideoUrlButton _shareButton;
        [SerializeField] private ShareVideoButton _shareVideoButton;
        [SerializeField] private SaveVideoButton _saveVideoButton;
        [SerializeField] private SlideInOutBehaviour _slideInOut;
        [SerializeField] private ShareSelectionPresenter _shareSelectionPresenter;
        [SerializeField] private GameObject _bottomPanel;

        [Inject] private LocalUserDataHolder _localUser;
        
        private void OnEnable()
        {
            _closeButton.onClick.AddListener(Cancel);
            _outsideButton.onClick.AddListener(Cancel);
        }

        private void OnDisable()
        {
            _closeButton.onClick.RemoveListener(Cancel);
            _outsideButton.onClick.RemoveListener(Cancel);
        }

        protected override void OnConfigure(ShareToPopupConfiguration configuration)
        {
            _shareSelectionPresenter.Initialize(new ShareSelectionContext
            {
                Preselected = configuration.PreselectedDestinationData,
                CompleteButtonText = configuration.CompleteButtonText,
                OnConfirmed = configuration.OnConfirmed,
                BlockConfirmButtonIfNoSelection = configuration.BlockConfirmButtonIfNoSelection
            });
            
            _bottomPanel.SetActive(configuration.VideoId.HasValue);
            _shareButton.SetActive(configuration.VideoId.HasValue);
            _saveVideoButton.SetActive(configuration.OwnVideo && configuration.VideoId.HasValue);
            _shareVideoButton.SetActive(configuration.OwnVideo && configuration.VideoId.HasValue);
            
            if (configuration.VideoId.HasValue)
            {
                _shareButton.Initialize(new ShareVideoUrlButtonArgs(configuration.VideoId.Value));

                if (configuration.OwnVideo)
                {
                    _shareVideoButton.Initialize(new SaveVideoButtonArgs(configuration.VideoId.Value));
                    _saveVideoButton.Initialize(new SaveVideoButtonArgs(configuration.VideoId.Value));
                }
            }

            _slideInOut.SlideIn();
        }

        public override void Hide()
        {
            _slideInOut.SlideOut(() => base.Hide(null));
        }

        public override void Hide(object result)
        {
            _slideInOut.SlideOut(() => base.Hide(result));
        }

        protected override void OnHidden()
        {
            if (!_shareSelectionPresenter.IsInitialized) return;
            _shareSelectionPresenter.CleanUp();
            base.OnHidden();
        }

        private void Cancel()
        {
            Configs.OnCancelled?.Invoke();
            Hide();
        }
    }
}