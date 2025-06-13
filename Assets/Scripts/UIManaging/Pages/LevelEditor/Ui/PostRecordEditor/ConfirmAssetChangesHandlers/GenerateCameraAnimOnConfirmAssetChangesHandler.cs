using Extensions;
using Modules.LevelManaging.Editing.LevelManagement;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.ConfirmAssetChangesHandlers
{
    internal class GenerateCameraAnimOnConfirmAssetChangesHandler : ConfirmAssetChangesHandler
    {
        protected readonly ICameraAnimationRegenerator CameraAnimationGenerator;
        public override DbModelType Type { get; }

        public GenerateCameraAnimOnConfirmAssetChangesHandler(DbModelType type, ICameraAnimationRegenerator cameraAnimationGenerator)
        {
            Type = type;  //DWC
            CameraAnimationGenerator = cameraAnimationGenerator;
        }

        public override void Run()
        {
            RegenerateCameraAnimationFile(); //DWC
        }

        protected void RegenerateCameraAnimationFile()
        {
            CameraAnimationGenerator.ReGenerateAnimationForTargetEvent();
        }
    }
}