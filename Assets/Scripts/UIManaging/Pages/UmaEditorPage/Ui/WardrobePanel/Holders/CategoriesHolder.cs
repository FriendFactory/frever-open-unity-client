using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Bridge.Models.ClientServer.StartPack.Metadata;
using System.Linq;
using System;
using Bridge.Models.ClientServer.EditorsSetting;
using Common;

namespace UIManaging.Pages.UmaEditorPage.Ui.WardrobePanel
{
    public class CategoriesHolder : BaseWardrobePanelUIHolder
    {
        private const long OUTFITS_TYPE_CATEGORY_ID = 4; 

        [SerializeField]
        private ScrollRect _scrollRect;
        [SerializeField]
        private GameObject _rightGradient;
        [SerializeField]
        private GameObject _leftGradient;
        [SerializeField]
        private GameObject _outfitCategoryPrefab;
        [SerializeField]
        private GameObject _collectionsCategoryPrefab;
        [SerializeField]
        private ToggleGroup _thisToggleGroup;
        [SerializeField]
        private HorizontalLayoutGroup _layoutGroup;

        public event Action<WardrobeCategory> CategorySelected;

        private Dictionary<long, List<CategoryItem>> _categoriesInType = new();

        private CharacterEditorSettings _editorSettings;

        private void Awake()
        {
            _scrollRect.onValueChanged.AddListener(OnScrollRectValueChanged);
        }

        public void SetSettings(CharacterEditorSettings editorSettings)
        {
            _editorSettings = editorSettings;
        }

        public void Setup(IEnumerable<WardrobeCategory> wardrobeCategories, bool isOnboarding = false)
        {
            _scrollRect = GetComponent<ScrollRect>();
            IEnumerable<WardrobeCategory> sortedCategories = wardrobeCategories.OrderBy(x => x.SortOrder);
            if (_editorSettings != null && _editorSettings.WardrobeCategories != null)
            {
                sortedCategories = sortedCategories.Where(x => x.Id < 0 || _editorSettings.WardrobeCategories.Any(y => y.Id == x.Id));
            }

            foreach (var category in sortedCategories)
            {
                if (!_categoriesInType.TryGetValue(category.WardrobeCategoryTypeId, out var categories))
                {
                    categories = new List<CategoryItem>();                  
                    _categoriesInType.Add(category.WardrobeCategoryTypeId, categories);
                }

                if (category.WardrobeCategoryTypeId != Constants.Wardrobes.DRESS_UP_CATEGORY_TYPE_ID && isOnboarding && category.SubCategories.All(subcategory => !subcategory.HasFreeAssets))
                {
                    continue;
                }
                
                CreateCategoryItem(category, categories);
            }
        }

        public void ShowCategoriesForType(long categoriesTypeId, long? startCategoryId, params long[] exceptIds)
        {
            _scrollRect.normalizedPosition = new Vector2(0, 0);
            gameObject.SetActive(categoriesTypeId != OUTFITS_TYPE_CATEGORY_ID);

            CategoryItem activeCategory = null;
            
            foreach (var type in _categoriesInType.Keys)
            {
                var activate = type == categoriesTypeId;
                var categories = _categoriesInType[type];
                var fixedCategoryId = startCategoryId != null && categories.Any(category => category.Entity.Id == startCategoryId)
                        ? startCategoryId
                        : categories[0].Entity.Id;
                
                foreach (var category in categories)
                {
                    category.gameObject.SetActive(activate && !exceptIds.Contains(category.Entity.Id));

                    if (activate && category.Entity.Id == fixedCategoryId)
                    {
                        activeCategory = category;
                        category.Toggle.isOn = true;
                        CategorySelected?.Invoke(category.Entity);
                    }
                    else
                    {
                        category.Toggle.isOn = false;
                    }
                }
            }
            
            ForceUpdateCanvas();

            if (activeCategory != null)
            {
                var itemTransform = activeCategory.GetComponent<RectTransform>();
                var itemWidth = itemTransform.rect.width;
                var normalizedPositionX = Mathf.InverseLerp(itemWidth / 2,
                                                            _scrollRect.content.rect.width - itemWidth / 2,
                                                            itemTransform.anchoredPosition.x);

                _scrollRect.normalizedPosition = new Vector2(normalizedPositionX, _scrollRect.normalizedPosition.y);
            }
        }

        public override void Hide()
        {
            base.Hide();
            _thisToggleGroup.SetAllTogglesOff();
        }

        public override void Clear()
        {
            var items = _categoriesInType.Values.SelectMany(item => item);
            foreach (var item in items)
            {
                Destroy(item.gameObject);
            }
            _categoriesInType = new Dictionary<long, List<CategoryItem>>();
            CategorySelected = null;
        }

        private void CreateCategoryItem(WardrobeCategory category, List<CategoryItem> categoriesInType)
        {
            GameObject categoryItemGO;
            CategoryItem categoryItem;

            switch (category.Id)
            {
                case Constants.Wardrobes.DRESS_UP_CATEGORY_ID:
                    categoryItemGO = Instantiate(_outfitCategoryPrefab, _content);
                    categoryItem = categoryItemGO.GetComponent<StaticCategoryItem>();
                    break;
                case Constants.Wardrobes.COLLECTIONS_CATEGORY_ID:
                    categoryItemGO = Instantiate(_collectionsCategoryPrefab, _content);
                    categoryItem = categoryItemGO.GetComponent<StaticCategoryItem>();
                    break;
                default:
                    categoryItemGO = Instantiate(_itemPrefab, _content);
                    categoryItem = categoryItemGO.GetComponent<CategoryItem>();
                    break;
            }
            
            categoryItemGO.name = category.Name;
            categoryItemGO.SetActive(false);
            categoriesInType.Add(categoryItem);
            categoryItem.Init(_bridge);
            categoryItem.Setup(category);

            categoryItem.Toggle.onValueChanged.AddListener((selected) =>
            {
                if (!selected) return;
                CategorySelected?.Invoke(category);
            });
            categoryItem.Toggle.group = _thisToggleGroup;
        }

        private void OnScrollRectValueChanged(Vector2 position)
        {
            _rightGradient.SetActive(position.x < 1f);
            _leftGradient.SetActive(position.x > 0);
        }

        private void ForceUpdateCanvas()
        {
            Canvas.ForceUpdateCanvases();
            _layoutGroup.enabled = false;
            _layoutGroup.enabled = true;
        }
    }
}