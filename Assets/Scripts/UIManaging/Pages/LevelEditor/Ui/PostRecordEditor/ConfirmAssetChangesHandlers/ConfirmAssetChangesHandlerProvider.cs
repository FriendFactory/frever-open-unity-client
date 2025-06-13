using System.Linq;
using Extensions;
using Modules.AssetsManaging.UncompressedBundles;
using Modules.CameraSystem.CameraSystemCore;
using Modules.LevelManaging.Editing.LevelManagement;
using UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.EventStateChecker;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.ConfirmAssetChangesHandlers
{
    internal sealed class ConfirmAssetChangesHandlerProvider
    {
        private readonly ConfirmAssetChangesHandler[] _confirmAssetChangesHandlers;

        public ConfirmAssetChangesHandlerProvider(CameraAnimationGenerator cameraAnimGenerator, PostRecordEditorPageModel editorPageModel,
            UncompressedBundlesManager uncompressedBundlesManager, ICameraSystem cameraSystem, ILevelManager levelManager, EventSettingsStateChecker eventSettingsStateChecker)
        {
            _confirmAssetChangesHandlers = new ConfirmAssetChangesHandler[]
            {
                new GenerateCameraAnimOnConfirmAssetChangesHandler(DbModelType.BodyAnimation, cameraAnimGenerator),
                new ConfirmSetLocationChangesHandler(DbModelType.SetLocation, cameraAnimGenerator, uncompressedBundlesManager, editorPageModel),
                new ConfirmCameraChangesHandler(DbModelType.CameraAnimation, cameraAnimGenerator, cameraSystem, levelManager, eventSettingsStateChecker)
            };
        }

        public ConfirmAssetChangesHandler GetHandler(DbModelType type)
        {
            return _confirmAssetChangesHandlers.FirstOrDefault(x => x.Type == type);
        }
    }
}
