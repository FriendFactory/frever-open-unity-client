using System;
using System.Collections.Generic;
using System.Threading;
using Bridge;
using Bridge.Models.ClientServer.StartPack.Metadata;
using Bridge.Models.ClientServer.Tasks;
using Bridge.Services.UserProfile;
using JetBrains.Annotations;
using Common;
using Modules.Amplitude;
using Modules.AssetsStoraging.Core;
using Modules.UserScenarios.Common;
using UIManaging.Localization;
using UIManaging.PopupSystem.Configurations;
using UIManaging.PopupSystem.Popups;
using UIManaging.PopupSystem.Popups.LevelCreation;
using UnityEngine;
using Utils;
using Zenject;

namespace UIManaging.PopupSystem
{
    [UsedImplicitly]
    public sealed class PopupManagerHelper
    {
        [Inject] private PopupManager _popupManager;
        [Inject] private IBridge _bridge;
        [Inject] private LevelEditorPopupLocalization _levelEditorPopupLocalization;
        [Inject] private LoadingOverlayLocalization _loadingOverlayLocalization;
        [Inject] private IMetadataProvider _metadataProvider;
        [Inject] private IScenarioManager _scenarioManager;

        public bool IsLoadingOverlayShowing => _popupManager.IsPopupOpen(PopupType.SimulatedPageLoading);

        [Inject] private ProfileLocalization _profileLocalization;
        [Inject] private CrewPageLocalization _crewPageLocalization;
        

        [Inject] private ChatLocalization _chatLocalization;
        
        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void OpenLeavePostRecordEditorPopup(Action onErase, Action onSaveDraft)
        {
            var variants = new List<KeyValuePair<string, Action>>()
            {
                new KeyValuePair<string, Action>(_levelEditorPopupLocalization.ExitPopupEraseChangesAndGoBackOption, onErase),
                new KeyValuePair<string, Action>(_levelEditorPopupLocalization.ExitPopupExitSaveDraftOption, onSaveDraft)
            };

            var popup = new ActionSheetPopupConfiguration
            {
                PopupType = PopupType.ActionSheet,
                Variants = variants
            };

            OpenPopup(popup);
        }
        
        public void OpenStartOverLevelEditorPopup(Action onExit, Action onStartOver, Action onSaveDraft, Action onCancel)
        {
            var variants = new List<KeyValuePair<string, Action>>();

            if (onExit != null)
            {
                var action = _levelEditorPopupLocalization.ExitPopupExitOption;
                variants.Add(new KeyValuePair<string, Action>(action, onExit));
            }

            if (onStartOver != null)
            {
                var action = _levelEditorPopupLocalization.ExitPopupDeleteStartOverOption;
                variants.Add(new KeyValuePair<string, Action>(action, onStartOver));
            }

            if (onSaveDraft != null)
            {
                var action = _levelEditorPopupLocalization.ExitPopupExitSaveDraftOption;
                variants.Add(new KeyValuePair<string, Action>(action, onSaveDraft));
            }

            var popup = new ActionSheetPopupConfiguration
            {
                PopupType = PopupType.ActionSheet,
                Variants = variants,
                MainVariantIndexes = new []{0,1},
                OnCancel = onCancel
            };

            OpenPopup(popup, true);
        }

        public void OpenDiscardAllRecordingEventsPopup(Action onDiscard, Action onCancel)
        {
            var configuration = new DialogPopupConfiguration
            {
                PopupType = PopupType.DiscardRecordingsPopup,
                Description = _levelEditorPopupLocalization.DiscardRecordingsPopupDescription,
                YesButtonText = _levelEditorPopupLocalization.DiscardRecordingsPopupDiscardButton,
                NoButtonText = _levelEditorPopupLocalization.DiscardRecordingsPopupCancelButton,
                OnYes = onDiscard,
                OnNo = onCancel,
                OnClose = x=> onCancel?.Invoke(),
            };
                
            OpenPopup(configuration);
        }

        public void OpenCreatingAccountLoadingPopup()
        {
            var questionPopupConfiguration = new InformationPopupConfiguration()
            {
                PopupType = PopupType.Loading,
                Title = "Creating account"
            };
            OpenPopup(questionPopupConfiguration);
        }

        public void OpenMainCharacterIsNotSelectedPopup()
        {
            var questionPopupConfiguration = new QuestionPopupConfiguration
            {
                PopupType = PopupType.Question,
                Title = "Whoops",
                Description = "It looks like you haven't selected a character. \n Select or create a new character in your character page.",
                Answers = new List<KeyValuePair<string, Action>> {new KeyValuePair<string, Action>("OK", null)}
            };

            OpenPopup(questionPopupConfiguration);
        }

        public void OpenVideoStillRenderingPopup()
        {
            var videoUploadedPopup = new InformationMessageConfiguration
            {
                PopupType = PopupType.Success,
                Title = "Render status",
                Description = "Video is still rendering."
            };
            
            OpenPopup(videoUploadedPopup);
        }

        public void ShowSuccessNotificationPopup(string message)
        {
            var config = new InformationMessageConfiguration
            {
                PopupType = PopupType.UserBlockingSucceed,
                Description = message
            };
            OpenPopup(config);
        }

        public void OpenDiskSpacePopup()
        {
            var minRequiredSpaceGb = Constants.Memory.MIN_DISK_SPACE_REQUIRED_DISPLAY_MB / 1024;
            var freeSpacePopup = new AlertPopupConfiguration
            {
                PopupType = PopupType.FilesInconsistency,
                Description = $"To use Frever, you need at least {minRequiredSpaceGb} GB of free space on your phone. Please try to delete a few Apps or large files on your device",
                Title = "Not enough space",
                ConfirmButtonText = "I Understand"
            };

            OpenPopup(freeSpacePopup);
        }

        public void OpenFileReachMaxSizeExceptionPopup(float maxAllowedSizeMb, float currentSizeMb, Action<object> onClose)
        {
            var configs = new InformationPopupConfiguration
            {
                PopupType = PopupType.FileSizeReachedLimitError,
                Title = "File is too big...",
                Description =
                    $"{FormatSize(maxAllowedSizeMb)} is the limit.\n" +
                    $"Your file is {FormatSize(currentSizeMb)}, try making it smaller by shortening it or choose another video",
                OnClose = onClose
            };
            
            OpenPopup(configs);
        }

        public void OpenFileReachMinSizeExceptionPopup(float minAllowedSizeMb, float currentSizeMb, Action<object> onClose)
        {
            var configs = new InformationPopupConfiguration
            {
                PopupType = PopupType.FileSizeReachedLimitError,
                Title = "File is too small...",
                Description =
                    $"{FormatSize(minAllowedSizeMb)} is the limit.\n" +
                    $"Your file is {FormatSize(currentSizeMb)}, try making it bigger by lengthening it or choose another video",
                OnClose = onClose
            };
            
            OpenPopup(configs);
        }

        public void OpenCaptionRemovingPopup(Action onRemove)
        {
            var variants = new List<KeyValuePair<string, Action>>()
            {
                new KeyValuePair<string, Action>("Delete text and exit", onRemove),
            };

            var configs = new ActionSheetPopupConfiguration
            {
                PopupType = PopupType.ActionSheet,
                Description = "All the changes will be discarded.\nAre you sure to delete and exit?",
                Variants = variants,
            };

            OpenPopup(configs);
        }

        public void ShowVariantsPopup(List<KeyValuePair<string, Action>> variants)
        {
            var configs = new ActionSheetPopupConfiguration
            {
                PopupType = PopupType.ActionSheet,
                Variants = variants
            };
            OpenPopup(configs);
        }
        
        public void ShowAlertPopup(string message, string title = "", string buttonText = "Ok", Action onConfirm = null)
        {
            var config = new AlertPopupConfiguration
            {
                PopupType = PopupType.AlertPopup,
                Title = title,
                Description = message,
                ConfirmButtonText = buttonText,
                OnConfirm = onConfirm
            };

            OpenPopup(config);
        }
        
        public void ShowConfirmBlockingPopup(Profile profile, Action onBlockConfirmed)
        {
            var variants = new List<KeyValuePair<string, Action>>
            {
                new KeyValuePair<string, Action>(_profileLocalization.BlockUserButton, onBlockConfirmed)
            };
            
            var configuration = new ActionSheetUserConfiguration
            {
                PopupType = PopupType.ActionSheet,
                Variants = variants,
                Profile = profile
            };
            
            OpenPopup(configuration);
        }

        public void ShowUserProfileRestrictedPopup()
        {
            var config = new UserPrivacySettingsRestrictedAccessPopupConfiguration(ClosePopup);
            OpenPopup(config);
            
            void ClosePopup()
            {
                _popupManager.ClosePopupByType(PopupType.ProfileAccessRestricted);
            }
        }

        public void ShowInformationMessage(string message)
        {
            var config = new InformationMessageConfiguration
            {
                PopupType = PopupType.InformationMessage,
                Description = message
            };
            
            OpenPopup(config);
        }
        
        public void ShowNotEnoughFundsPopup(bool showOnboardingText, Action onClose = null)
        {
            var config = new NotEnoughFundsPopupConfiguration
            {
                PopupType = PopupType.NotEnoughFundsPopup,
                IsOnboarding = showOnboardingText,
                OnClose = _ => onClose?.Invoke()
            };
            
            OpenPopup(config);
        }
        
        public void ShowNotEnoughFundsPopupWithBuyOption(Action onBuyClicked, Action onCancelClicked = null)
        {
            var config = new NotEnoughFundsWithBuyOptionPopupConfiguration()
            {
                PopupType = PopupType.NotEnoughFundsWithBuyOptionPopup,
                OnBuyClicked = onBuyClicked,
                OnCancelClicked = onCancelClicked,
            };
            
            OpenPopup(config);
        }

        public void HideShowNotEnoughFundsPopupWithBuyOption()
        {
            _popupManager.ClosePopupByType(PopupType.NotEnoughFundsWithBuyOptionPopup);
        }
        
        public void ShowConfirmPopup(Action onConfirm, int? softCurrencyAmount, string hardCurrencyCost)
        {
            var config = new ConfirmCoinPurchasePopupConfiguration
            {
                PopupType = PopupType.ConfirmCoinPurchase,
                OnConfirm = onConfirm,
                SoftCurrencyAmount = softCurrencyAmount?.ToString(),
                HardCurrencyCost = hardCurrencyCost
            };

            OpenPopup(config);
        }
        
        public void ShowStorePopup(bool showRewardsButton, Action onClosed = null)
        {
            var configuration = new StorePopupConfiguration
            {
                PopupType = PopupType.StorePopup,
                OnClosed = onClosed,
                ShowSeasonRewardsButtonOnPremiumPassPurchase = showRewardsButton
            };
                
            OpenPopup(configuration);
        }
        
        public void HideStorePopup()
        {
            _popupManager.ClosePopupByType(PopupType.StorePopup);
        }
        
        public void ShowPremiumPassPopup(Action onSuccessfulPassPurchase = null, Action<object> onOverlayClosed = null)
        {
            var config = new PremiumPassPopupConfiguration
            {
                PopupType = PopupType.PremiumPass,
                OnSuccessfulPassPurchase = onSuccessfulPassPurchase,
                OnClose = onOverlayClosed
            };
            
            OpenPopup(config);
        }

        public void HidePremiumPassPopup()
        {
            _popupManager.ClosePopupByType(PopupType.PremiumPass);
        }

        public void ShowPremiumPassPurchaseSucceedPopup(bool showSeasonRewardsButton, Action onSeasonRewardsClicked, Action onExitClicked)
        {
            var config = new PremiumPassPurchasedPopupConfiguration()
            {
                PopupType = PopupType.PremiumPassPurchaseSuccess,
                OnSeasonRewardsButtonClicked = onSeasonRewardsClicked,
                OnExitClicked = onExitClicked,
                ShowSeasonRewardsButton = showSeasonRewardsButton
            };

            OpenPopup(config);
        }

        public void HidePremiumPassPurchaseSucceedPopup()
        {
            _popupManager.ClosePopupByType(PopupType.PremiumPassPurchaseSuccess);
        }

        public void ShowLockedLevelCreationFeaturePopup(int challengesCountToUnlock)
        {
            var config = new CreationNewLevelLockedPopupConfig(challengesCountToUnlock);
            OpenPopup(config);
        }
        
        public void ShowUnLockedLevelCreationFeaturePopup(Action onExploreLevelEditorClicked)
        {
            var config = new CreationNewLevelUnLockedPopupConfig(onExploreLevelEditorClicked);
            OpenPopup(config);
        }

        public void ShowIPSelectionPopup(Action<Universe> onUse)
        {
            var config = new IPSelectionPopupConfiguration(_metadataProvider.MetadataStartPack.Universes,
                onUse);
            OpenPopup(config);
        }

        public void ShowSeasonEndedPopup(Action onButtonClick)
        {
            var config = new SeasonEndedPopupConfiguration()
            {
                OnButtonClick = onButtonClick,
                ButtonText = "Create New Character"
            };
            OpenPopup(config);
        }

        public void ShowTaskInfoPopup(TaskFullInfo taskFullInfo, Action onHide = null)
        {
            ShowTaskInfoPopup(taskFullInfo.Name, taskFullInfo.Description, onHide);
        }
        
        public void ShowTaskInfoPopup(string title, string description, Action onHide)
        {
            var config = new TaskInfoConfiguration
            {
                Title = title,
                Description = description,
                OnClose = x => onHide?.Invoke()
            };
            OpenPopup(config);
        }

        public void ShowVotingResultCongratsPopup(int place, bool playParticles, int softRewards, Action onClose)
        {
            var config = new VotingResultCongratsPopupConfiguration
            {
                Place = place,
                PlayCongratsParticles = playParticles,
                SoftCurrencyReward = softRewards,
                OnClose = x => onClose?.Invoke()
            };
            OpenPopup(config);
        }

        public async void ShowFollowAccountPopup(long groupId, CancellationToken token = default)
        {
            var profileResp = await _bridge.GetProfile(groupId, token);

            if (profileResp.IsError)
            {
                Debug.LogError($"Failed to load profile for user: {groupId}");
                return;
            }

            if (profileResp.IsSuccess)
            {
                var config = new FollowAccountConfiguration
                {
                    Profile = profileResp.Profile
                };
            
                OpenPopup(config);
            }
        }
        
        public void ShowCopyrightFailedPopup()
        {
            var config = new CopyrightFailedPopupConfiguration();
            OpenPopup(config);
        }

        public void ShowVerticalDialogPopup(string title, string description, string noText, Action onNo,
            string yesText, Action onYes, bool yesRedText)
        {
            var cfg = new DialogDarkPopupConfiguration()
            {
                PopupType = PopupType.DialogDarkV3Vertical,
                Title = title,
                Description = description,
                NoButtonText = noText,
                OnNo = onNo,
                YesButtonText = yesText,
                YesButtonSetTextColorRed = yesRedText,
                OnYes = onYes,
            };
            
            OpenPopup(cfg, true);
        }

        public void ShowEraseChangesPopup(Action onYes, Action onNo)
        {
            ShowVerticalDialogPopup(
                _crewPageLocalization.EraseSettingChangesPopupTitle,
                _crewPageLocalization.EraseSettingChangesPopupDescription,
                _crewPageLocalization.EraseSettingChangesPopupCancelButton,
                onNo,
                _crewPageLocalization.EraseSettingChangesPopupConfirmButton,
                onYes,
                true);
        }

        public void ShowDialogPopup(string title, string description, string noText, Action onNo,
            string yesText, Action onYes, bool yesRedText)
        {
            var cfg = new DialogDarkPopupConfiguration()
            {
                PopupType = PopupType.DialogDarkV3,
                Title = title,
                Description = description,
                NoButtonText = noText,
                OnNo = onNo,
                YesButtonText = yesText,
                YesButtonSetTextColorRed = yesRedText,
                OnYes = onYes,
            };
            
            OpenPopup(cfg, true);
        }
        
        public void ShowReversedDialogPopup(string title, string description, string noText, Action onNo,
            string yesText, Action onYes, bool yesRedText)
        {
            var cfg = new DialogDarkPopupConfiguration()
            {
                PopupType = PopupType.DialogDarkV3Reversed,
                Title = title,
                Description = description,
                NoButtonText = noText,
                OnNo = onNo,
                YesButtonText = yesText,
                YesButtonSetTextColorRed = yesRedText,
                OnYes = onYes,
            };
            
            OpenPopup(cfg, true);
        }

        public void ShowChatUserMessageActions(Action onReportClicked)
        {
            var cfg = new ActionSheetPopupConfiguration
            {
                PopupType = PopupType.ActionSheetDark,
                Variants = new List<KeyValuePair<string, Action>>
                {
                    new KeyValuePair<string, Action>(_chatLocalization.ReportMessageButton, onReportClicked),
                },
                OnCancel = () => _popupManager.ClosePopupByType(PopupType.ActionSheetDark)
            };
            
            _popupManager.SetupPopup(cfg);
            _popupManager.ShowPopup(cfg.PopupType);
        }

        public void ShowChatOwnMessageActions(Action onDeletionRequested)
        {
            var cfg = new ActionSheetPopupConfiguration
            {
                PopupType = PopupType.ActionSheetDark,
                Variants = new List<KeyValuePair<string, Action>>
                {
                    new KeyValuePair<string, Action>(_chatLocalization.DeleteMessageButton, onDeletionRequested),
                },
                OnCancel = () => _popupManager.ClosePopupByType(PopupType.ActionSheetDark)
            };

            _popupManager.SetupPopup(cfg);
            _popupManager.ShowPopup(cfg.PopupType);
        }
        
        public void ShowRateAppPopup()
        {
            if (!AmplitudeManager.GetInAppReviewEnabled()) return;
            _popupManager.PushPopupToQueue(new RateAppPopupConfiguration());
        }
        
        public void OpenShuffleEventPopup(Action onShuffle, Action onCancel)
        {
            var variants = new List<KeyValuePair<string, Action>>()
            {
                new KeyValuePair<string, Action>("Shuffle clip", onShuffle),
                new KeyValuePair<string, Action>("Cancel", onCancel)
            };

            var popup = new ActionSheetShufflePopupConfiguration
            {
                PopupType = PopupType.ActionSheetShuffle,
                Variants = variants,
                MainVariantIndexes = Array.Empty<int>(),
                OnCancel = onCancel,
                ShuffleButtonIndex = 0,
                ShuffleButtonDescription = "Shuffle the scene and animation"
            };

            OpenPopup(popup);
        }

        public void ShowWaitVideoPublishOverlay()
        {
            var configs = new WaitVideoSharingPopupConfiguration();
            _popupManager.SetupPopup(configs);
            _popupManager.ShowPopup(configs.PopupType);
        }

        public void HideWaitVideoPublishOverlay()
        {
            _popupManager.ClosePopupByType(PopupType.WaitVideoSharingPopup);
        }

        public void ShowStashEditorChangesPopup(Action onConfirm, Action onCancel)
        {
            var config = new StashEditorChangesPopupConfigs
            {
                Title = "Discard changes?",
                Description = "If you change editor, you will lose all your progress. This cannot be undone.",
                YesButtonText = "Leave",
                NoButtonText = "Cancel",
                YesButtonSetTextColorRed = true,
                OnYes = onConfirm,
                OnNo = onCancel
            };
            _popupManager.SetupPopup(config);
            _popupManager.ShowPopup(config.PopupType);
        }

        public void HideStashLevelEditorChangesPopup()
        {
            _popupManager.ClosePopupByType(PopupType.StashChangesBeforeExitEditorPopup);
        }

        public void ShowLoadingOverlay(string message, Action<object> onClose = null)
        {
            var config = new SimulatedPageLoadingPopupConfiguration(
                message,
                _loadingOverlayLocalization.LoadingProgressMessage,
                onClose);
            _popupManager.SetupPopup(config);
            _popupManager.ShowPopup(config.PopupType);
        }

        public void HideLoadingOverlay()
        {
            _popupManager.ClosePopupByType(PopupType.SimulatedPageLoading);
        }

        public void HideSeasonEndedPopup()
        {
            _popupManager.ClosePopupByType(PopupType.SeasonEndedPopup);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private static string FormatSize(float currentSizeMb)
        {
            return SizeUtil.FormatSize(currentSizeMb);
        }
        
        private void OpenPopup(PopupConfiguration config, bool onTop = false)
        {
            _popupManager.SetupPopup(config);
            _popupManager.ShowPopup(config.PopupType, onTop);
        }
    }
}