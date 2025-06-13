using UnityEngine;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui
{
    internal sealed class AudioRecordingPanelInstaller: MonoInstaller
    {
        [SerializeField] private AudioRecordingStateTransitionsProvider _transitionsProvider;
        
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<AudioRecordingStateController>().AsSingle().WithArguments(_transitionsProvider);
        }
    }
}