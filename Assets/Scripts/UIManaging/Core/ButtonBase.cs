using JetBrains.Annotations;
using Navigation.Core;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Core
{
    public abstract class ButtonBase : MonoBehaviour
    {
        [Inject] [UsedImplicitly]
        protected PageManager Manager;

        private Button _button;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        protected bool Interactable
        {
            get => Button.interactable;
            set => Button.interactable = value;
        }

        public Button Button 
        {
            get
            {
                if (_button == null)
                {
                    _button = GetComponent<Button>();
                }

                return _button;
            }
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected virtual void OnEnable()
        {
            Button.onClick.AddListener(OnClick);
        }

        protected virtual void OnDisable()
        {
            Button.onClick.RemoveListener(OnClick);
        }

        protected abstract void OnClick();
    }
}
