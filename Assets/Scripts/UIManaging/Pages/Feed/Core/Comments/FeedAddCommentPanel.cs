using Abstract;
using I2.Loc;
using TMPro;
using UIManaging.SnackBarSystem;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.Feed.Core
{
    internal sealed class FeedAddCommentPanel: BaseContextDataButton<FeedAddCommentPanelModel>
    {
        [SerializeField] private TMP_Text _placeholder;
        [SerializeField] private CanvasGroup _contentGroup;
        [Header("Localization")] 
        [SerializeField] private LocalizedString _commentsEnabledLoc;
        [SerializeField] private LocalizedString _commentsDisabledLoc;

        [Inject] private SnackBarHelper _snackBarHelper;

        protected override void OnInitialized()
        {
            Toggle(ContextData.CommentsEnabled);
        }

        protected override void OnUIInteracted()
        {
            base.OnUIInteracted();

            if (!ContextData.CommentsEnabled)
            {
                // show snackbar
                _snackBarHelper.ShowInformationDarkSnackBar(_commentsDisabledLoc);
                return;
            }
            
            ContextData.OnClicked?.Invoke();
        }

        private void Toggle(bool isOn)
        {
            _contentGroup.alpha = isOn ? 1f : 0.5f;
            
            _placeholder.text = isOn ? _commentsEnabledLoc : _commentsDisabledLoc;
        }
    }
}