using System;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Common.Buttons
{
    [RequireComponent(typeof(Button))]
    public abstract class BaseButton : MonoBehaviour
    {
        protected Button Button;
        
        public event Action Clicked;

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected abstract void OnClickHandler();

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        
    #if UNITY_EDITOR
        private void OnValidate()
        {
            if (Button) return;

            Button = GetComponent<Button>();
        }
    #endif

        protected virtual void Awake()
        {
            Button = GetComponent<Button>();
        }

        protected virtual void OnEnable()
        {
            Button.onClick.AddListener(OnClick);
        }

        protected virtual void OnDisable()
        {
            Button.onClick.RemoveListener(OnClick);
        }
        
        //---------------------------------------------------------------------
        // Private 
        //---------------------------------------------------------------------

        private void OnClick()
        {
            OnClickHandler();
            
            Clicked?.Invoke();
        }
    }
}