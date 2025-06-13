using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.ClientServer.EditorsSetting;
using Bridge.Models.ClientServer.StartPack.Metadata;
using Bridge.Models.Common;
using Common.ShoppingCart;
using Common.UserBalance;
using Configs;
using Extensions;
using Modules.Amplitude;
using Filtering;
using Modules.Amplitude.Signals;
using Modules.AssetsManaging;
using Modules.AssetsStoraging.Core;
using Modules.CharacterManagement;
using Modules.FreverUMA;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Assets.AssetDependencies;
using Modules.PhotoBooth.Character;
using Modules.WardrobeManaging;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.Pages.UmaEditorPage.Ui.Amplitude;
using UIManaging.Pages.UmaEditorPage.Ui.WardrobePanel;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UIManaging.SnackBarSystem;
using UMA;
using UMA.CharacterSystem;
using UnityEngine;
using Zenject;
using IAsset = Modules.LevelManaging.Assets.IAsset;
using UmaAdjustment = Bridge.Models.ClientServer.StartPack.Metadata.UmaAdjustment;

namespace UIManaging.Pages.UmaEditorPage.Ui
{
    public class UmaEditorPanel : MonoBehaviour, IWardrobeChangesPublisher
    {
        [SerializeField]
        private EditorUtilityView _utilityView;
        [SerializeField]
        private ZoomButton _zoomButton;
        [SerializeField]
        private CharacterEditorWardrobePanelUI _wardrobePanel;
        [SerializeField]
        private RectTransform _characterViewRect;
        [SerializeField]
        private float _minZoom;
        [SerializeField]
        private float _maxZoom;
        [SerializeField] 
        private UserBalanceView _userBalanceView;
        [SerializeField] 
        private ShoppingCart _shoppingCart;
        [SerializeField] 
        private GameObject _notEnoughMoneyIcon;
        [SerializeField]
        private ShoppingBagCounterUI _bagCounterUI;
        [SerializeField]
        private CharacterEditorZoomingSettings _characterEditorZoomingSettings;
        [SerializeField]
        private UmaEditorPageLocalization _localization;
        [SerializeField] 
        private WardrobePanelPurchaseHelper _wardrobePanelPurchaseHelper;
        
        [Inject] private IAssetManager _assetManager;
        [Inject] private IBridge _bridge;
        [Inject] private PopupManager _popupManager;
        [Inject] private PopupManagerHelper _popupManagerHelper;
        [Inject] private UMACharacterEditorRoom _editorRoom;
        [Inject] private CharacterManagerConfig _characterManagerConfig;
        [Inject] private readonly LocalUserDataHolder _userDataHolder;
        [Inject] private readonly ClothesCabinet _clothesCabinet;
        [Inject] private OutfitsManager _outfitsManager;
        [Inject] private AvatarHelper _avatarHelper;
        [Inject] private ICharacterEditor _characterEditor;
        [Inject] private AmplitudeManager _amplitudeManager;
        [Inject] private SnackBarManager _snackBarManager;
        [Inject] private WardrobesResponsesCache _wardrobesResponsesCache;
        [Inject] private IMetadataProvider _metadataProvider;
        [Inject] private SignalBus _signalBus;

        private DynamicCharacterAvatar _avatar;
        private bool _wardrobesInitialized;
        private bool _isNewCharacter;
        private bool _isInteractionLocked;
        private bool _isOnboarding;
        
        public long SelectedGenderId { get; private set; } 

        private AnimatorOverrideController _animatorOverrideController;
        private AnimationClipOverrides _animationClipOverrides;
        private CharacterEditorSettings _editorSettings;
        private long? _taskId;
        private long _startCategoryTypeId;
        private long? _startCategoryId;
        private long? _startSubCategoryId;
        private FilteringSetting _defaultFilteringSetting;

        private CancellationTokenSource _cancellationTokenSource;
        private Stopwatch _stopwatch = new ();

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        public bool IsCharacterModified => _characterEditor.IsCharacterModified;
        public bool HasNotOwnedWardrobes => GetNotOwnedWardrobes().Count > 0;
        public GameObject BagIcon => _bagCounterUI.gameObject;
        
        public bool IsWardrobeChangeInProgress { get; private set; }
        public long GenderId => SelectedGenderId;
        
        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        public event Action<bool> DNAPanelShown;
        public event Action<string> WardrobeTypeChanged;
        public event Action<bool> ShoppingCartShown;
        
        public event Action SaveOutfitClicked;
        public event Action<OutfitShortInfo> DeleteOutfitClicked;
        public event Action WardrobeChanged;
        public event Action WardrobeStartChanging;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        [Inject]
        private void Construct()               
        {
            _utilityView.ResetButton.onClick.AddListener(ResetCharacter);

            _utilityView.UndoButton.onClick.AddListener(() =>
            {
                if (_isInteractionLocked) return;
                _characterEditor.Undo();
            });

            _utilityView.RedoButton.onClick.AddListener(() =>
            {
                if (_isInteractionLocked) return;
                _characterEditor.Redo();
            });

            _zoomButton.Setup(_minZoom, _maxZoom);
            _zoomButton.ZoomClicked += OnZoomButtonClicked;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public async Task SpawnCharacter(long genderId, CancellationToken token = default)
        {
            _avatar = await _characterEditor.CreateNewAvatar(genderId, token);
            _characterEditor.SetTargetAvatar(_avatar);
            if (token.IsCancellationRequested) return;
            _avatar.rebuildSkeleton = true;
            _avatar.CharacterUpdated.AddListener(Initialize);
            _avatar.CharacterUpdated.AddListener(ApplyAnimation);
            _editorRoom.SetRawImageRect(_characterViewRect);
            _editorRoom.Show(_avatar, _metadataProvider.MetadataStartPack.GetRaceByGenderId(genderId));
            _editorRoom.BodyDisplayMode = BodyDisplayMode.FullBody;
        }

        public void Setup(CharacterEditorSettings settings, long? taskId, long startCategoryTypeId, long? startCategoryId, long? startSubCategoryId, FilteringSetting defaultFilteringSetting, bool isOnboarding)
        {
            _isOnboarding = isOnboarding;
            _editorSettings = settings;
            _taskId = taskId;
            _startCategoryTypeId = startCategoryTypeId;
            _startCategoryId = startCategoryId;
            _startSubCategoryId = startSubCategoryId;
            _defaultFilteringSetting = defaultFilteringSetting;
            _wardrobePanel.SetSettings(settings, taskId, startCategoryTypeId, startCategoryId, startSubCategoryId, defaultFilteringSetting);
        }

        public async Task Show(CharacterFullInfo character, OutfitFullInfo outfit, bool isNewCharacter, CancellationToken token = default)
        {
            _characterEditor.CharacterChanged += OnCharacterChanged;
            _characterEditor.CharacterDNAChanged += UpdateUndoRedoButtonsState;
            _avatarHelper.FinishedProcessingAvatar += UpdateUndoRedoButtonsState;
            _wardrobePanel.TypeChanged += CharacterEditorWardrobeTypeChanged;
            _shoppingCart.ItemSelectionChanged += OnShoppingCartItemSelectionChanged;
            
            if (_userDataHolder.IsOnboardingCompleted)
            {
                RefreshUserBalance();
                UpdateBalance(token);
            }
            
            if (!_wardrobesInitialized) InitializeWardrobePanel(character.GenderId);
            
            _wardrobePanel.SetGenderId(character.GenderId);
            _wardrobePanel.Show();

            SelectedGenderId = character.GenderId;
            //_isNewCharacter = character == null;

            _animatorOverrideController = new AnimatorOverrideController(_editorRoom.roomCharacterController);
            _animationClipOverrides = new AnimationClipOverrides(_animatorOverrideController.overridesCount);

            LoadBodyAnimation();
            _characterEditor.SetGenderId(SelectedGenderId);
            _characterEditor.SetCompressionEnabled(false);
            await SpawnCharacter(SelectedGenderId, token);
            if (token.IsCancellationRequested) return;

            var gender = _metadataProvider.MetadataStartPack.Genders.First(x => x.Id == character.GenderId);
            await _characterEditor.LoadCharacter(character, outfit, token);
            if (token.IsCancellationRequested) return;
            
            _editorRoom.ZoomChanged += OnZoomChanged;
            _wardrobePanel.SetDefaultDNAs(gender);
            _utilityView.SetCharacterCreationMode(isNewCharacter);
            _userDataHolder.RefreshUserInfo();
            UnlockItems();
        }

        public void SwitchCategoryType(WardrobeCategoryType type)
        {
            _wardrobePanel.SwitchCategoryType(type);
        }

        public async Task<Texture2D> GetSnapshotAsync(BodyDisplayMode bodyDisplayMode) 
        {
            return await _editorRoom.GetSnapshot(bodyDisplayMode);
        }

        public void Clear()
        {
            if (_avatar != null)
            {
                _avatarHelper.ReleaseAvatarResources(_avatar, true);
                _avatar = null;
            }

            _cancellationTokenSource?.CancelAndDispose();
            _wardrobePanel.Clear();
            _characterEditor.Clear();
            _userBalanceView.ContextData?.CleanUp();
            _userBalanceView.CleanUp();
            _editorRoom.Hide();
            _editorRoom.DespawnAvatar();
            _wardrobesInitialized = false;
            _wardrobePanel.ItemSelected -= OnWardrobeSelected;
            _wardrobePanel.PurchaseRequested -= OnPurchaseRequested;
            _wardrobePanel.AdjustmentChanged -= OnAdjustmentChanged;
            _wardrobePanel.AdjustmentChangedDiff -= OnAdjustmentChangedDiff;
            _wardrobePanel.ColorPicked -= OnColorPicked;
            _wardrobePanel.SubCategorySelected -= OnSubCategorySelected;
            _wardrobePanel.DnaPanelShown -= OnDnaPanelShown;
            _wardrobePanel.SaveOutfitClicked -= OnSaveOutfitClicked;
            _characterEditor.CharacterChanged -= OnCharacterChanged;
            _characterEditor.CharacterDNAChanged -= UpdateUndoRedoButtonsState;
            _avatarHelper.FinishedProcessingAvatar -= UpdateUndoRedoButtonsState;
            _wardrobePanel.TypeChanged -= CharacterEditorWardrobeTypeChanged;
            _shoppingCart.ItemSelectionChanged -= OnShoppingCartItemSelectionChanged;
            _shoppingCart.ShoppingCartClosed -= OnShoppingCartClosed;
            SaveOutfitClicked = null;
            DeleteOutfitClicked = null;
            WardrobeTypeChanged = null;
            WardrobeChanged = null;
            WardrobeStartChanging = null;
        }

        public string GetCharacterRecipe() 
        {
            return _avatar.GetCurrentRecipe();
        }

        public KeyValuePair<string, List<WardrobeFullInfo>> GetCharacterRecipeAndWardrobes()
        {
            return _characterEditor.GetCharacterRecipeAndWardrobes();
        }

        public Dictionary<string, Color32> GetCharacterColors()
        {
            return _characterEditor.GetColors();
        }

        public List<KeyValuePair<long, int>> GetCharacterColorsInt()
        {
            return _characterEditor.GetCharacterColorsInt();
        }

        public void OnOutfitSaved()
        {
            _wardrobePanel.UpdateOutfitList();
            _userDataHolder.RefreshUserInfo();
        }

        public void OnOutfitDeleted()
        {
            _wardrobePanel.UpdateOutfitList();
        }

        public void ShowShoppingCart(Action onPurchaseRequested)
        {
            _shoppingCart.Show(_isOnboarding, UnlockItems);
            _shoppingCart.Setup(GetNotOwnedWardrobes(), onPurchaseRequested);
            ShoppingCartShown?.Invoke(true);
            _editorRoom.BodyDisplayMode = BodyDisplayMode.FullBody;
        }

        public List<WardrobeShortInfo> GetNotOwnedWardrobes()
        {
            return _wardrobePanelPurchaseHelper.GetNotOwnedWardrobes();
        }

        public void UpdateShoppingCartIcons()
        {
            UpdateShoppingCartIconsInternal();
        }

        public void LockItems()
        {
            _isInteractionLocked = true;
        }

        public void UnlockItems()
        {
            _isInteractionLocked = false;
        }

        public bool CheckSufficientFunds()
        {
            var notOwnedList = GetNotOwnedWardrobes();
            var neededSoft = 0;
            var neededHard = 0;
            var hasLockedItem = false;

            var currentLevel = 0;
            if (_userDataHolder.LevelingProgress != null)
            {
                var levelObj = _userDataHolder?.LevelingProgress.Xp.CurrentLevel;
                if (levelObj != null) currentLevel = levelObj.Level;
            }

            foreach (var wardrobe in notOwnedList)
            {
                var assetOffer = wardrobe.AssetOffer;
                if (assetOffer == null) continue;

                if (assetOffer.AssetOfferSoftCurrencyPrice.HasValue)
                    neededSoft += assetOffer.AssetOfferSoftCurrencyPrice.Value;

                if (assetOffer.AssetOfferHardCurrencyPrice.HasValue)
                    neededHard += assetOffer.AssetOfferHardCurrencyPrice.Value;

                var requiredLevel = wardrobe.SeasonLevel;
                if (requiredLevel.HasValue && requiredLevel.Value > currentLevel)
                    hasLockedItem = true;
            }

            return !(neededSoft > 0 && neededSoft > _userDataHolder.UserBalance.SoftCurrencyAmount
                  || neededHard > 0 && neededHard > _userDataHolder.UserBalance.HardCurrencyAmount ||
                     hasLockedItem);
        }
        
        public void MarkAppliedWardrobesAsBought()
        {
            var notOwned = GetNotOwnedWardrobes();
            foreach (var wardrobe in notOwned)
            {
                _clothesCabinet.SetItemAsPurchased(wardrobe.Id);
            }
        }
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void Initialize(UMAData data)
        {
            _avatar.CharacterUpdated.RemoveListener(Initialize);
        }

        private void ResetCharacter()
        {
            var variants = new List<KeyValuePair<string, Action>>
            {
                new(_localization.ResetPopupDesignNewLookOption, OnSelectDesignNewLook),
                new(_localization.ResetPopupGoBackToLastSavedOption, _characterEditor.ResetCharacter)
            };

            var popup = new ActionSheetPopupConfiguration
            {
                PopupType = PopupType.ActionSheetDark,
                Description = _localization.ResetPopupTitle,
                Variants = variants
            };

            _popupManager.SetupPopup(popup);
            _popupManager.ShowPopup(popup.PopupType);
        }

        private void OnSelectDesignNewLook()
        {
            var configuration = new DialogPopupConfiguration
            {
                PopupType = PopupType.Dialog,
                Description = _localization.StartOverPopupDesc,
                YesButtonText = _localization.StartOverPopupOkButton,
                NoButtonText = _localization.StartOverPopupCancelButton,
                OnYes = _characterEditor.UndressCharacter
            };

            _popupManager.SetupPopup(configuration);
            _popupManager.ShowPopup(configuration.PopupType);
        }

        private void InitializeWardrobePanel(long genderId)
        {
            _wardrobesInitialized = true;
            _wardrobePanel.SetSettings(_editorSettings, _taskId, _startCategoryTypeId, _startCategoryId, _startSubCategoryId, _defaultFilteringSetting);
            _wardrobePanel.Setup(_isOnboarding, genderId);
            _wardrobePanel.ItemSelected += OnWardrobeSelected;
            _wardrobePanel.PurchaseRequested += OnPurchaseRequested;
            _wardrobePanel.AdjustmentChanged += OnAdjustmentChanged;
            _wardrobePanel.AdjustmentChangedDiff += OnAdjustmentChangedDiff;
            _wardrobePanel.ColorPicked += OnColorPicked;
            _wardrobePanel.SubCategorySelected += OnSubCategorySelected;
            _wardrobePanel.DnaPanelShown += OnDnaPanelShown;
            _wardrobePanel.SaveOutfitClicked += OnSaveOutfitClicked;
            _shoppingCart.ShoppingCartClosed += OnShoppingCartClosed;
        }

        private void OnWardrobeSelected(IEntity entity)
        {
            if (_isInteractionLocked || IsWardrobeChangeInProgress) return;
            
            switch (entity)
            {
                case WardrobeShortInfo wardrobeItem:
                {
                    _characterEditor.CharacterChanged += OnWardrobeChanged;
                    _characterEditor.ChangeWardrobeItem(wardrobeItem.Id);
                    UpdateLoadingState(entity);
                
                    _stopwatch.Restart();

                    void OnWardrobeChanged(IEntity[] entities)
                    {
                        _characterEditor.CharacterChanged -= OnWardrobeChanged;

                        var setting = _characterEditorZoomingSettings.ZoomingSettings.Find(x =>
                            wardrobeItem.WardrobeSubCategoryIds != null && wardrobeItem.WardrobeSubCategoryIds.Contains(x.Id)
                         || x.Id == -wardrobeItem.WardrobeCategoryId && x.ForWholeCategory);

                        _editorRoom.BodyDisplayMode = setting?.BodyDisplayMode ?? BodyDisplayMode.FullBody;
                    
                        _stopwatch.Stop();
                        
                        var amplitudeEvent = new WardrobeSelectedAmplitudeEvent(wardrobeItem.Id, _stopwatch.ElapsedMilliseconds.ToSecondsClamped());
                        
                        _signalBus.Fire(new AmplitudeEventSignal(amplitudeEvent));
                    }

                    break;
                }
                case OutfitShortInfo outfit:
                    HandleOutfitSelection(outfit);
                    break;
            }
        }

        private void OnPurchaseRequested(IEntity purchasedAsset)
        {
            if (_isInteractionLocked)
            {
                return;
            }
            
            _wardrobePanelPurchaseHelper.Purchase(purchasedAsset);
        }
        
        private void OnShoppingCartItemSelectionChanged(IEntity entity, bool selected)
        {
            OnWardrobeSelected(entity);
        }

        private void OnShoppingCartClosed()
        {
            UnlockItems();
            ShoppingCartShown?.Invoke(false);
            RefreshUserBalance();
        }

        private void UpdateLoadingState(IEntity entity)
        {
            IsWardrobeChangeInProgress = true;
            WardrobeStartChanging?.Invoke();
            _wardrobePanel.UpdateLoadingState(entity);
        }

        private void OnAdjustmentChanged(UmaAdjustment adjustment, float value)
        {
            var adjustmentName = adjustment.Key;
            _characterEditor.EditCharacterDNA(adjustmentName, value, true);
        }

        private void OnAdjustmentChangedDiff(UmaAdjustment adjustment, float startValue, float endValue)
        {
            var adjustmentName = adjustment.Key;
            _characterEditor.EndCharacterDNAEdit(adjustmentName, startValue, endValue);
        }

        private void OnColorPicked(string colorName, Color32 color)
        {
            _characterEditor.EditCharacterColor(colorName, color, _characterEditor.AppliedWardrobeItems.Select(x=>x.Wardrobe));
        }

        private async void LoadBodyAnimation()
        {
            //todo: replace hardcoded ids and body anim requests via paginated list requests when FREV-10152 is done
            var idleAnimation = await GetBodyAnimation(171);
            _assetManager.Load(idleAnimation, OnIdleAnimationAssetLoaded);
        }

        private async Task<BodyAnimationInfo> GetBodyAnimation(long id)
        {
            var result = await _bridge.GetBodyAnimationListAsync(id, 0, 0, 0);
            if (result.IsSuccess)
            {
                return result.Models.First();
            }
            throw new Exception($"Failed to load body animation. Reason: {result.ErrorMessage}");
        }

        private void OnIdleAnimationAssetLoaded(IAsset asset)
        {
            var bodyAnimationAsset = (IBodyAnimationAsset)asset;
            _animatorOverrideController.GetOverrides(_animationClipOverrides);

            _animationClipOverrides["Idle"] = bodyAnimationAsset.BodyAnimation;
            ApplyAnimation(null);
        }

        private void ApplyAnimation(UMAData data)
        {
            if (_avatar == null) return;

            var animator = _avatar.GetComponent<Animator>();
            if (animator == null) return;

            animator.runtimeAnimatorController = _animatorOverrideController;
            _animatorOverrideController.ApplyOverrides(_animationClipOverrides);
        }

        private void OnSubCategorySelected(long subCategoryId)
        {
            if(_wardrobePanel.CurrentCategory is null || _characterEditorZoomingSettings is null) return;

            var setting = _characterEditorZoomingSettings.ZoomingSettings.Find(x => x.Id == subCategoryId 
                            || (x.Id == -_wardrobePanel.CurrentCategory.Id && x.ForWholeCategory));

            _editorRoom.BodyDisplayMode = setting?.BodyDisplayMode ?? BodyDisplayMode.FullBody;
        }

        private void OnZoomButtonClicked(float newZoom)
        {
            _editorRoom.SetZoom(newZoom);
        }

        private void OnZoomChanged(float value)
        {
            _zoomButton.SetZoom(value);
        }

        private void OnDnaPanelShown(bool visibility)
        {
            DNAPanelShown?.Invoke(visibility);
        }

        private void OnSaveOutfitClicked()
        {
            SaveOutfitClicked?.Invoke();
        }

        private void HandleOutfitSelection(OutfitShortInfo outfit)
        {
            var firstVariant = _localization.DeleteOutfitOption;
            var secondVariant = _localization.ChangeToThisOutfitOption;
            var minVariantIndex = 0;
            Action firstVariantAction = () => DeleteOutfit(outfit);
            Action secondVariantAction = () => SelectOutfit(outfit);

            if (outfit.SaveMethod == SaveOutfitMethod.Automatic)
            {
                firstVariant = _localization.ChangeToThisLookOption;
                secondVariant = _localization.AddToFavouriteOutfitOption;
                minVariantIndex = 4;
                firstVariantAction = () => SelectOutfit(outfit);
                secondVariantAction = () => SaveHistoryAsManually(outfit);
            }

            var variants = new List<KeyValuePair<string, Action>>()
            {
                new KeyValuePair<string, Action>(firstVariant, firstVariantAction),
                new KeyValuePair<string, Action>(secondVariant, secondVariantAction)
            };
            var popupConfig = new ActionSheetPopupConfiguration
            {
                PopupType = PopupType.ActionSheetDark,
                Variants = variants,
                Description = _localization.OutfitOptionsHeader,
                MainVariantIndexes = new[] { minVariantIndex }
            };

            _popupManager.SetupPopup(popupConfig);
            _popupManager.ShowPopup(popupConfig.PopupType);
        }

        private void SelectOutfit(OutfitShortInfo outfit)
        {
            _characterEditor.ApplyOutfit(outfit, SelectedGenderId);
            UpdateLoadingState(outfit);
        }

        private void DeleteOutfit(OutfitShortInfo outfit)
        {
            DeleteOutfitClicked?.Invoke(outfit);
        }

        private async void SaveHistoryAsManually(OutfitShortInfo outfit)
        {
            await _outfitsManager.SaveAutosavedAsManual(outfit);       
        }

        private void OnCharacterChanged(IEntity[] entities)
        {
            IsWardrobeChangeInProgress = false;
            UpdateShoppingCartIconsInternal();
            WardrobeChanged?.Invoke();
            _shoppingCart.Unlock();
        }

        private void UpdateUndoRedoButtonsState()
        {
            _utilityView.UndoButton.interactable = _characterEditor.IsCharacterModified;
            _utilityView.RedoButton.interactable = !_characterEditor.IsRedoEmpty;
        }

        private void CharacterEditorWardrobeTypeChanged(string typeName)
        {
            WardrobeTypeChanged?.Invoke(typeName);
        }

        private void UpdateShoppingCartIconsInternal()
        {
            var notOwnedList = GetNotOwnedWardrobes();
            
            _notEnoughMoneyIcon.SetActive(!CheckSufficientFunds());
            _bagCounterUI.SetBagNumber(notOwnedList.Count);
        }

        private void RefreshUserBalance()
        {
            var userBalanceModel = new StaticUserBalanceModel(_userDataHolder);
            _userBalanceView.ContextData?.CleanUp();
            _userBalanceView.Initialize(userBalanceModel);
        }

        private void UpdateBalance(CancellationToken token = default)
        {
            _userDataHolder.UpdateBalance(token);
        }
    }
}