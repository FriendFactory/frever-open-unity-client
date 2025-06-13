using System.Threading.Tasks;
using Navigation.Core;
using UIManaging.Pages.DiscoverPeoplePage.StarCreator.AcceptedInvitations;
using UnityEngine;

namespace UIManaging.Pages.DiscoverPeoplePage.StarCreator
{
    internal sealed class StarCreatorDiscoverPeoplePage: BaseDiscoverPeoplePage<StarCreatorDiscoverPeoplePageArgs>
    {
        [Space]
        [SerializeField] private AcceptedInvitationsButton _acceptedInvitationsButton;
        [SerializeField] private BaseInvitationLinkSharePanel _sharePanel;
        
        public override PageId Id => PageId.StarCreatorDiscoverPeople;

        protected override void InitializeModels()
        {
            _acceptedInvitationsButton.Initialize(InvitationCodeModel);
            _sharePanel.Initialize(InvitationCodeModel);
        }
    }
}