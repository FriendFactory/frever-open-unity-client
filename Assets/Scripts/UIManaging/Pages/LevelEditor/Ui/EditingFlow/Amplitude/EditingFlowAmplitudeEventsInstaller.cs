using UIManaging.Pages.LevelEditor.Ui.EditingFlow.Signals;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.EditingFlow.Amplitude
{
    internal class EditingFlowAmplitudeEventsInstaller: MonoInstaller
    {
        [SerializeField] private EditingStepsFlowController _editingStepsFlowController;
        
        public override void InstallBindings()
        {
            Container.DeclareSignal<AssetSelectionButtonClickedSignal>();
            Container.DeclareSignal<WardrobeSelectionButtonClickedSignal>();

            Container.BindInterfacesAndSelfTo<TemplateSetupStepAmplitudeEventSignalEmitter>().AsSingle();
            Container.BindInterfacesAndSelfTo<DressUpStepAmplitudeEventSignalEmitter>().AsSingle();
            Container.BindInterfacesAndSelfTo<EditingFlowPageChangedAmplitudeEventSignalEmitter>().AsSingle().WithArguments(_editingStepsFlowController);
        }
    }
}