using Modules.InputHandling;
using UnityEngine;
using Zenject;

namespace UIManaging.SnackBarSystem.SnackBars
{
    /// <summary>
    /// Controls the input gestures active state during the snack bar showing
    /// If it gestures are OFF before showing snack bar, it will turn them ON.
    /// On snackbar closing it tries to keep latest requested input state
    /// (turn OFF if it was OFF before snack back showing and no other modules tried to activate
    /// or keep it active if any other module tried to activate during displaying time
    /// </summary>
    internal sealed class InputManagerSnackBarControl: MonoBehaviour
    {
        private bool _lastInputState;
        [Inject] private IInputManager _inputManager;
        
        private void OnEnable()
        {
            _lastInputState = _inputManager.Enabled;

            _inputManager.Enable(true);
            _inputManager.EnableStateChangeRequested += OnEnableStateChangeRequested;
        }

        private void OnDisable()
        {
            _inputManager.EnableStateChangeRequested -= OnEnableStateChangeRequested;
            _inputManager.Enable(_lastInputState);
        }
        
        private void OnEnableStateChangeRequested(bool isOn)
        {
            _lastInputState = isOn;
        }
    }
}