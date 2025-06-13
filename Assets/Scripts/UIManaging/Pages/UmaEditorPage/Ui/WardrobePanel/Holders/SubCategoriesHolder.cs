using Bridge.Models.ClientServer.EditorsSetting;
using Bridge.Models.ClientServer.StartPack.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using Bridge.Models.ClientServer.ThemeCollection;
using Common;
using UIManaging.Localization;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.UmaEditorPage.Ui.WardrobePanel
{
    public class SubCategoriesHolder : BaseWardrobePanelUIHolder
    {
        [Inject] private UmaEditorCategoriesLocalization _categoriesLocalization;
        
        public event Action<WardrobeSubCategory> SubCategorySelected;

        private Dictionary<WardrobeCategory, List<SubCategoryItem>> _subCategoriesInCategory = new Dictionary<WardrobeCategory, List<SubCategoryItem>>();

        [SerializeField] private ToggleGroup _thisToggleGroup;
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private HorizontalLayoutGroup _layoutGroup;
        
        private CharacterEditorSettings _editorSettings;

        public void SetSettings(CharacterEditorSettings editorSettings)
        {
            _editorSettings = editorSettings;
        }

        public void Setup(WardrobeCategory category, bool isOnboarding = false)
        {
            var subCategories = new List<SubCategoryItem>();          
            _subCategoriesInCategory.Add(category, subCategories);

            var sortedSubCategories = GetSortedSubCategories(category);

            if (category.HasSubCategoryAll)
            {
                var colorId = sortedSubCategories.FirstOrDefault()?.UmaSharedColorId;
                var subCategoryItem = CreateAllSubCategory(category, colorId);
                subCategories.Add(subCategoryItem);
            }

            if (category.Id == Constants.Wardrobes.DRESS_UP_CATEGORY_ID)
            {
                SetupOutfitSubCategories(category);
            }

            foreach (var subCategory in sortedSubCategories)
            {
                if (category.WardrobeCategoryTypeId != Constants.Wardrobes.DRESS_UP_CATEGORY_TYPE_ID && isOnboarding && !subCategory.HasFreeAssets)
                {
                    continue;
                }
                
                var subCategoryItem = CreateSubCategoryItem(subCategory);
                subCategories.Add(subCategoryItem);
            }
        }

        public void Setup(WardrobeCategory category, List<ThemeCollectionInfo> themeCollections)
        {
            var subCategories = SetupCollectionsSubCategories(themeCollections);
            _subCategoriesInCategory.Add(category, subCategories);
        }

        public void ShowSubCategoriesForCategory(WardrobeCategory targetCategory, long? startSubCategoryId)
        {
            SubCategoryItem activeSubCategory = null;
            
            foreach (var category in _subCategoriesInCategory.Keys)
            {
                var activate = category.Id == targetCategory.Id;
                var subCategories = _subCategoriesInCategory[category];

                if (subCategories.Count == 0) continue;
                
                var fixedSubCategoryId = startSubCategoryId != null && subCategories.Count > 0 && subCategories.Any(
                    subCategory => subCategory.Entity.Id == startSubCategoryId)
                        ? startSubCategoryId
                        : subCategories[0].Entity.Id;
                
                foreach (var subCategory in subCategories)
                {
                    subCategory.gameObject.SetActive(activate);

                    if (activate && subCategory.Entity.Id == fixedSubCategoryId)
                    {
                        activeSubCategory = subCategory;
                        subCategory.Toggle.isOn = true;
                    }
                    else
                    {
                        subCategory.Toggle.isOn = false;
                    }
                }
                
                if (activate && activeSubCategory != null)
                {
                    ForceUpdateCanvas();
                    
                    var itemTransform = activeSubCategory.GetComponent<RectTransform>();
                    var itemWidth = itemTransform.rect.width;
                    var normalizedPositionX = Mathf.InverseLerp(itemWidth / 2f,
                                                                _scrollRect.content.rect.width - itemWidth / 2f,
                                                                itemTransform.anchoredPosition.x);

                    _scrollRect.normalizedPosition = new Vector2(normalizedPositionX, _scrollRect.normalizedPosition.y);
                }
            }
        }

        public WardrobeCategory GetWardrobeCategoryById(long id)
        {
            return _subCategoriesInCategory.Keys.FirstOrDefault(x => x.Id == id);
        }
        
        public WardrobeSubCategory GetWardrobeSubCategoryById(long id)
        {
            return _subCategoriesInCategory.SelectMany(x => x.Value).First(x => x.Entity.Id == id).Entity;
        }

        public override void Clear()
        {
            var items = _subCategoriesInCategory.Values.SelectMany(item => item);
            foreach (var item in items)
            {
                Destroy(item.gameObject);
            }
            _subCategoriesInCategory = new Dictionary<WardrobeCategory, List<SubCategoryItem>>();
            SubCategorySelected = null;
        }

        private SubCategoryItem CreateSubCategoryItem(WardrobeSubCategory subCategory)
        {
            var subCategoryItemGO = Instantiate(_itemPrefab, _content);   
            var subCategoryItem = subCategoryItemGO.GetComponent<SubCategoryItem>();
            
            subCategoryItem.Init(_bridge);
            subCategoryItem.Setup(subCategory);
            subCategoryItem.SetLocalizedName(_categoriesLocalization);
            subCategoryItem.Toggle.group = _thisToggleGroup;

            subCategoryItem.Toggle.onValueChanged.AddListener((selected) =>
            {
                if (selected) SubCategorySelected?.Invoke(subCategory);
            });
            subCategoryItemGO.SetActive(false);
            return subCategoryItem;
        }

        private void ForceUpdateCanvas()
        {
            Canvas.ForceUpdateCanvases();
            _layoutGroup.enabled = false;
            _layoutGroup.enabled = true;
        }

        private void SetupOutfitSubCategories(WardrobeCategory category)
        {
            var subCategories = _subCategoriesInCategory[category];

            subCategories.Add(CreateMyOutfitsSubCategory());

            var historySubCategoryItem = CreateHistorySubCategory();
            subCategories.Add(historySubCategoryItem);
        }

        private List<SubCategoryItem> SetupCollectionsSubCategories(List<ThemeCollectionInfo> themeCollections)
        {
            var subCategories = new List<SubCategoryItem>();
            
            foreach (var themeCollection in themeCollections)
            {
                subCategories.Add(CreateCollectionSubCategory(themeCollection));
            }

            return subCategories;
        }

        private List<WardrobeSubCategory> GetSortedSubCategories(WardrobeCategory category)
        {
            IEnumerable<WardrobeSubCategory> sortedSubCategories = category.SubCategories.OrderBy(x => x.SortOrder);
            ApplyEditorSettingsToSortedSubCategories(category, ref sortedSubCategories);
            
            return sortedSubCategories.ToList();
        }

        private void ApplyEditorSettingsToSortedSubCategories(WardrobeCategory category, ref IEnumerable<WardrobeSubCategory> sortedSubCategories)
        {
            if (_editorSettings?.WardrobeCategories == null || category.Id <= 0) return;

            var categorySettings = _editorSettings.WardrobeCategories.FirstOrDefault(x => x.Id == category.Id);
            if (categorySettings?.Subcategories == null) return;

            sortedSubCategories = sortedSubCategories.Where(x => categorySettings.Subcategories.Any(y => y.Id == x.Id));
        }

        private SubCategoryItem CreateAllSubCategory(WardrobeCategory category, long? umaSharedColorId)
        {
            var allSubCategory = new WardrobeSubCategory
            {
                Id = -category.Id,
                Name = "All",
                UmaSharedColorId = umaSharedColorId
            };

            var subCategoryItem = CreateSubCategoryItem(allSubCategory);
            return subCategoryItem;
        }

        private SubCategoryItem CreateHistorySubCategory()
        {
            var myHistoryOutfits = new WardrobeSubCategory
            {
                Id = Constants.Wardrobes.RECENT_OUTFIT_SUBCATEGORY_ID,
                Name = "Recent Outfits",
            };

            var historySubCategoryItem = CreateSubCategoryItem(myHistoryOutfits);
            return historySubCategoryItem;
        }

        private SubCategoryItem CreateMyOutfitsSubCategory()
        {
            var myOutfits = new WardrobeSubCategory
            {
                Id = Constants.Wardrobes.FAVOURITE_OUTFIT_SUBCATEGORY_ID,
                Name = _categoriesLocalization.FavouriteOutfitsTab,
            };

            var subCategoryItem = CreateSubCategoryItem(myOutfits);
            return subCategoryItem;
        }

        private SubCategoryItem CreateCollectionSubCategory(ThemeCollectionInfo themeCollectionInfo)
        {
            var collection = new WardrobeSubCategory
            {
                Id = themeCollectionInfo.Id,
                Name = themeCollectionInfo.Name
            };
            
            var subCategoryItem = CreateSubCategoryItem(collection);
            return subCategoryItem;
        }
    }
}
