using System;
using System.Threading.Tasks;
using Bridge;
using Common;
using Common.UserBalance;
using Extensions;
using JetBrains.Annotations;
using Modules.DeepLinking;
using Navigation.Core;
using UIManaging.Common.SearchPanel;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.SnackBarSystem;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.DiscoverPeoplePage
{
    internal abstract class BaseDiscoverPeoplePage<TPageArgs> : GenericPage<TPageArgs> where TPageArgs : PageArgs
    {
        [SerializeField] private SearchHandler _searchHandler;
        [SerializeField] private CanvasGroup _searchCanvasGroup;
        [SerializeField] private UserBalanceView _userBalanceView;
        
        private IBridge _bridge;
        private IInvitationLinkHandler _invitationLinkHandler;
        private LocalUserDataHolder _dataHolder;
        private SnackBarHelper _snackBarHelper;

        //---------------------------------------------------------------------
        // Properties 
        //---------------------------------------------------------------------

        protected InvitationCodeModel InvitationCodeModel { get; private set; }

        //---------------------------------------------------------------------
        // Ctors 
        //---------------------------------------------------------------------

        [Inject]
        [UsedImplicitly]
        private void Construct(IBridge bridge, IInvitationLinkHandler invitationLinkHandler,
            LocalUserDataHolder dataHolder, SnackBarHelper snackBarHelper, InvitationCodeModel invitationCodeModel)
        {
            _bridge = bridge;
            _invitationLinkHandler = invitationLinkHandler;
            _dataHolder = dataHolder;
            _snackBarHelper = snackBarHelper;
            InvitationCodeModel = invitationCodeModel;
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override void OnInit(PageManager pageManager) { }

        protected sealed override void OnDisplayStart(TPageArgs args)
        {
            base.OnDisplayStart(args);

            PlayerPrefs.SetInt(Constants.DotIndicators.Invite, 1);
            _searchHandler.SearchedProfilesLoaded += OnSearchChanged;
            _searchHandler.SearchCleared += OnSearchCleared;

            var userBalanceModel = new StaticUserBalanceModel(_dataHolder);
            _userBalanceView.ContextData?.CleanUp();
            _userBalanceView.Initialize(userBalanceModel);

            InitializeModels();
        }

        protected sealed override void OnHidingBegin(Action onComplete)
        {
            base.OnHidingBegin(onComplete);
            
            _searchHandler.SearchedProfilesLoaded -= OnSearchChanged;
            _searchHandler.SearchCleared -= OnSearchCleared;
        }

        protected abstract void InitializeModels();
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnSearchChanged() =>_searchCanvasGroup.Enable();
        private void OnSearchCleared() => _searchCanvasGroup.Disable();
    }
}