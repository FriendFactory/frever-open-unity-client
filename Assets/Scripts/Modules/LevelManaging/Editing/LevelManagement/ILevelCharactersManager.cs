using System;
using System.Threading.Tasks;
using Bridge.Models.ClientServer.Assets;
using Modules.LevelManaging.Assets;
using UnityEngine;
using CharacterController = Models.CharacterController;

namespace Modules.LevelManaging.Editing.LevelManagement
{
    public interface ILevelCharactersManager
    {
        event Action CharacterSpawnStarted;
        event Action<ICharacterAsset> CharacterSpawned;
        event Action<CharacterFullInfo, CharacterFullInfo> CharacterReplacementStarted;
        event Action<ICharacterAsset> CharacterReplaced;
        event Action CharacterDestroyed;
        event Action TargetCharacterSequenceNumberChanged;
        event Action EditingCharacterSequenceNumberChanged;
        event Action CharactersOutfitsUpdatingBegan;
        event Action CharactersOutfitsUpdated;
        event Action<WardrobeFullInfo[]> OnOutfitUpdated; 
        event Action OnTrySavingEmptyOutfit; 
       
        bool IsReplacingCharacter { get; }
        bool IsChangingOutfit { get; }
        bool IsChangingWardrobe { get; }
        int TargetCharacterSequenceNumber { get; set; }
        int EditingCharacterSequenceNumber { get; set; }
        bool AllowReducingCharactersQuality { get; set; }
        ICharacterAsset TargetCharacterAsset { get; }
        ICharacterAsset EditingTargetCharacterAsset { get; }
        CharacterController TargetCharacterController { get; }
        CharacterController EditingCharacterController { get; }

        Task LoadAndKeepEditingAnimations();
        void ReleaseEditingAnimations();
        Task SetupCharactersForEditing();
        Task StopCharacterEditingMode();
        
        Task SwitchWardrobe(long characterId, WardrobeFullInfo wardrobeModel);
        Task EditCharacterColor(string colorName, Color32 newColor);
        void UndoLastCharacterOutfitChange();
        void RedoLastCharacterOutfitChange();
        void UndressCharacter();
        void ResetCharacterToInitialState();
        void ClearCharacterModificationHistory();
        void WarmupUmaBundlesForWardrobesModification();
        void UnloadUmaBundles();
        Task ChangeOutfit(OutfitFullInfo outfitFullInfo);
        Task RemoveOutfit();
        Task<bool> SaveEditedOutfit(bool saveManual);
        void SwapCharacters();
    }
}