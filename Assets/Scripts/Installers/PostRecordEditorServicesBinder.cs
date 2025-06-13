using UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.EventStateChecker;
using UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.TaskCheckListComponents;
using Zenject;

namespace Installers
{
    internal static class PostRecordEditorServicesBinder
    {
        public static void BindPostRecordEditorServices(this DiContainer container)
        {
            container.BindEventStateComparers();
        }
        
        private static void BindEventStateComparers(this DiContainer container)
        {
            container.Bind<EventSettingsStateChecker>().AsTransient();
            container.Bind<TaskEventSettingsComparer>().AsTransient();
            container.Bind<TaskCheckListController>().AsSingle();
            
            container.BindInterfacesAndSelfTo<BodyAnimationStateComparer>().AsTransient();
            container.BindInterfacesAndSelfTo<CameraAnimationTemplateStateComparer>().AsTransient();
            container.BindInterfacesAndSelfTo<CameraFilterStateComparer>().AsTransient();
            container.BindInterfacesAndSelfTo<CameraFilterVariantStateComparer>().AsTransient();
            container.BindInterfacesAndSelfTo<CaptionStateComparer>().AsTransient();
            container.BindInterfacesAndSelfTo<CharacterSpawnFormationStateComparer>().AsTransient();
            container.BindInterfacesAndSelfTo<CharacterSpawnPositionStateComparer>().AsTransient();
            container.BindInterfacesAndSelfTo<CharacterStateComparer>().AsTransient();
            container.BindInterfacesAndSelfTo<OutfitStateComparer>().AsTransient();
            container.BindInterfacesAndSelfTo<SetLocationStateComparer>().AsTransient();
            container.BindInterfacesAndSelfTo<VfxStateComparer>().AsTransient();
        }
    }
}
