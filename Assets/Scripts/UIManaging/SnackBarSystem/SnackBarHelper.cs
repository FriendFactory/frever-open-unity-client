using System;
using JetBrains.Annotations;
using UIManaging.SnackBarSystem.Configurations;
using UIManaging.SnackBarSystem.SnackBars;
using Zenject;

namespace UIManaging.SnackBarSystem
{
    [UsedImplicitly]
    public sealed class SnackBarHelper
    {
        [Inject] private SnackBarManager _snackBarManager;
        
        private SnackBarToken _informationSnackBarToken;
        private SnackBarToken _assetLoadingSnackBarToken;
        private SnackBarToken _seasonLikesSnackBarToken;
        private SnackBarToken _onboardingSeasonLikesSnackBarToken;
        private SnackBarToken _successSnackBarToken;
        private SnackBarToken _purchaseSuccessSnackBarToken;
        private SnackBarToken _purchaseFailedSnackBarToken;
        private SnackBarToken _inviterFollowedSnackBarToken;
        private SnackBarToken _failedSnackBarToken;
        private SnackBarToken _videoSharedToCrewSnackBarToken;
        
        public void ShowInformationSnackBar(string message, float? duration = null)
        {
            var snackBarConfiguration = new InformationSnackBarConfiguration
            {
                Title = message,
                Time = duration
            };

            if (_snackBarManager.IsShowing(snackBarConfiguration.Type))
            {
                _snackBarManager.PlayBlinking();
                return;
            }
            
            _informationSnackBarToken?.TryHide();
            _informationSnackBarToken = _snackBarManager.Show(snackBarConfiguration);
        }
        
        public void ShowInformationDarkSnackBar(string message, float? duration = null)
        {
            var snackBarConfiguration = new InformationDarkSnackBarConfiguration
            {
                Title = message,
                Time = duration
            };

            if (_snackBarManager.IsShowing(snackBarConfiguration.Type))
            {
                return;
            }
            
            _informationSnackBarToken?.TryHide();
            _informationSnackBarToken = _snackBarManager.Show(snackBarConfiguration);
        }
        
        public void ShowInformationShortSnackBar(string message, float? duration = null)
        {
            var snackBarConfiguration = new InformationShortSnackBarConfiguration
            {
                Title = message,
                Time = duration
            };

            if (_snackBarManager.IsShowing(snackBarConfiguration.Type))
            {
                _snackBarManager.PlayBlinking();
                return;
            }
            
            _informationSnackBarToken?.TryHide();
            _informationSnackBarToken = _snackBarManager.Show(snackBarConfiguration);
        }

        public void ShowMessagesLockedSnackBar(string message, float? duration = null)
        {
            var snackBarConfiguration = new MessagesLockedSnackBarConfiguration
            {
                Title = message,
                Time = duration
            };

            if (_snackBarManager.IsShowing(snackBarConfiguration.Type))
            {
                _snackBarManager.PlayBlinking();
                return;
            }
            
            _informationSnackBarToken?.TryHide();
            _informationSnackBarToken = _snackBarManager.Show(snackBarConfiguration);
        }
        
        public void ShowAssetLoadingSnackBar(float? duration)
        {
            var configuration = new AssetLoadingSnackBarConfiguration
            {
                Time = duration
            };
            
            _assetLoadingSnackBarToken?.TryHide();
            _assetLoadingSnackBarToken = _snackBarManager.Show(configuration);
        }

        public void ShowAssetInaccessibleSnackBar()
        {
            ShowInformationSnackBar("Cannot open because assets are no longer accessible", 3);
        }
        
        public void ShowCharacterLoadingFailedSnackBar()
        {
            ShowInformationSnackBar("Something went wrong when loading character");
        }

        public void ShowProfileBlockedSnackBar()
        {
            ShowInformationSnackBar("Account is not visible due to privacy", 3);
        }
        
        public void ShowSuccessSnackBar(string message, float? duration = null)
        {
            var snackBarConfiguration = new SuccessSnackBarConfiguration()
            {
                Title = message,
                Time = duration
            };

            _successSnackBarToken?.TryHide();
            _successSnackBarToken = _snackBarManager.Show(snackBarConfiguration);
        }

        public void ShowSuccessDarkSnackBar(string message, float? duration = null)
        {
            var snackBarConfiguration = new SuccessDarkSnackBarConfiguration()
            {
                Title = message,
                Time = duration
            };

            _successSnackBarToken?.TryHide();
            _successSnackBarToken = _snackBarManager.Show(snackBarConfiguration);
        }
        
        public void ShowSeasonLikesSnackBar(long notificationId, long questId, string message, float? duration = null)
        {
            var snackBarConfiguration = new SeasonLikesSnackBarConfiguration
            {
                NotificationId = notificationId,
                QuestId = questId,
                Title = message,
                Time = duration
            };
            
            _seasonLikesSnackBarToken = _snackBarManager.Show(snackBarConfiguration);
        }
        
        public void ShowOnboardingSeasonLikesSnackBar()
        {
            var snackBarConfiguration = new OnboardingSeasonLikesSnackBarConfiguration();

            if (!_snackBarManager.IsShowing(snackBarConfiguration.Type))
            {
                _onboardingSeasonLikesSnackBarToken = _snackBarManager.Show(snackBarConfiguration);
            }
        }

        public void ShowInviterFollowedSnackBar(string nickname, Action onClick)
        {
            var config = new InviterFollowedSnackbarConfiguration(nickname, onClick);

            _inviterFollowedSnackBarToken = _snackBarManager.Show(config);
        }

        public void ShowPurchaseSuccessSnackBar(string title)
        {
            var snackBarConfiguration = new PurchaseSuccessSnackBarConfiguration
            {
                Title = title,
                Time = 3,
            };
            
            _purchaseSuccessSnackBarToken?.TryHide();
            _purchaseSuccessSnackBarToken = _snackBarManager.Show(snackBarConfiguration); 
        }
        
        public void ShowPurchaseFailedSnackBar(string title)
        {
            var snackBarConfiguration = new PurchaseFailedSnackBarConfiguration
            {
                Title = title,
                Time = 10,
            };
            
            _purchaseFailedSnackBarToken?.TryHide();
            _purchaseFailedSnackBarToken = _snackBarManager.Show(snackBarConfiguration); 
        }

        public void ShowFailSnackBar(string message, float? duration = null)
        {
            var snackBarConfiguration = new FailSnackBarConfiguration()
            {
                Title = message,
                Time = duration
            };

            _failedSnackBarToken?.TryHide();
            _failedSnackBarToken = _snackBarManager.Show(snackBarConfiguration);
        }

        public void HideSnackBar(SnackBarType snackBarType)
        {
            switch (snackBarType)
            {
                case SnackBarType.Information:
                    _informationSnackBarToken?.TryHide();
                    break;
                case SnackBarType.Undefined:
                    break;
                case SnackBarType.AssetLoading:
                    _assetLoadingSnackBarToken?.TryHide();
                    break;
                case SnackBarType.VideoPublished:
                    break;
                case SnackBarType.Success:
                case SnackBarType.SuccessDark:
                    _successSnackBarToken?.TryHide();
                    break;
                case SnackBarType.Fail:
                    _failedSnackBarToken?.TryHide();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(snackBarType), snackBarType, null);
            }
        }

        public void ShowVideoSharedToCrewSnackBar(Action onViewButton)
        {
            var snackBarConfiguration = new VideoSharedToChatSnackbarConfiguration
            {
                Title = "Shared to crew",
                Time = 5,
                OnViewButton = onViewButton
            };
            
            _snackBarManager.Show(snackBarConfiguration); 
        }
    }
}
