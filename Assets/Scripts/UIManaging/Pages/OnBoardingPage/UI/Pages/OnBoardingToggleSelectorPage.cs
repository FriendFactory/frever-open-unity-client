using System;
using Navigation.Core;
using UiManaging.Pages.OnBoardingPage.UI.Args;
using UnityEngine;
using UnityEngine.UI;

namespace UiManaging.Pages.OnBoardingPage.UI.Pages
{
    public class OnBoardingToggleSelectorPage : OnBoardingBasePage<OnBoardingToggleSelectorPageArgs>
    {
        public override PageId Id { get; } = PageId.OnBoardingToggleSelectorPage;
        
        [SerializeField] private ToggleGroup _toggleGroup;
        [SerializeField] private Toggle[] _toggles;

        private void Awake()
        {
            _toggleGroup.SetAllTogglesOff();
        }

        protected override void OnDisplayStart(OnBoardingToggleSelectorPageArgs args)
        {
            base.OnDisplayStart(args);
            
            for (int i = 0; i < _toggles.Length; i++)
            {
                _toggles[i].onValueChanged.AddListener(OnToggleValueChanged);
            }
        }

        protected override void OnHidingBegin(Action onComplete)
        {
            base.OnHidingBegin(onComplete);
            
            for (int i = 0; i < _toggles.Length; i++)
            {
                _toggles[i].onValueChanged.RemoveListener(OnToggleValueChanged);
            }
        }
        
        private void OnToggleValueChanged(bool value)
        {
            if(value)
            {
                for (int i = 0; i < _toggles.Length; i++)
                {
                    if (_toggles[i].isOn)
                    {
                        OpenPageArgs.SetValue?.Invoke(i);
                        RefreshContinueButtonInteractivity();
                        return;
                    }
                }
            }

            OpenPageArgs.OnAllTogglesUnselectedAction?.Invoke();
            RefreshContinueButtonInteractivity();
        }
    }
}