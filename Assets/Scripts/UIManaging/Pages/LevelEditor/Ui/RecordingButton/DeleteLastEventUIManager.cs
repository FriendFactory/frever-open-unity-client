using System;
using System.Collections;
using System.Collections.Generic;
using DigitalRubyShared;
using JetBrains.Annotations;
using Modules.LevelManaging.Editing.LevelManagement;
using UIManaging.Pages.LevelEditor.Ui.FeatureControls;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.RecordingButton
{
    internal sealed class DeleteLastEventUIManager : MonoBehaviour
    {
        private const float CONFIRM_DELETE_DISPLAY_TIME = 5f;

        [SerializeField] private DeleteLastEventButton _deleteLastEventButton;
        [SerializeField] private DeleteLastEventButtonActivator _deleteButtonActivator;
        
        private ILevelManager _levelManager;
        private IDeleteEventFeatureControl _deleteEventFeatureControl;
        private bool _isConfirmingUIActive;

        private FingersScript _fingersScript;
        private readonly TapGestureRecognizer _gesture = new TapGestureRecognizer{SendBeginState = true};
        private HashSet<Type> _denyPassThroughTypes;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        [Inject]
        [UsedImplicitly]
        private void Construct(ILevelManager levelManager, IDeleteEventFeatureControl deleteEventFeatureControl, FingersScript fingersScript)
        {
            _levelManager = levelManager;
            _deleteEventFeatureControl = deleteEventFeatureControl;
            _fingersScript = fingersScript;
        }
        
        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void OnEnable()
        {
            _deleteButtonActivator.AddListener(ShowConfirmingUI);
            HideConfirmingUI();
        }

        private void OnDisable()
        {
            _deleteButtonActivator.RemoveListener(ShowConfirmingUI);
        }

        private void OnDestroy()
        {
            CleanUp();
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public void Initialize()
        {
            if (!_deleteEventFeatureControl.IsFeatureEnabled)
            {
                Display(false);
                return;
            }
            
            _levelManager.RecordingStarted += OnRecordingStarted;
            _levelManager.RecordingCancelled += OnRecordingCancelled;
            _levelManager.EventSaved += RefreshState;
            _levelManager.EventDeleted += OnEventDeleted;
            _levelManager.EventDeletionStarted += OnEventDeletionStarted;
            RefreshState();
        }

        public void CleanUp()
        {
            _levelManager.EventDeletionStarted -= OnEventDeletionStarted;
            _levelManager.RecordingStarted -= OnRecordingStarted;
            _levelManager.RecordingCancelled -= OnRecordingCancelled;
            _levelManager.EventSaved -= RefreshState;
            _levelManager.EventDeleted -= OnEventDeleted;
            HideConfirmingUI();
        }

        private void Display(bool display)
        {
            _isConfirmingUIActive = false;
            StopCoroutine(AutoHideConfirmingUI());
            CleanUpGesture();

            if (display)
            {
                _deleteButtonActivator.ShowImmediately();
                _deleteLastEventButton.HideImmediately();
            }
            else
            {

                _deleteButtonActivator.HideImmediately();
                _deleteLastEventButton.HideImmediately();
            }
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void ShowConfirmingUI()
        {
            _isConfirmingUIActive = true;
            _deleteButtonActivator.Hide(_deleteLastEventButton.Show);
            StartCoroutine(AutoHideConfirmingUI());
            SetupGesture();
        }

        private void HideConfirmingUI()
        {
            if (!_isConfirmingUIActive) return;
            _isConfirmingUIActive = false;

            StopCoroutine(AutoHideConfirmingUI());
            HideConfirmingButton();
            CleanUpGesture();
        }

        private void HideConfirmingButton()
        {
            _deleteLastEventButton.Hide(_deleteButtonActivator.Show);
        }

        private IEnumerator AutoHideConfirmingUI()
        {
            yield return new WaitForSeconds(CONFIRM_DELETE_DISPLAY_TIME);
            HideConfirmingUI();
        }

        private void RefreshState()
        {
            var shouldBeActive = _deleteEventFeatureControl.IsFeatureEnabled
                              && !_levelManager.IsLevelEmpty
                              && _deleteEventFeatureControl.CanDeleteEvent(_levelManager.GetLastEvent());

            Display(shouldBeActive);
        }
        
        private void OnEventDeleted()
        {
            _deleteButtonActivator.Interactable = true;
            HideConfirmingUI();
            RefreshState();
        }

        private void OnRecordingCancelled()
        {
            RefreshState();
        }

        private void OnRecordingStarted()
        {
            Display(false);
        }

        private void OnEventDeletionStarted()
        {
            _deleteButtonActivator.Interactable = false;
        }

        //---------------------------------------------------------------------
        // Gestures
        //---------------------------------------------------------------------

        private void UpdateGesture(GestureRecognizer gesture)
        {
            if (_gesture.State != GestureRecognizerState.Began) return;
            HideConfirmingUI();
        }

        private void SetupGesture()
        {
            _gesture.StateUpdated += UpdateGesture;
            _fingersScript.AddGesture(_gesture);
            AllowGesturePassThroughAllExceptDeleteButton(true);
        }

        private void CleanUpGesture()
        {
            _fingersScript.RemoveGesture(_gesture);
            _gesture.Dispose();
            _gesture.StateUpdated -= UpdateGesture;
            AllowGesturePassThroughAllExceptDeleteButton(false);
        }

        private void AllowGesturePassThroughAllExceptDeleteButton(bool allow)
        {
            if (allow)
            {
                _denyPassThroughTypes = new HashSet<Type>(_fingersScript.ComponentTypesToDenyPassThrough);
                _fingersScript.ComponentTypesToDenyPassThrough.Clear();
                _fingersScript.ComponentTypesToDenyPassThrough.Add(_deleteLastEventButton.GetType());
            }
            else
            {
                if (_denyPassThroughTypes == null) return;

                foreach (var denyPassThroughType in _denyPassThroughTypes)
                {
                    _fingersScript.ComponentTypesToDenyPassThrough.Add(denyPassThroughType);
                }
                _fingersScript.ComponentTypesToDenyPassThrough.Remove(_deleteLastEventButton.GetType());
            }
        }
        
    }
}