using Bridge;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.UserProfile.Ui.Buttons
{
    [RequireComponent(typeof(Button))]
    internal sealed class ShareProfileButton : MonoBehaviour
    {
        [Inject] private IBridge _bridge;
        private readonly NativeShare _nativeShare = new NativeShare();
        private Button _button;
        private string _url;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        
        private void Start()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(OnClick);
        }

        private void OnDestroy()
        {
           _button.onClick.RemoveListener(OnClick);
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void UpdateProfileURL(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                Debug.LogWarning($" The username is null or empty.");
                return;
            }

            _url = _bridge.GetUserProfilePublicUrl(username);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnClick()
        {
            if (string.IsNullOrEmpty(_url))
            {
                Debug.LogWarning($" The profile url is null or empty.");
                return;
            }

            _nativeShare.SetText(_url).Share();
        }
    }
}