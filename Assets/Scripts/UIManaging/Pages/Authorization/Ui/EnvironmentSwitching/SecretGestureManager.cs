using DigitalRubyShared;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.Authorization.Ui.EnvironmentSwitching
{
    internal sealed class SecretGestureManager: MonoBehaviour
    {
        private const float SWIPE_DELTA_Y_THRESHOLD = 100f;
        private const string ENVIRONMENT_SELECTION_PASSWORD = "SmultronLime21";
        [Inject] private FingersScript _fingersScript;
        [Inject] private PopupManager _popupManager;
        [SerializeField] private Button _inEditorButton;

        public UnityEvent Passed;

        private SwipeGestureRecognizer _gesture;
        
        private void Start()
        {
            _gesture = new SwipeGestureRecognizer { MinimumNumberOfTouchesToTrack = 2 };
            CreateGestureTarget();
            _fingersScript.AddGesture(_gesture);
            _gesture.StateUpdated -= UpdateGesture;
            _gesture.StateUpdated += UpdateGesture;
            _inEditorButton.gameObject.SetActive(Application.isEditor);
            _inEditorButton.onClick.AddListener(DisplayPasswordPopup);
        }
        
        private void UpdateGesture(GestureRecognizer gesture)
        {
            if (_gesture.State != GestureRecognizerState.Ended) return;
            if (_gesture.DeltaY > SWIPE_DELTA_Y_THRESHOLD)
                DisplayPasswordPopup();
        }

        private void OnDestroy()
        {
            if (_gesture == null) return;
            _fingersScript.RemoveGesture(_gesture);
            _gesture.Dispose();
            _gesture.StateUpdated -= UpdateGesture;
        }

        private void DisplayPasswordPopup()
        {
            var popupConfiguration = new PasswordPopupConfiguration
            {
                PopupType = PopupType.Password,
                OnSuccess = () => Passed?.Invoke(),
                Password = ENVIRONMENT_SELECTION_PASSWORD
            };

            _popupManager.SetupPopup(popupConfiguration);
            _popupManager.ShowPopup(popupConfiguration.PopupType);
        }
        
        private void CreateGestureTarget()
        {
            var panel = new GameObject("GestureTarget", typeof(RectTransform));
            panel.transform.SetParent(transform.parent);
            panel.transform.SetAsFirstSibling();
            
            var image = panel.AddComponent<Image>();
            image.color = new Color(1, 1, 1, 0);

            var rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = new Vector2(1, 1f);
            rect.localPosition = Vector3.zero;
            rect.localRotation = Quaternion.identity;
            rect.localScale = Vector3.one;
            rect.offsetMax = Vector2.zero;
            rect.offsetMin = Vector2.zero;

            _gesture.PlatformSpecificView = panel;
        }
    }
}