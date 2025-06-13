using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using Bridge;
using DG.Tweening;
using EnhancedUI.EnhancedScroller;
using Extensions;
using I2.Loc;
using Modules.Crew;
using Modules.Notifications;
using Navigation.Args;
using Navigation.Core;
using TMPro;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.Pages.InboxPage.Models;
using UIManaging.Pages.InboxPage.Views;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.InboxPage
{
    internal sealed class InboxPage: GenericPage<InboxPageArgs>
    {
        private const float SCROLL_BACK_BUTTON_DELTA = 0.1f;
        private const float UPDATE_PERIOD = 5f;
        
        [SerializeField] private ChatListView _chatListView;
        [SerializeField] private GameObject _unreadNotificationsIcon;
        [SerializeField] private Button _startChatBtn;
        [Space]
        [SerializeField] private Button _joinCrewBtn;
        [SerializeField] private GameObject _joinCrewObj;
        [SerializeField] private RawImage _joinCrewBackground;
        [Space]
        [SerializeField] private CrewChatItemView _crewChatItemView;
        [Space]
        [SerializeField] private EnhancedScroller _scroller;
        [SerializeField] private Button _scrollBackButton;
        [Space] 
        [SerializeField] private GameObject _lockedContainer;
        [SerializeField] private TextMeshProUGUI _lockedTimer;
        [SerializeField] private LocalizedString _lockedFormat;

        [Inject] private IBridge _bridge;
        [Inject] private PopupManager _popupManager;
        [Inject] private INotificationHandler _notificationHandler;
        [Inject] private LocalUserDataHolder _dataHolder;
        [Inject] private CrewService _crewService;
        [Inject] private LocalUserDataHolder _localUser;
        
        private ChatListModel _chatListModel;
        private float _defaultScrollPos;
        private CancellationTokenSource _tokenSource;
        private ChatItemModel _crewChatItemModel;
        private Coroutine _timerCoroutine;
        
        public override PageId Id => PageId.Inbox;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void OnEnable()
        {
            _tokenSource = new CancellationTokenSource();
            _joinCrewBackground.texture = _crewService.GetCrewBall();
            _timerCoroutine = StartCoroutine(TimerCoroutine());
        }

        private void OnDisable()
        {
            _timerCoroutine = null;
            _tokenSource.CancelAndDispose();
            _tokenSource = null;
        }
        
        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override void OnInit(PageManager pageManager)
        {
            
        }

        protected override void OnDisplayStart(InboxPageArgs args)
        {
            base.OnDisplayStart(args);

            var locked = _localUser.UserProfile.ChatAvailableAfterTime > DateTime.UtcNow;

            if (_chatListModel != null)
            {
                _chatListModel.CleanUp();
                _chatListModel.OpenChatRequested -= OpenChat;
            }
            
            _chatListModel = new ChatListModel(_bridge);
            _chatListView.Initialize(locked ? new EmptyChatListModel() : _chatListModel);
            _chatListModel.OpenChatRequested += OpenChat;

            UpdateCrewViews();
            _unreadNotificationsIcon.SetActive(_notificationHandler.HasUnreadNotifications);

            _defaultScrollPos = _scroller.Container.position.y;
            
            UpdateScrollBackButton();
            
            _startChatBtn.onClick.AddListener(OnStartChat);
            _joinCrewBtn.onClick.AddListener(OnJoinCrew);
            _scrollBackButton.onClick.AddListener(AnimateScroll);
            _scroller.ScrollRect.onValueChanged.AddListener(OnScrollValueChanged);
            
            _startChatBtn.targetGraphic.SetAlpha(locked ? 0.5f : 1);
            
            _lockedContainer.SetActive(locked);

            if (_timerCoroutine != null)
            {
                StopCoroutine(_timerCoroutine);
            }
            
            _timerCoroutine = StartCoroutine(TimerCoroutine());
        }

        protected override void OnHidingBegin(Action onComplete)
        {
            if (_timerCoroutine != null)
            {
                StopCoroutine(_timerCoroutine);
                _timerCoroutine = null;
            }
            
            _startChatBtn.onClick.RemoveListener(OnStartChat);
            _joinCrewBtn.onClick.RemoveListener(OnJoinCrew);
            _scrollBackButton.onClick.RemoveListener(AnimateScroll);
            _scroller.ScrollRect.onValueChanged.RemoveListener(OnScrollValueChanged);
            
            _chatListView.CleanUp();
            
            if (_chatListModel != null)
            {
                _chatListModel.CleanUp();
                _chatListModel.OpenChatRequested -= OpenChat;
            }

            _crewChatItemView.CleanUp();
            
            if (_crewChatItemModel != null)
            {
                _crewChatItemModel.OpenChatRequested -= OpenCrew;
            }

            base.OnHidingBegin(onComplete);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnStartChat()
        {
            PopupConfiguration config = DateTime.UtcNow > _localUser.UserProfile.ChatAvailableAfterTime
                ? new StartChatPopupConfiguration(OpenChat)
                : new DirectMessagesLockedPopupConfiguration();
            
            _popupManager.SetupPopup(config);
            _popupManager.ShowPopup(config.PopupType);
            
        }

        private async void OpenChat(long chatId)
        {
            var result = await _bridge.GetChatById(chatId, _tokenSource.Token);

            if (result.IsRequestCanceled || _tokenSource == null || _tokenSource.IsCancellationRequested)
            {
                Debug.LogWarning($"Canceled chat info request.");
                return;
            }

            if (result.IsError)
            {
                Debug.LogError($"Failed to receive chat info, reason: {result.ErrorMessage}");
                return;
            }

            if (result.IsSuccess)
            {
                Manager.MoveNext(new ChatPageArgs(result.Model));
            }
        }

        private async void UpdateCrewViews()
        {
            var hasCrew = _dataHolder.UserProfile.CrewProfile != null;
            
            _joinCrewObj.SetActive(!hasCrew);
            _crewChatItemView.SetActive(false);

            if (!hasCrew) return;

            var result = await _bridge.GetMyCrewChat(_tokenSource.Token);

            if (result.IsSuccess)
            {
                if (_crewChatItemModel != null)
                {
                    _crewChatItemModel.OpenChatRequested -= OpenCrew;
                }

                var chatInfo = result.Model;
                var motd = await RequestCrewMOTD(_dataHolder.UserProfile.CrewProfile.Id, _tokenSource.Token);
                chatInfo.LastMessageText = motd;

                _crewChatItemModel = new ChatItemModel(chatInfo, Array.Empty<long>());
                _crewChatItemModel.OpenChatRequested += OpenCrew;
                _crewChatItemView.Initialize(_crewChatItemModel);

                _crewChatItemView.SetActive(true);
            }
            else if (result.IsError)
            {
                Debug.LogError($"Failed to fetch crew chat data, reason: {result.ErrorMessage}");
            }
        }

        private void OnJoinCrew()
        {
            Manager.MoveNext(new CrewSearchPageArgs());
        }

        private void AnimateScroll()
        {
            _scroller.Container.DOMoveY(_defaultScrollPos, 0.5f).SetEase(Ease.InOutCubic);
        }

        private void OnScrollValueChanged(Vector2 pos)
        {
            UpdateScrollBackButton();
        }
        
        private void UpdateScrollBackButton()
        {
            var isActive = Mathf.Abs(_scroller.Container.position.y - _defaultScrollPos) > SCROLL_BACK_BUTTON_DELTA;

            if (_scrollBackButton.gameObject.activeSelf != isActive)
            {
                _scrollBackButton.SetActive(isActive);
            }
        }

        private void OpenCrew(long crewChatId)
        {
            Manager.MoveNext(new CrewPageArgs());
        }

        private async Task<string> RequestCrewMOTD(long crewId, CancellationToken token)
        {
            var result = await _bridge.GetCrew(crewId, token);

            if (result.IsSuccess) return result.Model.MessageOfDay;
            if (result.IsError) Debug.LogError($"Failed to fetch crew data, reason: {result.ErrorMessage}");

            return null;
        }
        
        private IEnumerator TimerCoroutine()
        {
            do
            {
                var timeLeft = _localUser.UserProfile.ChatAvailableAfterTime - DateTime.UtcNow;

                try
                {
                    if (timeLeft < TimeSpan.Zero)
                    {
                        _lockedTimer.text = string.Format(_lockedFormat, 0, 0);
                    }
                    else
                    {
                        _lockedTimer.text = string.Format(_lockedFormat, (int)Math.Floor(timeLeft.TotalHours), timeLeft.Minutes);
                    }
                }
                catch (FormatException)
                {
                    Debug.LogError("Incorrect locked timer text format: " + _lockedFormat);
                    yield break;
                }

                yield return new WaitForSeconds(UPDATE_PERIOD);
            } 
            while (_timerCoroutine != null);
        }
    }
}