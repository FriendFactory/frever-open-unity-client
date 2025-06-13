using System;
using System.Linq;
using System.Threading;
using Common;
using Extensions;
using Modules.AssetsManaging.LoadArgs;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Editing.Players;
using UnityEngine;
using Event = Models.Event;
using PlayMode = Modules.LevelManaging.Editing.Players.EventPlaying.PlayMode;

namespace Modules.LevelManaging.Editing.LevelManagement
{
    internal sealed partial class LevelManager
    {
        public event Action EventStarted;
        public event Action EventPreviewStarted;
        public event Action EventSaved;
        public event Action EventLoadingStarted;
        public event Action EventLoadingCompleted;
        public event Action PlayingEventSwitched;
        public event Action LevelPreviewStarted;
        public event Action LevelPreviewCompleted;
        public event Action EventPreviewCompleted;
        public event Action NextLevelPiecePlayingStarted;
        public event Action LevelPiecePlayingCompleted;
        public event Action PreviewCancelled;
        public PlayMode CurrentPlayMode => _previewManager.PlayMode;
        public PlayingType CurrentPlayingType => _previewManager.PlayingType;
        public float LevelDurationSec => CalculateEventsDuration();
        public bool IsRunningLevelPreview { get; private set; }

        partial void InitializeLevelPlayer()
        {
            _previewManager.NextLevelPiecePlayingStarted += () => NextLevelPiecePlayingStarted?.Invoke();
            _previewManager.LevelPiecePlayingCompleted += () => LevelPiecePlayingCompleted?.Invoke();
        }
        
        public void PlayLevelPreview(MemoryConsumingMode previewMode, PreviewCleanMode cleanMode, RenderTexture renderTexture = null)
        {
            PlayLevelPreview(0, previewMode, cleanMode, renderTexture);
        }

        public void PlayLevelPreviewForRendering(bool isLandscape)
        {
            SetPictureInPictureRenderScale(Constants.VideoMessage.SCALE_MAX);//for video message best quality
            var ip = _metadataStartPack.GetIntellectualPropertyForLevel(CurrentLevel);
            if (ip.Watermark != null)
            {
                LevelPreviewStarted += SetupWatermark;
                _watermarkService.FetchWaterMark(ip);
            }
            LevelPreviewCompleted += RevertBackPictureInPictureScale;
            PreviewCancelled += RevertBackPictureInPictureScale;
            
            PlayLevelPreview(MemoryConsumingMode.SafeMemory, PreviewCleanMode.ReleaseAll);

            void SetupWatermark()
            {
                LevelPreviewStarted -= SetupWatermark;
                _watermarkControl.StartShowing(ip, isLandscape);
            }
            
            return;
            void RevertBackPictureInPictureScale()
            {
                LevelPreviewCompleted -= RevertBackPictureInPictureScale;
                PreviewCancelled -= RevertBackPictureInPictureScale;
                SetPictureInPictureRenderScale(1);
            }
            
            void SetPictureInPictureRenderScale(float scale)
            {
                var setLocations = _assetManager.GetAllLoadedAssets<ISetLocationAsset>();
                foreach (var setLocationAsset in setLocations)
                {
                    setLocationAsset.SetPictureInPictureRenderScale(scale);
                }
                SetLocationLoadArgs.PictureInPictureRenderScale = scale;
            }
        }

        public void PlayLevelPreview(int firstEventIndex, MemoryConsumingMode previewMode, PreviewCleanMode cleanMode, RenderTexture renderTexture = null)
        {
            _previewManager.PreviewStarted += OnPreviewStarted;
            _previewManager.PlayLevelPreview(CurrentLevel, firstEventIndex, previewMode, cleanMode, renderTexture, ()=>
            {
                IsRunningLevelPreview = true;
                OnEventStarted();
            }, OnPreviewCompleted);
        }

        public void PlayEventPreview(Event targetEvent, Action onStarted = null, Action onCompleted = null)
        {
            if (targetEvent == null) return;

            onStarted += OnEventStarted;
            onStarted += OnEventPreviewStarted;
            _previewManager.PreviewStarted += OnPreviewStarted;
            _previewManager.PlayEvent(PlayMode.Preview, targetEvent, PreviewCleanMode.KeepAll ,onStarted, ()=>
            {
                onCompleted?.Invoke();
                EventPreviewCompleted?.Invoke();
            });
        }

        public void PutEventOnFirstFrame(Action onCompleted = null)
        {
            PlayEvent(PlayMode.StayOnFirstFrame, TargetEvent, onStartPlay: onCompleted);
        }

        public void Simulate(float time, PlayMode? playMode = null, params DbModelType[] assetsToSimulate)
        {
            //We have limitation: we can simulate body animation pose, but not transform position/rotation if animation applies root motion.
            //For fixing bugs related to moving animations we have such way: if user tries to simulate first frame of such event
            //we just put characters on start position(spawn position). Proper fix requires long term solution with saving character moving curve
            if (time < 0.0001f)
            {
                ForceCharactersToStartPosition();
            }

            var needChangePlayMode = playMode.HasValue && _previewManager.PlayMode != playMode.Value;
            if (needChangePlayMode)
            {
                _previewManager.ChangePlayMode(playMode.Value);
            }
            _previewManager.Simulate(time, assetsToSimulate);
        }

        public void StopAudio()
        {
            _previewManager.PauseAudio();
        }

        public void PauseCaption(long captionId)
        {
            _previewManager.PauseCaption(captionId);
        }

        public void ResumeCaption(long captionId)
        {
            _previewManager.ResumeCaption(captionId);
        }

        public void CancelTargetEventPreview()
        {
            _eventEditor.ResetEventLoadingCallback();
            _previewManager.CancelTargetEventPreview();
            OnPreviewCancelled();
        }

        public void CancelPreview(PreviewCleanMode cleanMode = PreviewCleanMode.KeepAll)
        {
            if (CurrentPlayingType == PlayingType.SingleEvent)
            {
                CancelTargetEventPreview();
            }
            else
            {
                CancelLevelPreview(cleanMode);
            }
        }

        private void CancelLevelPreview(PreviewCleanMode cleanMode)
        {
            _eventEditor.ResetEventLoadingCallback();
            _previewManager.CancelLevelPreview(cleanMode);
            _assetManager.CancelLoadingCurrentAssets();
            OnPreviewCancelled();
        }

        public void PlayEvent(PlayMode mode, Event targetEvent, Action onStartPlay = null, Action onEventPlayed = null)
        {
            _previewManager.EventStarted += OnEventStarted;
            _eventLoadingCancellationSource = new CancellationTokenSource();
            onStartPlay += () => _eventLoadingCancellationSource = null;
            _previewManager.PlayEvent(mode, targetEvent ?? TargetEvent, PreviewCleanMode.KeepLastEvent ,onStartPlay, onEventPlayed, _eventLoadingCancellationSource.Token);
        }
        
        public void PauseEventPlayMode()
        {
            _previewManager.PauseEventPlayMode();
        }

        public void ResumeEventPlayMode()
        {
            _previewManager.ResumeEventPlayMode();
        }

        public void StopCurrentPlayMode()
        {
            _previewManager.StopEventPlayMode();

            if (_previewManager.PlayMode == PlayMode.Preview)
            {
                OnPreviewCancelled();
            }
        }

        private void OnPreviewStarted()
        {
            _previewManager.PreviewStarted -= OnPreviewStarted;
            LevelPreviewStarted?.Invoke();
        }

        private void OnPreviewCompleted()
        {
            IsRunningLevelPreview = false;
            LevelPreviewCompleted?.Invoke();
        }

        private void OnEventStarted()
        {
            _previewManager.EventStarted -= OnEventStarted;
            EventStarted?.Invoke();
        }

        private void OnEventPreviewStarted()
        {
            EventPreviewStarted?.Invoke();
        }

        private void OnPreviewCancelled()
        {
            PreviewCancelled?.Invoke();
        }

        private void RefreshAssetsOnScene(DbModelType type)
        {
            switch (type)
            {
                case DbModelType.Character:
                    RefreshCharacterRelatedAssets();
                    return;
                case DbModelType.SetLocation:
                    _previewManager.RefreshPlayers(TargetEvent, DbModelType.SetLocation, DbModelType.VideoClip, DbModelType.UserPhoto, DbModelType.SetLocationBackground);
                    break;
                default: 
                    _previewManager.RefreshPlayers(TargetEvent, type);
                    return;
            }
        }
        
        private void RefreshCharacterRelatedAssets()
        {
            RefreshAssetsOnScene(DbModelType.BodyAnimation);
            RefreshAssetsOnScene(DbModelType.VoiceTrack);
            RefreshAssetsOnScene(DbModelType.FaceAnimation);
        }

        private void ForceCharactersToStartPosition()
        {
            var setLocation = GetTargetEventSetLocationAsset();
            var characters = GetCurrentCharactersAssets();

            var hasAppliedSpawnFormation = TargetEvent.AreCharactersOnTheSameSpawnPosition();
            if (hasAppliedSpawnFormation)
            {
                _characterSpawnFormationChanger.Run(TargetEvent.CharacterSpawnPositionFormationId ,characters, TargetEvent.CharacterController.ToArray(), setLocation, TargetEvent.CurrentCharacterSpawnPosition());
            }
            else
            {
                setLocation.ResetPosition(characters);
            }
        }
    }
}