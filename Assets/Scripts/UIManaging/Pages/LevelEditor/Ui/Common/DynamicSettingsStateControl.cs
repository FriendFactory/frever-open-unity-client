using System;
using System.Linq;
using Modules.EditorsCommon;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.Common
{
    internal abstract class DynamicSettingsStateControl<TFeatureControl, TState>: StateListenerBase<TState>
        where TFeatureControl: IFeatureControl
        where TState: Enum
    {
        [SerializeField] protected TState[] ShouldBeActiveOnStates;
        [Inject] protected TFeatureControl FeatureControl;

        protected override void OnInitialize()
        {
            if (!FeatureControl.IsFeatureEnabled)
            {
                SetActive(false);
                return;
            }
            
            StartListenToStateChanging();
        }

        public override void OnStateChanged(TState state)
        {
            var shouldBeActive = ShouldBeActive(state);
            SetActive(shouldBeActive);
        }

        protected virtual bool ShouldBeActive(TState nextState)
        {
            return FeatureControl.IsFeatureEnabled && ShouldBeActiveOnStates.Contains(nextState);
        }

        protected void SetActive(bool isActive)
        {
            gameObject.SetActive(isActive);
        }
    }
}