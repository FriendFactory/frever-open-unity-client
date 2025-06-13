using System;
using System.Threading;
using Bridge;
using Extensions;
using Modules.Amplitude.Events;
using Modules.Amplitude.Signals;
using UIManaging.Pages.Common.SongOption.MusicLicense;
using UnityEngine;
using Zenject;
using IInitializable = Abstract.IInitializable;

namespace UIManaging.Pages.Common.SongOption.Search
{
    public class MusicSearchHandler : MonoBehaviour, IInitializable
    {
        [SerializeField] private AllMusicSearchView _allMusicSearchView;
        [SerializeField] private CommercialMusicSearchView _commercialMusicSearchView;
        [SerializeField] private MusicSearchPanelView _searchPanel;

        [Inject] private IMusicBridge _musicServiceBridge;
        [Inject] private MusicLicenseManager _musicLicenseManager;
        [Inject] private SignalBus _signalBus;
        
        private MusicSearchListModelsProvider _searchListModelsProvider;
        private TrackSearchListModel _trackSearchListModel;
        private SongSearchListModel _songSearchListModel;
        private TrendingUserSoundsSearchListModel _userSoundSearchListModel;
        private CommercialSongSearchListModel _commercialSongSearchListModel;

        private MusicSearchType _currentSearchType;
        private bool _searchPerformedAtLeastOnce;
        private MusicSearchView _activeSearchView;
        private CancellationTokenSource _cancellationTokenSource;
        private string _lastSearchQuery;
        private MusicSearchType _lastSearchType;

        public bool IsInitialized { get; private set; }

        private MusicLicenseType ActiveMusicLicenseType => _musicLicenseManager.ActiveLicenseType;
        private bool PremiumSoundsEnabled => _musicLicenseManager.PremiumSoundsEnabled;

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        public event Action SearchSelected;
        public event Action SearchDeselected;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            _searchPanel.OnSelect.AddListener(OnSearchSelect);
            _searchPanel.OnDeselect.AddListener(OnSearchDeselect);
        }

        private void OnEnable()
        {
            _searchPanel.InputOnSubmit += OnSubmitSearch;
            _searchPanel.SearchRequested += OnSubmitSearch;
            _searchPanel.ClearInputButtonClicked += OnCancelRequested;

            if (IsInitialized)
            {
                _searchPanel.ToggleControls(true);
            }
        }

        private void OnDisable()
        {
            _searchPanel.ClearWithoutNotify();
            _searchPanel.InputOnSubmit -= OnSubmitSearch;
            _searchPanel.SearchRequested -= OnSubmitSearch;
            _searchPanel.ClearInputButtonClicked -= OnCancelRequested;
        }

        private void OnDestroy()
        {
            _searchPanel.OnSelect.RemoveListener(OnSearchSelect);
            _searchPanel.OnDeselect.RemoveListener(OnSearchDeselect);
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Initialize()
        {
            if (IsInitialized) return;

            _searchListModelsProvider = new MusicSearchListModelsProvider(_musicServiceBridge);
            _trackSearchListModel = _searchListModelsProvider.GetListModel<TrackSearchListModel>(MusicSearchType.Music);
            _songSearchListModel = _searchListModelsProvider.GetListModel<SongSearchListModel>(MusicSearchType.Moods);
            _userSoundSearchListModel = _searchListModelsProvider.GetListModel<TrendingUserSoundsSearchListModel>(MusicSearchType.TrendingUserSounds);
            _commercialSongSearchListModel = _searchListModelsProvider.GetListModel<CommercialSongSearchListModel>(MusicSearchType.CommercialSongs);

            _activeSearchView = ActiveMusicLicenseType == MusicLicenseType.AllSounds
                ? _allMusicSearchView
                : _commercialMusicSearchView as MusicSearchView;
            _currentSearchType = ActiveMusicLicenseType == MusicLicenseType.AllSounds
                ? (PremiumSoundsEnabled ? MusicSearchType.Music : MusicSearchType.Moods)
                : MusicSearchType.CommercialSongs;
            
            _activeSearchView.Initialize(_searchListModelsProvider);
            _activeSearchView.Show();

            _musicLicenseManager.MusicLicenseChanged += OnMusicLicenseChanged;

            _allMusicSearchView.TabSelectionChanged += OnSearchTabChanged;
            _commercialMusicSearchView.TabSelectionChanged += OnSearchTabChanged;
            
            _searchPanel.ToggleControls(true);

            IsInitialized = true;
        }

        public void CleanUp()
        {
            if (!IsInitialized)
            {
                Debug.LogError($"[{GetType().Name}] Failed to perform clean up - component is not initialized");
                return;
            }

            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.CancelAndDispose();
                _cancellationTokenSource = null;
            }
            
            _searchListModelsProvider?.Dispose();

            _searchPanel.Text = string.Empty;
            _lastSearchQuery = string.Empty;
            _searchPanel.ToggleControls(false);

            _activeSearchView.Hide();
            _activeSearchView.CleanUp();
            
            _musicLicenseManager.MusicLicenseChanged -= OnMusicLicenseChanged;

            _allMusicSearchView.TabSelectionChanged -= OnSearchTabChanged;
            _commercialMusicSearchView.TabSelectionChanged -= OnSearchTabChanged;

            IsInitialized = false;
            _searchPerformedAtLeastOnce = false;
        }

        private void OnSearchTabChanged(MusicSearchType searchType)
        {
            _currentSearchType = searchType;

            if (!_searchPerformedAtLeastOnce) return;
            
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.CancelAndDispose();
                _cancellationTokenSource = null;
            }
            
            OnSubmitSearch(_searchPanel.Text);
        }
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private async void OnSubmitSearch(string searchQuery)
        {
            if (string.IsNullOrEmpty(searchQuery)) return;

            var queryChanged = !string.Equals(searchQuery, _lastSearchQuery, StringComparison.InvariantCultureIgnoreCase);
            var searchTypeChanged = _currentSearchType != _lastSearchType;
            
            if (!queryChanged && !searchTypeChanged) return;

            _lastSearchQuery = searchQuery;
            _lastSearchType = _currentSearchType;
            
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.CancelAndDispose();
                _cancellationTokenSource = null;
            }
            
            _cancellationTokenSource = new CancellationTokenSource();
            _searchPerformedAtLeastOnce = true;

            switch (_currentSearchType)
            {
                case MusicSearchType.Music:
                {
                    var amplitudeEvent = new SearchForExternalTrackEvent(searchQuery);
                    _signalBus.Fire(new AmplitudeEventSignal(amplitudeEvent));
                    await _trackSearchListModel.SearchAsync(searchQuery, _cancellationTokenSource.Token);
                    break;
                }
                case MusicSearchType.Moods:
                {
                    searchQuery = searchQuery.ToLower();
                    await _songSearchListModel.SearchAsync(searchQuery, _cancellationTokenSource.Token);
                    break;
                }
                case MusicSearchType.TrendingUserSounds:
                    await _userSoundSearchListModel.SearchAsync(searchQuery, _cancellationTokenSource.Token);
                    break;
                case MusicSearchType.CommercialSongs:
                    await _commercialSongSearchListModel.SearchAsync(searchQuery, _cancellationTokenSource.Token);
                    break;
            }
        }
        
        private void OnSearchSelect(string input)
        {
            Initialize();
            
            SearchSelected?.Invoke();
        }

        private void OnSearchDeselect(string input)
        {
            if (!string.IsNullOrEmpty(input)) return;
            
            SearchDeselected?.Invoke();
        }

        private void OnCancelRequested(string text)
        {
            CleanUp();
        }

        private void OnMusicLicenseChanged(MusicLicenseType _)
        {
            CleanUp();
        }
    }
}
