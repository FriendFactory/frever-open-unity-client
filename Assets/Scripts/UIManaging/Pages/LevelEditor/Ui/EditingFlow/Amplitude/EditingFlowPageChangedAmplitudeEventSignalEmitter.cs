using System;
using JetBrains.Annotations;
using Modules.Amplitude.Signals;
using Modules.LevelManaging.Editing;
using Modules.LevelManaging.Editing.LevelManagement;
using UIManaging.Pages.UmaEditorPage.Ui;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.EditingFlow.Amplitude
{
    [UsedImplicitly]
    internal sealed class EditingFlowPageChangedAmplitudeEventSignalEmitter: BaseAmplitudeEventSignalEmitter
    {
        private readonly EditingStepsFlowController _editingStepsFlowController;
        private readonly UmaLevelEditorPanelModel _umaLevelEditorPanelModel;
        private readonly ILevelManager _levelManager;
        private readonly SignalBus _signalBus;

        private EditingFlowType FlowType => _editingStepsFlowController.FlowType;
        private EditingStepModel CurrentStep => _editingStepsFlowController.CurrentStep;

        public EditingFlowPageChangedAmplitudeEventSignalEmitter(SignalBus signalBus,
            EditingStepsFlowController editingStepsFlowController, UmaLevelEditorPanelModel umaLevelEditorPanelModel,
            ILevelManager levelManager) : base(signalBus)
        {
            _editingStepsFlowController = editingStepsFlowController;
            _umaLevelEditorPanelModel = umaLevelEditorPanelModel;
            _levelManager = levelManager;
        }

        public override void Initialize()
        {
            _editingStepsFlowController.Transitioned += OnStepChanged;

            _umaLevelEditorPanelModel.PanelOpened += OnWardrobePanelOpened;
            _umaLevelEditorPanelModel.PanelClosed += OnWardrobePanelClosed;

            // TODO: move to dedicate LE related signal emitter
            _levelManager.TemplateApplied += OnTemplateApplied;
        }

        public override void Dispose()
        {
            _editingStepsFlowController.Transitioned -= OnStepChanged;
            
            _umaLevelEditorPanelModel.PanelOpened -= OnWardrobePanelOpened;
            _umaLevelEditorPanelModel.PanelClosed -= OnWardrobePanelClosed;
            
            _levelManager.TemplateApplied -= OnTemplateApplied;
        }

        private void OnStepChanged(EditingStepModel stepModel)
        {
            // wardrobe panel will be opened in this case, so, it doesn't make sense to send an event
            if (stepModel.State is LevelEditorState.Dressing && !stepModel.IsFirstInFlow) return;
            
            Emit(new EditingFlowStepChangeAmplitudeEvent(stepModel.State, FlowType));
        }

        private void OnWardrobePanelOpened()
        {
            if (CurrentStep is not { State: LevelEditorState.Dressing }) return;
            
            Emit(new EditingFlowStepChangeAmplitudeEvent(LevelEditorState.PurchasableAssetSelection, FlowType));
        }
        
        private void OnWardrobePanelClosed()
        {
            if (CurrentStep is not { State: LevelEditorState.Dressing }) return;
            
            // wardrobe panel is not a separated step, so, in order to track switching to the preview step, we need to send an event here
            Emit(new EditingFlowStepChangeAmplitudeEvent(CurrentStep.State, FlowType));
        }

        private void OnTemplateApplied(ApplyingTemplateArgs applyingTemplateArgs)
        {
            Emit(new EditingFlowTemplateAppliedAmplitudeEvent(applyingTemplateArgs));
        }
    }
}