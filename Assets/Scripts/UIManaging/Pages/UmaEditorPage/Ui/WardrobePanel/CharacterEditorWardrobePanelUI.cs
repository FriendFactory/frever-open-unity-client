using System.Collections.Generic;
using System.Linq;
using Common;
using Modules.AssetsStoraging.Core;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.UmaEditorPage.Ui.WardrobePanel
{
    internal sealed class CharacterEditorWardrobePanelUI : WardrobePanelUIBase
    {
        [SerializeField] private ToggleGroup _tabTypeSwitcher;
        [SerializeField] private Toggle _categoryTypeTogglePrefab;
        
        [Inject] private IMetadataProvider _metadata;
        
        private readonly List<CategoryTypeToggle> _categoryTypeToggles = new();

        public override void Setup(bool isOnboarding, long genderId)
        {
            SetupTabs();
            base.Setup(isOnboarding, genderId);
        }

        public override void Show()
        {
            long? categoryId = StartCategoryId switch
            {
                > 0 => _metadata.WardrobeCategories
                                .FirstOrDefault(x => x.SubCategories.Any(subCategory => subCategory.Id == StartSubCategoryId))?
                                .Id,
                < 0 => StartSubCategoryId.HasValue ? (long) Mathf.Abs(StartSubCategoryId.Value) : null,
                _ => null
            };

            var matchingCategoryType = _categoryTypeToggles.Find(x => x.CategoryType.Id == StartCategoryTypeId);
           
            if (matchingCategoryType != null && !matchingCategoryType.Toggle.isOn) 
            {
                matchingCategoryType.Toggle.isOn = true;
                SwitchCategoryType(matchingCategoryType.CategoryType, categoryId);
            }
            base.Show();
        }

        private void SetupTabs()
        {
            var filteredToggles = ClothesCabinet.CategoryTypes;
            if (EditorSettings is { WardrobeCategories: not null })
            {
                var typesWithAvailableCategories = ClothesCabinet.Categories.Where(x => EditorSettings.WardrobeCategories.Any(y => y.Id == x.Id)).Select(x => x.WardrobeCategoryTypeId);
                filteredToggles = filteredToggles.Where(x => x.Id == Constants.Wardrobes.DRESS_UP_CATEGORY_TYPE_ID || typesWithAvailableCategories.Contains(x.Id));
            }

            foreach (var categoryType in filteredToggles)
            {
                var toggle = Instantiate(_categoryTypeTogglePrefab, _tabTypeSwitcher.transform, false);
                
                toggle.group = _tabTypeSwitcher;
                toggle.onValueChanged.AddListener((on) => OnTypeStateChanged(categoryType, on));
                var typeToggle = toggle.GetComponent<CategoryTypeToggle>();
                typeToggle.Setup(categoryType);
                _categoryTypeToggles.Add(typeToggle);
            }
        }

        protected override void OnColorButtonClick()
        {
            _tabTypeSwitcher.gameObject.SetActive(false);
            base.OnColorButtonClick();
        }

        protected override void OnApplyPanelChangesConfirmed()
        {
            _tabTypeSwitcher.gameObject.SetActive(true);
            base.OnApplyPanelChangesConfirmed();
        }

        public override void Clear()
        {
            foreach (var item in _categoryTypeToggles)
            {
                Destroy(item.gameObject);
            }
            _categoryTypeToggles.Clear();
            base.Clear();
        }
    }
}