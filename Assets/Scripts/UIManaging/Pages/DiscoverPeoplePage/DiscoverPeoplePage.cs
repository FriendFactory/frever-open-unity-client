using Navigation.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace UIManaging.Pages.DiscoverPeoplePage
{
    internal sealed class DiscoverPeoplePage : BaseDiscoverPeoplePage<DiscoverPeoplePageArgs>
    {
        [Space]
        [SerializeField] private DiscoverPeoplePageContentList _contentList;

        private InvitationAcceptedRewardListModel _contentListModel;
        
        public override PageId Id => PageId.DiscoverPeoplePage;

        protected override void InitializeModels()
        {
            _contentListModel = new InvitationAcceptedRewardListModel(InvitationCodeModel);
            
            _contentListModel.Initialize();
            _contentList.Initialize(_contentListModel);
        }

        [Button]
        private void ForceDisplayStart()
        {
            OnDisplayStart(OpenPageArgs);
        }
    }
}