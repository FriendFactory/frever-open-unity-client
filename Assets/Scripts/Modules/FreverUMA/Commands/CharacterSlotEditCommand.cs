using Common;
using Configs;
using Modules.WardrobeManaging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Extensions;
using JetBrains.Annotations;
using UMA.CharacterSystem;
using UnityEngine;
using Zenject;

namespace Modules.FreverUMA
{
    [UsedImplicitly]
    public sealed class CharacterSlotEditCommand : UserCommand
    {
        public class Factory : PlaceholderFactory<DynamicCharacterAvatar, string, AccessoryItem, AccessoryItem, ICollection<AccessoryItem>, ColorData, CharacterSlotEditCommand>
        {}

        public string Slot { get; }
        public AccessoryItem StartValue { get; }
        public AccessoryItem FinalValue { get; }
        public ColorData ColorData { get; }
        public AccessoryItem[] ClearedItems => _clearedItems.ToArray();

        [Inject]
        private readonly AvatarHelper _avatarHelper;
        [Inject]
        private readonly CharacterManagerConfig _characterManagerConfig;
        [Inject]
        private readonly ClothesCabinet _clothesCabinet;
        private readonly DynamicCharacterAvatar _avatar;
        private readonly List<AccessoryItem> _clearedItems = new();
        private readonly ICollection<AccessoryItem> _initialWardrobes;

        public CharacterSlotEditCommand(DynamicCharacterAvatar avatar, string slot, AccessoryItem startValue, AccessoryItem finalValue, ICollection<AccessoryItem> initialItems, ColorData colorData)
        {
            StartValue = startValue;
            FinalValue = finalValue;
            Slot = slot;
            _avatar = avatar;
            _initialWardrobes = initialItems;
            ColorData = colorData;
        }

        public override async void ExecuteCommand()
        {
            CollectClippingItems();
            await SetAccessory(FinalValue);
            base.ExecuteCommand();
        }

        public override async void CancelCommand()
        {
            await RestoreClearedSlots();
            await SetAccessory(StartValue);
            base.CancelCommand();
        }

        private async Task SetAccessory(AccessoryItem item)
        {
            var umaBundles = item?.Wardrobe?.UmaBundle.GetBundleWithDependencies().ToArray();
            _avatarHelper.UnloadSlotBundle(Slot, umaBundles);

            if (ColorData != null)
            {
                _avatar.SetColor(ColorData.ColorName, ColorData.Color32);
            }
            if (item == null || item.IsEmpty)
            {
                var remainedWardrobes = _initialWardrobes.Where(x => x.Slot != Slot).Select(x=>x.Wardrobe).ToArray();
                await _avatarHelper.ClearSlot(_avatar, Slot, remainedWardrobes);
            }
            else
            {
                await _avatarHelper.ApplySlot(_avatar, item, _clearedItems, _initialWardrobes);
            }
        }
        
        private void CollectClippingItems()
        {
            var slotClipping = _characterManagerConfig.SlotsClippingMatrix.FirstOrDefault(x => x.Slot == Slot);
            if (slotClipping == null)
            {
                return;
            }

            foreach (var slotToClear in slotClipping.ClippingSlots)
            {
                if (!_avatar.WardrobeRecipes.TryGetValue(slotToClear, out var assetName)) continue;

                var clearedItem = _clothesCabinet.GetAccessoryItemByAssetName(_avatar.WardrobeRecipes[slotToClear].name) as AccessoryItem;
                if (clearedItem != null && !_clearedItems.Contains(clearedItem)) _clearedItems.Add(clearedItem);
            }
        }

        private async Task RestoreClearedSlots()
        {
            foreach (var item in _clearedItems)
            {
                await _avatarHelper.SetSlot(_avatar, item);
            }
        }
    }

    public sealed class ColorData
    {
        public readonly string ColorName;
        public readonly Color32 Color32;

        public ColorData(string colorName, Color32 color32)
        {
            ColorName = colorName;
            Color32 = color32;
        }
    }
}