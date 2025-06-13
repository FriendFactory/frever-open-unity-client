using UI.UIAnimators;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Common.Toggles
{
    [RequireComponent(typeof(Toggle))]
    internal class ToggleAnimator: MonoBehaviour
    {
        [SerializeField] private Toggle _toggle;
        [SerializeField] private ParallelUiAnimationPlayer _animationPlayer;
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_toggle) return;

            _toggle = GetComponent<Toggle>();
        }
#endif

        private void OnEnable()
        {
            if (_toggle.isOn)
            {
                _animationPlayer.PlayShowAnimationInstant();
            }
            else
            {
                _animationPlayer.PlayHideAnimationInstant();
            }
            
            _toggle.onValueChanged.AddListener(OnValueChanged);
        }

        private void OnDisable()
        {
            _toggle.onValueChanged.RemoveListener(OnValueChanged);
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        private void OnValueChanged(bool isOn)
        {
            PlayToggleState(isOn);
        }

        private void PlayToggleState(bool isOn)
        {
            _toggle.interactable = false;
            
            if (isOn)
            {
                _animationPlayer.PlayShowAnimation(() => _toggle.interactable = true);
            }
            else
            {
                _animationPlayer.PlayHideAnimation(() => _toggle.interactable = true);
            }
        }
    }
}