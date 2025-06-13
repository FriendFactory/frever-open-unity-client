using System;
using UnityEngine;
using Zenject;
#if UNITY_EDITOR
using Sirenix.OdinInspector;
#endif

namespace UIManaging.Pages.LevelEditor.Ui
{
    public class AudioRecordingStateControllerHelper: MonoBehaviour
    {
        [SerializeField] private AudioRecordingTrigger _trigger;
        
        [Inject] private AudioRecordingStateController _stateController;
        
#if UNITY_EDITOR
        [Button]
        private void FireAsync()
        {
            try
            {
                _stateController.FireAsync(_trigger);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        [Button]
        private void LogStates()
        {
            Debug.Log($"[{GetType().Name}] state: {_stateController.State}, previous: {_stateController.PreviousState}");
        }
#endif
    }
}