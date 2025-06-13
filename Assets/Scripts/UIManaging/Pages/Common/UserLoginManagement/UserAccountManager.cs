using System;
using System.Threading.Tasks;
using Bridge;
using Bridge.Authorization.Models;
using JetBrains.Annotations;
using Modules.DeepLinking;
using Modules.UserInitialization;
using UIManaging.Pages.Common.ErrorsManagement;
using UIManaging.Pages.Common.FollowersManagement;
using UIManaging.Pages.Feed.Ui.Feed;
using Zenject;

namespace UIManaging.Pages.Common.UserLoginManagement
{
    [UsedImplicitly]
    public sealed class UserAccountManager
    {
        public event Action OnUserLoggedIn;
        public event Action OnUserLoggedOut;
        
        [Inject] private IBridge _bridge;
        [Inject] private ErrorsManager _errorsManager;
        [Inject] private FollowersManager _followersManager;
        [Inject] private VideoViewSender _videoViewSender;
        [Inject] private VideoViewFileHandler _videoViewFileHandler;
        [Inject] private IInvitationLinkHandler _invitationLinkHandler;
        [Inject] private UserInitializationService _userInitializationService;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public bool IsLoggedIn => _bridge.IsLoggedIn;

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public async Task<bool> LogInAsync(ICredentials credentials, bool savePassword, Action<string> onFail = null)
        {
            var result = await _bridge.LogInAsync(credentials, savePassword);

            if (result.IsSuccess)
            {
                _invitationLinkHandler.Clear();
                await OnLoggedIn();
                return true;
            }

            var errorMessage = $"Failed to log in. Reason: {result.ErrorMessage} {result.ErrorType}";

            if (result.ErrorType != null)
            {
                errorMessage = _errorsManager.GetLoginErrorMessageByType(result.ErrorType.Value);
            }

            onFail?.Invoke(errorMessage);
            return false;
        }

        public async void DeleteAccount(Action onComplete)
        {
            var deleteAccountTask = await _bridge.DeleteMyAccount();
            if (deleteAccountTask.IsSuccess)
            {
                onComplete?.Invoke();
            }
            else
            {
                throw new InvalidOperationException(
                    $"Account couldn't be deleted. [Reason]: {deleteAccountTask.ErrorMessage}");
            }
        }

        public void Logout(Action onComplete, Action<string> onFail)
        {
            var result = _bridge.LogOut(true);
            if (result.IsSuccess)
            {
                _userInitializationService.CleanUp();
                
                OnUserLoggedOut?.Invoke();
                onComplete?.Invoke();
            }
            else
            {
                onFail?.Invoke(result.ErrorMessage);
            }
        }

        public async Task OnLoggedIn()
        {
            if (!_userInitializationService.IsInitialized)
            {
                await _userInitializationService.InitializeAsync();
            }
            
            SendSavedVideoViews();
            
            OnUserLoggedIn?.Invoke();
        }

        private void SendSavedVideoViews()
        {
            if (!_videoViewFileHandler.HasDataFromPreviousSession()) return;

            var videoViews = _videoViewFileHandler.LoadPreviousSessionNotSentData();
            _videoViewSender.Send(videoViews);
            _videoViewFileHandler.DeleteSavedDataFromPreviousSession();
        }
    }
}