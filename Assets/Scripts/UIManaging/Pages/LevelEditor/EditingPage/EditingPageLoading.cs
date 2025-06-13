using System;
using System.Collections;
using Common;
using DG.Tweening;
using Extensions;
using JetBrains.Annotations;
using Modules.InputHandling;
using TMPro;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.LevelEditor.EditingPage
{
    public sealed class EditingPageLoading : MonoBehaviour
    {
        private const float DEFAULT_FADE_DURATION = 0.25f;
        private const float DEFAULT_TIMEOUT = 30f;

        [SerializeField] private Button _cancelButton;
        [SerializeField] private TextMeshProUGUI _darkThemeMessage;
        [SerializeField] private GameObject _darkThemeProgress;
        [SerializeField] private GameObject _darkThemeSpinner;
        
        [SerializeField] private TextMeshProUGUI _messageText;
        
        [SerializeField] private GameObject _defaultTheme; // Group of UI elements to show as regular loading
        [SerializeField] private GameObject _darkTheme; // Group of UI elements to show on 'Back' actions

        private CanvasGroup _canvasGroup;

        private Coroutine _timerCoroutine;
        private PopupManager _popupManager;
        private IBackButtonEventHandler _backButtonEventHandler;

        private GameObject _theme;

        private Action _onCancel;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        [Inject]
        [UsedImplicitly]
        private void Construct(PopupManager popupManager, IBackButtonEventHandler backButtonEventHandler)
        {
            _popupManager = popupManager;
            _backButtonEventHandler = backButtonEventHandler;
        }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            Configure(Constants.NavigationMessages.DEFAULT_LEVEL_EDITOR_MESSAGE);
            
            _cancelButton.onClick.AddListener(OnCancelButtonClicked);
        }

        private void OnEnable()
        {
            _backButtonEventHandler.ProcessEvents(false);
        }
        
        private void OnDisable()
        {
            _backButtonEventHandler.ProcessEvents(true);
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
            _cancelButton.onClick.RemoveListener(OnCancelButtonClicked);
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Configure(string message)
        {
            _messageText.text = message;
        }

        public void Show(bool fadeIn = false, float duration = DEFAULT_FADE_DURATION, Action onCancel = null)
        {
            Show(DEFAULT_TIMEOUT, fadeIn, duration, onCancel);
        }

        public void Show(float timeout, bool fadeIn = false, float duration = DEFAULT_FADE_DURATION, Action onCancel = null)
        {
            _onCancel = onCancel;
            ShowTheme(_defaultTheme, timeout, fadeIn, duration);
        }

        public void ShowDarkOverlay()
        {
            ShowDark(String.Empty, false, false);
        }
        
        public void ShowDark(string message = null, bool showSpinner = true, Action onCancel = null)
        {
            ShowDark(message, true, showSpinner, onCancel);
        }

        public void ShowDark(string message, bool showProgress, bool showSpinner = true, Action onCancel = null)
        {
            _onCancel = onCancel;
            ShowDarkTheme(message, showProgress, showSpinner);
        }

        public void Hide(float duration = DEFAULT_FADE_DURATION, Action callback = null)
        {
            if (this != null && _timerCoroutine != null)
            {
                StopCoroutine(_timerCoroutine);
            }

            FadeOut(duration, callback);
            _onCancel = null;
        }

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        private void OnCancelButtonClicked()
        {
            _onCancel?.Invoke();
        }
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void ShowTheme(GameObject theme, float timeout = DEFAULT_TIMEOUT, bool fadeIn = false, float duration = DEFAULT_FADE_DURATION)
        {
            if (theme == _defaultTheme)
            {
                _darkTheme.SetActive(false);
            }
            else
            {
                _defaultTheme.SetActive(false);
            }
            
            _theme = theme;
            _theme.SetActive(true);
            gameObject.SetActive(true);

            if (gameObject.activeInHierarchy)
            {
                _timerCoroutine = StartCoroutine(ShowTimeoutPopup(timeout));
            }

            _canvasGroup.alpha = 1f;

            if (fadeIn) FadeIn(duration);
        }

        private void ShowDarkTheme(string message, bool showProgress = true, bool showSpinner = true)
        {
            _darkThemeProgress.SetActive(showProgress);
            _darkThemeMessage.text = message ?? "Loading...";
            _darkThemeSpinner.SetActive(showSpinner);
            ShowTheme(_darkTheme);
        }

        private void FadeIn(float duration)
        {
            _canvasGroup.alpha = 0;
            _canvasGroup.DOFade(1f, duration).SetUpdate(true);
        }

        private void FadeOut(float duration, Action callback)
        {
            _canvasGroup.DOFade(0f, duration).SetUpdate(true).OnComplete(() =>
            {
                if (_theme) _theme.SetActive(false);
                gameObject.SetActive(false);
                callback?.Invoke();
            });
        }

        //---------------------------------------------------------------------
        // Coroutines
        //---------------------------------------------------------------------

        private IEnumerator ShowTimeoutPopup(float timeOut)
        {
            yield return new WaitForSecondsRealtime(timeOut);

            var timeoutPopupConfiguration = new InformationPopupConfiguration()
            {
                PopupType = PopupType.SlowConnection
            };

            _popupManager.SetupPopup(timeoutPopupConfiguration);
            _popupManager.ShowPopup(timeoutPopupConfiguration.PopupType);

            _cancelButton.SetActive(true);
        }
    }
}