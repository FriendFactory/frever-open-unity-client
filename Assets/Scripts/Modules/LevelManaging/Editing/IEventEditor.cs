using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.ClientServer.Level.Shuffle;
using Bridge.Models.Common;
using Extensions;
using Models;
using Modules.LevelManaging.Assets;
using IAsset = Modules.LevelManaging.Assets.IAsset;
using CharacterSpawnPositionFormation = Bridge.Models.ClientServer.StartPack.Metadata.CharacterSpawnPositionFormation;

namespace Modules.LevelManaging.Editing
{
    internal interface IEventEditor
    {
        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------
        
        event Action<DbModelType> Updated;
        event Action<DbModelType> AssetUpdatingFailed;
        event Action<DbModelType, long> AssetStartedUpdating;
        event Action<long?> SpawnFormationSetup;
        event Action<long?> SpawnFormationChanged;
        event Action UseSameFaceFxChanged;
        event Action EventLoadingStarted;
        event Action EventLoadingComplete;
        event Action CharactersOutfitsUpdatingBegan;
        event Action<Event> TargetEventChanged;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        Event TargetEvent { get; }
        int TargetCharacterSequenceNumber { get; set; }
        int LastNonGroupTargetCharacterSequenceNumber { get; set; }
        int EditingCharacterSequenceNumber { get; set; }
        ICharacterAsset TargetCharacterAsset { get; }
        CharacterController TargetCharacterController { get; }
        bool UseSameBodyAnimation { get; set; }
        bool UseSameFaceFx { get; set; }
        bool IsChangingAsset { get; }
        bool IsChangingOutfit { get; }

        //---------------------------------------------------------------------
        // Methods
        //---------------------------------------------------------------------

        void SetTargetEvent(Event target, Action onEventLoaded, bool refocusCamera, CancellationToken cancellationToken = default);
        Task CreateFreshEventBasedOnTemplate(ApplyingTemplateArgs args);

        void Change<T>(T next, Action<IAsset> onCompleted = null, Action onCancelled = null, bool unloadPrevious = true, long subAssetId = -1L)
            where T : class, IEntity;
        Task ChangeOutfit(OutfitFullInfo outfitFullInfo);
        Task RemoveOutfit();
        void ApplyCharacterSpawnFormation(CharacterSpawnPositionFormation nextFormation);
        void ChangeCameraFilter(CameraFilterInfo cameraFilter, long variantId, Action<IAsset> onCompleted = null);
        void ChangeSetLocation(SetLocationFullInfo nextLocation, long? nextSpawnPointId, SetLocationChanged onCompleted, bool allowChangingAnimations, bool recenterCamera);
        void Shuffle(ShuffleAssets assetTypesFlag, EventShuffleResult shuffleResult, 
            SetLocationFullInfo[] setLocations, BodyAnimationInfo[] bodyAnimations, SetLocationChanged callback);
        void ChangeCharacterSpawnPosition(CharacterSpawnPositionInfo spawnPosition, bool allowChangingAnimations, Action<DbModelType[]> callback);
        void ChangeBodyAnimation(BodyAnimationInfo bodyAnimation, Action callback);
        void RemoveCameraFilter(Action onRemoved);
        void Cleanup();
        void ResetEventLoadingCallback();
        void ResetEditingEvent();
        void SpawnCharacter(CharacterFullInfo character, Action<ICharacterAsset> onSuccess);
        void ReplaceCharacter(CharacterFullInfo oldCharacter, CharacterFullInfo newCharacter, bool unloadOld, Action<ICharacterAsset> onSuccess);
        void DestroyCharacter(CharacterFullInfo target, Action onSuccess);
        void UnloadFaceAndVoice();
        void SetFilterValue(float value);
        void SetLevelSequenceNumber(int number);
        void ChangeCameraAnimation(CameraAnimationFullInfo next, string animationText);
        void RefreshCharactersOnSpawnPosition();
        void ApplySetLocationBackground(PhotoFullInfo photo, Action onApplied);
        void ApplySetLocationBackground(SetLocationBackground background, Action onApplied);
        void ApplySetLocationBackground(VideoClipFullInfo videoClip);
        void ResetSetLocationBackground();
    }
    
    internal delegate void SetLocationChanged(ISetLocationAsset asset, params DbModelType[] updatedDependantAssetTypes);
}