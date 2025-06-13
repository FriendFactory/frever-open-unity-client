using Abstract;
using Extensions;
using TMPro;
using UIManaging.Pages.Common;
using UIManaging.Pages.Common.NotificationBadge;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.AppSettingsPage.UI
{
    public class SettingItemView : BaseContextDataButton<SettingItemViewArgs>
    {
        [SerializeField] private Image icon;
        [SerializeField] private Image arrowIcon;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI headerText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI detailText;
        [SerializeField] private Toggle _toggle;
        [SerializeField] private GameObject topDividerGameObject;
        [SerializeField] private GameObject bottomDividerGameObject;
        [SerializeField] private GameObject arrowParentGameObject;
        [SerializeField] private GameObject toggleParentGameObject;
        [SerializeField] private GameObject detailTextParentGameObject;
        [SerializeField] private GameObject titleParentGameObject;
        [SerializeField] private GameObject headerParentGameObject;
        [SerializeField] private GameObject panel;
        [SerializeField] private NotificationBadge _notificationBadge;
        
        protected override void OnInitialized()
        {
            if (ContextData.Icon != null)
            {
                icon.sprite = ContextData.Icon;
                icon.SetNativeSize();
            }

            if (ContextData.ShowCustomArrowIcon)
            {
                arrowIcon.sprite = ContextData.CustomArrowIcon;
                arrowIcon.SetNativeSize();
            }

            titleText.text = ContextData.Title;
            descriptionText.text = ContextData.Description;
            descriptionText.SetActive(ContextData.ShowDescription);
            titleText.color = ContextData.TitleColor;
            headerText.text = ContextData.Header;
            RefreshDetailText();

            var hasOnClickedEvent = ContextData.OnClicked != null && ContextData.OnClicked.GetInvocationList().Length > 0;
            
            if(hasOnClickedEvent)
            {
                _button.onClick.AddListener(OnButtonClicked);
            }
            
            _button.gameObject.SetActive(hasOnClickedEvent);
            _toggle.SetActive(ContextData.ShowToggle);

            topDividerGameObject.SetActive(ContextData.ShowTopDivider);
            bottomDividerGameObject.SetActive(ContextData.ShowBottomDivider);
            arrowParentGameObject.SetActive(ContextData.ShowArrow);
            titleParentGameObject.SetActive(ContextData.ShowTitle);
            detailTextParentGameObject.SetActive(ContextData.ShowDetailText);
            headerParentGameObject.SetActive(ContextData.ShowHeader);
            toggleParentGameObject.SetActive(ContextData.ShowToggle);
            panel.SetActive(ContextData.ShowDetailPanel);
            
            ContextData.OnDetailTextChangedEvent += RefreshDetailText;
            
            if(ContextData.ShowToggle) SetupToggle();

            SetupNotificationBadge(ContextData.NotificationBadgeModel);
        }

        protected override void BeforeCleanup()
        {
            base.BeforeCleanup();
            _button.onClick.RemoveListener(OnButtonClicked);

            if (ContextData != null)
            {
                ContextData.OnDetailTextChangedEvent -= RefreshDetailText;
            }
            
            _notificationBadge.CleanUp();
        }

        private void RefreshDetailText()
        {
            detailText.text = ContextData.DetailText;
            detailTextParentGameObject.SetActive(ContextData.ShowDetailText);
        }

        private void OnButtonClicked()
        {
            ContextData.OnClicked();
        }

        private void SetupToggle()
        {
            _button.SetActive(true);
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(OnToggleButtonClick);
            
            _toggle.SetActive(true);
            _toggle.isOn = ContextData.IsToggleOn;
            _toggle.onValueChanged.RemoveAllListeners();
            _toggle.onValueChanged.AddListener((isOn) => ContextData.OnToggleValueChanged(isOn));
        }

        private void OnToggleButtonClick()
        {
            _toggle.isOn = !_toggle.isOn;
        }

        private void SetupNotificationBadge(NotificationBadgeModel notificationBadgeModel)
        {
            if (notificationBadgeModel == null) return;
            
            _notificationBadge.Initialize(notificationBadgeModel);
        }
    }
}