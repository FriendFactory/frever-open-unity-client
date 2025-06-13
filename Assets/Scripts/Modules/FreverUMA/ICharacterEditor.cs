using Bridge.Models.ClientServer.Assets;
using Bridge.Models.ClientServer.StartPack.Metadata;
using Bridge.Models.Common;
using Modules.WardrobeManaging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UMA.CharacterSystem;
using UnityEngine;

namespace Modules.FreverUMA
{
    public interface ICharacterEditor
    {
        event Action CharacterChangingBegan;
        event Action<IEntity[]> CharacterChanged;
        event Action CharacterDNAChanged;
        event Action CharacterUndressed;
        public event Action RemoveAllWardrobesCommandFinished;

        bool IsCharacterModified { get; }
        bool IsRedoEmpty { get; }
        bool IsUndoEmpty { get; }
        long GenderId { get; }
        IEnumerable<UmaSharedColor> UmaColors { get; }
        IEnumerable<WardrobeItem> AppliedWardrobeItems { get; }

        void Init(WardrobeSubCategory[] subCategories);
        
        void SetCompressionEnabled(bool isEnabled);
        void SetSubCategory(long id);
        Task<DynamicCharacterAvatar> CreateNewAvatar(long genderId, CancellationToken token = default);
        void SetTargetAvatar(DynamicCharacterAvatar avatar);
        void SetEditingCharacterModel(CharacterFullInfo characterModel);
        void LoadUmaBundles(CharacterFullInfo character, OutfitFullInfo outfit);
        void UnloadUmaBundles();
        void UnloadNotUsedUmaBundles(DynamicCharacterAvatar avatar);
        void ResetCharacter();
        Task LoadCharacter(CharacterFullInfo character, OutfitFullInfo outfit, CancellationToken token = default);
        void SetGenderId(long genderId);

        Dictionary<string, float> GetDNAValues();
        Dictionary<string, Color32> GetColors();
        List<KeyValuePair<long, int>> GetCharacterColorsInt();

        void EndCharacterDNAEdit(string bodyPart, float startValue, float endValue);
        void EditCharacterDNA(string bodyPart, float value, bool saveToHistory = false);

        void ChangeWardrobeItem(long wardrobeId);
        void ChangeWardrobeItem(DynamicCharacterAvatar avatar, WardrobeFullInfo wardrobe);

        void UndressCharacter();

        void EditCharacterColor(string type, Color color, IEnumerable<WardrobeFullInfo> currentWardrobes, Action onCompleted = null);
        
        KeyValuePair<string, List<WardrobeFullInfo>> GetCharacterRecipeAndWardrobes();
        List<WardrobeFullInfo> GetCharacterWardrobes();

        void ApplyPreset(PresetItem newItem, PresetItem basePreset, Dictionary<string, float> savedDNA = null);

        void ApplyOutfit(OutfitShortInfo outfit, long genderId = -1);
        
        void SetAppliedWardrobeItems(WardrobeFullInfo[] outfitWardrobes);

        void Undo();
        void Redo();
        void ClearModificationsHistory();
        void Clear();

        void ShowHighlightingWardrobe(AccessoryItem wardrobe);
        float GetHeelsHeight(DynamicCharacterAvatar avatar);
    }
}