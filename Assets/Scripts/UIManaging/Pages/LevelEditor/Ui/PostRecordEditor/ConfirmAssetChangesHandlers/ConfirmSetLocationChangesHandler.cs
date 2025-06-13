using System.Linq;
using Extensions;
using Modules.AssetsManaging.UncompressedBundles;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.ConfirmAssetChangesHandlers
{
    internal sealed class ConfirmSetLocationChangesHandler : GenerateCameraAnimOnConfirmAssetChangesHandler
    {
        private readonly PostRecordEditorPageModel _editorPageModel;
        private readonly UncompressedBundlesManager _uncompressedBundlesManager;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public ConfirmSetLocationChangesHandler(DbModelType type, CameraAnimationGenerator cameraAnimationGenerator, UncompressedBundlesManager uncompressedBundlesManager, PostRecordEditorPageModel editorPageModel) : base(type, cameraAnimationGenerator)
        {
            _uncompressedBundlesManager = uncompressedBundlesManager;
            _editorPageModel = editorPageModel;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public override void Run()
        {
            base.Run();
            DecompressSetLocationBundle();  //DWC is it that we are not doing this in the ConfirmSetLocationChangesHandler?
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void DecompressSetLocationBundle()
        {
            var selectedEvent = _editorPageModel.PostRecordEventsTimelineModel.SelectedEvent;
            var setLocationWasChanged = !_editorPageModel.TargetEventOriginal.HasSameSetLocation(selectedEvent.Event);

            if (!setLocationWasChanged) return;
            
            var setLocationBundle = selectedEvent.Event.GetSetLocationBundle();
            _uncompressedBundlesManager.DecompressBundle(setLocationBundle);
        }
    }
}
