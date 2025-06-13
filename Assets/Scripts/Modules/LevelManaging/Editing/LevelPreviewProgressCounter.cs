using System;
using Modules.LevelManaging.Editing.LevelManagement;
using UnityEngine;
using Zenject;

namespace Modules.LevelManaging.Editing
{
    public sealed class LevelPreviewProgressCounter: ITickable
    {
        public event Action<float> PreviewElapsed; 
        
        private readonly ILevelPlayer _levelPlayer;
        private float _levelDurationSec;
        private float _currentProgress;
        private float _playedSec;
        private bool _isRunning;

        public LevelPreviewProgressCounter(ILevelPlayer levelPlayer)
        {
            _levelPlayer = levelPlayer;
        }

        public void Run()
        {
            _levelDurationSec = _levelPlayer.LevelDurationSec;
            _levelPlayer.LevelPiecePlayingCompleted += Pause;
            _levelPlayer.NextLevelPiecePlayingStarted += Resume;
            _levelPlayer.LevelPreviewCompleted += Stop;
            _levelPlayer.PreviewCancelled += Stop;
            
            if (_levelPlayer.IsRunningLevelPreview)
            {
                Start();
            }
            else
            {
                _levelPlayer.LevelPreviewStarted += Start;
            }
        }
        
        public void Tick()
        {
            if(!_isRunning) return;
            _playedSec += Time.deltaTime;
            _currentProgress = _playedSec / _levelDurationSec;
            
            PreviewElapsed?.Invoke(_currentProgress);
        }

        private void Start()
        {
            _currentProgress = 0;
            _playedSec = 0;
            _isRunning = true;
        }
        
        private void Resume()
        {
            _isRunning = true;
        }

        private void Pause()
        {
            _isRunning = false;
        }

        private void Stop()
        {
            _isRunning = false;
            _levelPlayer.LevelPreviewStarted -= Start;
            _levelPlayer.LevelPiecePlayingCompleted -= Pause;
            _levelPlayer.NextLevelPiecePlayingStarted -= Resume;
            _levelPlayer.LevelPreviewCompleted -= Stop;
            _levelPlayer.PreviewCancelled -= Stop;
        }
    }
}