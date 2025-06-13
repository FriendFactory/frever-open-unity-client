using System;
using System.Threading.Tasks;
using Modules.DeepLinking;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.SnackBarSystem;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
#if UNITY_ANDROID
using SA.Android.App;
using SA.Android.Content;
#endif

namespace UIManaging.Pages.ContactsPage
{
    internal class InviteContactButton : MonoBehaviour
    {
        private const string EMOJI_UNICODE = "\U0001F31F"; //Star emoji. 
        private const string INVITE_MESSAGE_FORMAT = "Come hang with me on Frever!" + EMOJI_UNICODE + "\n{0}";
        private const float GENERATE_LINK_TIMEOUT = 5f;

        private string _phoneNumber;
        private Button _button;

        [Inject] private IInvitationLinkHandler _invitationLinkHandler;
        [Inject] private LocalUserDataHolder _localUser;
        [Inject] private SnackBarHelper _snackBarHelper;
        
        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(SendInvite);
        }

        public void Initialize(string phoneNumber)
        {
            _phoneNumber = phoneNumber;
        }

        private async void SendInvite()
        {
            var linkGenerated = false;
            
            _invitationLinkHandler.OnLinkGenerated += OnLinkGenerated;
            _invitationLinkHandler.GenerateLink((int)_localUser.GroupId, _localUser.IsStarCreator);
            _button.interactable = false;
            
            var startTime = Time.time; 
            while (!linkGenerated)
            {
                if (Time.time - startTime > GENERATE_LINK_TIMEOUT)
                {
                    _snackBarHelper.ShowFailSnackBar("Failed to generate invitation link - time out reached");
                    _button.interactable = true;
                    break;
                }
                
                await Task.Delay(25);
            }
            
            void OnLinkGenerated(string link)
            {
                _invitationLinkHandler.OnLinkGenerated -= OnLinkGenerated;
                _button.interactable = true;
                linkGenerated = true;
            
                #if UNITY_IOS
                string URL = "sms:"+_phoneNumber+"?&body=" + Uri.EscapeDataString(string.Format(INVITE_MESSAGE_FORMAT, link));
                Application.OpenURL(URL);
                #elif UNITY_ANDROID
                var uri = new Uri($"smsto:{_phoneNumber}");
                var intent = new AN_Intent(AN_Intent.ACTION_SENDTO, uri);
                intent.PutExtra("sms_body", string.Format(INVITE_MESSAGE_FORMAT, link));

                AN_MainActivity.Instance.StartActivity(intent);
                #endif
            }
        }
    }
}