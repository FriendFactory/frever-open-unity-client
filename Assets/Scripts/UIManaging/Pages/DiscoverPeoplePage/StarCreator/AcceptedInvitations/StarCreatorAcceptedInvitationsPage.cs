using System;
using Bridge;
using Navigation.Core;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.DiscoverPeoplePage.StarCreator.AcceptedInvitations
{
    public class StarCreatorAcceptedInvitationsPage: GenericPage<StarCreatorAcceptedInvitationsPageArgs>
    {
        [SerializeField] private AcceptedInvitationsList _acceptedInvitationsList;
        
        [Inject] private IBridge _bridge;
        
        private AcceptedInvitationsListModel _acceptedInvitationsListModel;
        
        public override PageId Id => PageId.StarCreatorAcceptedInvitations;

        protected override void OnInit(PageManager pageManager)
        {
            _acceptedInvitationsListModel = new AcceptedInvitationsListModel(_bridge);
        }

        protected override async void OnDisplayStart(StarCreatorAcceptedInvitationsPageArgs args)
        {
            base.OnDisplayStart(args);
            
            await _acceptedInvitationsListModel.InitializeAsync(args.Invitations);
            _acceptedInvitationsList.Initialize(_acceptedInvitationsListModel);
        }
    }
}