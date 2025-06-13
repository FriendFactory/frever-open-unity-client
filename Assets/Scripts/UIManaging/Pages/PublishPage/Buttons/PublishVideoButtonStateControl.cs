using Common.Publishers;
using Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.PublishPage.Buttons
{
    internal sealed class PublishVideoButtonStateControl: MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _label;
        [SerializeField] private PublishPageLocalization _localization;
        
        public void UpdateIconAndLabel(PublishingType publishingType)
        {
            _icon.SetActive(false);
            _label.text = GetLabelText(publishingType);
            _label.alignment = TextAlignmentOptions.Midline;
        }
        
        private string GetLabelText(PublishingType publishingType)
        {
            return publishingType == PublishingType.VideoMessage ? _localization.SendMessageButtonText : _localization.PublishVideoButtonText;
        }
    }
}