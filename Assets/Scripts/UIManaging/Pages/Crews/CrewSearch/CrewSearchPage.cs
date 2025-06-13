using Modules.Amplitude;
using Modules.AssetsStoraging.Core;
using Navigation.Args;
using Navigation.Core;
using UIManaging.Common.Args.Buttons;
using UIManaging.Common.PageHeader;
using UIManaging.Common.SearchPanel;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.Pages.Crews.CrewSearch;
using UIManaging.Pages.DiscoveryPage;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class CrewSearchPage : GenericPage<CrewSearchPageArgs>
{
    [SerializeField] private PageHeaderView _pageHeader;
    [SerializeField] private CrewsPanel _crewsPanel;
    [SerializeField] private Button _createCrewButton;
    [SerializeField] private GameObject _createCrewLockIcon;
    [SerializeField] private SearchPanelView _searchPanelView;

    [SerializeField] private CrewSearchPageLocalization _localization;

    [Inject] private IDataFetcher _dataFetcher;
    [Inject] private LocalUserDataHolder _userData;
    [Inject] private AmplitudeManager _amplitudeManager;
    [Inject] private PopupManager _popupManager;

    //---------------------------------------------------------------------
    // Properties
    //---------------------------------------------------------------------
    
    public override PageId Id => PageId.CrewSearch;

    //---------------------------------------------------------------------
    // Protected
    //---------------------------------------------------------------------

    protected override void OnInit(PageManager pageManager)
    {
        _searchPanelView.InputCleared += () => OpenPageArgs.SearchQuery = string.Empty;
        _searchPanelView.InputCompleted += searchQuery => OpenPageArgs.SearchQuery = searchQuery;
        _createCrewButton.onClick.AddListener(OnCreateCrewButton);
    }

    protected override void OnDisplayStart(CrewSearchPageArgs args)
    {
        base.OnDisplayStart(args);

        InitCreateCrewButton();

        if (!args.Backed)
        {
            _crewsPanel.Show(args.SearchQuery, args.Backed);
        }
        _pageHeader.Init(new PageHeaderArgs(_localization.PageHeader, new ButtonArgs(string.Empty, OnBackButton)));
    }

    //---------------------------------------------------------------------
    // Helpers
    //---------------------------------------------------------------------

    private async void InitCreateCrewButton()
    {
        _createCrewButton.gameObject.SetActive(false);

        await _userData.DownloadProfile();
        if (_createCrewButton.IsDestroyed()) return;

        _createCrewButton.gameObject.SetActive(_userData.UserProfile.CrewProfile == null);
        _createCrewLockIcon.SetActive(!_userData.LevelingProgress.AllowCrewCreation);
    }

    private void OnCreateCrewButton()
    {
        if (!_userData.LevelingProgress.AllowCrewCreation)
        {
            var popupConfig = new AlertPopupConfiguration { PopupType = PopupType.CrewCreationLocked };
            _popupManager.PushPopupToQueue(popupConfig);
            return;
        }
        
        Manager.MoveNext(new CrewCreatePageArgs());
    }

    private void OnBackButton()
    {
        Manager.MoveBack();
    }
}