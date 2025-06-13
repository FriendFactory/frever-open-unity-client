using System;
using System.Collections.Generic;
using Bridge;
using Common;
using Components;
using Extensions;
using I2.Loc;
using Modules.AssetsManaging.UncompressedBundles;
using Modules.CharacterManagement;
using Modules.Sound;
using Modules.VideoStreaming.UIAnimators;
using Navigation.Args;
using Navigation.Core;
using OneSignalHelpers;
using Settings;
using TMPro;
using UIManaging.Common.Args.Buttons;
using UIManaging.Common.NotificationBadge;
using UIManaging.Common.PageHeader;
using UIManaging.Pages.AppSettingsPage.UI.Args;
using UIManaging.Pages.Common.UserLoginManagement;
using UIManaging.Pages.Common.UsersManagement;
using UiManaging.Pages.OnBoardingPage.UI.Args;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UIManaging.PopupSystem.Popups;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Color = UnityEngine.Color;

namespace UIManaging.Pages.AppSettingsPage.UI
{
    internal sealed class AppSettingsPage : GenericPage<AppSettingsPageArgs>
    {
        private const string CACHE_SIZE_FORMAT_STRING = "{0:0.##}";

        [SerializeField] private Transform _otherControlsContainer;
        [SerializeField] private UnverifiedAccountBannerPanel _unverifiedAccountBannerPanel;
        [SerializeField] private ViewSpawner _viewSpawner;
        [SerializeField] private PageHeaderView _pageHeaderView;
        [SerializeField] private PageUiAnimator _pageUiAnimator;
        [SerializeField] private TextMeshProUGUI _environmentText;
        [SerializeField] private RuntimeBuildInfo _runtimeBuildInfo;
        [SerializeField] private Button _logOutButton;
        [SerializeField] private ScrollRect _scrollRect;

        [SerializeField] private Sprite _profileHeaderIcon;
        [SerializeField] private Sprite _accountHeaderIcon;
        [SerializeField] private Sprite _appHeaderIcon;
        [SerializeField] private Sprite _linkIcon;
        [Header("Localization")]
        [SerializeField] private AppSettingsPageLoc _loc;
        [SerializeField] private TMP_Dropdown _localizationTestOptionsDropdown;

        [Inject] private PopupManager _popupManager;
        [Inject] private PageManager _pageManager;
        [Inject] private IBridge _bridge;
        [Inject] private LocalUserDataHolder _localUserDataHolder;
        [Inject] private OneSignalManager _oneSignalManager;
        [Inject] private UserAccountManager _userAccountManager;
        [Inject] private UncompressedBundlesManager _uncompressedBundlesManager;
        [Inject] private CharacterManager _characterManager;
        [Inject] private SoundManager _soundManager;
        [Inject] private LoginMethodsNotificationBadgeDataProvider _loginMethodsNotificationBadgeDataProvider;

        private SettingItemViewArgs _clearCacheArgs;
        private SettingItemViewArgs _editProfileSetting;
        private ManageAccountPageArgs _manageAccountPageArgs;
        private bool _isOtherPageOpenedOnTop;
        private bool _isHiding;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public override PageId Id { get; } = PageId.AppSettings;

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnInit(PageManager pageManager)
        {
            _pageHeaderView.Init(new PageHeaderArgs(_loc.PageHeader, new ButtonArgs(string.Empty, _pageManager.MoveBack)));
            _manageAccountPageArgs = new ManageAccountPageArgs();
            
            SpawnSettingsItems();
        }

        protected override void OnDisplayStart(AppSettingsPageArgs args)
        {
            _scrollRect.normalizedPosition = Vector2.up;
            _pageUiAnimator.PrepareForDisplay();
            RefreshCacheSpaceText();
            _isHiding = false;

            if (!_isOtherPageOpenedOnTop)
            {
                _pageUiAnimator.PlayShowAnimation(() => base.OnDisplayStart(args));
            }
            else
            {
                base.OnDisplayStart(args);
                _isOtherPageOpenedOnTop = false;
            }

            InitLocalizationTestOptions();
            
            _unverifiedAccountBannerPanel.Initialize(_loginMethodsNotificationBadgeDataProvider.NotificationBadgeModel);
        }

        protected override void OnHidingBegin(Action onComplete)
        {
            _unverifiedAccountBannerPanel.CleanUp();
            
            _isHiding = true;
            if (_isOtherPageOpenedOnTop)
            {
                base.OnHidingBegin(onComplete);
            }
            else
            {
                _pageUiAnimator.PlayHideAnimation(() => OnHideAnimationFinished(onComplete));
            }
        }

        protected override void OnCleanUp()
        {
            _viewSpawner.CleanUp<SettingItemViewArgs, SettingItemView>();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void SpawnSettingsItems()
        {
            Color titleColor = Color.white;
            
            _clearCacheArgs =  new SettingItemViewArgs
            {
                ShowTopDivider = true,
                Title = _loc.ClearCacheTitle,
                Description = _loc.ClearCacheDesc,
                OnClicked = OnClearCacheClicked,
                TitleColor = titleColor
            };

            _editProfileSetting = new SettingItemViewArgs
            {
                Title = _loc.EditProfileTitle,
                ShowArrow = true,
                OnClicked = OnEditProfileClicked,
                TitleColor = titleColor,
            };

            var settingsItems = new List<SettingItemViewArgs>
            {
                new SettingItemViewArgs
                {
                    Icon = _profileHeaderIcon,
                    Header = _loc.ProfileHeader
                },

                _editProfileSetting,

                new SettingItemViewArgs
                {
                    ShowTopDivider = true
                },

                new SettingItemViewArgs
                {
                    Icon = _accountHeaderIcon,
                    Header = _loc.AccountHeader
                },

                new SettingItemViewArgs
                {
                    Title = _loc.ManageAccountTitle,
                    ShowArrow = true, 
                    OnClicked = OnManageAccountClicked,
                    TitleColor = titleColor,
                    NotificationBadgeModel = _loginMethodsNotificationBadgeDataProvider.NotificationBadgeModel,
                },

                new SettingItemViewArgs
                {
                    Title = _loc.PrivacyTitle,
                    ShowArrow = true, 
                    OnClicked = OnPrivacyClicked,
                    TitleColor = titleColor,
                    ShowTopDivider = true
                },

                new SettingItemViewArgs
                {
                    CustomArrowIcon = _linkIcon, 
                    Title = _loc.CommunitySupportTitle,
                    ShowArrow = true, 
                    OnClicked = OnCommunitySupportClicked,
                    TitleColor = titleColor,
                    ShowTopDivider = true
                },

                new SettingItemViewArgs
                {
                    CustomArrowIcon = _linkIcon,
                    Title = _loc.FaqTitle,
                    ShowArrow = true, 
                    OnClicked = OnFAQClicked,
                    TitleColor = titleColor,
                    ShowTopDivider = true
                },

                new SettingItemViewArgs
                {
                    CustomArrowIcon = _linkIcon,
                    Title = _loc.TermsOfUseTitle,
                    ShowArrow = true, 
                    OnClicked = OnTermsOfUseClicked,
                    TitleColor = titleColor,
                    ShowTopDivider = true
                },

                new SettingItemViewArgs
                {
                    CustomArrowIcon = _linkIcon,
                    Title = _loc.PrivacyPolicyTitle,
                    ShowArrow = true, 
                    OnClicked = OnPrivacyPolicyClicked,
                    TitleColor = titleColor,
                    ShowTopDivider = true
                },

                new SettingItemViewArgs
                {
                    CustomArrowIcon = _linkIcon,
                    Title = _loc.OpenSourceTitle,
                    ShowArrow = true, 
                    OnClicked = OnOpenSourceClicked,
                    TitleColor = titleColor,
                    ShowTopDivider = true
                },

                new SettingItemViewArgs
                {
                    CustomArrowIcon = _linkIcon,
                    Title = _loc.EpidemicSoundTitle,
                    Description = _loc.EpidemicSoundDesc,
                    ShowArrow = true,
                    OnClicked = OnEpidemicSubscriptionClicked,
                    TitleColor = titleColor,
                    ShowTopDivider = true
                },

                new SettingItemViewArgs
                {
                    ShowTopDivider = true
                },

                new SettingItemViewArgs
                {
                    Icon = _appHeaderIcon,
                    Header = _loc.AppHeader
                },
                
                // Disabled while waiting for new designs
                /*new SettingItemViewArgs
                {
                    Title = "Sound effects",
                    TitleColor = titleColor,
                    ShowToggle = true,
                    OnToggleValueChanged = b => _soundManager.MuteChannel(!b, MixerChannel.SpecialEffects),
                    IsToggleOn = !_soundManager.IsChannelMuted(MixerChannel.SpecialEffects)
                },
                
                new SettingItemViewArgs
                {
                    ShowTopDivider = true,
                    Title = "Button sounds",
                    TitleColor = titleColor,
                    ShowToggle = true,
                    OnToggleValueChanged = OnButtonSoundToggleValueChanged,
                    IsToggleOn = !_soundManager.IsChannelMuted(MixerChannel.Button)
                },*/

                _clearCacheArgs,
                
                new SettingItemViewArgs
                {
                    Title = _loc.HapticsEnabled,
                    ShowToggle = true,
                    IsToggleOn = AppSettings.HapticsEnabled,
                    OnToggleValueChanged = OnHapticsValueChanged,
                    TitleColor = titleColor,
                    ShowTopDivider = true,
                },
                
                new SettingItemViewArgs
                {
                    Title = _loc.AdvancedTitle,
                    ShowArrow = true, 
                    OnClicked = OnAdvancedClicked,
                    TitleColor = titleColor,
                    ShowTopDivider = true,
                    ShowBottomDivider = true
                },

                #if TV_BUILD
                    new SettingItemViewArgs {Title = "Feed auto scroll", ShowToggle = true, IsToggleOn = AppSettings.FeedAutoScroll, OnToggleValueChanged = OnFeedAutoScrollChanged, TitleColor = titleColor},
                #endif
            };

            if (_bridge.Profile.IsEmployee || _bridge.Profile.IsQA)
            {
                settingsItems.Insert(14,
                                     new SettingItemViewArgs
                                     {
                                         Title = "Rate app",
                                         ShowArrow = true,
                                         OnClicked = () => {_popupManager.PushPopupToQueue(new RateAppPopupConfiguration());},
                                         TitleColor = titleColor,
                                         ShowTopDivider = true
                                     });
            }

            _viewSpawner.Spawn<SettingItemViewArgs, SettingItemView>(settingsItems);
            _logOutButton.onClick.AddListener(OnLogoutClicked);

            MoveOtherControlsToList();
        }

        private void MoveOtherControlsToList()
        {
            var otherControlsCount = _otherControlsContainer.childCount;

            for (int i = 0; i < otherControlsCount; i++)
            {
                var child = _otherControlsContainer.GetChild(0);
                child.parent = _viewSpawner.transform;
            }
        }

        #if TV_BUILD
        private void OnFeedAutoScrollChanged(bool value)
        {
            AppSettings.FeedAutoScroll = value;
        }
        #endif
        

        private async void RefreshCacheSpaceText()
        {
            var bridgeCacheKb = await _bridge.GetCacheSizeKb();
            var unpackedBundlesCacheSizeKb = (int)(_uncompressedBundlesManager.GetStorageUseMbs() * 1024);
            var cacheSizeInBytes = (bridgeCacheKb + unpackedBundlesCacheSizeKb) * 1024;
            var space = GetSizeOfCachedDataString(cacheSizeInBytes);
            _clearCacheArgs.SetDetailText(space);
        }

        private string GetSizeOfCachedDataString(long cacheSizeInBytes)
        {
            var resultString = string.Format($"{CACHE_SIZE_FORMAT_STRING} bytes", cacheSizeInBytes);

            if (cacheSizeInBytes > 1024)
            {
                var cacheSizeInKilobytes = cacheSizeInBytes / 1024f;
                resultString = string.Format($"{CACHE_SIZE_FORMAT_STRING} KB", cacheSizeInKilobytes);

                if (cacheSizeInKilobytes > 1024)
                {
                    var cacheSizeInMegabytes = cacheSizeInKilobytes / 1024f;
                    resultString = string.Format($"{CACHE_SIZE_FORMAT_STRING} MB", cacheSizeInMegabytes);

                    if(cacheSizeInMegabytes > 1024)
                    {
                        var cacheSizeInGigabytes = cacheSizeInMegabytes / 1024f;
                        resultString = string.Format($"{CACHE_SIZE_FORMAT_STRING} GB", cacheSizeInGigabytes);
                    }
                }
            }

            return $"<nobr>{resultString}</nobr>";
        }

        private void OnEditProfileClicked()
        {
            if (_isHiding) return;
            
            _isOtherPageOpenedOnTop = true;
            
            var transitionArgs = new PageTransitionArgs
            {
                SaveCurrentPageToHistory = true,
                HidePreviousPageOnOpen = true
            };
            _pageManager.MoveNext(PageId.EditProfile, new EditProfilePageArgs(_localUserDataHolder.UserProfile), transitionArgs);
        }

        private void OnManageAccountClicked()
        {
            _isOtherPageOpenedOnTop = true;
            
            var transitionArgs = new PageTransitionArgs
            {
                SaveCurrentPageToHistory = true,
                HidePreviousPageOnOpen = true
            };
            
            _pageManager.MoveNext(PageId.ManageAccountPage, _manageAccountPageArgs, transitionArgs);
        }

        private void OnPrivacyClicked()
        {
            _isOtherPageOpenedOnTop = true;
            var pageArgs = new PrivacySettingsPageArgs();
            var transitionArgs = new PageTransitionArgs
            {
                SaveCurrentPageToHistory = true,
                HidePreviousPageOnOpen = true
            };
            _pageManager.MoveNext(PageId.PrivacySettings, pageArgs, transitionArgs);
        }

        private void OnAdvancedClicked()
        {
            _isOtherPageOpenedOnTop = true;
            var pageArgs = new AdvancedSettingsPageArgs();
            var transitionArgs = new PageTransitionArgs
            {
                SaveCurrentPageToHistory = true,
                HidePreviousPageOnOpen = true
            };
            _pageManager.MoveNext(PageId.AdvancedSettings, pageArgs, transitionArgs);
        }

        private void OnFAQClicked()
        {
            Application.OpenURL(Constants.FAQ_LINK);
        }
        
        private void OnCommunitySupportClicked()
        {
            Application.OpenURL(Constants.DISCORD_LINK);
        }
        
        private void OnTermsOfUseClicked()
        {
            Application.OpenURL(Constants.TERMS_OF_USE_LINK);
        }
        
        private void OnPrivacyPolicyClicked()
        {
            Application.OpenURL(Constants.PRIVACY_POLICY_LINK);
        }
        
        private void OnOpenSourceClicked()
        {
            Application.OpenURL(Constants.OPEN_SOURCE);
        }

        private void OnEpidemicSubscriptionClicked()
        {
            Application.OpenURL(Constants.EPIDEMIC_SUBSCRIPTION_URL);
        }

        private void OnClearCacheClicked()
        {
            CleanCacheAsync();
        }

        private async void CleanCacheAsync()
        {
            await _bridge.ClearCacheWithoutKeyFileStorage();
            _bridge.DeleteTempFiles();
            _uncompressedBundlesManager.CleanCache();

            var successPopup = new InformationMessageConfiguration
            {
                PopupType = PopupType.Success,
                Title = "Cache cleared!",
                Description = "Cache cleared!"
            };
            
            _popupManager.SetupPopup(successPopup);
            _popupManager.ShowPopup(successPopup.PopupType);
            RefreshCacheSpaceText();
        }

        private void OnEnable()
        {
            var dotsColor = new Color32(0x2D, 0x30, 0x36, 0xFF);
            var width = 950f;
            var environmentString = BuildSeparatedString(_environmentText, width, dotsColor, 
                                                         "Environment: ",
                                                         $" {_bridge.Environment}");
            var versionString = BuildSeparatedString(_environmentText, width, dotsColor, 
                                                         "Version: ",
                                                         $" {Application.version}");
            var buildNumberString = BuildSeparatedString(_environmentText, width, dotsColor, 
                                                     "Build number: ",
                                                     $" {_runtimeBuildInfo.BuildNumber}");
            var text = $"{environmentString}\n{versionString}\n{buildNumberString}";
            _environmentText.text = text;
        }

        private string BuildSeparatedString(TextMeshProUGUI labelContext, float width, Color dotsColor,
            string startText, string endText)
        {
            var dotWidth = labelContext.GetPreferredValues(".").x;
            var textWidth = labelContext.GetPreferredValues(startText + endText).x;
            var dotsCount = (int) ((width - textWidth) / dotWidth);
            dotsCount = Mathf.Max(dotsCount, 0);
            string dots = new string('.', dotsCount);
            var stringRgba = ColorUtility.ToHtmlStringRGBA(dotsColor);
            return $"{startText}<color=#{stringRgba}>{dots}</color>{endText}";
        }
        
        private void OnLogoutClicked()
        {
            _userAccountManager.Logout(OnLogoutSuccess, OnLogoutFailed);
        }

        private void OnLogoutSuccess()
        {
            _uncompressedBundlesManager.CleanCache();
            _oneSignalManager.RemoveExternalUser();
            _characterManager.ResetSelectedCharacterId();
            _pageManager.MoveNext(PageId.OnBoardingPage, new OnBoardingPageArgs());
        }

        private void OnLogoutFailed(string msg)
        {
            var informationPopupConfiguration = new InformationPopupConfiguration()
            {
                PopupType = PopupType.Fail,
                Description = msg
            };
            _popupManager.SetupPopup(informationPopupConfiguration);
            _popupManager.ShowPopup(informationPopupConfiguration.PopupType);
        }
        
        private void OnHideAnimationFinished(Action onComplete)
        {
            // Check if page is not destroyed yet (scene unloaded)
            if (this != null)
            {
                base.OnHidingBegin(onComplete);
            }
            else
            {
                onComplete?.Invoke();
            }
        }

        private void OnButtonSoundToggleValueChanged(bool value)
        {
            _soundManager.MuteChannel(!value, MixerChannel.Button);

            if (value)
            {
                _soundManager.Play(SoundType.Button1,  MixerChannel.Button);
            }
        }

        private void OnHapticsValueChanged(bool isOn)
        {
            AppSettings.HapticsEnabled = isOn;
        }
        
        private void InitLocalizationTestOptions()
        {
            _localizationTestOptionsDropdown.SetActive(_bridge.Profile.IsEmployee);
            
            if(!_bridge.Profile.IsEmployee) return;

            _localizationTestOptionsDropdown.options = new List<TMP_Dropdown.OptionData>
            {
                new TMP_Dropdown.OptionData("Default"),
                new TMP_Dropdown.OptionData("Display keys"),
                new TMP_Dropdown.OptionData("Display prefixes")
            };

            var defaultValue = 0;

            if (LocalizationTestTools.DisplayKeys)
            {
                defaultValue = 1;
            }
            else if (LocalizationTestTools.DisplayPrefixes)
            {
                defaultValue = 2;
            }
            
            _localizationTestOptionsDropdown.SetValueWithoutNotify(defaultValue);
            
            _localizationTestOptionsDropdown.onValueChanged.RemoveAllListeners();
            _localizationTestOptionsDropdown.onValueChanged.AddListener(OnValueChanged);
            return;

            void OnValueChanged(int value)
            {
                switch (value)
                {
                    case 0:
                    {
                        LocalizationTestTools.DisplayKeys = false;
                        LocalizationTestTools.DisplayPrefixes = false;
                        break;
                    }
                    case 1: LocalizationTestTools.DisplayKeys = true;
                        break;
                    case 2: LocalizationTestTools.DisplayPrefixes = true;
                        break;
                }
            }
        }
    }
}