using System;
using System.Collections.Generic;
using Extensions;
using Modules.AssetsManaging;
using Modules.FreverUMA;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Modules.WardrobeManaging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Abstract;
using Bridge;
using Common;
using Resolution = Bridge.Models.Common.Files.Resolution;
using Modules.CharacterManagement;
using Modules.PhotoBooth.Character;
using Navigation.Args;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.Common;
using Bridge.Models.ClientServer.EditorsSetting;
using Common.UserBalance;
using Modules.AssetsStoraging.Core;
using TMPro;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.Pages.Tasks;
using UIManaging.Pages.Tasks.TaskRewardFlow;
using UIManaging.Pages.UmaEditorPage.Ui.WardrobePanel;
using UnityEngine.SceneManagement;

namespace UIManaging.Pages.UmaEditorPage.Ui
{
    public sealed class UmaEditor : BaseContextDataView<UmaEditorArgs>
    {
        [SerializeField] private UmaEditorPanel _umaEditorPanel;
        [SerializeField] private GameObject[] _onboardingElements;
        [SerializeField] private GameObject[] _nonOnboardingElements;
        [Space]
        [SerializeField] private GameObject _closeButtonContainer;
        [SerializeField] private Button _closeButton;
        [Space]
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _makeupButton;
        [SerializeField] private Button _clothesButton;
        [Space]
        [SerializeField] private Button _storeButton;
        [SerializeField] private Button _saveCharacterButton;
        [SerializeField] private Button _saveCharacterOnboardingButton;
        [Space]
        [SerializeField] private Image _wardrobeArrow;
        [SerializeField] private RewardFlowManager _rewardFlowManager;
        [SerializeField] private UserBalanceView _userBalanceView;
        [SerializeField] private GameObject _greyOverlay;
        [SerializeField] private GiftOverlay _giftOverlay;
        [SerializeField] private Color _saveButtonColorActive;
        [SerializeField] private Color _saveButtonColorInactive;
        [SerializeField]
        private string _backgroundSceneName = "Set_CharacterEditorBackground";
        [Header("Localization")]
        [SerializeField]
        private UmaEditorPageLocalization _localization;

        [Inject] private LocalUserDataHolder _localUserData;
        [Inject] private IAssetManager _assetManager;
        [Inject] private CharacterManager _characterManager;
        [Inject] private PopupManager _popupManager;
        [Inject] private PopupManagerHelper _popupManagerHelper;
        [Inject] private AvatarHelper _avatarHelper;
        [Inject] private ClothesCabinet _clothesCabinet;
        [Inject] private readonly OutfitsManager _outfitsManager;
        [Inject] private WardrobesResponsesCache _wardrobesResponsesCache;
        [Inject] private IBridge _bridge;
        [Inject] private IMetadataProvider _metadataProvider;
        [Inject] private ICharacterEditor _characterEditor;

        private CharacterEditorSettings _editorSettings;
        private CancellationTokenSource _tokenSource;
        private bool _isSaveInProgress;
        private TextMeshProUGUI _saveButtonText;
        private Image _saveButtonImg;
        private bool _isSaveOutfitInProgress;
        private OnboardingState _onboardingState = OnboardingState.None;

        private readonly Dictionary<BodyDisplayMode, Resolution> _bodyDisplayModeResolutions =
            new Dictionary<BodyDisplayMode, Resolution>()
            {
                { BodyDisplayMode.FullBody, Resolution._512x512 },
                { BodyDisplayMode.HighresFullBody, Resolution._512x512 },
                { BodyDisplayMode.Head, Resolution._128x128 },
                { BodyDisplayMode.HalfBody, Resolution._256x256 },
            };

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        private bool IsOnboarding => ContextData.ConfirmActionType == CharacterEditorConfirmActionType.Onboarding;
        private bool HasStartGift => !IsOnboarding && (!_localUserData.InitialAccountBalance?.IsAdded ?? false) && _metadataProvider.MetadataStartPack.GetUniverseByGenderId(_umaEditorPanel.SelectedGenderId).AllowStartGift;
        private bool IsWardrobeChangeInProgress => _umaEditorPanel.IsWardrobeChangeInProgress;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        protected override void OnDestroy()
        {
            _outfitsManager.OutfitAdded -= OnOutfitSaved;
            _outfitsManager.OutfitDeleted -= OnOutfitDeleted;
            _clothesCabinet.PurchasedItemsUpdated -= OnPurchasedItemsUpdated;
            base.OnDestroy();
        }
        
        private void Awake()
        {
            _outfitsManager.OutfitAdded += OnOutfitSaved;
            _outfitsManager.OutfitDeleted += OnOutfitDeleted;
            _saveButtonText = _saveCharacterButton.GetComponentInChildren<TextMeshProUGUI>();
            _saveButtonImg = _saveCharacterButton.GetComponent<Image>();
            _clothesCabinet.PurchasedItemsUpdated += OnPurchasedItemsUpdated;
            
            _closeButton.onClick.AddListener(ShowExitPopup);
            _saveCharacterButton.onClick.RemoveAllListeners();
            _saveCharacterButton.onClick.AddListener(OnSaveClicked);
            _saveCharacterOnboardingButton.onClick.RemoveAllListeners();
            _saveCharacterOnboardingButton.onClick.AddListener(OnSaveClicked);
            _backButton.onClick.RemoveAllListeners();
            _backButton.onClick.AddListener(OnBackButtonClick);
            _makeupButton.onClick.RemoveAllListeners();
            _makeupButton.onClick.AddListener(OnMakeupClick);
            _clothesButton.onClick.RemoveAllListeners();
            _clothesButton.onClick.AddListener(OnClothesClick);
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override async void OnInitialized()
        {
            _tokenSource = new CancellationTokenSource();
            _clothesCabinet.UpdatePurchasedWardrobes();
            _wardrobesResponsesCache.Invalidate();
            _wardrobeArrow.gameObject.SetActive(false);
            _storeButton.interactable = ContextData.EnableStoreButton;
            _avatarHelper.ReduceTexturesQuality = DeviceInformationHelper.IsLowEndDevice();
            _editorSettings = ContextData.CharacterEditorSettings;
            ApplyEditorSettings();

            _umaEditorPanel.Setup(_editorSettings, ContextData.TaskId, ContextData.CategoryTypeId, 
                                  ContextData.ThemeCollectionId != null ? Constants.Wardrobes.COLLECTIONS_CATEGORY_ID : ContextData.CategoryId, 
                                  ContextData.ThemeCollectionId ?? ContextData.SubCategoryId, 
                                  ContextData.DefaultFilteringSetting, IsOnboarding);

            _umaEditorPanel.SaveOutfitClicked += OnSaveOutfitClicked;
            _umaEditorPanel.DeleteOutfitClicked += OnDeleteOutfitClicked;
            _umaEditorPanel.WardrobeChanged += OnWardrobeChanged;
            _umaEditorPanel.DNAPanelShown += OnDnaPanelShown;

            if (ContextData.ShowTaskInfo)
            {
                _popupManagerHelper.ShowTaskInfoPopup(ContextData.TaskFullInfo);
            }

            SetupButtonsAndOnboardingElements(true);
            LoadBackgroundScene();
            
            await ShowEditor();
            
            SetupButtonsAndOnboardingElements(true);
            ContextData.LoadCompleteAction?.Invoke();
        }

        protected override void BeforeCleanup()
        {
            if (!IsInitialized) return;
            _umaEditorPanel.SaveOutfitClicked -= OnSaveOutfitClicked;
            _umaEditorPanel.DeleteOutfitClicked -= OnDeleteOutfitClicked;
            _umaEditorPanel.WardrobeChanged -= OnWardrobeChanged;
            _umaEditorPanel.DNAPanelShown -= OnDnaPanelShown;
            
            _umaEditorPanel.Clear();
            _assetManager.UnloadAll();
            _avatarHelper.UnloadAllUmaBundles();
            _tokenSource?.Cancel(true);
            UnloadBackgroundScene();
            StopListenStore();
            _wardrobesResponsesCache.Invalidate();
        }

        private void SetupButtonsAndOnboardingElements(bool full)
        {
            foreach (var onboardingElement in _onboardingElements)
            {
                onboardingElement.SetActive(IsOnboarding);
            }
            
            foreach (var nonOnboardingElement in _nonOnboardingElements)
            {
                nonOnboardingElement.SetActive(!IsOnboarding);
            }
            
            _closeButtonContainer.SetActive(!IsOnboarding && ContextData.BackButtonAction != null);

            if (full)
            {
                if (IsOnboarding)
                {
                    _onboardingState = OnboardingState.Body;
                    _makeupButton.SetActive(true);
                    _backButton.SetActive(ContextData.IsNewCharacter);
                }

                _clothesButton.SetActive(false);
                _saveCharacterOnboardingButton.SetActive(false);
            }
        }

        private async void HandleStartGift()
        {
            _userBalanceView.Initialize(new AnimatedUserBalanceModel(
                                            new UserBalanceArgs(_localUserData.UserBalance?.SoftCurrencyAmount ?? 0, 
                                                                _localUserData.UserBalance?.HardCurrencyAmount ?? 0)));
            if (_localUserData.UserBalance is null) await _localUserData.UpdateBalance();
            var userBalance = _localUserData.UserBalance;
            _rewardFlowManager.Initialize(userBalance.SoftCurrencyAmount, userBalance.HardCurrencyAmount, 1, 0);

            var softAmount = _localUserData.InitialAccountBalance.SoftCurrency; 
            var hardAmount = _localUserData.InitialAccountBalance.HardCurrency;
            
            _greyOverlay.SetActive(true);
            _giftOverlay.Show(softAmount, hardAmount, OnGiftOverlayCloseRequested);
        }

        private async void OnGiftOverlayCloseRequested()
        {
            var result = await _bridge.ClaimWelcomeGift();

            if (result.IsError)
            {
                Debug.LogError($"Failed to claim welcome gift, reason: {result.ErrorMessage}");
                _greyOverlay.SetActive(false);
                StartListenStore();
                ContextData.WelcomeGiftReceivedAction?.Invoke();
                return;
            }

            var newSoftAmount = _localUserData.UserBalance.SoftCurrencyAmount +
                                _localUserData.InitialAccountBalance.SoftCurrency;
            var newHardAmount = _localUserData.UserBalance.HardCurrencyAmount +
                                _localUserData.InitialAccountBalance.HardCurrency;
            _localUserData.UserBalance.SoftCurrencyAmount = newSoftAmount;
            _localUserData.UserBalance.HardCurrencyAmount = newHardAmount;

            _localUserData.InitialAccountBalance.IsAdded = true;
            PlayerPrefs.SetInt(Constants.Onboarding.RECEIVED_START_GIFT_IDENTIFIER, 1);
            _rewardFlowManager.StartAnimation(0, newSoftAmount, newHardAmount);
            _rewardFlowManager.FlowCompleted += OnGiftFlowCompleted;
        }

        private void OnBalanceUpdated()
        {
            _userBalanceView.Initialize(new StaticUserBalanceModel(_localUserData));
        }

        private void OnGiftFlowCompleted(RewardFlowResult rewardFlowResult)
        {
            _rewardFlowManager.FlowCompleted -= OnGiftFlowCompleted;
            _greyOverlay.SetActive(false);
            StartListenStore();
            ContextData.WelcomeGiftReceivedAction?.Invoke();
        }

        private async Task ShowEditor()
        {
            var characterInfo = ContextData.IsNewCharacter ? ContextData.Style : _characterManager.SelectedCharacter;

            CharacterFullInfo character;
            if (ContextData.Character != null)
            {
                character = ContextData.Character;
            }
            else
            {
                character = await _characterManager.GetCharacterAsync(characterInfo.Id, _tokenSource.Token);
            }
            
            if (_tokenSource.IsCancellationRequested) return;

            await _umaEditorPanel.Show(character, ContextData.Outfit, false, _tokenSource.Token);
            
            ContextData.HideLoadingPopup?.Invoke();
            
            if (_tokenSource.IsCancellationRequested) return;
            
            if (HasStartGift)
            {
                HandleStartGift();
            }
            else
            {
                StartListenStore();
            }
        }

        private void ApplyEditorSettings()
        {
            var saveAvailable = (_editorSettings == null || _editorSettings.SavingCharacterSettings == null) 
                                || _editorSettings.SavingCharacterSettings.AllowSaveCharacter;

            _saveCharacterButton.gameObject.SetActive(saveAvailable);
        }

        private void ShowExitPopup()
        {
            if (_umaEditorPanel.IsCharacterModified)
            {
                var configuration = new DialogDarkPopupConfiguration
                {
                    PopupType = PopupType.DialogDarkV2,
                    Title = _localization.ExitPopupTitle,
                    Description = _localization.ExitPopupUnsavedChangesDesc,
                    YesButtonText = _localization.ExitPopupExitButton,
                    NoButtonText = _localization.ExitPopupCancelButton,
                    OnYes = OpenInitiatorPage,
                };
                
                _popupManager.SetupPopup(configuration);
                _popupManager.ShowPopup(configuration.PopupType);
            }
            else
            {
                OpenInitiatorPage();
            }
        }
        
        private void OpenInitiatorPage()
        {
            _umaEditorPanel.SaveOutfitClicked -= OnSaveOutfitClicked;
            
            ContextData.BackButtonAction?.Invoke();
        }

        private void FinishedEditing(CharacterFullInfo savedCharacter, OutfitFullInfo outfit) 
        {
            var result = new CharacterEditorOutput()
            {
                Character = savedCharacter,
                Outfit = outfit
            };

            _isSaveInProgress = false;
            if (ContextData.ConfirmAction == null)
            {
                ContextData.ConfirmButtonAction?.Invoke();
            }
            else
            {
                _umaEditorPanel.SaveOutfitClicked -= OnSaveOutfitClicked;
                ContextData.ConfirmAction?.Invoke(result);
            }
        }

        private void OnSaveClicked()
        {
            if (_isSaveInProgress || IsWardrobeChangeInProgress) return;

            if (_umaEditorPanel.HasNotOwnedWardrobes)
            {
                ShowShoppingCart(SaveCharacter);
            }
            else
            {
                SaveCharacter();
            }
        }

        private void OnBackButtonClick()
        {
            if (_onboardingState - 1 > OnboardingState.None)
            {
                SwitchToOnboardingState(_onboardingState - 1);
            }
            else
            {
                ShowExitPopup();
            }
        }

        private void OnMakeupClick()
        {
            SwitchToOnboardingState(OnboardingState.Makeup);
        }

        private void OnClothesClick()
        {
            SwitchToOnboardingState(OnboardingState.Clothing);
        }

        private void SwitchToOnboardingState(OnboardingState state)
        {
            switch (state)
            {
                case OnboardingState.Body:
                    _umaEditorPanel.SwitchCategoryType(_clothesCabinet.CategoryTypes.FirstOrDefault(type => type.Id == Constants.Wardrobes.BODY_CATEGORY_TYPE_ID));
                    _makeupButton.SetActive(true);
                    _clothesButton.SetActive(false);
                    _onboardingState = OnboardingState.Body;
                    _saveCharacterOnboardingButton.SetActive(false);
                    _backButton.SetActive(ContextData.IsNewCharacter);
                    break;
                case OnboardingState.Makeup:
                    _onboardingState = OnboardingState.Makeup;
                    _umaEditorPanel.SwitchCategoryType(_clothesCabinet.CategoryTypes.FirstOrDefault(type => type.Id == Constants.Wardrobes.MAKEUP_CATEGORY_TYPE_ID));
                    _makeupButton.SetActive(false);
                    _clothesButton.SetActive(true);
                    _backButton.SetActive(true);
                    _saveCharacterOnboardingButton.SetActive(false);
                    break;
                case OnboardingState.Clothing:
                    _onboardingState = OnboardingState.Clothing;
                    _umaEditorPanel.SwitchCategoryType(_clothesCabinet.CategoryTypes.FirstOrDefault(type => type.Id == Constants.Wardrobes.CLOTHES_CATEGORY_TYPE_ID));
                    _makeupButton.SetActive(false);
                    _clothesButton.SetActive(false);
                    _backButton.SetActive(true);
                    _saveCharacterOnboardingButton.SetActive(true);
                    break;
            }
        }

        private void SaveCharacter()
        {
            FinishEditing();
        }

        private async void FinishEditing()
        {
            _isSaveInProgress = true;
            _umaEditorPanel.LockItems();
            var selectedCharacter = ContextData.Character;
            var isUpdate = selectedCharacter != null && ContextData.IsNewCharacter == false;

            var photosToTake = SelectPhotoTypesToBeTaken(selectedCharacter);
            var photos = await GetCharacterPhotosAsync(photosToTake.ToArray());   
            
            var loadingPopupConfig = new ContentSavingPopupConfiguration
            {
                PopupType = PopupType.ContentSaving,
                ProgressMessage = isUpdate ? _localization.UpdatingCharacterLoadingTitle : _localization.SavingCharacterLoadingTitle,
            };

            _popupManager.SetupPopup(loadingPopupConfig);
            _popupManager.ShowPopup(loadingPopupConfig.PopupType);

            switch (ContextData.ConfirmActionType)
            {
                case CharacterEditorConfirmActionType.SaveCharacter:
                default:
                    await SaveOutfit(SaveOutfitMethod.Automatic, 
                    (saved) =>
                    {
                        SaveCharacterInternal(photos, OnDone);
                    },
                    (message) =>
                    {
                        ShowFailAlertPopup("", message);
                        OnDone();
                    });
                    
                    break;
                case CharacterEditorConfirmActionType.Onboarding:
                    await SaveOutfit(SaveOutfitMethod.Automatic, 
                    (saved) =>
                    {
                        SaveCharacterInternal(photos, OnDone);
                    },
                    (message) =>
                    {
                        ShowFailAlertPopup("", message);
                        OnDone();
                    });
                    
                    break;
                case CharacterEditorConfirmActionType.SaveOutfitAsAutomatic:
                    await SaveOutfit(SaveOutfitMethod.Automatic,
                    (saved) =>
                    {
                        FinishedEditing(null, saved);
                        OnDone();
                    },
                    (message) =>
                    {
                        ShowFailAlertPopup("", message);
                        OnDone();
                    });
                    break;
            }

            void OnDone()
            {
                _umaEditorPanel.MarkAppliedWardrobesAsBought();
                
                _popupManager.ClosePopupByType(loadingPopupConfig.PopupType);
                foreach (var photo in photos)
                {
                    Destroy(photo.Value);
                }
                _isSaveInProgress = false;
                _umaEditorPanel.UnlockItems();
            }
        }

        private List<BodyDisplayMode> SelectPhotoTypesToBeTaken(CharacterFullInfo targetCharacter)
        {
            var photosToTake = new List<BodyDisplayMode>
            {
                BodyDisplayMode.HighresFullBody
            };
            var isUpdate = ContextData.Character != null && ContextData.IsNewCharacter == false;
            if (isUpdate)
            {
                var mainCharacter = _characterManager.SelectedCharacter;
                var isUpdatingMainCharacter = mainCharacter.Id == targetCharacter.Id;
                if (isUpdatingMainCharacter) return photosToTake;
            }

            photosToTake.Add(BodyDisplayMode.Head);
            photosToTake.Add(BodyDisplayMode.HalfBody);

            return photosToTake;
        }

        private void ShowFailAlertPopup(string title, string message)
        {
            var configuration = new AlertPopupConfiguration
            {
                PopupType = PopupType.AlertPopup,
                Title = title,
                Description = message,
                ConfirmButtonText = _localization.StartOverPopupOkButton
            };

            _popupManager.SetupPopup(configuration);
            _popupManager.ShowPopup(configuration.PopupType);
        }

        private void ShowShoppingCart(Action onPurchaseRequested)
        {
            if (IsWardrobeChangeInProgress) return;
            _umaEditorPanel.ShowShoppingCart(onPurchaseRequested);
            _umaEditorPanel.LockItems();
        }

        private void OnSaveOutfitClicked()
        {
            if (_isSaveOutfitInProgress) return;
            
            if (_umaEditorPanel.HasNotOwnedWardrobes)
            {
                ShowShoppingCart(SaveOutfit);
            }
            else
            {
                SaveOutfit();
            }
        }

        private async void SaveOutfit()
        {
            _isSaveOutfitInProgress = true;
            _saveCharacterButton.interactable = false;
            await SaveOutfit(SaveOutfitMethod.Manual);
        }

        private async Task SaveOutfit(SaveOutfitMethod saveOutfitMethod, Action<OutfitFullInfo> onSuccess = null, Action<string> onFail = null)
        { 
            var wardrobeIds = _umaEditorPanel.GetCharacterRecipeAndWardrobes().Value.Select(x=>x.Id);
            var characterColors = _umaEditorPanel.GetCharacterColors();
            var colorList = new List<KeyValuePair<long, int>>();

            var snapshotTexture128 = await _umaEditorPanel.GetSnapshotAsync(BodyDisplayMode.Head);
            var snapshotTexture512 = await _umaEditorPanel.GetSnapshotAsync(BodyDisplayMode.FullBody);

            var snapshot256 = new KeyValuePair<Resolution, Texture2D>(Resolution._256x256, snapshotTexture512);
            var snapshot128 = new KeyValuePair<Resolution, Texture2D>(Resolution._128x128, snapshotTexture128);
            var snapshot512 = new KeyValuePair<Resolution, Texture2D>(Resolution._512x512, snapshotTexture512);

            var photos = new[] { snapshot128, snapshot256, snapshot512 };

            foreach (var color in characterColors)
            {
                if (color.Key == "Skin") continue;

                var sharedWithName = _clothesCabinet.UmaSharedColors.FirstOrDefault(x => x.Name == color.Key);
                if (sharedWithName == null)
                {
                    continue;
                }
                colorList.Add(new KeyValuePair<long, int>(sharedWithName.Id, color.Value.ConvertToInt()));
            }
            
            foreach (var photo in photos)
            {
                Destroy(photo.Value);
            }
            
            await _outfitsManager.SaveOutfit(wardrobeIds, colorList, saveOutfitMethod, photos, OnSuccess, OnFail);

            void OnSuccess(OutfitFullInfo outfitFullInfo)
            {
                _clothesCabinet.UpdatePurchasedWardrobes();
                _umaEditorPanel.MarkAppliedWardrobesAsBought();
                _isSaveOutfitInProgress = false;
                _saveCharacterButton.interactable = true;
                onSuccess?.Invoke(outfitFullInfo);
            }

            void OnFail(string message)
            {
                _isSaveOutfitInProgress = false;
                _saveCharacterButton.interactable = true;
                ShowFailAlertPopup("", message);
                onFail?.Invoke(message);
            }
        }

        private async void SaveCharacterInternal(KeyValuePair<Resolution, Texture2D>[] photos, Action onDone)
        {
            var isUpdate = ContextData.Character != null && ContextData.IsNewCharacter == false;
            var genderId = _umaEditorPanel.GenderId;
            var recipeAndWardrobes = _umaEditorPanel.GetCharacterRecipeAndWardrobes();
            
            CharacterFullInfo savedCharacter;
            
            if (isUpdate)
            {
                var fullCharacter = await _characterManager.GetCharacterAsync(ContextData.Character.Id);
                savedCharacter = await _characterManager.UpdateCharacter(fullCharacter, recipeAndWardrobes, genderId, photos);
            }
            else
            {
                if (!_characterManager.UserCharacters.IsNullOrEmpty()) _localUserData.SetUserGenderId(genderId);
                savedCharacter = await _characterManager.CreateCharacter(recipeAndWardrobes, photos, genderId);
            }

            var userProfile = _localUserData.UserProfile;
            // We need to update the user profile data only if the profile already exists
            if (userProfile is not null &&
                (userProfile.MainCharacter == null || userProfile.MainCharacter.Id == savedCharacter.Id))
            {
                _localUserData.SetMainCharacter(savedCharacter.ToCharacterInfo());
            }

            onDone?.Invoke();
            FinishedEditing(savedCharacter, null);
        }

        private void OnDeleteOutfitClicked(OutfitShortInfo outfit)
        {
            if (ContextData.OutfitsUsedInLevel != null && ContextData.OutfitsUsedInLevel.Contains(outfit.Id))
            {
                _popupManagerHelper.ShowAlertPopup(_localization.CannotDeleteCharacterUsedInRecordingSnackbarDesc);
                return;
            }
            
            _outfitsManager.DeleteOutfit(outfit);
        }

        private void OnOutfitSaved(IEntity outfit)
        {
            _umaEditorPanel.OnOutfitSaved();
        }

        private void OnOutfitDeleted(IEntity outfit)
        {
            _umaEditorPanel.OnOutfitDeleted();
        }

        private void OnWardrobeChanged()
        {
            UpdateDoneButtonText();
        }

        private void OnDnaPanelShown(bool active)
        {
            if (active)
            {
                return;
            }
            
            SetupButtonsAndOnboardingElements(false);
        }

        private void OnPurchasedItemsUpdated()
        {
            _wardrobesResponsesCache.Invalidate();
            UpdateDoneButtonText();
        }

        private void UpdateDoneButtonText()
        {
            switch(ContextData.ConfirmActionType)
            {
                case CharacterEditorConfirmActionType.SaveCharacter:
                    {
                        if (_umaEditorPanel.GetNotOwnedWardrobes().Count > 0)
                        {
                            _saveButtonText.text = _localization.SaveOutfitBuyButton;
                        }
                        else 
                        { 
                            _saveButtonText.text = _localization.SaveButton; 
                        }
                    }
                    break;
                case CharacterEditorConfirmActionType.SaveOutfitAsAutomatic:
                    {
                        _saveButtonText.text = _localization.SaveOutfitButton;
                    }
                    break;
                case CharacterEditorConfirmActionType.Onboarding:
                    {
                        if (_umaEditorPanel.CheckSufficientFunds())
                        {
                            if (_umaEditorPanel.GetNotOwnedWardrobes().Count > 0)
                            {
                                _saveButtonText.text = _localization.SaveOutfitBuyButton;
                            }
                            else 
                            { 
                                _saveButtonText.text = _localization.SaveButton; 
                            }
                            
                            _saveButtonImg.color = _saveButtonColorActive;
                            _saveCharacterButton.interactable = true;
                            _umaEditorPanel.BagIcon.SetActive(false);
                        }
                        else
                        {
                            _saveButtonText.text = _localization.SaveOutfitNotEnoughFundsButton;
                            _saveButtonImg.color = _saveButtonColorInactive;
                            _saveCharacterButton.interactable = true;
                            _umaEditorPanel.BagIcon.SetActive(false);
                        }
                    }
                    break;
            }

            _umaEditorPanel.UpdateShoppingCartIcons();
        }

        private async Task<KeyValuePair<Resolution, Texture2D>[]> GetCharacterPhotosAsync(params BodyDisplayMode[] photoTypes)
        {
            if (photoTypes == null || photoTypes.Length == 0)
                return Array.Empty<KeyValuePair<Resolution, Texture2D>>();

            var output = new Dictionary<Resolution, Texture2D>();
            
            foreach (var photoType in photoTypes)
            {
                var photo = await _umaEditorPanel.GetSnapshotAsync(photoType);
                var resolution = _bodyDisplayModeResolutions[photoType];
                output[resolution] = photo;
            }

            return output.ToArray();
        }
        
        private void StartListenStore()
        {
            _localUserData.UserBalanceUpdated += OnBalanceUpdated;
            _userBalanceView.Initialize(new StaticUserBalanceModel(_localUserData));
        }
        
        private void StopListenStore()
        {
            _localUserData.UserBalanceUpdated -= OnBalanceUpdated;
        }

        private void LoadBackgroundScene()
        {
            SceneManager.LoadSceneAsync(_backgroundSceneName, LoadSceneMode.Additive);
        }

        private void UnloadBackgroundScene()
        {
            if (UnityEngine.SceneManagement.SceneManager.GetSceneByName(_backgroundSceneName).isLoaded)
            {
                UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(_backgroundSceneName);
            }
        }

        //---------------------------------------------------------------------
        // Nested
        //---------------------------------------------------------------------

        private enum OnboardingState
        {
            None = 0, Body = 1, Makeup = 2, Clothing = 3
        }
    }
}