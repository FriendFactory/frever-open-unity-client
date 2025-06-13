using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abstract;
using Bridge;
using Common.UserBalance;
using Extensions;
using JetBrains.Annotations;
using ModestTree;
using Modules.LevelManaging.Editing.LevelManagement;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.Pages.LevelEditor.Ui.EditingFlow.ShoppingCart;
using UIManaging.Pages.LevelEditor.Ui.Permissions;
using UIManaging.Pages.UmaEditorPage.Ui;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.EditingFlow
{
    internal sealed class EditingStepsFlowController: MonoBehaviour
    {
        [SerializeField] private List<BaseEditingStepView> _views;
        [SerializeField] private ShoppingCartHelper _shoppingCartHelper;
        [SerializeField] private UserBalanceView _userBalanceView;

        private Dictionary<LevelEditorState, EditingStepModel> _models;
        private Dictionary<LevelEditorState, BaseEditingStepPresenter> _presenters;

        private LevelEditorPageModel _levelEditorPageModel;
        private EditingFlowTransitionManager _transitionManager;
        private LevelEditorState[] _availableStates;
        private List<BaseEditingStepView> _availableViews;

        private EditingStepModel _currentStep;
        private int _currentStepIndex = -1;
        private int _previousStepIndex = -1;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        private IEnumerable<BaseEditingStepView> AvailableViews => _availableViews;

        public bool IsInitialized { get; private set; }

        private int CurrentStepIndex
        {
            get => _currentStepIndex;
            set
            {
                if (value == _currentStepIndex) return;
                
                _currentStepIndex = Mathf.Clamp(value, 0, AvailableViews.Count() - 1);
            }
        }
        
        private int PreviousStepIndex
        {
            get => _previousStepIndex;
            set
            {
                if (value == _previousStepIndex) return;

                _previousStepIndex = Mathf.Clamp(value, 0, _views.Count - 1);
            }
        }

        public EditingStepModel CurrentStep
        {
            get => _currentStep;
            private set
            {
                if (_currentStep != null)
                {
                    _currentStep.MoveBack -= MovePreviousStep;
                    _currentStep.MoveNext -= MoveNextStep;
                    _currentStep.MoveToDefault -= MoveToDefaultState;
                }

                if (_currentStep == value) return;

                _currentStep = value;

                _currentStep.MoveBack += MovePreviousStep;
                _currentStep.MoveNext += MoveNextStep;
                _currentStep.MoveToDefault += MoveToDefaultState;
            }
        }
        
        public EditingFlowType FlowType { get; private set; }

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------
        
        public event Action ExitRequested;
        public event Action<EditingStepModel> Transitioned;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        [Inject, UsedImplicitly]
        private void Construct(LevelEditorPageModel levelEditorPageModel, IBridge bridge, ILevelManager levelManager,
            LocalUserDataHolder userData, UmaLevelEditorPanelModel umaLevelEditorPanelModel, CameraPermissionHandler cameraPermissionHandler)
        {
            _levelEditorPageModel = levelEditorPageModel;
            
            _models = new Dictionary<LevelEditorState, EditingStepModel>()
            {
                { LevelEditorState.TemplateSetup, new EditingStepModel(LevelEditorState.TemplateSetup) },
                { LevelEditorState.Dressing, new EditingStepModel(LevelEditorState.Dressing) },
                { LevelEditorState.Default, new EditingStepModel(LevelEditorState.Default) },
            };

            _presenters = new Dictionary<LevelEditorState, BaseEditingStepPresenter>() 
            {
                { LevelEditorState.TemplateSetup, new SetStageStepPresenter(bridge, levelManager) },
                { LevelEditorState.Dressing, new DressUpStepPresenter(levelManager, _shoppingCartHelper, _userBalanceView, userData, umaLevelEditorPanelModel) },
                { LevelEditorState.Default, new RecordingStepPresenter(umaLevelEditorPanelModel, levelManager, cameraPermissionHandler) }
            };
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public async Task InitializeAsync(EditingFlowModel editingFlowModel)
        {
            if (IsInitialized)
            {
                CleanUp();
            }
            
            IsInitialized = true;
            FlowType = editingFlowModel.InitialState is LevelEditorState.TemplateSetup
                ? EditingFlowType.CreateChallenge
                : EditingFlowType.JoinChallenge;
            
            _availableStates = editingFlowModel.Steps;
            _availableViews = _views.Where(x => _availableStates.Contains(x.State)).ToList();
            
            foreach (var view in _views)
            {
                var presenter = _presenters[view.State];
                var model = _models[view.State];
                view.Initialize(model);
                presenter.Initialize(model, view);
            }
            
            _transitionManager = new EditingFlowTransitionManager(AvailableViews.ToList());
            
            // need to run async because instant animation is completed only on sequence start
            await _transitionManager.InitializeAsync();
            
            _shoppingCartHelper.Initialize();
            
            Run(editingFlowModel.InitialState);
        }

        public void CleanUp()
        {
            _views.ForEach(view => {
                view.Hide();
                view.CleanUp();
            });
            _presenters.Values.ForEach(presenter => presenter.CleanUp());
            
            _shoppingCartHelper.Cleanup();
            
            if (_currentStep != null)
            {
                _currentStep.MoveBack -= MovePreviousStep;
                _currentStep.MoveNext -= MoveNextStep;
                _currentStep.MoveToDefault -= MoveToDefaultState;

                _currentStep = null;
            }
            
            IsInitialized = false;
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void Run(LevelEditorState initialState)
        {
            CurrentStepIndex = AvailableViews.Select(x=>x.State).ToArray().IndexOf(initialState);
            ActivateCurrentState(instant: true);
        }

        private void Move(bool forward)
        {
            if (CurrentStepIndex == 0 && !forward)
            {
                ExitRequested?.Invoke();
                CurrentStep.IsExiting = true;
                return;
            }
            
            PreviousStepIndex = CurrentStepIndex;
            CurrentStepIndex += forward ? 1 : -1;
            
            ActivateCurrentState(forward);
        }

        private async void ActivateCurrentState(bool forward = true, bool instant = false)
        {
            var view = AvailableViews.ElementAt(CurrentStepIndex);
            CurrentStep = _models[view.State];
            var previousStepState = GetPreviousStepState();
            
            CurrentStep.IsFirstInFlow = CurrentStepIndex == 0;

            // made all transitions instant for now
            await _transitionManager.SwitchAsync(previousStepState, CurrentStep.State, forward, true);
            
            Transitioned?.Invoke(CurrentStep);

            _levelEditorPageModel.ChangeState(CurrentStep.State);

            LevelEditorState GetPreviousStepState() => PreviousStepIndex >= 0
                ? AvailableViews.ElementAt(PreviousStepIndex).State
                : LevelEditorState.None;
        }

        private void MoveNextStep() => Move(true);
        private void MovePreviousStep() => Move(false);
        
        private void MoveToDefaultState()
        {
            _availableStates = new [] { LevelEditorState.Default };
            _availableViews = _views.Where(x => _availableStates.Contains(x.State)).ToList();

            _previousStepIndex = -1;
            
            Run(LevelEditorState.Default);
        }
    }
}