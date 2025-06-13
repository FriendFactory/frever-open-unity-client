using Abstract;
using Modules.DeepLinking;
using Navigation.Core;
using TMPro;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.DiscoverPeoplePage.StarCreator.AcceptedInvitations
{
    internal class AcceptedInvitationsButton: BaseContextDataButton<InvitationCodeModel>
    {
        [SerializeField] private TMP_Text _countText;

        [Inject] private PageManager _manager;
        
        protected override void OnUIInteracted()
        {
            var pageArgs = new StarCreatorAcceptedInvitationsPageArgs { Invitations = ContextData.InvitationCode.InviteGroups };
            var transitionArgs = new PageTransitionArgs
            {
                SaveCurrentPageToHistory = true
            };
            
            _manager.MoveNext(pageArgs, transitionArgs);
        }

        protected override void OnInitialized()
        {
            var invitees = ContextData.InvitationCode.InviteGroups;
            _countText.text = invitees?.Length.ToString() ?? "0";
        }
    }
}