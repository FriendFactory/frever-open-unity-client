using System;
using Bridge;
using JetBrains.Annotations;
using QFSW.QC;
using QFSW.QC.Utilities;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Scripting;
using Zenject;

namespace Development
{
    [DisallowMultipleComponent]
    public class ConsoleControlButton : MonoBehaviour, IDragHandler, IPointerClickHandler
    {
        [SerializeField] private QuantumConsole _console;
        [SerializeField] private RectTransform _resizeRoot;
        [SerializeField] private Canvas _resizeCanvas;

        [Inject] private IBridge _bridge;

        private float _buttonHeight;
        private static bool _sudoMode;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            #if QC_DISABLED
                Destroy(_console.gameObject);
                return;
            #else
                _buttonHeight = GetComponent<RectTransform>().sizeDelta.y + 20f;
                _console.IsInvokeAllowed += IsInvokeAllowed;
            #endif
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!eventData.dragging) _console.Toggle();
        }

        public void OnDrag(PointerEventData eventData)
        {
            var scaleFactor = _resizeCanvas.scaleFactor;

            var minBoundCanvas = _resizeRoot.offsetMax.y;
            var maxBoundCanvas = Screen.height / scaleFactor;

            var minBoundScreen = minBoundCanvas * scaleFactor;
            var maxBoundScreen = Screen.height;

            var delta = eventData.delta;

            if (delta.y < 0 && _resizeRoot.sizeDelta.y >= maxBoundCanvas - _buttonHeight)
            {
                _resizeRoot.sizeDelta = new Vector2(_resizeRoot.sizeDelta.x, maxBoundCanvas - _buttonHeight);
                return;
            }

            if (delta.y > 0 && _resizeRoot.sizeDelta.y <= minBoundCanvas)
            {
                _resizeRoot.sizeDelta = new Vector2(_resizeRoot.sizeDelta.x, minBoundCanvas);
                return;
            }

            var posCurrent = eventData.position;
            var posLast = posCurrent - delta;

            var posCurrentBounded = Mathf.Clamp(posCurrent.y, minBoundScreen, maxBoundScreen);
            var posLastBounded = Mathf.Clamp(posLast.y, minBoundScreen, maxBoundScreen);

            var deltaBounded = (posCurrentBounded - posLastBounded) / _resizeCanvas.scaleFactor;

            _resizeRoot.sizeDelta += new Vector2(0, -deltaBounded);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private bool IsInvokeAllowed()
        {
            // Allow to use the console in all environments if sudo mode is enabled
            if (_sudoMode) return true;

            // Allow to use the console in all environments except production
            var environment = _bridge.Environment;
            var isInvokeAllowed = environment != FFEnvironment.Production;
            if (!isInvokeAllowed) Debug.LogWarning($"You can't use the console in the current environment: {_bridge.Environment}");

            return isInvokeAllowed;
        }

        //---------------------------------------------------------------------
        // Commands
        //---------------------------------------------------------------------

        [Preserve]
        [Command("sudo", "Enables sudo mode for the console", MonoTargetType.Single)]
        private void EnableSudoMode(string password)
        {
            var theme = _console.Theme;

            if (password != $"jedi{DateTime.Today:ddMM}")
            {
                _sudoMode = false;
                Debug.Log("Sudo access denied.\n".ColorText(theme.ErrorColor) +
                          "\"The greatest teacher, failure is.\"".ColorText(theme.WarningColor));
                return;
            }

            _sudoMode = true;
            Debug.Log("Sudo mode enabled. You're allowed to use the console in any environment.\n".ColorText(theme.SuccessColor) +
                      "\"With great power comes great responsibility.\"".ColorText(theme.WarningColor));
        }
    }
}