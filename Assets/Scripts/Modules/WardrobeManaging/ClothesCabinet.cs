using System;
using System.Collections.Generic;
using System.Linq;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.ClientServer.StartPack.Metadata;
using Bridge.Models.ClientServer.StartPack.UserAssets;
using Bridge.Models.ClientServer.ThemeCollection;
using Configs;
using JetBrains.Annotations;
using Modules.AssetsStoraging.Core;
using Zenject;
using Extensions;

namespace Modules.WardrobeManaging
{
    [UsedImplicitly]
    public sealed class ClothesCabinet
    {
        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------
       
        public event Action PurchasedItemsUpdated;

        //---------------------------------------------------------------------
        // Private fields
        //---------------------------------------------------------------------
        
        [Inject] private DNASlidersGroupingSettings _dnaSlidersGroupingSettings;
        [Inject] private readonly IDataFetcher _dataFetcher;
        [Inject] private readonly IWardrobeCategoriesProvider _wardrobeCategoriesProvider;
        private Dictionary<long, WardrobeItem> _wardrobeItems = new();
        private List<WardrobeCategoryType> _types = new();
        private Dictionary<long, WardrobeCategory> _categories = new();
        private Dictionary<long, WardrobeSubCategory> _subCategories = new();
        private Dictionary<long, ThemeCollectionInfo> _themeCollections = new();
        private Dictionary<long, List<WardrobeFullInfo>> _wardrobesByGender = new();
        private Dictionary<long, UmaSharedColor> _umaSharedColors = new();
        private Dictionary<long, UmaAdjustment> _umaAdjustments = new();
        private HashSet<long> _purchasedWardrobes;
        private WardrobeFullInfo[] _highlightingWardrobes;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        public IEnumerable<WardrobeCategoryType> CategoryTypes => _types;

        public IEnumerable<WardrobeCategory> Categories => _categories.Values;

        public IEnumerable<WardrobeSubCategory> SubCategories => _subCategories.Values;
        public IEnumerable<ThemeCollectionInfo> ThemeCollections => _themeCollections.Values;
        public IEnumerable<WardrobeItem> WardrobeItems => _wardrobeItems.Values;
        public IEnumerable<UmaSharedColor> UmaSharedColors => _umaSharedColors.Values;
        public IEnumerable<WardrobeFullInfo> HighlightingWardrobes => _highlightingWardrobes;

        [Inject]
        public void Construct()
        {
            _dataFetcher.OnUserAssetsFetched += ProcessUserAssets;
            if (_dataFetcher.DefaultUserAssets != null)
            {
                ProcessUserAssets(_dataFetcher.DefaultUserAssets);
            }

            if (_dataFetcher.MetadataStartPack == null || _dataFetcher.WardrobeCategoriesForGenders == null)
            {
                _dataFetcher.OnDataFetched += ProcessFetchedData;
            }
            else
            {
                ProcessFetchedData();
            }
        }

        public WardrobeItem GetWardrobeItem(long wardrobeId)
        {
            _wardrobeItems.TryGetValue(wardrobeId, out var wardrobe);
            return wardrobe;
        }

        public WardrobeItem GetWardrobeItem(string wardrobeName)
        {
            return _wardrobeItems.FirstOrDefault(kv => kv.Value.Wardrobe.Name == wardrobeName).Value;
        }

        public WardrobeItem GetAccessoryItemByAssetName(string assetName)
        {
            return _wardrobeItems.Values.FirstOrDefault((item) => (item as AccessoryItem)?.AssetName == assetName);
        }

        public UmaSharedColor GetUmaSharedColor(long id)
        {
            _umaSharedColors.TryGetValue(id, out var sharedColor);
            return sharedColor;
        }

        public bool TryGetUmaAdjustment(long id, out UmaAdjustment adjustment)
        {
            return _umaAdjustments.TryGetValue(id, out adjustment);
        }

        public void AddUmaBundleToWardrobe(WardrobeFullInfo wardrobeFullInfo, long wardrobeId)
        {
            if (_wardrobeItems.ContainsKey(wardrobeId)) return;

            var asset = wardrobeFullInfo.GetUmaAsset();

            WardrobeItem wardrobeItem;
            if (asset == null)
            {
                wardrobeItem = new PresetItem(wardrobeFullInfo, wardrobeFullInfo.UmaBundle, "");
            }
            else
            {
                wardrobeItem = new AccessoryItem(wardrobeFullInfo, asset.SlotName, asset.Name);
            }
            _wardrobeItems.Add(wardrobeId, wardrobeItem);
        }

        public bool IsWardrobePurchased(long id)
        {
            return _purchasedWardrobes != null && _purchasedWardrobes.Contains(id);
        }

        public void UpdatePurchasedWardrobes()
        {
            _dataFetcher.FetchUserAssets();
        }

        public WardrobeFullInfo GetHighlightingWardrobe(string wardrobeName)
        {
            return _highlightingWardrobes.FirstOrDefault(x => x.Name == wardrobeName);
        }

        public AccessoryItem GetHighlightingAccessoryItem(string wardrobeName)
        {
            var wardrobe = _highlightingWardrobes.FirstOrDefault(x => x.Name == wardrobeName);
            if(wardrobe  == null) return null;
            var asset = wardrobe.GetUmaAsset();
            return new AccessoryItem(wardrobe, asset.SlotName, asset.Name);
        }

        private void ProcessFetchedData()
        {
            _dataFetcher.OnDataFetched -= ProcessFetchedData;
            ClearData();
            ProcessCategoryTypes();
            ProcessCategories();
            ProcessColors();
            ProcessUmaAdjustments();
            FillAdjustmentsSubCategories();
            FetchHighlightingWardrobes();
        }

        private void ProcessCategoryTypes()
        {
            //Because FREV-14181
            _types = _dataFetcher.MetadataStartPack.WardrobeCategoryTypes.ToList();
            if (_types.Count > 2)
            {
                var clothes = _types[1];
                _types[1] = _types[2];
                _types[2] = clothes;
            }
            //Because FREV-14181 /
            var myTabCategory = new WardrobeCategoryType(){ Id = 4, Name = "My assets"};
            _types.Add(myTabCategory);
        }

        private void ProcessCategories()
        {
            _categories = _dataFetcher.MetadataStartPack.WardrobeCategories.ToDictionary(x => x.Id);
            _subCategories = _categories.Values.SelectMany(x => x.SubCategories).ToDictionary(x=>x.Id);
            if (_dataFetcher.MetadataStartPack.ThemeCollections != null)
            {
                _themeCollections = _dataFetcher.MetadataStartPack.ThemeCollections.ToDictionary(x => x.Id);
            }
        }

        private void ProcessColors()
        {
            _umaSharedColors = _dataFetcher.MetadataStartPack.UmaSharedColors.ToDictionary(x=>x.Id);
        }

        private void ProcessUmaAdjustments()
        {
            _umaAdjustments = _dataFetcher.MetadataStartPack.UmaAdjustments.ToDictionary(x => x.Id);
        }

        private void ProcessUserAssets(DefaultUserAssets userAssets)
        {
            var purchasedAssets = userAssets.PurchasedAssetsData;
            if (purchasedAssets?.Wardrobes == null) return;

            _purchasedWardrobes = new HashSet<long>(purchasedAssets.Wardrobes);
            PurchasedItemsUpdated?.Invoke();
        }

        public void SetItemAsPurchased(long wardrobeId)
        {
            _purchasedWardrobes ??= new HashSet<long>();
            
            _purchasedWardrobes.Add(wardrobeId);
            
            PurchasedItemsUpdated?.Invoke();
        }

        private void ClearData()
        {
            _wardrobeItems.Clear();
            _types.Clear();
            _categories.Clear();
            _subCategories.Clear();
            _wardrobesByGender.Clear();
            _umaAdjustments.Clear();
        }

        private async void FetchHighlightingWardrobes()
        {
            var uiWardrobeNames = _dnaSlidersGroupingSettings.GroupingSettings.Select(x=>x.HighlightingItemName).Distinct()
                                                                                .Where(x=>!string.IsNullOrEmpty(x)).ToArray();
            _highlightingWardrobes = await _dataFetcher.GetHighlightingWardrobes(1, uiWardrobeNames);
        }

        private void FillAdjustmentsSubCategories()
        {
            var allCategoryModels = _dataFetcher.WardrobeCategoriesForGenders
                                                .Where(x=> x.WardrobeCategories != null)
                                                .SelectMany(x => x.WardrobeCategories)
                                                .Concat(_dataFetcher.MetadataStartPack.WardrobeCategories);
            foreach (var category in allCategoryModels)
            {
                CreateAdjustmentsSubCategoriesForCategory(category);
            }
        }

        private void CreateAdjustmentsSubCategoriesForCategory(WardrobeCategory category)
        {
            var subCategoriesWithAdjustments = category.SubCategories.Where(x => x.UmaAdjustments is { Length: > 0 });

            if (!subCategoriesWithAdjustments.Any()) return;

            var newSubCategories = new List<WardrobeSubCategory>();
            foreach (var subCategory in subCategoriesWithAdjustments)
            {
                var adjustments = GetAdjustmentsForSubCategory(subCategory);
                AddAdjustmentsToNewSubCategories(adjustments, newSubCategories);
            }
            category.SubCategories.AddRange(newSubCategories);
        }

        private IEnumerable<UmaAdjustment> GetAdjustmentsForSubCategory(WardrobeSubCategory subCategory)
        {
            return _umaAdjustments.Values.Where(x => subCategory.UmaAdjustments.Contains(x.Id));
        }

        private void AddAdjustmentsToNewSubCategories(IEnumerable<UmaAdjustment> adjustments, List<WardrobeSubCategory> newSubCategories)
        {
            var commonElements = (from gs in _dnaSlidersGroupingSettings.GroupingSettings
                                  from s in gs.DNASliderSettings
                                  join a in adjustments on s.DNAKey equals a.Key
                                  orderby s.SortOrder
                                  select new { adjustment = a, sliderSetting = s, groupingSetting = gs })
                                  .ToArray();

            foreach (var item in commonElements)
            {
                var groupId = item.groupingSetting.GetId();

                ApplyRenameForDNAFromGroupingSetting(item.adjustment, item.groupingSetting);

                var subForGrouping = newSubCategories.Find(x => x.Id == groupId);
                if (subForGrouping == null)
                {
                    subForGrouping = new WardrobeSubCategory
                    {
                        Id = groupId,
                        Name = item.groupingSetting.Group,
                        SortOrder = item.groupingSetting.SortOrder,
                        UmaAdjustments = new long[] { item.adjustment.Id },
                        HasFreeAssets = true
                    };
                    newSubCategories.Add(subForGrouping);
                    _subCategories[subForGrouping.Id] = subForGrouping;
                }
                else
                {
                    subForGrouping.UmaAdjustments = subForGrouping.UmaAdjustments.Append(item.adjustment.Id).ToArray();
                }
            }
        }

        private void ApplyRenameForDNAFromGroupingSetting(UmaAdjustment adjustment, DNASlidersGroupingSettings.DNASliderGroupingSetting groupingSetting)
        {
            var setting = groupingSetting.DNASliderSettings.Find(x => x.DNAKey == adjustment.Key);
            if (setting == null) return;

            if (!string.IsNullOrEmpty(setting.NewName))
                adjustment.Name = setting.NewName;
        }
    }
}