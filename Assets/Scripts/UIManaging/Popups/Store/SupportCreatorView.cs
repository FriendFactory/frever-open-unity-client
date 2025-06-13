using Bridge;
using TMPro;
using UIManaging.Common.Args.Views.Profile;
using UIManaging.Localization;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UIManaging.SnackBarSystem;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Popups.Store
{
    internal class SupportCreatorView : MonoBehaviour
    {
        [SerializeField] private UserPortraitView _creatorAvatar;
        [SerializeField] private TMP_Text _creatorNameText;
        [SerializeField] private Button _confirmSupportButton;
        [SerializeField] private Button _cancelSupportButton;
        [SerializeField] private Button _infoPanelButton;
        [SerializeField] private Button _closeInfoPanelButton;
        [SerializeField] private GameObject _supportActiveGroup;
        [SerializeField] private GameObject _description;
        [SerializeField] private GameObject _infoPanel;
        [SerializeField] private RectTransform _gridContainer;

        [Inject] private PopupManager _popupManager;
        [Inject] private IBridge _bridge;
        [Inject] private SnackBarHelper _snackBarHelper;
        [Inject] private StorePageLocalization _localization;

        private void Awake()
        {
            _confirmSupportButton.onClick.AddListener(ShowSupportCreatorPopup);
            _cancelSupportButton.onClick.AddListener(ShowCancelSupportDialogPopup);
            _infoPanelButton.onClick.AddListener(() => _infoPanel.SetActive(true));
            _closeInfoPanelButton.onClick.AddListener(() => _infoPanel.SetActive(false));
        }

        private void ShowSupportCreatorPopup()
        {
            var config = new InformationPopupConfiguration
            {
                PopupType = PopupType.SupportCreator,
                OnClose = OnSupportPopupClose
            };
            
            _popupManager.SetupPopup(config);
            _popupManager.ShowPopup(config.PopupType);
        }

        public async void Initialize()
        {
            _supportActiveGroup.SetActive(false);
            _description.SetActive(false);
            
            var profile = (await _bridge.GetCurrentUserInfo()).Profile;
            var isSupportingCreator = profile.SupportedStarCreator != null;
            
            _supportActiveGroup.SetActive(isSupportingCreator);
            _description.SetActive(isSupportingCreator);
            
            LayoutRebuilder.ForceRebuildLayoutImmediate(_gridContainer);

            if (!isSupportingCreator)
            {
                return;
            }
            
            _creatorAvatar.Initialize(new UserPortraitModel()
            {
                UserGroupId = profile.SupportedStarCreator.GroupId
            });

            _creatorNameText.text = profile.SupportedStarCreator.GroupNickName;
        }

        private void ShowCancelSupportDialogPopup()
        {
            var config = new DialogDarkPopupConfiguration()
            {
                PopupType = PopupType.DialogDark,
                Title = _localization.CancelSupportTitle,
                Description = _localization.CancelSupportDescription,
                OnYes = CancelSupport,
                YesButtonText = _localization.CancelSupportConfirm,
                NoButtonText = _localization.CancelSupportCancel
            };
            _popupManager.SetupPopup(config);
            _popupManager.ShowPopup(config.PopupType);
        }

        private void OnSupportPopupClose(object result)
        {
            if (result is bool supportUpdated && supportUpdated)
            {
                Initialize();
            }
        }

        private async void CancelSupport()
        {
            var result = await _bridge.CancelSupportCreator();
            
            if (result.IsSuccess)
            {
                Initialize();
            }
            else
            {
                _snackBarHelper.ShowInformationSnackBar(_localization.CancelSupportFailedMessage);
            }
        }
    }
}