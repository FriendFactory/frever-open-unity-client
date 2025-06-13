using Bridge.Models.ClientServer.Assets;
using Bridge.Models.ClientServer.EditorsSetting;
using Bridge.Models.ClientServer.StartPack.Metadata;
using Bridge.Models.Common;
using Configs;
using Extensions;
using Modules.Amplitude;
using Filtering;
using Filtering.UI;
using JetBrains.Annotations;
using Modules.FreverUMA;
using Modules.WardrobeManaging;
using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Modules.AssetsStoraging.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using static Common.Constants.Wardrobes;
using static Configs.DNASlidersGroupingSettings;

namespace UIManaging.Pages.UmaEditorPage.Ui.WardrobePanel
{
    public abstract class WardrobePanelUIBase : MonoBehaviour
    {
        #pragma warning disable CS0649
        
        [SerializeField]
        private ApplyPanel _applyPanel;
        
        [SerializeField]
        private CategoriesHolder _categoriesHolder;
        [SerializeField]
        private SubCategoriesHolder _subCategoriesHolder;
        [SerializeField]
        private ColorsHolder _colorsHolder;
        [SerializeField]
        private AdjustmentHolder _adjustmentHolder;
        [SerializeField]
        private WardrobeItemsHolder _wardrobesHolder;
        [SerializeField]
        private GameObject _contentPanel;
        [SerializeField]
        private ColorPalettePanel _palettePanel;
        [SerializeField]
        private FilteringPanelUI _filteringPanel;
        [SerializeField]
        private FilteringButton _filteringButton;
        [SerializeField]
        private EquippedItemsPanel _equippedItemsPanel;
        [SerializeField]
        private Button _equippedItemsButton;
        [SerializeField]
        private TMP_Text _equippedItemsCounter;
        [SerializeField] 
        private WardrobePanelExtraSettings _wardrobePanelExtraSettings;
        
        [Inject]
        protected ClothesCabinet ClothesCabinet;
        [Inject]
        private ICharacterEditor _characterEditor;
        [Inject]
        private AmplitudeManager _amplitudeManager;
        [Inject]
        private DNASlidersGroupingSettings _dnaSlidersGroupingSettings;
        [Inject]
        private ColorPalletHidingConfiguration _colorPalletHidingConfiguration;
        [Inject] 
        private IWardrobeCategoriesProvider _wardrobeCategoriesProvider;
        [Inject] 
        private IMetadataProvider _metadataProvider;
#pragma warning restore CS0649

        public WardrobeCategory CurrentCategory => _currentCategory;
        public WardrobeSubCategory CurrentSubCategory => _currentSubCategory;
        public long GenderId => _wardrobesHolder.GenderId;

        private WardrobeCategory _currentCategory;
        private WardrobeSubCategory _currentSubCategory;
        private bool _subCategoryHasColor;
        protected CharacterEditorSettings EditorSettings;
        private FilteringSetting _filteringSetting;
        protected long StartCategoryTypeId;
        protected long? StartCategoryId;
        protected long? StartSubCategoryId;
        
        private readonly WardrobeCategory _outfitsCategory = new()
        {
            Id = DRESS_UP_CATEGORY_ID,
            Name = "Looks",
            SubCategories = new List<WardrobeSubCategory>(),
            HasSubCategoryAll = false,
            WardrobeCategoryTypeId = DRESS_UP_CATEGORY_TYPE_ID,
            SortOrder = -1
        };
        
        private readonly WardrobeCategory _themeCollectionsCategory = new()
        {
            Id = COLLECTIONS_CATEGORY_ID,
            Name = "Collections",
            SubCategories = new List<WardrobeSubCategory>(),
            HasSubCategoryAll = false,
            WardrobeCategoryTypeId = CLOTHES_CATEGORY_TYPE_ID,
            SortOrder = -1
        };

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        public event Action<IEntity> ItemSelected;
        public event Action<IEntity> PurchaseRequested;
        public event Action<UmaAdjustment, float> AdjustmentChanged;
        public event Action<UmaAdjustment, float, float> AdjustmentChangedDiff;
        public event Action<string, Color32> ColorPicked;
        public event Action<long, long> CategoryChanged; 
        public event Action<long> SubCategorySelected;
        public event Action<bool> DnaPanelShown;
        public event Action<string> TypeChanged;
        public event Action SaveOutfitClicked;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        [Inject]
        [UsedImplicitly]
        private void Construct(ClothesCabinet clothesCabinet, ICharacterEditor characterEditor, AmplitudeManager amplitudeManager)
        {
            ClothesCabinet = clothesCabinet;
            _characterEditor = characterEditor;
            _amplitudeManager = amplitudeManager;    
        }
        
        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void SetSettings(CharacterEditorSettings editorSettings, long? taskId, long startCategoryTypeId, long? startCategoryId, long? startSubCategoryId, FilteringSetting filteringSetting)
        {
            EditorSettings = editorSettings;
            _wardrobesHolder.TaskId = taskId;
            SetStartCategories(startCategoryTypeId, startCategoryId, startSubCategoryId);
            ApplyEditorSettings();
            SetFilteringSetting(filteringSetting);
        }

        public void SetStartCategories(long startCategoryTypeId, long? startCategoryId, long? startSubCategoryId)
        {
            _currentCategory = null;
            _currentSubCategory = null;
            
            StartCategoryTypeId = startCategoryTypeId;
            StartCategoryId = startCategoryId;
            StartSubCategoryId = startSubCategoryId;
        }
        
        public virtual void Setup(bool isOnboarding, long genderId)
        {           
            SetupCategories(isOnboarding, genderId);
            SetupSubCategories(isOnboarding, genderId);
            SetupColors();
            SetupAdjustments();
            SetupWardrobes();
            
            _characterEditor.CharacterChanged += OnCharacterChanged;
            _characterEditor.CharacterDNAChanged += OnCharacterDNAChanged;
            _applyPanel.ChangeConfirmed += OnApplyPanelChangesConfirmed;
            _filteringButton.AddButtonAction(ShowFilteringPanel);
            _filteringPanel.FilteringApplied += OnFilteringApplied;
            if (_equippedItemsButton is not null)
            {
                _equippedItemsButton.onClick.AddListener(ShowEquippedItemsPanel);
            }
        }

        public virtual void Show()
        {
            if (StartSubCategoryId.HasValue && StartSubCategoryId != 0)
            {
                var subCategory = GetWardrobeSubCategoryById(StartSubCategoryId.Value);
                SetSubCategory(subCategory);
            }

            if (StartCategoryId.HasValue)
            {
                _categoriesHolder.SetActive(ShouldShowOtherCategories(StartCategoryId.Value));
            }
            
            _equippedItemsPanel.Hide();
            _adjustmentHolder.Hide();
            _colorsHolder.Hide();
            _contentPanel.SetActive(true);
        }

        public WardrobeSubCategory GetWardrobeSubCategoryById(long id)
        {
            return _subCategoriesHolder.GetWardrobeSubCategoryById(id);
        }
        
        public WardrobeCategory GetWardrobeCategoryById(long id)
        {
            return _subCategoriesHolder.GetWardrobeCategoryById(id);
        }

        public void SetGenderId(long genderID)
        {
            _equippedItemsPanel.SetGenderID(genderID);
            if (_wardrobesHolder.GenderId == genderID) return;
            _wardrobesHolder.GenderId = genderID;

            if (_currentCategory != null && _currentSubCategory != null)
            {
                ShowItems();
            }
        }

        public virtual void Clear()
        {
            _categoriesHolder.Clear();
            _subCategoriesHolder.Clear();
            _wardrobesHolder.Clear();
            _adjustmentHolder.Clear();
            _colorsHolder.Clear();
            
            _categoriesHolder.CategorySelected -= OnCategorySelected;
            _subCategoriesHolder.SubCategorySelected -= OnSubCategorySelected;
            _wardrobesHolder.WardrobeItemSelected -= OnWardrobeSelected;
            _equippedItemsPanel.WardrobeItemSelected -= OnWardrobeSelected;
            _wardrobesHolder.WardrobePurchaseRequested -= OnPurchaseRequested;
            _adjustmentHolder.AdjustmentChanged -= OnAdjustmentChanged;
            _adjustmentHolder.AdjustmentChangedDiff -= OnAdjustmentChangedDiff;
            _colorsHolder.ColorPicked -= OnColorPicked;
            _palettePanel.ColorPicked -= OnColorPicked;
            _palettePanel.ColorButtonClicked -= OnColorButtonClick;

            _currentCategory = null;
            _currentSubCategory = null;
            
            _applyPanel.ChangeConfirmed -= OnApplyPanelChangesConfirmed;
            _filteringButton.RemoveButtonListener(ShowFilteringPanel);
            _filteringPanel.FilteringApplied -= OnFilteringApplied;
            
            if (_characterEditor == null) return;
            _characterEditor.CharacterChanged -= OnCharacterChanged;
            _characterEditor.CharacterDNAChanged -= OnCharacterDNAChanged;

            if (_equippedItemsButton is not null)
            {
                _equippedItemsButton.onClick.RemoveListener(ShowEquippedItemsPanel);
            }
        }

        public void UpdateOutfitList()
        {
            if (_currentSubCategory?.Id == RECENT_OUTFIT_SUBCATEGORY_ID)
            {
                ShowOutfitsHistory();
            }
            else
            {
                ShowOutfits();
            }
        }

        public void UpdateLoadingState(IEntity entity)
        {
            _wardrobesHolder.UpdateLoadingItem(entity);
        }

        public void UpdateAfterPurchase(IEntity entity)
        {
            _wardrobesHolder.UpdateAfterPurchase(entity);
        }

        public void SetDefaultDNAs(Gender gender)
        {
            _adjustmentHolder.SetStartValues(gender);
        }

        public void UpdateSelected(IEntity[] entities, bool updateEquippedItems = true)
        {
            _wardrobesHolder.UpdateSelections(entities);
            if (updateEquippedItems)
            {
                UpdateEquppedItemsList(entities);
            }
        }

        public void SwitchCategoryType(WardrobeCategoryType categoryType, long? startCategory = null)
        {
            startCategory ??= StartCategoryId;
            var categoriesToHide = startCategory.HasValue? GetCategoriesToHide(startCategory.Value) : Array.Empty<long>();
            _categoriesHolder.ShowCategoriesForType(categoryType.Id, startCategory, categoriesToHide);
            HideColorAndAdjustmentPanels();
            TypeChanged?.Invoke(categoryType.Name);
        }

        public void SwitchCategory(long categoryId, long wardrobeSubCategoryId)
        {
            var category = GetWardrobeCategoryById(categoryId);
            SetStartCategories(category.WardrobeCategoryTypeId, categoryId, wardrobeSubCategoryId);
            var categoryType = ClothesCabinet.CategoryTypes.First(x => x.Id == category.WardrobeCategoryTypeId);
            SwitchCategoryType(categoryType, categoryId);
            _categoriesHolder.SetActive(ShouldShowOtherCategories(categoryId));
            CategoryChanged?.Invoke(categoryId, wardrobeSubCategoryId);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        private void ApplyEditorSettings()
        {
            var filteringDisabled = EditorSettings is { FilterSettings: { AllowFiltering: false } };

            _filteringButton.gameObject.SetActive(!filteringDisabled);
        }

        private void SetupCategories(bool isOnboarding, long genderId)
        {
            var categories = _wardrobeCategoriesProvider.GetWardrobeCategories(genderId).ToList();
            
            categories.Add(_outfitsCategory);

            if (!isOnboarding)
            {
                categories.Add(_themeCollectionsCategory);
            }
            
            _categoriesHolder.CategorySelected += OnCategorySelected;
            _categoriesHolder.SetSettings(EditorSettings);
            _categoriesHolder.Setup(categories, isOnboarding);
        }

        private void SetupSubCategories(bool isOnboarding, long genderId)
        {
            _subCategoriesHolder.SubCategorySelected += OnSubCategorySelected;
            _subCategoriesHolder.SetSettings(EditorSettings);
            
            foreach (var category in _wardrobeCategoriesProvider.GetWardrobeCategories(genderId).Where(category => 
                        category.WardrobeCategoryTypeId == DRESS_UP_CATEGORY_TYPE_ID
                     || !isOnboarding || category.SubCategories.Any(subCategory => subCategory.HasFreeAssets)))
            {
                _subCategoriesHolder.Setup(category, isOnboarding);
            }

            var universe = _metadataProvider.MetadataStartPack.GetUniverseByGenderId(genderId);
            _subCategoriesHolder.Setup(_outfitsCategory);
            _subCategoriesHolder.Setup(_themeCollectionsCategory, ClothesCabinet.ThemeCollections.Where(x=> x.UniverseId == universe.Id).ToList());
        }

        private void SetupColors()
        {
            _palettePanel.ColorButtonClicked += OnColorButtonClick;
            _colorsHolder.ColorPicked += OnColorPicked;
            _palettePanel.ColorPicked += OnColorPicked;
            foreach (var subCategory in ClothesCabinet.SubCategories)
            {
                _colorsHolder.Setup(subCategory);
            }
        }

        private void SetupAdjustments()
        {
            _adjustmentHolder.AdjustmentChanged += OnAdjustmentChanged;
            _adjustmentHolder.AdjustmentChangedDiff += OnAdjustmentChangedDiff;
            
            foreach (var subCategory in ClothesCabinet.SubCategories)
            {
                if (subCategory.UmaAdjustments == null) continue;
                _adjustmentHolder.Setup(subCategory);
            }
        }

        private void SetupWardrobes()
        {
            _wardrobesHolder.WardrobeItemSelected += OnWardrobeSelected;
            _equippedItemsPanel.WardrobeItemSelected += OnWardrobeSelected;
            _wardrobesHolder.WardrobePurchaseRequested += OnPurchaseRequested;
        }

        private void OnAdjustmentChanged(UmaAdjustment adjustment, float value)
        {
            AdjustmentChanged?.Invoke(adjustment, value);
        }

        private void OnAdjustmentChangedDiff(UmaAdjustment adjustment, float startValue, float endValue)
        {
            AdjustmentChangedDiff?.Invoke(adjustment, startValue, endValue);
        }

        private void OnColorPicked(string colorName, Color32 color)
        {
            ColorPicked?.Invoke(colorName, color);
        }

        protected void OnTypeStateChanged(WardrobeCategoryType categoryType, bool activate)
        {
            if (!activate) return;
            SwitchCategoryType(categoryType);
        }

        private void OnCategorySelected(WardrobeCategory category)
        {
            if (_currentCategory == category) return;
          
            AmplitudeLogCategorySelected(category.Id, category.Name);
            _currentCategory = category;
            _subCategoriesHolder.ShowSubCategoriesForCategory(category, StartSubCategoryId);
        }

        private void OnSubCategorySelected(WardrobeSubCategory subCategory)
        {
            if (_currentSubCategory == subCategory) return;

            AmplitudeLogSubCategorySelected(_currentCategory.Id, _currentCategory.Name,subCategory.Id, subCategory.Name);

            SetSubCategory(subCategory);
        }

        private void SetSubCategory(WardrobeSubCategory subCategory)
        {
            _characterEditor.SetSubCategory(subCategory.Id);
            _currentSubCategory = subCategory;
            _subCategoryHasColor = subCategory.UmaSharedColorId != null;

            HideAdjustmentsPanel();

            if (_subCategoryHasColor)
            {
                UpdateColorPallet();
                _palettePanel.UpdateColorsList(subCategory.UmaSharedColorId.Value);
                _palettePanel.UpdateSelections(_characterEditor.GetColors());
            }
            else
            {
                _palettePanel.gameObject.SetActive(false);
            }

            ShowItems();

            SubCategorySelected?.Invoke(subCategory.Id);
        }

        private void ShowItems()
        {
            if (_currentCategory.Id == COLLECTIONS_CATEGORY_ID)
            {
                _wardrobesHolder.CurrentThemeCollectionId = _currentSubCategory.Id;
                _wardrobesHolder.CurrentSubCategoryId = null;
                _wardrobesHolder.ShowItems(WardrobeItemsHolder.CategoryType.ThemeCollection);
                return;
            }

            if (_currentSubCategory.Id == FAVOURITE_OUTFIT_SUBCATEGORY_ID)
            {
                ShowOutfits();
                return;
            }

            if (_currentSubCategory.Id == RECENT_OUTFIT_SUBCATEGORY_ID)
            {
                ShowOutfitsHistory();
                return;
            }

            if (IsAdjustmentsSubCategory(_currentSubCategory, out var groupingSetting))
            {
                _wardrobesHolder.ChangeAssetsToggleVisibility(false);
                //ShowHighlighting(ClothesCabinet.GetHighlightingAccessoryItem(groupingSetting.HighlightingItemName));
                ShowAdjustmentsPanel();
                return;
            }

            _wardrobesHolder.CurrentThemeCollectionId = null;
            _wardrobesHolder.CurrentSubCategoryId = _currentSubCategory.Id;
            _wardrobesHolder.ShowItems(WardrobeItemsHolder.CategoryType.Regular, true, OnClearButtonClick);
        }

        private void OnWardrobeSelected(IEntity wardrobe)
        {
            ItemSelected?.Invoke(wardrobe);
        }

        private void OnPurchaseRequested(IEntity wardrobe)
        {
            PurchaseRequested?.Invoke(wardrobe);
        }

        private void ShowAdjustmentsPanel()
        {
            _adjustmentHolder.ShowAdjustmentsForSubCategory(_currentSubCategory);
            _adjustmentHolder.Show();
            _wardrobesHolder.SetActive(false);
        }

        private void HideAdjustmentsPanel()
        {
            _wardrobesHolder.SetActive(true);
            _adjustmentHolder.Hide();
        }

        private void OnClearButtonClick()
        {
            var selectedItems = _wardrobesHolder.GetSelectedItems().ToArray();
            var subCategoriesIds = new List<long>();
            if (_currentSubCategory.Id < 0)
            {
                foreach (var subCategory in _currentCategory.SubCategories)
                {
                    subCategoriesIds.Add(subCategory.Id);
                }
            }
            else
            {
                subCategoriesIds.Add(_currentSubCategory.Id);
            }
            foreach (var selected in selectedItems)
            {
                var wardrobeItem = selected as WardrobeFullInfo;
                if (wardrobeItem == null) continue;

                if (subCategoriesIds.Contains(wardrobeItem.WardrobeSubCategoryIds.First()))
                {
                    ItemSelected?.Invoke(new WardrobeShortInfo() { Id = wardrobeItem.Id});
                }
            }
        }

        protected virtual void OnColorButtonClick()
        {
            _colorsHolder.ShowColorsForSubCategory(_currentSubCategory);
            _colorsHolder.Show();
            _contentPanel.SetActive(false);
            _applyPanel.ShowApplyPanel(true);
            _palettePanel.gameObject.SetActive(false);
            DnaPanelShown?.Invoke(true);
        }

        protected virtual void OnApplyPanelChangesConfirmed()
        {
            _applyPanel.HideApplyPanel();
            _palettePanel.gameObject.SetActive(_subCategoryHasColor);
            _colorsHolder.Hide();
            _contentPanel.SetActive(true);
            DnaPanelShown?.Invoke(false);
        }

        private void OnCharacterChanged(IEntity[] entities)
        {
            UpdateEquppedItemsList(entities);
            
            if(_currentSubCategory == null) return; // Character editor was closed earlier

            _wardrobesHolder.UpdateSelections(entities);
            _palettePanel.UpdateSelections(_characterEditor.GetColors());
            if (_subCategoryHasColor) UpdateColorPallet();
        }
        
        private void UpdateEquppedItemsList(IEntity[] entities)
        {
            var wardrobes = entities
                .Where(e => e.GetType().IsAssignableFrom(typeof(WardrobeFullInfo)) ||
                            e.GetType().IsAssignableFrom(typeof(WardrobeShortInfo)))
                .Select(e => e switch
                {
                    WardrobeFullInfo fullInfo => fullInfo.ToShortInfo(),
                    WardrobeShortInfo shortInfo => shortInfo,
                    _ => null
                })
                .Where(e => e != null)
                .Where(item => 
                {
                    var category = GetWardrobeCategoryById(item.WardrobeCategoryId);
                    return category != null && category.WardrobeCategoryTypeId != BODY_CATEGORY_TYPE_ID;
                });

            _equippedItemsPanel.Setup(wardrobes);
            _equippedItemsCounter.text = wardrobes.Count().ToString();
        }

        private void OnCharacterDNAChanged()
        {
            var dnaValues = _characterEditor.GetDNAValues();
            _adjustmentHolder.UpdateDnaValues(dnaValues);
            var characterColors = _characterEditor.GetColors();
            _colorsHolder.UpdateSelections(characterColors);
            _palettePanel.UpdateSelections(characterColors);
        }

        private void HideColorAndAdjustmentPanels()
        {
            OnApplyPanelChangesConfirmed();
        }

        private void ShowOutfits()
        {
            _wardrobesHolder.CurrentSubCategoryId = null;
            _wardrobesHolder.ShowItems(WardrobeItemsHolder.CategoryType.Outfit, true, CreateOutfit);
        }

        private void CreateOutfit()
        {
            SaveOutfitClicked?.Invoke();
        }

        private void ShowOutfitsHistory()
        {
            _wardrobesHolder.CurrentSubCategoryId = null;
            _wardrobesHolder.ShowItems(WardrobeItemsHolder.CategoryType.Outfit);
        }

        private void ShowFilteringPanel()
        {
            _filteringPanel.SetFilteringSettings(_filteringSetting);
            _filteringPanel.Show();        
        }

        private void ShowEquippedItemsPanel()
        {
            _equippedItemsPanel.Show();        
        }

        private void SetFilteringSetting(FilteringSetting filteringSetting)
        {
            _filteringSetting = filteringSetting;
            _wardrobesHolder._filteringSetting = filteringSetting;
            UpdateFilteringButtonState();
        }

        private void OnFilteringApplied(FilteringSetting setting)
        {
            SetFilteringSetting(setting);

            if (_currentSubCategory != null)
            {
                ShowItems();
            }
            
            UpdateFilteringButtonState();
        }

        private void UpdateFilteringButtonState()
        {
            _filteringButton.SetActive(!_filteringSetting.IsNoFiltersSetted);
        }

        private void AmplitudeLogCategorySelected(long id, string name)
        {
            var categoryMetaData = new Dictionary<string, object>
            {
                [AmplitudeEventConstants.EventProperties.CATEGORY_ID] = id,
                [AmplitudeEventConstants.EventProperties.CATEGORY_NAME] = name
            };
            _amplitudeManager.LogEventWithEventProperties(AmplitudeEventConstants.EventNames.ASSET_CATEGORY_SELECTED, categoryMetaData);
        }
        
        private void AmplitudeLogSubCategorySelected(long categoryId, string categoryName, long subCategoryId, string subCategoryName)
        {
            var subCategoryMetaData = new Dictionary<string, object>
            {
                [AmplitudeEventConstants.EventProperties.CATEGORY_ID] = categoryId,
                [AmplitudeEventConstants.EventProperties.CATEGORY_NAME] = categoryName,
                [AmplitudeEventConstants.EventProperties.SUBCATEGORY_ID] = subCategoryId,
                [AmplitudeEventConstants.EventProperties.SUBCATEGORY_NAME] = subCategoryName
            };
            _amplitudeManager.LogEventWithEventProperties(AmplitudeEventConstants.EventNames.ASSET_SUB_CATEGORY_SELECTED, subCategoryMetaData);
        }

        private bool IsAdjustmentsSubCategory(WardrobeSubCategory wardrobeSubCategory, out DNASliderGroupingSetting groupingSetting)
        {
            groupingSetting = _dnaSlidersGroupingSettings.GroupingSettings.FirstOrDefault(x => x.GetId() == wardrobeSubCategory.Id);
            return groupingSetting != null;
        }

        private void UpdateColorPallet()
        {
            if (!IsSubCategoryInColorPalletConfiguration())
            {
                _palettePanel.gameObject.SetActive(true);
                return;
            }

            var show = _characterEditor.AppliedWardrobeItems.Any(x => x.Wardrobe.WardrobeSubCategoryIds.Intersect(GetCurrentSubCategories()).Any());
            _palettePanel.gameObject.SetActive(show);
        }

        private bool IsSubCategoryInColorPalletConfiguration()
        {
            long[] included = GetCurrentSubCategories();

            return _colorPalletHidingConfiguration.SubCategoryIDs.Intersect(included).Any();
        }

        private long[] GetListSubCategoriesInAllCategory(WardrobeCategory wardrobeCategory)
        { 
            return wardrobeCategory.SubCategories.Select(x => x.Id).ToArray();
        }

        private long[] GetCurrentSubCategories()
        {
            long[] included;
            if (CurrentSubCategory.Id < 0)
            {
                included = GetListSubCategoriesInAllCategory(CurrentCategory); ;
            }
            else
            {
                included = new[] { CurrentSubCategory.Id };
            }
            return included;
        }

        private void ShowHighlighting(AccessoryItem item)
        {
            if (item == null) return;
            _characterEditor.ShowHighlightingWardrobe(item);
        }

        private bool ShouldShowOtherCategories(long currentCategoryId)
        {
            if (_wardrobePanelExtraSettings == null) return true;
            var categorySettings = _wardrobePanelExtraSettings.WardrobeCategoryDatas.FirstOrDefault(x => x.CategoryId == currentCategoryId);
            if (categorySettings == null) return true;
            return !categorySettings.ShowAlone;
        }

        private long[] GetCategoriesToHide(long currentCategoryId)
        {
            if (_wardrobePanelExtraSettings == null) return Array.Empty<long>();
            var categorySetting = _wardrobePanelExtraSettings.WardrobeCategoryDatas.FirstOrDefault(
                    x => x.CategoryId == currentCategoryId);
            if (categorySetting == null || categorySetting.ShowAlone) return Array.Empty<long>();
            return _wardrobePanelExtraSettings.WardrobeCategoryDatas.Where(x => x.ShowAlone).Select(x => x.CategoryId).ToArray();
        }
    }
}