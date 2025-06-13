using System;
using System.Threading.Tasks;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.ClientServer.Level.Full;
using Bridge.Models.ClientServer.Template;
using Bridge.Models.Common;
using Extensions;
using Models;
using Modules.LevelManaging.Assets;
using UnityEngine;
using Event = Models.Event;
using IAsset = Modules.LevelManaging.Assets.IAsset;
using PlayMode = Modules.LevelManaging.Editing.Players.EventPlaying.PlayMode;
using CharacterSpawnPositionFormation = Bridge.Models.ClientServer.StartPack.Metadata.CharacterSpawnPositionFormation;

namespace Modules.LevelManaging.Editing.LevelManagement
{
    public interface ILevelEditor
    {
        event Action EventDeletionStarted;
        event Action EventDeleted;
        event Action TemplateApplyingStarted;
        event Action TemplateApplyingCompleted;
        public event Action<ApplyingTemplateArgs> TemplateApplied;
       
        event Action<DbModelType, long> StartUpdatingAsset;
        event Action<DbModelType, long> StopUpdatingAsset;
        event Action<IEntity> AssetLoaded;
        event Action<DbModelType> AssetUpdateCompleted;
        event Action<DbModelType> AssetUpdateFailed;
        event Action<DbModelType> AssetUpdateCancelled;
        event Action<DbModelType, long> AssetUpdateStarted;
        event Action CharactersPositionsSwapped; 
        event Action<long?> SpawnFormationSetup;
        event Action<long?> SpawnFormationChanged;
        event Action<ISetLocationAsset> SetLocationChangeFinished;
        event Action BodyAnimationChanged;
        event Action SpawnPositionChanged;
        event Action SongChanged;
        event Action<Event> TargetEventChanged;
        event Action CurrentLevelChanged;
        event Action UseSameFaceFxChanged;
        event Action ShufflingBegun;
        event Action ShufflingDone;
        event Action ShufflingFailed;
        event Action PhotoOnSetLocationChanged;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        Level CurrentLevel { get; set; }
        float LevelDurationSeconds { get; }
        float MaxLevelDurationSec { get; }
        float MinEventDurationMs { get; }
        Event TargetEvent { get; }
        bool IsSongSelected { get; }
        bool UseSameBodyAnimation { get; set; }
        bool UseSameFaceFx { get; set; }
        int VoiceVolume { get; }
        int MusicVolume { get; }
        bool IsLevelEmpty { get;}
        bool IsDeletingEvent { get; }
        bool IsChangingAsset { get; }
        bool IsFaceTrackingEnabled { get; }
        bool IsShuffling { get; }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        Event GetLastEvent();
        void PrepareNewEventBasedOnTarget();
        Level CreateLevelInstance();
        void SaveLevel(Level level, Action<Level> onSaved, Action<string> onFailure);
        void SaveRecordedEvent();
        void SaveEditingEvent();
        Task DeleteLastEvent();
        void DeleteEvent(long eventId);
        void SetTargetEvent(Event @event, Action onEventLoaded, PlayMode playMode);
        void RefreshTargetEventAssets();
        void CreateFreshEventBasedOnTemplate(ApplyingTemplateArgs args);
        void CreateFreshEventBasedOnTemplate(TemplateInfo info, CharacterFullInfo[] characters);
        void UnlinkEventFromTemplate();
        void ReplaceEvent(Event @event);
        bool CanUseForRecording(IPlayableMusic song, ref string reason);
        float GetAllowedDurationForNextRecordingInSec(long externalSongId);
        bool CanUseForReplacing(IPlayableMusic song, int activationCue, ref string reason);
        void ChangeSong(IPlayableMusic song, int activationCue = 0,  Action<IAsset> onCompleted = null);
        void SetVoiceVolume(int volume);
        void SetMusicVolume(int volume);
        int GetMusicActivationCue();
        float GetMusicVolume();
        void ApplySongStartingFromTargetEvent(IPlayableMusic playableMusic, int activationCue, Action onApplied);
        void SaveDayNightControllerValues();
        void SetFaceTracking(bool isEnabled);
        void Change<T>(T next, Action<IAsset> onCompleted = null, Action onCancelled = null, bool unloadPrevious = true, long subAssetId = -1L) where T : class, IEntity;
        void ChangeSetLocation(SetLocationFullInfo setLocation, Action<ISetLocationAsset, DbModelType[]> onCompleted = null,
            Action onCancelled = null, long? spawnPositionId = null, bool allowChangingAnimations = true, bool recenterCamera = false);
        void ChangeBodyAnimation(BodyAnimationInfo bodyAnimation, Action callback);
        void ChangeCharacterSpawnPosition(CharacterSpawnPositionInfo spawnPosition, bool allowChangingAnimations);
        void ChangeCameraAnimation(CameraAnimationFullInfo next, string animationString);
        void ApplyFormation(CharacterSpawnPositionFormation formation);
        void ChangeCameraFilter(CameraFilterInfo cameraFilter, long variantId, Action<IAsset> onCompleted = null);
        void RemoveCameraFilter(Action onRemoved);
        void ApplySetLocationBackground(PhotoFullInfo photo);
        void ApplySetLocationBackground(SetLocationBackground background);
        void ApplySetLocationBackground(VideoClipFullInfo videoClip);
        void ResetSetLocationBackground();
        void ApplyRenderTextureToSetLocationCamera(RenderTexture renderTexture);
        void AddCaption(CaptionFullInfo caption);
        void RemoveCaption(long captionId);
        void RefreshCaption(CaptionFullInfo caption);
        void Shuffle(ShuffleModel shuffleModel, Action callback = null);//todo: delete as obsolete
        void ShuffleAI(ShuffleModel shuffleModel, Action callback = null);
        void CleanUp();
        void ReplaceCharacter(CharacterFullInfo oldCharacter, CharacterFullInfo newCharacter, bool unloadOld, Action<ICharacterAsset> onSuccess);
        void SpawnCharacter(CharacterFullInfo character, Action<ICharacterAsset> onSuccess);
        void DestroyCharacter(CharacterFullInfo target, Action onSuccess = null);
        void UnloadFaceAndVoice();
        void UnloadNotUsedByLevelAssets(Level level = null);
        void UnloadNotUsedByTargetEventAssets();
        void UnloadNotUsedByEventsAssets(params Event[] events);
        void UnloadAllAssets();
        void UnloadAsset(IAsset asset);
        void UnloadAllAssets(params IAsset[] except);
        void DeactivateAllAssets();
        void CancelLoadingCurrentAssets();
        Task<bool> IsLevelModified(Level original, Level current);
        void SetFilterValue(float value);
        void RemoveAllVfx();
        void PreventUnloadingUsedLicensedSongs();
        void ReleaseNotUsedLicensedSongs();
    }
}