using Modules.ContentModeration;
using UIManaging.Common.Hashtags;
using UIManaging.Core;
using UIManaging.Pages.SharingPage.Ui;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.PublishPage.Buttons
{
    public class FinishDescriptionEditButton: ButtonBase
    {
        [SerializeField] private DescriptionPanel _descriptionPanel;
        [SerializeField] private DescriptionFieldAnimator _descriptionFieldAnimator;
        [SerializeField] private HashtagsPanel _hashtagsPanel;

        [Inject] private TextContentValidator _textContentValidator;
        
        protected override async void OnClick()
        {
            var parsedText = _descriptionPanel.InputFieldAdapter.GetParsedText();
            var moderationPassed = await _textContentValidator.ValidateTextContent(parsedText);
            
            if (!moderationPassed) return;
            
            _hashtagsPanel.Hide();
            _descriptionFieldAnimator.AnimateShrink();
        }
    }
}