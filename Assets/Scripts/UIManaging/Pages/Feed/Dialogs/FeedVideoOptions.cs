using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Abstract;
using Bridge;
using Bridge.Models;
using Bridge.Models.ClientServer.Chat;
using Bridge.Models.VideoServer;
using Extensions;
using Modules.Chat;
using Modules.Crew;
using Modules.VideoStreaming.Feed;
using Navigation.Args;
using Navigation.Core;
using UIManaging.Animated.Behaviours;
using UIManaging.Common.Buttons;
using UIManaging.Localization;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.Pages.Common.VideoManagement;
using UIManaging.Pages.ReportPage.Ui.Args;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UIManaging.PopupSystem.Popups;
using UIManaging.PopupSystem.Popups.Views;
using UIManaging.SnackBarSystem;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Zenject;
using ButtonStateSelection = Extensions.ButtonExtension.ButtonStateSelection;

namespace UIManaging.Pages.Feed.Dialogs
{
    public class FeedVideoOptions : BaseContextDataView<FeedVideoModel>
    {
        [Range(0,1f)]
        [SerializeField] private float _disabledButtonStateAlpha = 0.5f;

        [SerializeField] private SaveVideoButton saveButton;
        [SerializeField] private GameObject _saveLoadingCircle;
        [SerializeField] private CanvasGroup _saveLoadingCanvasGroup;
        [Space]
        [SerializeField] private Button overlayButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button editTemplateButton;
        [SerializeField] private Button reportButton;
        [SerializeField] private Button deleteButton;
        [SerializeField] private PrivacyButton privacyButton;
        [Space]
        [SerializeField] private SlideInOutBehaviour _slideInOut;

        [Inject] private IBridge _bridge;
        [Inject] private PopupManager _popupManager;
        [Inject] private PopupManagerHelper _popupManagerHelper;
        [Inject] private SnackBarHelper _snackBarHelper;
        [Inject] private PageManager _pageManager;
        [Inject] private VideoManager _videoManager;
        [Inject] private LocalUserDataHolder _localUserDataHolder;
        [Inject] private CrewService _crewService;
        [Inject] private IChatService _chatService;
        [Inject] private FeedLocalization _localization;

        private CancellationTokenSource _tokenSource;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        private bool IsLocalUserVideo => ContextData.Video.GroupId == _bridge.Profile.GroupId;

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        public event Action<long> OnDeletionConfirmedEvent;
        public event Action<long, VideoAccess> OnPrivacyChangedEvent;
        public event Action OnEditTemplateButton;
        
        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override void OnInitialized()
        {
            _tokenSource = new CancellationTokenSource();
            
            overlayButton.onClick.AddListener(OnOverlayButtonClicked);
            closeButton.onClick.AddListener(OnCloseButtonClicked);
            reportButton.onClick.AddListener(OnReportButtonClicked);
            deleteButton.onClick.AddListener(OnDeleteButtonClicked);
            editTemplateButton.transform.parent.SetActive(false);

            var isMyVideo = IsLocalUserVideo;
            
            deleteButton.transform.parent.SetActive(isMyVideo);
            privacyButton.transform.parent.SetActive(isMyVideo);
            saveButton.transform.parent.SetActive(isMyVideo);
            
            if (isMyVideo)
            {
                var video = ContextData.Video;
                editTemplateButton.onClick.AddListener(OnEditTemplateButtonClicked);
                privacyButton.Access = ContextData.VideoAccess;
                privacyButton.SelectedCallback = result => OnResultSelected(video, result);
                editTemplateButton.transform.parent.SetActive(true);
                SetupTemplateButtonColor(ContextData.Video.TemplateFromVideo.AllowFeature);
                SetPrivacyButtonProfiles();
                saveButton.Initialize(new SaveVideoButtonArgs(video.Id));
            }

            _slideInOut.SlideIn();
        }

        protected override void BeforeCleanup()
        {
            base.BeforeCleanup();
            
            overlayButton.onClick.RemoveListener(OnOverlayButtonClicked);
            closeButton.onClick.RemoveListener(OnCloseButtonClicked);
            reportButton.onClick.RemoveListener(OnReportButtonClicked);
            editTemplateButton.onClick.RemoveListener(OnEditTemplateButtonClicked);
            deleteButton.onClick.RemoveListener(OnDeleteButtonClicked);
            _tokenSource?.CancelAndDispose();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnCloseButtonClicked()
        { 
            Close();
        }
        
        private void OnOverlayButtonClicked()
        {
            Close();
        }



        private void OnEditTemplateButtonClicked()
        {
            OnEditTemplateButton?.Invoke();
        }

        private async void OnReportButtonClicked()
        {
            var isVideoAvailable = await IsVideoAvailable();
            if (!isVideoAvailable)
            {
                ShowSnackBar(_localization.ReportVideoFailedSnackbarMessage);
                Close();
                return;
            }

            _pageManager.MoveNext(PageId.ReportPage, new ReportPageArgs(ContextData.Video));
        }
        
        private void OnShareWithCrewButtonClicked()
        {
            if (_localUserDataHolder.UserProfile.CrewProfile == null)
            {
                _snackBarHelper.ShowInformationSnackBar("You are not a member of a crew yet");
                return;
            }

            if (ContextData.Video.Access != VideoAccess.Public)
            {
                _popupManagerHelper.ShowDialogPopup("Video is not public", 
                                    "Video may not be available to all of your crew members. Share it anyway?",
                                    "No", null, "Yes", ShareVideoToCrew, false);
                return;
            }

            ShareVideoToCrew();
        }

        private async void ShareVideoToCrew()
        {
            Close();
            
            var messageModel = new AddMessageModel
            {
                VideoId = ContextData.Video.Id
            };
            
            await _crewService.RefreshCrewDataAsync(_tokenSource.Token);
            var result = await _chatService.PostMessage(_crewService.Model.ChatId, messageModel);
            if (!result.IsSuccess)
            {
                Debug.LogError(result.ErrorMessage);
                return;
            }
            
            _snackBarHelper.ShowVideoSharedToCrewSnackBar(OpenCrewChat);
        }

        private void OnResultSelected(Video video, PrivacyPopupResult result)
        {
            if (_bridge.Profile.GroupId != video.GroupId)
            {
                return;
            }

            if (video.GeneratedTemplateId.HasValue
                && result.Access != VideoAccess.Public)
            {
                OpenPrivacyChangeForTemplatePopup(video, result);
                return;
            }

            ChangeVideoPrivacy(video, result);
        }

        private void ChangeVideoPrivacy(Video video, PrivacyPopupResult result)
        {
            var cachedAccess = video.Access;
            var cachedGroups = video.NonCharacterTaggedGroups;
            
            video.Access = result.Access;
            video.NonCharacterTaggedGroups = result.SelectedProfiles
                                                   .Where(profile => video.TaggedGroups?.All(group => group.GroupId != profile.Id) ?? true)
                                                   .Select(profile => new TaggedGroup
                                                    {
                                                        GroupId = profile.Id,
                                                        GroupNickname = profile.Nickname
                                                    }).ToArray();
            
            _videoManager.ChangeVideoPrivacy(video, result.Access, 
                    result.SelectedProfiles.Select(profile => profile.Id).ToArray(), () =>
            {
                _videoManager.RefreshVideoKPIInCache(video.Id, default);
                _snackBarHelper.ShowInformationSnackBar(string.Format(_localization.VideoPrivacyUpdateSuccessSnackbarMessageFormat, result.Access.ToText()));
                OnPrivacyChangedEvent?.Invoke(video.Id, result.Access);
            }, () =>
            {
                video.Access = cachedAccess;
                video.NonCharacterTaggedGroups = cachedGroups;
                _snackBarHelper.ShowInformationSnackBar(_localization.VideoPrivacyUpdateFailedSnackbarMessage);
            });

            if (!IsDestroyed && gameObject.activeSelf)
            {
                Close();
            }
        }

        private void OnDeleteButtonClicked()
        {
            ShowDeleteVideoConfirmation();
            Close();
        }
        
        private void OpenPrivacyChangeForTemplatePopup(Video video, PrivacyPopupResult result)
        {
            var config = new DialogDarkPopupConfiguration()
            {
                Title = _localization.VideoPrivacyPopupTitle,
                Description = _localization.VideoPrivacyPopupDescription,
                YesButtonText = _localization.VideoPrivacyPopupConfirmButton,
                NoButtonText = _localization.VideoPrivacyPopupCancelButton,
                OnYes = () => ChangeVideoPrivacy(video, result),
                OnNo = () => privacyButton.Access = VideoAccess.Public,
                PopupType = PopupType.DialogDark
            };
            
            _popupManager.PushPopupToQueue(config);
        }
        
        private void ShowDeleteVideoConfirmation()
        {
            var videoId = ContextData.Video.Id;
            var description = GetDeletePopupDescription();
            
            var configuration = new DialogDarkPopupConfiguration()
            {
                PopupType = PopupType.DialogDark,
                Title = _localization.DeleteVideoPopupTitle,
                Description = description,
                YesButtonText = _localization.DeleteVideoPopupConfirmButton,
                NoButtonText = _localization.DeleteVideoPopupCancelButton,
                OnYes = () => OnDeletionConfirmedEvent?.Invoke(videoId),
            };
            
            _popupManager.SetupPopup(configuration);
            _popupManager.ShowPopup(configuration.PopupType);
        }

        private string GetDeletePopupDescription()
        {
            if (ContextData.Video.LevelId == null || ContextData.Video.LevelTypeId == ServerConstants.LevelType.VIDEO_MESSAGE)
            {
                return _localization.DeleteVideoPopupUploadedOrMessageDescription;
            }
            
            return ContextData.Video.GeneratedTemplateId == null
                ? _localization.DeleteVideoPopupDescription
                : _localization.DeleteVideoPopupTemplateDescription;
        }

        private void Close()
        {
            _slideInOut.SlideOut(()=> gameObject.SetActive(false));
        }


        private void ShowSnackBar(string message)
        {
            _snackBarHelper.ShowInformationSnackBar(message, 2);
        }
        
        private async Task<bool> IsVideoAvailable()
        {
            var videoResult = await _bridge.GetVideoAsync(ContextData.Video.Id);
            return videoResult.IsSuccess;
        }
        
        private async void SetPrivacyButtonProfiles()
        {
            privacyButton.enabled = false;

            var taggedGroups = ContextData.Video.TaggedGroups ?? Array.Empty<TaggedGroup>();
            var taggedIds = taggedGroups.Select(group => group.GroupId).ToArray();
            IEnumerable<TaggedGroup> selectedIdEnumerable = taggedGroups;

            if (ContextData.Video.NonCharacterTaggedGroups != null)
            {
                selectedIdEnumerable = selectedIdEnumerable.Concat(ContextData.Video.NonCharacterTaggedGroups);
            }
            
            var selectedIds = selectedIdEnumerable.Select(group => group.GroupId).Distinct().ToArray();

            var result = await _bridge.GetProfilesShortInfo(selectedIds, _tokenSource.Token);

            if (result.IsError)
            {
                Debug.LogError($"Failed to receive tagged and mentioned profiles, reason: {result.ErrorMessage}");
                return;
            }

            if (result.IsSuccess)
            {
                privacyButton.TaggedUsers = result.Profiles.Where(profile => taggedIds.Contains(profile.Id)).ToList();
                privacyButton.SelectedUsers = result.Profiles.ToList();
                privacyButton.enabled = true;
            }
        }

        private void OpenCrewChat()
        {
            _pageManager.MoveNext(new CrewPageArgs());
        }
        
        private void SetupTemplateButtonColor(bool featureAllowed)
        {
            var templateButtonColor = featureAllowed ? Color.white : new Color(1, 1, 1, _disabledButtonStateAlpha);
            var targetStates = new[] { ButtonStateSelection.Normal, ButtonStateSelection.Highlighted, ButtonStateSelection.Selected };
            editTemplateButton.SetStatesColor(targetStates, templateButtonColor);
        }
    }
}