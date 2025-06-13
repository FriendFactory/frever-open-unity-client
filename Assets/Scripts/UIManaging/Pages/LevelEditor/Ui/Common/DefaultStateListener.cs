using System;
using System.Linq;
using UnityEngine;

namespace UIManaging.Pages.LevelEditor.Ui.Common
{
    /// <summary>
    /// Activates game object on defined states
    /// </summary>
    internal abstract class DefaultStateListener<TState> : StateListenerBase<TState>
        where TState: Enum
    {
        [SerializeField] private TState[] _activeOnStates;

        protected override void OnInitialize()
        {
            StartListenToStateChanging();
        }

        public override void OnStateChanged(TState state)
        {
            var shouldBeActive = _activeOnStates.Contains(state);
            gameObject.SetActive(shouldBeActive);
        }
    }
}