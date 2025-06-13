using System.Linq;
using Common;
using Common.TimeManaging;
using Extensions;
using Modules.LevelManaging.Editing.Players.AssetPlayers;
using UnityEngine;
using Zenject;

namespace Modules.LevelManaging.Editing.Players.EventPlaying.Algorithms
{
    /// <summary>
    /// Play 1 loop of preview and stops
    /// </summary>
    internal abstract class EventSingleLoopPreviewAlgorithmBase: EventPlayingAlgorithm, ITickable
    {
        private readonly ITimeSourceControl _eventTimeSourceControl;
        private float _currentFrameTime;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        protected EventSingleLoopPreviewAlgorithmBase(IPlayersManager playerManager, EventAssetsProvider eventAssetsProvider, ITimeSourceControl eventTimeSourceControl) 
            : base(playerManager, eventAssetsProvider)
        {
            _eventTimeSourceControl = eventTimeSourceControl;
        }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        
        public void Tick()
        {
            if (!IsRunning) return;

            _currentFrameTime += Time.deltaTime;
            
            _eventTimeSourceControl.SetElapsed(_currentFrameTime);
            
            if (_currentFrameTime < EventLength) return;
      
            //allow to finish playing current(last) frame
            CoroutineSource.Instance.ExecuteAtEndOfFrame(OnEventDone);
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnPlayStarted()
        {
            base.OnPlayStarted();
            _currentFrameTime = 0;
            _eventTimeSourceControl.SetElapsed(_currentFrameTime);
        }
        
        protected override void OnEventDone()
        {
            var characterAssetPlayers = CurrentPlayers.Where(x => x.TargetType == DbModelType.Character).ToArray();
            
            for (var i = 0; i < characterAssetPlayers.Length; i++)
            {
                characterAssetPlayers[i]?.Stop();
            }

            Stop();
            base.OnEventDone();
        }
    }
}