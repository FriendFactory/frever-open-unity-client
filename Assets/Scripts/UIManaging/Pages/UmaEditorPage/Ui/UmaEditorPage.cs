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
using Common;
using Resolution = Bridge.Models.Common.Files.Resolution;
using Modules.CharacterManagement;
using Modules.PhotoBooth.Character;
using Navigation.Args;
using Navigation.Core;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.Common;
using Bridge.Models.ClientServer.EditorsSetting;
using Common.UserBalance;
using TMPro;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.Pages.Tasks;
using UIManaging.Pages.Tasks.TaskRewardFlow;
using UIManaging.Pages.UmaEditorPage.Ui.WardrobePanel;
using UnityEngine.SceneManagement;

namespace UIManaging.Pages.UmaEditorPage.Ui
{
    internal sealed class UmaEditorPage : GenericPage<UmaEditorArgs>
    {
        private const string WARDROBE_TYPE_NAME = "Dress up";
        
        [SerializeField] private UmaEditorPanel _umaEditorPanel;
        [SerializeField] private GameObject _goBackButtonContainer;
        [SerializeField] private Button _goBackButton;
        [SerializeField] private Button _saveCharacterButton;
        [SerializeField] private Button _storeButton;
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

        private CharacterEditorSettings _editorSettings;
        private CancellationTokenSource _tokenSource;
        private bool _isSavePopupShown;
        private bool _clickedWardrobeButton;
        private bool _isSaveInProgress;
        private TextMeshProUGUI _saveButtonText;
        private Image _saveButtonImg;
        private bool _isSaveOutfitInProgress;

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

        public override PageId Id => PageId.UmaEditor;
        private bool HasStartGift => OpenPageArgs.ConfirmActionType == CharacterEditorConfirmActionType.Onboarding && OpenPageArgs.IsNewCharacter;
        private bool IsWardrobeChangeInProgress => _umaEditorPanel.IsWardrobeChangeInProgress;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            _outfitsManager.OutfitAdded += OnOutfitSaved;
            _outfitsManager.OutfitDeleted += OnOutfitDeleted;
            _saveButtonText = _saveCharacterButton.GetComponentInChildren<TextMeshProUGUI>();
            _saveButtonImg = _saveCharacterButton.GetComponent<Image>();
            _clothesCabinet.PurchasedItemsUpdated += OnPurchasedItemsUpdated;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _outfitsManager.OutfitAdded -= OnOutfitSaved;
            _outfitsManager.OutfitDeleted -= OnOutfitDeleted;
            _clothesCabinet.PurchasedItemsUpdated -= OnPurchasedItemsUpdated;
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnInit(PageManager manager)
        {
            _goBackButton.onClick.AddListener(ShowExitPopup);
            _saveCharacterButton.onClick.RemoveAllListeners();
            _saveCharacterButton.onClick.AddListener(OnSaveClicked);
            _isSavePopupShown = false;
        }
        
        protected override async void OnDisplayStart(UmaEditorArgs args)
        {
            base.OnDisplayStart(args);
            
            _tokenSource = new CancellationTokenSource();
            _clothesCabinet.UpdatePurchasedWardrobes();
            _wardrobesResponsesCache.Invalidate();
            _wardrobeArrow.gameObject.SetActive(false);
            _storeButton.interactable = args.EnableStoreButton;
            
            _editorSettings = args.CharacterEditorSettings;
            ApplyEditorSettings();

            
            _umaEditorPanel.Setup(_editorSettings, args.TaskId, args.CategoryTypeId, 
                                  args.ThemeCollectionId != null ? (long?)Constants.Wardrobes.COLLECTIONS_CATEGORY_ID : args.CategoryId, 
                                  args.ThemeCollectionId ?? args.SubCategoryId, args.DefaultFilteringSetting, 
                                  args.ConfirmActionType == CharacterEditorConfirmActionType.Onboarding);
            _umaEditorPanel.SaveOutfitClicked += OnSaveOutfitClicked;
            _umaEditorPanel.DeleteOutfitClicked += OnDeleteOutfitClicked;
            _umaEditorPanel.WardrobeTypeChanged += OnWardrobeTypeChanged;
            _umaEditorPanel.WardrobeChanged += OnWardrobeChanged;

            if (args.ShowTaskInfo)
            {
                _popupManagerHelper.ShowTaskInfoPopup(args.TaskFullInfo, null);
            }
            
            _goBackButtonContainer.SetActive(args.BackButtonAction != null);
            LoadBackgroundScene();
            await ShowEditor();
            if (_tokenSource.IsCancellationRequested) return;
            _goBackButtonContainer.SetActive(args.BackButtonAction != null);
            OpenPageArgs.LoadCompleteAction?.Invoke();
        }

        protected override void OnHidingBegin(Action onComplete)
        {
            base.OnHidingBegin(onComplete);
            _umaEditorPanel.Clear();
            _assetManager.UnloadAll();
            _avatarHelper.UnloadAllUmaBundles();
            _tokenSource.Cancel(true);
            _clickedWardrobeButton = false;
            UnloadBackgroundScene();
            StopListenStore();
            _wardrobesResponsesCache.Invalidate();
        }

        private async void HandleStartGift()
        {
            _rewardFlowManager.Initialize(0, 0, 1, 0);
            if (_localUserData.UserBalance is null) await _localUserData.UpdateBalance();
            var userBalance = _localUserData.UserBalance;
            
            _greyOverlay.SetActive(true);
            _giftOverlay.Show(userBalance.SoftCurrencyAmount, userBalance.HardCurrencyAmount, () =>
            {
                PlayerPrefs.SetInt(Constants.Onboarding.RECEIVED_START_GIFT_IDENTIFIER, 1);
                _rewardFlowManager.StartAnimation(0, userBalance.SoftCurrencyAmount, userBalance.HardCurrencyAmount);
                _rewardFlowManager.FlowCompleted += OnGiftFlowCompleted;
            });
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
            OpenPageArgs.WelcomeGiftReceivedAction?.Invoke();
        }

        private async Task ShowEditor()
        {
            var characterInfo = OpenPageArgs.IsNewCharacter ? OpenPageArgs.Style : _characterManager.SelectedCharacter;

            CharacterFullInfo character;
            if (OpenPageArgs.Character != null)
            {
                character = OpenPageArgs.Character;
            }
            else
            {
                character = await _characterManager.GetCharacterAsync(characterInfo.Id, _tokenSource.Token);
            }
            
            if (_tokenSource.IsCancellationRequested) return;

            await _umaEditorPanel.Show(character, OpenPageArgs.Outfit, false, _tokenSource.Token);
            
            OpenPageArgs.HideLoadingPopup?.Invoke();
            
            if (_tokenSource.IsCancellationRequested) return;

            base.OnDisplayStart(OpenPageArgs);
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
            
            OpenPageArgs.BackButtonAction?.Invoke();
        }

        private void FinishedEditing(CharacterFullInfo savedCharacter, OutfitFullInfo outfit) 
        {
            var result = new CharacterEditorOutput()
            {
                Character = savedCharacter,
                Outfit = outfit
            };

            _isSaveInProgress = false;
            if (OpenPageArgs.ConfirmAction == null)
            {
                OpenPageArgs.ConfirmButtonAction?.Invoke();
            }
            else
            {
                _umaEditorPanel.SaveOutfitClicked -= OnSaveOutfitClicked;
                OpenPageArgs.ConfirmAction?.Invoke(result);
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

        private void SaveCharacter()
        {
            if (ShouldShowSavePopup())
            {
                ShowSavePopup();
            }
            else
            {
                FinishEditing();
            }
        }

        private async void FinishEditing()
        {
            _isSaveInProgress = true;
            _umaEditorPanel.LockItems();
            var selectedCharacter = OpenPageArgs.Character;
            var isUpdate = selectedCharacter != null && OpenPageArgs.IsNewCharacter == false;

            var photosToTake = SelectPhotoTypesToBeTaken(selectedCharacter);
            var photos = await GetCharacterPhotosAsync(photosToTake.ToArray());   
            
            var loadingPopupConfig = new ContentSavingPopupConfiguration
            {
                PopupType = PopupType.ContentSaving,
                ProgressMessage = isUpdate ? _localization.UpdatingCharacterLoadingTitle : _localization.SavingCharacterLoadingTitle,
            };

            _popupManager.SetupPopup(loadingPopupConfig);
            _popupManager.ShowPopup(loadingPopupConfig.PopupType);

            switch (OpenPageArgs.ConfirmActionType)
            {
                case CharacterEditorConfirmActionType.SaveCharacter:
                case CharacterEditorConfirmActionType.Onboarding:
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
            var isUpdate = OpenPageArgs.Character != null && OpenPageArgs.IsNewCharacter == false;
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

        private void ShowSavePopup()
        {
            _isSavePopupShown = true;
            _wardrobeArrow.gameObject.SetActive(true);
            
            var configuration = new DialogPopupConfiguration
            {
                PopupType = PopupType.Dialog,
                Description = _localization.SaveCharacterPopupDesc,
                YesButtonText = _localization.SaveCharacterPopupSaveButton,
                NoButtonText = _localization.SaveCharacterPopupCancelButton,
                OnYes = FinishEditing,
                OnNo = ()=> _wardrobeArrow.gameObject.SetActive(false),
                OnClose = (x)=> _wardrobeArrow.gameObject.SetActive(false)
            };
                
            _popupManager.SetupPopup(configuration);
            _popupManager.ShowPopup(configuration.PopupType);
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
            var colorList = _umaEditorPanel.GetCharacterColorsInt();

            var snapShot = await _umaEditorPanel.GetSnapshotAsync(BodyDisplayMode.FullBody);

            var snapshot256 = new KeyValuePair<Resolution, Texture2D>(Resolution._256x256, snapShot);
            var snapshot128 = new KeyValuePair<Resolution, Texture2D>(Resolution._128x128, snapShot);
            var snapshot512 = new KeyValuePair<Resolution, Texture2D>(Resolution._512x512, snapShot);

            var photos = new[] { snapshot128, snapshot256, snapshot512 };
            
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
            var isUpdate = OpenPageArgs.Character != null && OpenPageArgs.IsNewCharacter == false;
            var genderId = _umaEditorPanel.GenderId;
            var recipeAndWardrobes = _umaEditorPanel.GetCharacterRecipeAndWardrobes();
            
            CharacterFullInfo savedCharacter;
            
            if (isUpdate)
            {
                var fullCharacter = await _characterManager.GetCharacterAsync(OpenPageArgs.Character.Id);
                savedCharacter = await _characterManager.UpdateCharacter(fullCharacter, recipeAndWardrobes, genderId, photos);
            }
            else
            {
                if (!_characterManager.UserCharacters.IsNullOrEmpty()) _localUserData.SetUserGenderId(genderId);
                savedCharacter = await _characterManager.CreateCharacter(recipeAndWardrobes, photos, genderId);
            }
            
            onDone?.Invoke();
            FinishedEditing(savedCharacter, null);
        }

        private void OnDeleteOutfitClicked(OutfitShortInfo outfit)
        {
            if (OpenPageArgs.OutfitsUsedInLevel != null && OpenPageArgs.OutfitsUsedInLevel.Contains(outfit.Id))
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

        private void OnWardrobeTypeChanged(string typeName)
        {
            if (typeName == WARDROBE_TYPE_NAME)
            {
                _clickedWardrobeButton = true;
            }
        }

        private void OnWardrobeChanged()
        {
            UpdateDoneButtonText();
        }

        private void OnPurchasedItemsUpdated()
        {
            _wardrobesResponsesCache.Invalidate();
            UpdateDoneButtonText();
        }

        private void UpdateDoneButtonText()
        {
            switch(OpenPageArgs.ConfirmActionType)
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
                        }
                        else
                        {
                            _saveButtonText.text = _localization.SaveOutfitNotEnoughFundsButton;
                            _saveButtonImg.color = _saveButtonColorInactive;
                            _saveCharacterButton.interactable = true;
                        }
                    }
                    break;
            }

            _umaEditorPanel.UpdateShoppingCartIcons();
        }

        private bool ShouldShowSavePopup()
        {
            return !_localUserData.IsOnboardingCompleted && !_isSavePopupShown && !_clickedWardrobeButton;
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
            UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(_backgroundSceneName, LoadSceneMode.Additive);
        }

        private void UnloadBackgroundScene()
        { 
            if (UnityEngine.SceneManagement.SceneManager.GetSceneByName(_backgroundSceneName).isLoaded)
            {
                UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(_backgroundSceneName);
            }
        }
    }
}