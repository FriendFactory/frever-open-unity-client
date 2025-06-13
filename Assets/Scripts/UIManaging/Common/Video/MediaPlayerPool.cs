using System.Collections.Generic;
using Extensions;
using Navigation.Args;
using RenderHeads.Media.AVProVideo;
using StansAssets.Foundation.Patterns;
using UnityEngine;

namespace UIManaging.Common
{
    public sealed class MediaPlayerPool : MonoSingleton<MediaPlayerPool>
    {
        [SerializeField] private MediaPlayer _playerPrototype;

        private readonly List<VideoThumbnail> _toggleQueue = new List<VideoThumbnail>();
        private readonly HashSet<MediaPlayer> _activePlayers = new HashSet<MediaPlayer>();
        private readonly Stack<MediaPlayer> _pooledPlayers = new Stack<MediaPlayer>();
        private uint _queueDelayCounter;

        private uint VideoInitializationQueueDelayFrames
        {
            get
            {
                #if UNITY_ANDROID
                return 1;
                #else
                return 0;
                #endif
            }
        }
        
        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------        
        
        private void Update()
        {
            PlayerInitializationQueue();
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public void AddToInitializationQueue(VideoThumbnailModel videoThumbnail)
        {
            videoThumbnail.OnPlayerVisible += PlayerVisible;
        }
        
        public void Clear()
        {
            foreach (var player in _pooledPlayers)
            {
                Destroy(player);
            }
            
            _pooledPlayers.Clear();
            
            foreach (var player in _activePlayers)
            {
                Destroy(player);
            }
            
            _activePlayers.Clear();
        }
        
        //---------------------------------------------------------------------
        // Private
        //---------------------------------------------------------------------
        
        private MediaPlayer Get()
        {
            var player = _pooledPlayers.Count > 0 
                ? _pooledPlayers.Pop() 
                : MediaPlayer.Instantiate(_playerPrototype, transform);
            _activePlayers.Add(player);
            player.SetActive(true);
            return player;
        }

        private void Return(VideoThumbnail thumbnail)
        {
            var player = thumbnail.MediaPlayer;
            
            if(!player) return;
            
            player.Events.RemoveAllListeners();
            player.Pause();
            player.PlaybackRate = 1f;
            player.CloseMedia();
            
            thumbnail.MediaPlayer = null;
            
            _activePlayers.Remove(player);
            _pooledPlayers.Push(player);
            player.SetActive(false);
        }

        private void PlayerInitializationQueue()
        {
            if (_toggleQueue.Count <= 0)
            {
                _queueDelayCounter = VideoInitializationQueueDelayFrames;
                return;
            }

            if (++_queueDelayCounter < VideoInitializationQueueDelayFrames) return;

            if (VideoInitializationQueueDelayFrames > 0)
            {
                _queueDelayCounter = 0;
                RunNextPlayer();
            }
            else
            {
                RunAllPlayers();
            }
        }

        private void RunNextPlayer()
        {
            _toggleQueue[0].SetupMediaPlayer();
            _toggleQueue.RemoveAt(0);
        }

        private void RunAllPlayers()
        {
            foreach (var videoThumbnail in _toggleQueue)
            {
                videoThumbnail.SetupMediaPlayer();
            }
            _toggleQueue.Clear();
        }

        private void PlayerVisible(object sender)
        {
            var videoThumbnail = (VideoThumbnail) sender;
            videoThumbnail.MediaPlayer = Get();
            _toggleQueue.Add(videoThumbnail);
            
            videoThumbnail.ContextData.OnPlayerVisible -= PlayerVisible;
            
            videoThumbnail.ContextData.OnPlayerError += PlayerError;
            videoThumbnail.ContextData.OnPlayerDisabled += PlayerDisabled;
            videoThumbnail.ContextData.OnPlayerCleared += PlayerCleared;
        }

        private void PlayerError(object sender)
        {
            var videoThumbnail = (VideoThumbnail) sender;
            if(videoThumbnail.MediaPlayer) videoThumbnail.MediaPlayer.CloseMedia();
            _toggleQueue.Add(videoThumbnail);
        }
        
        private void PlayerDisabled(object sender)
        {
            var videoThumbnail = (VideoThumbnail) sender;
            
            Return(videoThumbnail);
            
            videoThumbnail.ContextData.OnPlayerVisible += PlayerVisible;
            
            videoThumbnail.ContextData.OnPlayerError -= PlayerError;
            videoThumbnail.ContextData.OnPlayerDisabled -= PlayerDisabled;
            videoThumbnail.ContextData.OnPlayerCleared -= PlayerCleared;
            
            _toggleQueue.Remove(videoThumbnail);
        }

        private void PlayerCleared(object sender)
        {
            var videoThumbnail = (VideoThumbnail) sender;
            
            Return(videoThumbnail);
            
            videoThumbnail.ContextData.OnPlayerVisible -= PlayerVisible;
            videoThumbnail.ContextData.OnPlayerError -= PlayerError;
            videoThumbnail.ContextData.OnPlayerDisabled -= PlayerDisabled;
            videoThumbnail.ContextData.OnPlayerCleared -= PlayerCleared;
            
            _toggleQueue.Remove(videoThumbnail);
        }
    }
}