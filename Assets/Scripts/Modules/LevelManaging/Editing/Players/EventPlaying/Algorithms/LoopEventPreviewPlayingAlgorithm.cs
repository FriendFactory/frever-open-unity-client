using Common.TimeManaging;
using JetBrains.Annotations;
using Modules.LevelManaging.Editing.Players.AssetPlayers;
using UnityEngine;
using Zenject;

namespace Modules.LevelManaging.Editing.Players.EventPlaying.Algorithms
{
    [UsedImplicitly]
    internal sealed class LoopEventPreviewPlayingAlgorithm: EventPlayingAlgorithm, ITickable
    {
        private readonly ITimeSourceControl _eventTimeSourceControl;
        private float _currentFrameTime;
        
        public LoopEventPreviewPlayingAlgorithm(IPlayersManager playerManager, EventAssetsProvider eventAssetsProvider, ITimeSourceControl timeSourceControl) : base(playerManager, eventAssetsProvider)
        {
            _eventTimeSourceControl = timeSourceControl;
        }

        protected override void OnPlayStarted()
        {
            base.OnPlayStarted();
            _currentFrameTime = 0;
        }

        public void Tick()
        {
            if (!IsRunning) return;

            _currentFrameTime += Time.deltaTime;
            
            _eventTimeSourceControl.SetElapsed(_currentFrameTime);
            
            if (_currentFrameTime < EventLength) return;

            Restart();
        }

        private void Restart()
        {
            _currentFrameTime = 0;
            Play();
        }
    }
}