using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Abstract;
using Bridge;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.ClientServer.EditorsSetting;
using Bridge.Models.ClientServer.StartPack.Metadata;
using Bridge.Models.Common;
using Bridge.Models.Common.Files;
using Common;
using Common.UI;
using Common.UserBalance;
using Extensions;
using Extensions.Wardrobe;
using Modules.Amplitude;
using Modules.Amplitude.Signals;
using Modules.AssetsStoraging.Core;
using Modules.CameraSystem.CameraAnimations;
using Modules.CameraSystem.CameraSystemCore;
using Modules.CharacterManagement;
using Modules.FreverUMA;
using Modules.LevelManaging.Editing.LevelManagement;
using Modules.PhotoBooth.Character;
using Navigation.Args;
using Sirenix.OdinInspector;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.Pages.UmaEditorPage;
using UIManaging.Pages.UmaEditorPage.Ui;
using UIManaging.Pages.UmaEditorPage.Ui.Amplitude;
using UIManaging.Pages.UmaEditorPage.Ui.Shared;
using UIManaging.Pages.UmaEditorPage.Ui.ShoppingCartInfo;
using UIManaging.Pages.UmaEditorPage.Ui.WardrobePanel;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UIManaging.SnackBarSystem;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils;
using Zenject;
using CharacterController = Models.CharacterController;
using Debug = UnityEngine.Debug;
using FileInfo = Bridge.Models.Common.Files.FileInfo;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Pages.LevelEditor.Ui.Wardrobe
{
    public sealed class UmaLevelEditor : BaseContextDataView<UmaEditorArgs>, IWardrobeChangesPublisher
    {
        private const float DUTCH_DEFAULT = 0;
        private const float DOF_DEFAULT = 65;
        public event Action WardrobeStartChanging;
        public event Action WardrobeChanged;
        public event Action<IEntity> WardrobeEntityChanged;
        public event Action<OutfitFullInfo> OutfitSelected;
        
        [SerializeField] private float _cameraTweenTime = 0.5f;
        [SerializeField] private float _rotateSpeed = 1f;
        
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _doneButton;
        [SerializeField] private QuickClickDetector _quickClickDetector;
        [SerializeField] private LevelEditorWardrobePanel _wardrobePanel;
        [SerializeField] private CharacterPhotoBooth _photoBooth;
        [SerializeField] private EditorUtilityView _utilityView;
        [SerializeField] private UserBalanceView _userBalanceView;

        [SerializeField] private CharacterEditorZoomingSettings _characterEditorZoomingSettings;
        //[SerializeField] private CameraBodyPartFocusSetting[] _zoomSettings;
        [SerializeField] private CameraBodyPartFocusSettingContainer[] _zoomSettingsContainer;
        [SerializeField] private UmaEditorPageLocalization _localization;
        [SerializeField] private WardrobePanelPurchaseHelper _wardrobePanelPurchaseHelper;
        [SerializeField] private ShoppingCartInfo _shoppingCartInfo; 
        [SerializeField] private UmaEditorStartingGiftController _startingGiftController;
        
        [Inject] private ILevelManager _levelManager;
        [Inject] private AmplitudeManager _amplitudeManager;
        [Inject] private IBridge _bridge;
        [Inject] private ICharacterEditor _characterEditor;
        [Inject] private OutfitsManager _outfitsManager;
        
        [Inject] private ICameraSystem _cameraSystem;
        [Inject] private PopupManager _popupManager;
        [Inject] private PopupManagerHelper _popupManagerHelper;
        [Inject] private SnackBarHelper _snackBarHelper;
        [Inject] private LocalUserDataHolder _userDataHolder;
        [Inject] private WardrobesResponsesCache _wardrobesResponsesCache;
        [Inject] private SnackBarManager _snackBarManager;
        [Inject] private ShoppingCartInfoModel _shoppingCartInfoModel;
        [Inject] private UmaLevelEditorPanelModel _panelModel;
        [Inject] private IMetadataProvider _metadataProvider;
        [Inject] private SignalBus _signalBus;
        
        private CharacterEditorSettings _editorSettings;
        private CancellationTokenSource _cancellationTokenSource;

        private Dictionary<object, float> _storedCameraSettings = new();

        private bool IsFollowingStored;
        private bool IsLookAtStored;
        private int _cullingMaskStored;

        private bool _isWardrobeChangeInProgress;
        private BodyDisplayMode? _currentDisplayMode;

        private CharacterController _targetCharacterController;
        private Stopwatch _stopwatch = new();

        private bool IsWardrobeChangeInProgress
        {
            get => _isWardrobeChangeInProgress;
            set
            {
                _isWardrobeChangeInProgress = value;
                _doneButton.interactable = !value;
                _backButton.interactable = !value;
            }
        }

        public void Show(long categoryId, long? subcategoryId)
        {
            var targetCharacterNumber = _levelManager.EditingCharacterSequenceNumber == -1
                ? 0
                : _levelManager.EditingCharacterSequenceNumber;
            
            _targetCharacterController = _levelManager.TargetEvent.GetCharacterController(targetCharacterNumber);
            
            var categoryType = _wardrobePanel.GetWardrobeCategoryById(categoryId).WardrobeCategoryTypeId;
            var genderId = _targetCharacterController.Character.GenderId;
            _wardrobePanel.SetGenderId(genderId);
            _wardrobePanel.SetStartCategories(categoryType, categoryId ,subcategoryId);
            _wardrobePanel.Show();
            _wardrobePanel.UpdateSelected(_targetCharacterController.GetUsedWardrobeEntities());
            gameObject.SetActive(true);
            _currentDisplayMode = null;
            FocusCameraOnCharacter();
            var subC = _wardrobePanel.CurrentSubCategory?.Id ?? 0;
            OnSubCategorySelected(subC);
            
            _wardrobePanel.ItemSelected += OnWardrobeSelected;
            _wardrobePanel.PurchaseRequested += OnPurchaseRequested;
            _wardrobePanel.AdjustmentChanged += OnAdjustmentChanged;
            _wardrobePanel.AdjustmentChangedDiff += OnAdjustmentChangedDiff;
            _wardrobePanel.ColorPicked += OnColorPicked;
            _wardrobePanel.SubCategorySelected += OnSubCategorySelected;
            _wardrobePanel.DnaPanelShown += OnDnaPanelShown;
            _wardrobePanel.SaveOutfitClicked += OnSaveOutfitClicked;
            
            _utilityView.ResetButton.onClick.AddListener(OnResetButton);
            _utilityView.UndoButton.onClick.AddListener(OnUndoButton);
            _utilityView.RedoButton.onClick.AddListener(OnRedoButton);

            _outfitsManager.OutfitDeleted += OnOutfitDeleted;
            _outfitsManager.OutfitAdded += OnOutfitAdded;
            _userBalanceView.Initialize(new StaticUserBalanceModel(_userDataHolder));
            
            _shoppingCartInfo.Initialize(_shoppingCartInfoModel);
            _startingGiftController.Initialize(ContextData);
            
            _panelModel.OnPanelOpened();
        }

        private void OnOutfitDeleted(IEntity obj)
        {
            _wardrobePanel.UpdateOutfitList();
        }

        public void OnOutfitAdded(IEntity obj)
        {
            _wardrobePanel.UpdateOutfitList();
        }

        private void OnRedoButton()
        {
            if (IsWardrobeChangeInProgress) return;
            _levelManager.RedoLastCharacterOutfitChange();
        }

        private void OnUndoButton()
        {
            if (IsWardrobeChangeInProgress) return;
            _levelManager.UndoLastCharacterOutfitChange();
        }
        
        private void OnResetButton()
        {
            if (IsWardrobeChangeInProgress) return;
            
            var variants = new List<KeyValuePair<string, Action>>
            {
                new(_localization.ResetPopupDesignNewLookOption, OnSelectDesignNewLook),
                new(_localization.ResetPopupGoBackToLastSavedOption, _levelManager.ResetCharacterToInitialState)
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
                OnYes = _levelManager.UndressCharacter
            };

            _popupManager.SetupPopup(configuration);
            _popupManager.ShowPopup(configuration.PopupType);
        }

        [Button]
        public async void TakeOutfitPhoto()
        {
            var tex = await CaptureOutfitThumbnail();
            
            if(!Directory.Exists(Path.Combine(Application.persistentDataPath, "PhotoTest")))
            {
                Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, "PhotoTest"));
            }
                
            File.WriteAllBytes(Path.Combine(Application.persistentDataPath, "PhotoTest", "TestPhoto.png"), tex.EncodeToPNG());
        }

        private async Task<Texture2D> CaptureOutfitThumbnail()
        {
            await Task.Yield();
            var character = _levelManager.EditingTargetCharacterAsset.GameObject.transform;
            var raceId = _metadataProvider.MetadataStartPack.GetRaceByGenderId(ContextData.Gender.Id).Id;
            return await _photoBooth.GetPhotoAsync(raceId, BodyDisplayMode.FullBody, character.position, character.rotation, false);
        }

        public async Task UpdateOutfitThumbnail()
        {
            if (!_levelManager.EditingCharacterController.UnsavedOutfitEnabled)
            {
                return;
            }
            
            var thumbnail = await CaptureOutfitThumbnail();

            _levelManager.EditingCharacterController.Outfit.Files = new()
            {
                new FileInfo(thumbnail, FileExtension.Png, Resolution._128x128),
                new FileInfo(thumbnail, FileExtension.Png, Resolution._256x256),
                new FileInfo(thumbnail, FileExtension.Png, Resolution._512x512)
            };
        }

        private void FocusCameraOnCharacter()
        {
            _storedCameraSettings[CameraAnimationProperty.AxisX] =
                _cameraSystem.GetValueOf(CameraAnimationProperty.AxisX);
            _storedCameraSettings[CameraAnimationProperty.AxisY] =
                _cameraSystem.GetValueOf(CameraAnimationProperty.AxisY);
            _storedCameraSettings[CameraAnimationProperty.OrbitRadius] =
                _cameraSystem.GetValueOf(CameraAnimationProperty.OrbitRadius);
            _storedCameraSettings[CameraAnimationProperty.FieldOfView] =
                _cameraSystem.GetValueOf(CameraAnimationProperty.FieldOfView);
            _storedCameraSettings[CameraAnimationProperty.Dutch] =
                _cameraSystem.GetValueOf(CameraAnimationProperty.Dutch);
            _storedCameraSettings[CameraAnimationProperty.DepthOfField] =
                _cameraSystem.GetValueOf(CameraAnimationProperty.DepthOfField);
            
            IsFollowingStored = _cameraSystem.Follow;
            IsLookAtStored = _cameraSystem.LookAt;
            
            _cullingMaskStored = _cameraSystem.LayerMask;
            var activeCharacterMask = LayerMask.GetMask(LayerMask.LayerToName(_levelManager.EditingTargetCharacterAsset.View.GameObject.layer));
            var otherCharactersMask = LayerMask.GetMask("Character0", "Character1", "Character2") ^ activeCharacterMask;
            _cameraSystem.LayerMask ^= otherCharactersMask;
            
            _cameraSystem.SetTweened(CameraAnimationProperty.Dutch, DUTCH_DEFAULT, _cameraTweenTime);
            _cameraSystem.SetTweened(CameraAnimationProperty.DepthOfField, DOF_DEFAULT, _cameraTweenTime);

            _photoBooth.CullingMask = activeCharacterMask;
            //DeadZone?
        }

        public void Hide()
        {
            _wardrobePanel.ItemSelected -= OnWardrobeSelected;
            _wardrobePanel.PurchaseRequested -= OnPurchaseRequested;
            _wardrobePanel.AdjustmentChanged -= OnAdjustmentChanged;
            _wardrobePanel.AdjustmentChangedDiff -= OnAdjustmentChangedDiff;
            _wardrobePanel.ColorPicked -= OnColorPicked;
            _wardrobePanel.SubCategorySelected -= OnSubCategorySelected;
            _wardrobePanel.DnaPanelShown -= OnDnaPanelShown;
            _wardrobePanel.SaveOutfitClicked -= OnSaveOutfitClicked;
            
            _utilityView.ResetButton.onClick.RemoveListener(OnResetButton);
            _utilityView.UndoButton.onClick.RemoveListener(OnUndoButton);
            _utilityView.RedoButton.onClick.RemoveListener(OnRedoButton);
            
            _outfitsManager.OutfitDeleted -= OnOutfitDeleted;
            _outfitsManager.OutfitAdded -= OnOutfitAdded;
            
            gameObject.SetActive(false);
            _levelManager.ClearCharacterModificationHistory();
            ResetCamera();
            
            _shoppingCartInfo.CleanUp();
            _startingGiftController.CleanUp();
            
            _panelModel.OnPanelClosed();
        }
        
        protected override void OnInitialized()
        {
            _editorSettings = ContextData.CharacterEditorSettings;
                
            _wardrobePanel.SetSettings(_editorSettings, ContextData.TaskId, ContextData.CategoryTypeId, 
                                       ContextData.ThemeCollectionId != null ? Constants.Wardrobes.COLLECTIONS_CATEGORY_ID : ContextData.CategoryId, 
                                       ContextData.ThemeCollectionId ?? ContextData.SubCategoryId, ContextData.DefaultFilteringSetting);
            
            _wardrobePanel.Setup(false, ContextData.Gender.Id);
            
            _characterEditor.CharacterChanged += OnCharacterUpdated;

            _backButton.onClick.AddListener(OnBackButton);
            _doneButton.onClick.AddListener(OnDoneButton);
            _quickClickDetector.Clicked += OnClickedOutside;
        }

        private void OnDoneButton()
        {
            ContextData.ConfirmAction?.Invoke(default);
        }
        
        private void OnBackButton()
        {
            ContextData.BackButtonAction?.Invoke();
        }

        private void OnCharacterUpdated(IEntity[] entities)
        {
            WardrobeChanged?.Invoke();
        }

        protected override void BeforeCleanup()
        {
            _wardrobePanel.ItemSelected -= OnWardrobeSelected;
            _wardrobePanel.PurchaseRequested -= OnPurchaseRequested;
            _wardrobePanel.AdjustmentChanged -= OnAdjustmentChanged;
            _wardrobePanel.AdjustmentChangedDiff -= OnAdjustmentChangedDiff;
            _wardrobePanel.ColorPicked -= OnColorPicked;
            _wardrobePanel.SubCategorySelected -= OnSubCategorySelected;
            _wardrobePanel.DnaPanelShown -= OnDnaPanelShown;
            _wardrobePanel.SaveOutfitClicked -= OnSaveOutfitClicked;
            
            _wardrobePanel.Clear();
            
            _characterEditor.CharacterChanged -= OnCharacterUpdated;
            
            _outfitsManager.OutfitDeleted -= OnOutfitDeleted;
            
            _backButton.onClick.RemoveAllListeners();
            _doneButton.onClick.RemoveAllListeners();
            _quickClickDetector.Clicked -= OnClickedOutside;
        }

        private void OnDnaPanelShown(bool obj)
        {
            
        }

        private void OnSubCategorySelected(long subCategoryId)
        {
            if (_wardrobePanel.CurrentCategory is null || _characterEditorZoomingSettings is null) return;
            
            var setting = _characterEditorZoomingSettings.ZoomingSettings.FirstOrDefault(x => x.Id == subCategoryId 
             || x.Id == -_wardrobePanel.CurrentCategory.Id && x.ForWholeCategory);
            
            var displayMode = setting?.BodyDisplayMode ?? BodyDisplayMode.FullBody;
            
            SetCameraTargetBodyPart(displayMode);
        }

        private void SetCameraTargetBodyPart(BodyDisplayMode displayMode)
        {
            if (_currentDisplayMode == displayMode) return;
            _currentDisplayMode = displayMode;
            var zoomSettings = GetZoomSettings(displayMode);
            
            var targetBone = GetFocusedBone(zoomSettings);
            _cameraSystem.SetTargets(targetBone.gameObject, targetBone.gameObject, true);

            var lookToFaceXAxisValue = _cameraSystem.GetXAxisValueToFocusOnPoint(_levelManager.EditingTargetCharacterAsset.View.GameObject.transform);
            
            _cameraSystem.SetTweened(CameraAnimationProperty.AxisX, lookToFaceXAxisValue, _cameraTweenTime);
            _cameraSystem.SetTweened(CameraAnimationProperty.AxisY, zoomSettings.AxisY, _cameraTweenTime);
            _cameraSystem.SetTweened(CameraAnimationProperty.OrbitRadius, zoomSettings.OrbitRadius, _cameraTweenTime);
            _cameraSystem.SetTweened(CameraAnimationProperty.FieldOfView, zoomSettings.Zoom, _cameraTweenTime, OnComplete);
            
            _cameraSystem.EnableCollider(false);
            
            void OnComplete()
            {
                _cameraSystem.EnableCollider(true);
                _cameraSystem.SetFollow(true);
                _cameraSystem.SetLookAt(true);
            }
        }

        private Transform GetFocusedBone(CameraBodyPartFocusSetting zoomSettings)
        {
            return _levelManager.EditingTargetCharacterAsset.View.GameObject.transform.FindChildInHierarchy(zoomSettings.BoneName);
        }

        private CameraBodyPartFocusSetting GetZoomSettings(BodyDisplayMode displayMode)
        {
            var raceId = _metadataProvider.MetadataStartPack.GetRaceByGenderId(ContextData.Gender.Id).Id;
            var zoomSettings = _zoomSettingsContainer.FirstOrDefault(zs => zs.RaceId == raceId);
            return zoomSettings.Settings.FirstOrDefault(item => item.DisplayMode == displayMode);
        }

        private void ResetCamera()
        {
            _cameraSystem.SetTweened(CameraAnimationProperty.AxisX, _storedCameraSettings[CameraAnimationProperty.AxisX], _cameraTweenTime);
            _cameraSystem.SetTweened(CameraAnimationProperty.AxisY, _storedCameraSettings[CameraAnimationProperty.AxisY], _cameraTweenTime);
            _cameraSystem.SetTweened(CameraAnimationProperty.OrbitRadius, _storedCameraSettings[CameraAnimationProperty.OrbitRadius], _cameraTweenTime);
            _cameraSystem.SetTweened(CameraAnimationProperty.FieldOfView, _storedCameraSettings[CameraAnimationProperty.FieldOfView], _cameraTweenTime, OnComplete);
            _cameraSystem.SetTweened(CameraAnimationProperty.Dutch, _storedCameraSettings[CameraAnimationProperty.Dutch], _cameraTweenTime);
            _cameraSystem.SetTweened(CameraAnimationProperty.DepthOfField, _storedCameraSettings[CameraAnimationProperty.DepthOfField], _cameraTweenTime);
            
            var target = _levelManager.EditingTargetCharacterAsset;
            _cameraSystem.SetTargets(target.LookAtBoneGameObject, target.GameObject, true);
            _cameraSystem.LayerMask = _cullingMaskStored;
            
            void OnComplete()
            {
                _cameraSystem.Follow = IsFollowingStored;
                _cameraSystem.LookAt = IsLookAtStored;
            }
        }
        
        private async void OnColorPicked(string colorName, Color32 color)
        {
            await _levelManager.EditCharacterColor(colorName, color);
        }

        private void OnAdjustmentChangedDiff(UmaAdjustment arg1, float arg2, float arg3)
        {
            
        }

        private void OnAdjustmentChanged(UmaAdjustment arg1, float arg2)
        {
            
        }

        private void OnPurchaseRequested(IEntity purchasedAsset)
        {
            _wardrobePanelPurchaseHelper.Purchase(purchasedAsset);
        }
        
        public void OnDragCharacterViewport(BaseEventData eventData)
        {
            var rotation = ((PointerEventData)eventData).delta.x * _rotateSpeed * Time.deltaTime
                         + _cameraSystem.GetValueOf(CameraAnimationProperty.AxisX) ;

            var minMax = _cameraSystem.GetMinMaxValuesOf(CameraAnimationProperty.AxisX);
            var range = Mathf.Abs(minMax.y - minMax.x);
            
            rotation %= range;

            if (rotation < 0) rotation += range;
            
            _cameraSystem.Set(CameraAnimationProperty.AxisX, rotation);
        }

        private void OnClickedOutside()
        {
            if (IsWardrobeChangeInProgress) return;
            
            ContextData.ClickedOutside?.Invoke();
        }

        public async void OnWardrobeSelected(IEntity entity)
        {
            if (IsWardrobeChangeInProgress) return;

            switch (entity)
            {
                case WardrobeFullInfo wardrobe:
                    UpdateLoadingState(entity);
                    await _levelManager.SwitchWardrobe(_levelManager.EditingCharacterController.CharacterId, wardrobe);
                    WardrobeEntityChanged?.Invoke(entity);
                    break;
                case WardrobeShortInfo wardrobeItem:
                {
                    _stopwatch.Restart();
                    
                    UpdateLoadingState(entity);

                    var wardrobeFullInfo = await _bridge.GetWardrobe(wardrobeItem.Id);

                    if (!wardrobeFullInfo.IsSuccess)
                    {
                        IsWardrobeChangeInProgress = false;
                        Debug.LogError($"Failed to load wardrobe {wardrobeItem.Id}");
                        return;
                    }

                    await _levelManager.SwitchWardrobe(_levelManager.EditingCharacterController.CharacterId, wardrobeFullInfo.Model);
                    
                    _stopwatch.Stop();

                    var amplitudeEvent = new WardrobeSelectedAmplitudeEvent(wardrobeItem.Id, _stopwatch.ElapsedMilliseconds.ToSecondsClamped());

                    _signalBus.Fire(new AmplitudeEventSignal(amplitudeEvent));
                    
                    RefreshCameraFocusTarget();
                    WardrobeEntityChanged?.Invoke(entity);
                    break;
                }
                case OutfitShortInfo outfit:
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
                        new(firstVariant, firstVariantAction),
                        new(secondVariant, secondVariantAction)
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
                    break;
                }
            }

            IsWardrobeChangeInProgress = false;
            WardrobeChanged?.Invoke();
        }
        
        private async void SelectOutfit(OutfitShortInfo outfit)
        {
            UpdateLoadingState(outfit);
            
            var outfitFullInfo = await _bridge.GetOutfitAsync(outfit.Id, default);
            if (!outfitFullInfo.IsSuccess)
            {
                IsWardrobeChangeInProgress = false;
                Debug.LogError($"Failed to load outfit {outfit.Id}");
                return;
            }
            
            await _levelManager.ChangeOutfit(outfitFullInfo.Model);

            IsWardrobeChangeInProgress = false;
            
            RefreshCameraFocusTarget();
            
            OutfitSelected?.Invoke(outfitFullInfo.Model);
        }

        private void RefreshCameraFocusTarget()
        {
            var zoomSettings = GetZoomSettings(_currentDisplayMode.Value);
            var targetBone = _levelManager.EditingTargetCharacterAsset.View.GameObject.transform.FindChildInHierarchy(zoomSettings.BoneName);
            _cameraSystem.SetTargets(targetBone.gameObject, targetBone.gameObject, false);
        }

        private void DeleteOutfit(OutfitShortInfo outfit)
        {
            if (ContextData.OutfitsUsedInLevel != null && ContextData.OutfitsUsedInLevel.Contains(outfit.Id))
            {
                _popupManagerHelper.ShowAlertPopup(_localization.CannotDeleteCharacterUsedInRecordingSnackbarDesc);
                return;
            }
            
            _outfitsManager.DeleteOutfit(outfit);
        }

        private async void SaveHistoryAsManually(OutfitShortInfo outfit)
        {
            await _outfitsManager.SaveAutosavedAsManual(outfit);
        }

        private void OnSaveOutfitClicked()
        {
            var loadingPopupConfig = new ContentSavingPopupConfiguration
            {
                PopupType = PopupType.ContentSaving,
                ProgressMessage = _localization.SavingCharacterLoadingTitle,
            };

            ContextData.SaveOutfitAsFavouriteAction?.Invoke();
        }
        
        private void UpdateLoadingState(IEntity entity)
        {
            IsWardrobeChangeInProgress = true;
            WardrobeStartChanging?.Invoke();
            _wardrobePanel.UpdateLoadingState(entity);
        }

        public void UpdateAfterPurchase(WardrobeFullInfo wardrobeItem)
        {
            _wardrobePanel.UpdateAfterPurchase(wardrobeItem);
        }

        public void UpdateSelected()
        {
            // OutfitShortInfo is missed in the OnOutfitUpdated callback, so, we need to substitute it here [FREV-20355]
            var outfit = _levelManager.EditingCharacterController.Outfit;
            var entities = outfit != null
                ? _levelManager.EditingCharacterController.Outfit.Wardrobes.Cast<IEntity>().ToList()
                : _levelManager.EditingCharacterController.Character.Wardrobes.Cast<IEntity>().ToList();

            if (outfit != null)
            {
                entities.Add(outfit.ToShortInfo());
            }
            
            // we need to filter outfit list for the equipped items since the list contains copies from the other genders [FREV-20973]
            _wardrobePanel.UpdateSelected(entities.ToArray(), outfit is null);
        }
    }
}