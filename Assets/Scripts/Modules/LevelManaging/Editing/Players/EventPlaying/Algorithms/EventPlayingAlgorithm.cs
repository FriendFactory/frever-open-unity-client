using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Editing.Players.AssetPlayers;
using Event = Models.Event;

namespace Modules.LevelManaging.Editing.Players.EventPlaying.Algorithms
{
    internal abstract class EventPlayingAlgorithm
    {
        protected static readonly HashSet<DbModelType> AUDIO_ASSET_TYPES = new HashSet<DbModelType>() 
        {
            DbModelType.UserSound,
            DbModelType.VoiceTrack,
            DbModelType.Song,
            DbModelType.ExternalTrack
        };

        private readonly EventAssetsProvider _eventAssetsProvider;
        protected readonly IPlayersManager PlayerManager;
        protected float EventLength;
        protected List<IAssetPlayer> CurrentPlayers = new List<IAssetPlayer>();
        private IReadOnlyCollection<DbModelType> _assetTypesToIgnore;
        private DbModelType[] _targetPlayingTypes;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public bool IsRunning { get; private set; }

        private IReadOnlyCollection<DbModelType> AssetTypesToIgnore => _assetTypesToIgnore ?? (_assetTypesToIgnore = GetAssetTypesToIgnore());

        private IReadOnlyCollection<DbModelType> AllPlayingTypes => PlayerManager.PlayableTypes;

        protected DbModelType[] TargetPlayingTypes => _targetPlayingTypes ?? (_targetPlayingTypes = AllPlayingTypes.Where(ShouldPlayAsset).ToArray());

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        public event Action EventDone;
        public event Action EventStarted;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        protected EventPlayingAlgorithm(IPlayersManager playerManager, EventAssetsProvider eventAssetsProvider)
        {
            PlayerManager = playerManager;
            _eventAssetsProvider = eventAssetsProvider;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Init(Event ev)
        {
            EventLength = ev.Length.ToSeconds();
            if (CurrentPlayers != null)
            {
                DisposePlayers(CurrentPlayers);
            }
            CurrentPlayers = CreatePlayers(ev);
            SetupPlayers(ev);
        }

        private void SetupPlayers(Event ev)
        {
            foreach (var playingType in AllPlayingTypes)
            {
                SetupPlayers(ev, playingType);
            }
        }

        public void Play()
        {
            IsRunning = true;
            OnPlayStarted();
            OnEventStarted();
        }

        public void Pause()
        {
            if(!IsRunning) return;
            
            foreach (var assetPlayer in CurrentPlayers)
            {
                assetPlayer.Pause();
            }
            
            IsRunning = false;
        }

        public void Resume()
        {
            IsRunning = true;
            
            foreach (var assetPlayer in CurrentPlayers)
            {
                assetPlayer.Resume();
            }
        }

        public void Stop()
        {
            StopEvent();
            OnStopped();
        }

        public void Cancel()
        {
            OnCanceled();
            StopEvent();
        }
        
        public void RefreshAssetPlayers(DbModelType targetType, Event ev)
        {
            if(!ShouldPlayAsset(targetType)) return;
            
            RemovePlayers(targetType);
            var newPlayers = CreatePlayers(targetType, ev);
            CurrentPlayers.AddRange(newPlayers);
            SetupPlayers(ev, targetType);
            OnNewPlayersAdded(newPlayers);
            if (!IsRunning) return;
            foreach (var player in newPlayers)
            {
                player.Play();
            }
        }

        public void RemoveAllAssetPlayers()
        {
            DisposePlayers(CurrentPlayers);
            CurrentPlayers.Clear();
        }

        public void PauseAudio()
        {
            var audioPlayer = CurrentPlayers.FirstOrDefault(x => x is IAudioAssetPlayer);
            audioPlayer?.Pause();
        }

        public void PauseCaption(long captionId)
        {
            var captionPlayer = CurrentPlayers.FirstOrDefault(x => x.AssetId == captionId && x is CaptionAssetPlayer);
            captionPlayer?.Pause();
        }
        
        public void ResumeCaption(long captionId)
        {
            var captionPlayer = CurrentPlayers.FirstOrDefault(x => x.AssetId == captionId && x is CaptionAssetPlayer);
            captionPlayer?.Resume();
        }

        public virtual void Simulate(float time, params DbModelType[] targetTypes)
        {
            foreach (var player in CurrentPlayers)
            {
                if(!targetTypes.Contains(player.TargetType)) continue;
                player.Simulate(time);
            }
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected virtual void OnPlayStarted()
        {
            RunPlayers();
        }
        
        protected virtual void OnCanceled()
        {
        }

        protected virtual void OnStopped()
        {
        }

        protected virtual IReadOnlyCollection<DbModelType> GetAssetTypesToIgnore()
        {
            return Array.Empty<DbModelType>();
        }

        protected virtual void OnNewPlayersAdded(ICollection<IAssetPlayer> players)
        {
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void RunPlayers()
        {
            foreach (var assetPlayer in CurrentPlayers)
            {
                assetPlayer.Play();
            }
        }

        private void SetupPlayers(Event ev, DbModelType targetType)
         {
             var setup = PlayerManager.GetSetup(targetType);

             var players = CurrentPlayers.Where(x => x.TargetType == targetType).ToArray();
             foreach (var player in players)
             {
                 setup.Setup(player, ev);
             }
         }

        private void StopEvent()
        {
            if (!IsRunning) return;

            IsRunning = false;

            foreach (var assetPlayer in CurrentPlayers)
            {
                assetPlayer.Stop();
            }
        }

        private List<IAssetPlayer> CreatePlayers(Event ev)
        {
            var output = new List<IAssetPlayer>();
            
            foreach (var dbModel in AllPlayingTypes)
            {
                if(!ShouldPlayAsset(dbModel)) continue;
                
                var players = CreatePlayers(dbModel, ev);
                output.AddRange(players);
            }

            return output;
        }

        private void RemovePlayers(DbModelType targetType)
        {
            DisposePlayers(CurrentPlayers.Where(x => x.TargetType == targetType));
            CurrentPlayers = CurrentPlayers.Where(x => x.TargetType != targetType).ToList();
        }

        private List<IAssetPlayer> CreatePlayers(DbModelType type, Event ev)
        {
            var assets = _eventAssetsProvider.GetLoadedAssets(ev, type);
            return CreatePlayers(assets);
        }

        private List<IAssetPlayer> CreatePlayers(IAsset[] assets)
        {
            var players = new List<IAssetPlayer>();

            foreach (var asset in assets)
            {
                var player = PlayerManager.CreateAssetPlayer(asset);
                players.Add(player);
            }

            return players;
        }

        protected virtual void OnEventDone()
        {
            EventDone?.Invoke();
        }

        private void OnEventStarted()
        {
            EventStarted?.Invoke();
        }

        private bool ShouldPlayAsset(DbModelType assetType)
        {
            return PlayerManager.IsSupported(assetType) && !AssetTypesToIgnore.Contains(assetType);
        }

        private void DisposePlayers(IEnumerable<IAssetPlayer> players)
        {
            foreach (var player in players)
            {
                player.Cleanup();
            }
        }
    }
}