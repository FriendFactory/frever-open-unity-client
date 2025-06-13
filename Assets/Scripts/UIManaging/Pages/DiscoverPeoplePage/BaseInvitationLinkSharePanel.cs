using Abstract;
using Modules.DeepLinking;
using UnityEngine;
using UnityEngine.UI;
using VoxelBusters.EssentialKit;

namespace UIManaging.Pages.DiscoverPeoplePage
{
    internal abstract class BaseInvitationLinkSharePanel: BaseContextDataView<InvitationCodeModel>
    {
        private const string SHARE_MESSAGE_TEMPLATE =
            "Come and hang out with me on Frever!🌟\nUse my link to sign up and get a reward:\n{0}";
        
        [SerializeField] private Button _shareButton;
        
        
        private string ShareMessage => string.Format(SHARE_MESSAGE_TEMPLATE, ContextData.ShareLink);

        private void OnEnable()
        {
            _shareButton.onClick.AddListener(OnShareButtonClick);
        }

        private void OnDisable()
        {
            _shareButton.onClick.RemoveListener(OnShareButtonClick);
        }

        protected override void OnInitialized() { }

        private void OnShareButtonClick()
        {
            var shareSheet = ShareSheet.CreateInstance();
            shareSheet.AddText(ShareMessage);
            shareSheet.SetCompletionCallback((result, error) => {
                Debug.Log($"[{GetType().Name}] Share Sheet was closed. Result code: {result.ResultCode}");
            });
            shareSheet.Show();
        }
    }
}