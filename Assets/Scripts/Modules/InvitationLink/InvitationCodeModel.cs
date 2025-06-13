using System.Threading.Tasks;
using Bridge;
using Bridge.Models.ClientServer.Invitation;
using JetBrains.Annotations;
using UIManaging.SnackBarSystem;
using UnityEngine;

namespace Modules.DeepLinking
{
    [UsedImplicitly]
    public sealed class InvitationCodeModel
    {
        private const float GENERATE_LINK_TIMEOUT = 5f;
        
        private readonly IBridge _bridge;
        private readonly IInvitationLinkHandler _invitationLinkHandler;
        private readonly SnackBarHelper _snackBarHelper;
        private readonly bool _isStarCreator;
        
        public InvitationCode InvitationCode { get; private set; }
        public string ShareLink { get; private set; }

        public InvitationCodeModel(IBridge bridge, IInvitationLinkHandler invitationLinkHandler, SnackBarHelper snackBarHelper, bool isStarCreator = false)
        {
            _bridge = bridge;
            _invitationLinkHandler = invitationLinkHandler;
            _snackBarHelper = snackBarHelper;
            _isStarCreator = isStarCreator;
        }

        public async Task InitializeAsync()
        {
            await GetShareLinkAsync();
            await GetInvitationCodeAsync();
        }

        private async Task GetShareLinkAsync()
        {
            var linkGenerated = false;
            
            _invitationLinkHandler.OnLinkGenerated += OnLinkGenerated;
            
            _invitationLinkHandler.GenerateLink((int)_bridge.Profile.GroupId, _isStarCreator);

            var startTime = Time.time; 
            while (!linkGenerated)
            {
                if (Time.time - startTime > GENERATE_LINK_TIMEOUT)
                {
                    _snackBarHelper.ShowFailSnackBar("Failed to generate invitation link - time out reached");
                    break;
                }
                
                await Task.Delay(25);
            }

            void OnLinkGenerated(string link)
            {
                _invitationLinkHandler.OnLinkGenerated -= OnLinkGenerated;

                linkGenerated = true;
                ShareLink = link;
            }
        }

        private async Task GetInvitationCodeAsync()
        {
            var result = await _bridge.GetInvitationCode();

            if (result.IsError)
            {
                Debug.LogError($"[{GetType().Name}] Failed to get invitation code # {result.ErrorMessage}");
                return;
            }

            InvitationCode = result.Model;
        }
    }
}