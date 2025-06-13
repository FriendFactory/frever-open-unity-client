using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Components
{
    [RequireComponent(typeof(Button))]
    public class ClickListener : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        private const float DOUBLE_CLICK_TIME_THRESHOLD = 0.22f;
        
        private bool _firstClickPerformed;
        private Coroutine _waitForDoubleClickCoroutine;
        private Button _button;
        private WaitForSeconds _doubleClickDelay;
    
        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------
        
        public event Action OnClickedEvent;
        public event Action OnDoubleClickedEvent;
        public event Action<int> PointerDown;
        public event Action<int> PointerUp;
        
        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            _button = GetComponent<Button>();
            _doubleClickDelay = new WaitForSeconds(DOUBLE_CLICK_TIME_THRESHOLD);
        }

        private void OnEnable()
        {
            _button.onClick.AddListener(OnPointerClick);
        }

        private void OnDisable()
        {
            StopWaitCoroutine();
            _button.onClick.RemoveListener(OnPointerClick);
        }
        
        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public void OnPointerDown(PointerEventData eventData)
        {
            PointerDown?.Invoke(eventData.pointerId);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            PointerUp?.Invoke(eventData.pointerId);
        }
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnPointerClick()
        {
            if (!_firstClickPerformed)
            {
                _waitForDoubleClickCoroutine = StartCoroutine(WaitForDoubleClick());
            }
            else
            {
                StopWaitCoroutine();
                OnDoubleClickedEvent?.Invoke();
                _firstClickPerformed = false;
            }
        }

        private void StopWaitCoroutine()
        {
            if (_waitForDoubleClickCoroutine != null)
            {
                StopCoroutine(_waitForDoubleClickCoroutine);
            }
        }

        private IEnumerator WaitForDoubleClick()
        {
            _firstClickPerformed = true;
            yield return _doubleClickDelay;
            OnClickedEvent?.Invoke();
            _firstClickPerformed = false;
        }
    }
}