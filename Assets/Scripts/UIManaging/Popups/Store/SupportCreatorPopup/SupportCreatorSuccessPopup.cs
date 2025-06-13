using TMPro;
using UIManaging.Common.Args.Views.Profile;
using UIManaging.PopupSystem.Popups;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.SupportCreatorPage
{
    public class SupportCreatorSuccessPopup : ConfigurableBasePopup<SupportCreatorSuccessPopupConfiguration>
    {
        [SerializeField] private UserPortraitView _userPortraitView;
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _descriptionText;
        [SerializeField] private Button _closeButton;

        private void Awake()
        {
            _closeButton.onClick.AddListener(Hide);
        }

        protected override void OnConfigure(SupportCreatorSuccessPopupConfiguration configuration)
        {
            _titleText.text = configuration.Title;
            _descriptionText.text = configuration.Description;
            _userPortraitView.Initialize(new UserPortraitModel { UserGroupId = configuration.SupportedCreatorGroupId });
        }
    }
}