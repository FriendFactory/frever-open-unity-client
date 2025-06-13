using System;
using System.Threading;
using System.Threading.Tasks;
using Abstract;
using Extensions;
using Stateless;
using UIManaging.Pages.Common.SongOption.Favorites;
using UIManaging.Pages.Common.SongOption.Playlists;
using UIManaging.Pages.Common.SongOption.SongDiscovery;
using UIManaging.Pages.Common.SongOption.StateChanger;
using UIManaging.Pages.Common.SongOption.Uploads;
using UnityEngine;

namespace UIManaging.Pages.Common.SongOption
{
    internal class MusicSelectionStateController : MonoBehaviour, IInitializable
    {
        [Header("Views")] 
        [SerializeField] private MoodsCategoryView _moodsCategoryView;
        [SerializeField] private UploadsCategoryView _uploadsCategoryView;
        [SerializeField] private FullPlaylistView _fullPlaylistView;
        [SerializeField] private PlaylistsView _playlistsView;
        [SerializeField] private GenresView _genresView;
        [SerializeField] private SoundSettingsView _soundSettingsView;
        [SerializeField] private FavoritesCategoryView _favoritesCategoryView;

        private StateMachine<SongMenuState, MusicNavigationCommand> _machine;
        private SongMenuState _currentMenuState;
        private CancellationTokenSource _tokenSource;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public bool IsInitialized { get; private set; }

        private StateMachine<SongMenuState, MusicNavigationCommand>.TriggerWithParameters<FullPlaylistListModel> PlaylistTrigger { get; set; }
        private StateMachine<SongMenuState, MusicNavigationCommand>.TriggerWithParameters<PlaylistsViewModel> PlaylistsTrigger { get; set; }
        private StateMachine<SongMenuState, MusicNavigationCommand>.TriggerWithParameters<GenresViewModel> GenresTrigger { get; set; }
        private StateMachine<SongMenuState, MusicNavigationCommand>.TriggerWithParameters<SoundSettingsViewModel> SoundSettingsTrigger { get; set; }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void ChangeToPreviousState()
        {
            _machine.FireAsync(MusicNavigationCommand.MoveBack);
        }

        public void Initialize()
        {
            _tokenSource = new CancellationTokenSource();
            
            InitializeMachine();
            ConfigureStates();
            
            IsInitialized = true;

            _moodsCategoryView.SetActive(false);
            
            _machine.Fire(MusicNavigationCommand.OpenMusic);
        }

        public void CleanUp()
        {
            if (!IsInitialized) return;

            _tokenSource?.CancelAndDispose();
            
            IsInitialized = false;
        }

        public void Fire(MusicNavigationCommand command)
        {
            _machine.Fire(command);
        }
        
        public void FireAsync(MusicNavigationCommand command)
        {
            _machine.FireAsync(command);
        }

        public void FireAsync<TParameters>(MusicNavigationCommand command, TParameters parameters) where TParameters: MusicViewModel
        {
            switch (command)
            {
                case MusicNavigationCommand.OpenPlaylist:
                    _machine.FireAsync(PlaylistTrigger, parameters as FullPlaylistListModel);
                    break;
                case MusicNavigationCommand.OpenPlaylists:
                    _machine.FireAsync(PlaylistsTrigger, parameters as PlaylistsViewModel);
                    break;
                case MusicNavigationCommand.OpenGenres:
                    _machine.FireAsync(GenresTrigger, parameters as GenresViewModel);
                    break;
                case MusicNavigationCommand.OpenSoundsSettings:
                    _machine.FireAsync(SoundSettingsTrigger, parameters as SoundSettingsViewModel);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(command), command, null);
            }
        }
        
        //---------------------------------------------------------------------
        // Helpers 
        //---------------------------------------------------------------------

        private void InitializeMachine()
        {
            _currentMenuState = SongMenuState.Music;
            _machine = new StateMachine<SongMenuState, MusicNavigationCommand>(GetState, SetState);
            
            PlaylistTrigger = _machine.SetTriggerParameters<FullPlaylistListModel>(MusicNavigationCommand.OpenPlaylist);
            PlaylistsTrigger = _machine.SetTriggerParameters<PlaylistsViewModel>(MusicNavigationCommand.OpenPlaylists);
            GenresTrigger = _machine.SetTriggerParameters<GenresViewModel>(MusicNavigationCommand.OpenGenres);
            SoundSettingsTrigger = _machine.SetTriggerParameters<SoundSettingsViewModel>(MusicNavigationCommand.OpenSoundsSettings);
            
            SongMenuState GetState() => _currentMenuState;
            void SetState(SongMenuState state) => _currentMenuState = state;
        }

        private void ConfigureStates()
        {
            _machine.Configure(SongMenuState.Music)
                    .PermitReentry(MusicNavigationCommand.OpenMusic)
                    .Permit(MusicNavigationCommand.OpenMoods, SongMenuState.Moods)
                    .Permit(MusicNavigationCommand.OpenUploads, SongMenuState.Upload)
                    .Permit(MusicNavigationCommand.OpenFavorites, SongMenuState.Favorites)
                    .InternalTransitionAsync(PlaylistTrigger, OpenPlaylistAsync)
                    .InternalTransitionAsync(PlaylistsTrigger, OpenPlaylistsViewAsync)
                    .InternalTransitionAsync(GenresTrigger, OpenGenresViewAsync)
                    .InternalTransition(MusicNavigationCommand.ClosePlaylist, ClosePlaylist)
                    .InternalTransition(MusicNavigationCommand.ClosePlaylists, ClosePlaylistsView)
                    .InternalTransition(MusicNavigationCommand.CloseGenres, CloseGenresView);

            _machine.Configure(SongMenuState.Moods)
                    .Permit(MusicNavigationCommand.MoveBack, SongMenuState.Music)
                    .InternalTransitionAsync(PlaylistTrigger, OpenPlaylistAsync)
                    .InternalTransitionAsync(PlaylistsTrigger, OpenPlaylistsViewAsync)
                    .InternalTransitionAsync(GenresTrigger, OpenGenresViewAsync)
                    .InternalTransition(MusicNavigationCommand.ClosePlaylist, ClosePlaylist)
                    .InternalTransition(MusicNavigationCommand.ClosePlaylists, ClosePlaylistsView)
                    .InternalTransition(MusicNavigationCommand.CloseGenres, CloseGenresView)
                    .OnEntryAsync( async () => {
                         // TODO: replace with parametrized trigger
                         if (!_moodsCategoryView.Initialized)
                         {
                            var musicViewModel = new MusicViewModel();
                            await _moodsCategoryView.InitializeAsync(musicViewModel, _tokenSource.Token);
                         }
                         
                         if (_tokenSource.IsCancellationRequested) return;
                         
                         _moodsCategoryView.ShowContent();
                     })
                    .OnExit(() =>
                     {
                         _moodsCategoryView.HideContent();
                     });
            
            _machine.Configure(SongMenuState.Upload)
                    .Permit(MusicNavigationCommand.MoveBack, SongMenuState.Music)
                    .InternalTransitionAsync(SoundSettingsTrigger, OpenSoundSettingsAsync)
                    .InternalTransition(MusicNavigationCommand.CloseSoundSettings, CloseSoundsSettings)
                    .OnEntryAsync( async () => {
                         var musicViewModel = new MusicViewModel();
                         if (!_uploadsCategoryView.Initialized)
                         {
                            await _uploadsCategoryView.InitializeAsync(musicViewModel, _tokenSource.Token);
                         }
                         
                         if (_tokenSource.IsCancellationRequested) return;
                         
                         _uploadsCategoryView.ShowContent();
                     })
                    .OnExit(() =>
                     {
                         _uploadsCategoryView.HideContent();
                     });

            _machine.Configure(SongMenuState.Favorites)
                    .Permit(MusicNavigationCommand.MoveBack, SongMenuState.Music)
                    .OnEntryAsync( async () => {
                         if (!_favoritesCategoryView.Initialized)
                         {
                            var musicViewModel = new MusicViewModel();
                            await _favoritesCategoryView.InitializeAsync(musicViewModel, _tokenSource.Token);
                         }
                         
                         if (_tokenSource.IsCancellationRequested) return;
                         
                         _favoritesCategoryView.ShowContent();
                     })
                    .OnExit(() =>
                     {
                         _favoritesCategoryView.HideContent();
                         _favoritesCategoryView.CleanUp();
                     });
        }

        private async Task OpenPlaylistAsync(FullPlaylistListModel model, StateMachine<SongMenuState, MusicNavigationCommand>.Transition _)
        {
            await _fullPlaylistView.InitializeAsync(model, _tokenSource.Token);
            
            if (_tokenSource.IsCancellationRequested) return;
            
            _fullPlaylistView.ShowContent();
        }

        private void ClosePlaylist()
        {
            _fullPlaylistView.HideContent();
            _fullPlaylistView.CleanUp();
        }

        private async Task OpenPlaylistsViewAsync(PlaylistsViewModel model, StateMachine<SongMenuState, MusicNavigationCommand>.Transition _)
        {
            await _playlistsView.InitializeAsync(model, _tokenSource.Token);
            
            if (_tokenSource.IsCancellationRequested) return;
            
            _playlistsView.ShowContent();
        }
        
        private void ClosePlaylistsView()
        {
            _playlistsView.HideContent();
            _playlistsView.CleanUp();
        }
        
        private async Task OpenGenresViewAsync(GenresViewModel model, StateMachine<SongMenuState, MusicNavigationCommand>.Transition _)
        {
            await _genresView.InitializeAsync(model, _tokenSource.Token);
            
            if (_tokenSource.IsCancellationRequested) return;
            
            _genresView.ShowContent();
        }
        
        private void CloseGenresView()
        {
            _genresView.HideContent();
            _genresView.CleanUp();
        }

        private async Task OpenSoundSettingsAsync(SoundSettingsViewModel model, StateMachine<SongMenuState, MusicNavigationCommand>.Transition _)
        {
            await _soundSettingsView.InitializeAsync(model, _tokenSource.Token);
            
            if (_tokenSource.IsCancellationRequested) return;
            
            _soundSettingsView.ShowContent();
        }

        private void CloseSoundsSettings()
        {
            _soundSettingsView.HideContent();
            _soundSettingsView.CleanUp();
        }
    }
}
