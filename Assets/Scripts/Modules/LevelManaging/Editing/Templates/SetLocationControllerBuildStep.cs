using System.Collections.Generic;
using System.Linq;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.Common.Files;
using Extensions;
using Models;

namespace Modules.LevelManaging.Editing.Templates
{
    internal sealed class SetLocationControllerBuildStep: EventBuildStep
    {
        protected override void RunInternal()
        {
            AddControllerIfNotExist();
            ApplyAssets();
            ApplyControllerSettings();
        }

        private void AddControllerIfNotExist()
        {
            if (Destination.GetSetLocationController() != null) return;
            Destination.SetSetLocationController(new SetLocationController());
        }
        
        private void ApplyAssets()
        {
            Destination.SetSetLocation(Template.GetSetLocation());
            CopyPhoto(Destination, Template.GetPhoto());
            CopyVideoClip(Destination, Template.GetVideo());
        }

        private void ApplyControllerSettings()
        {
            var templateController = Template.GetSetLocationController();
            var destController = Destination.GetSetLocationController();
            
            destController.WeatherId = templateController.WeatherId;
            destController.ActivationCue = templateController.ActivationCue;
            destController.EndCue = templateController.EndCue; 
            destController.VideoActivationCue = templateController.VideoActivationCue;
            destController.VideoEndCue = templateController.VideoEndCue;
            destController.TimeOfDay = templateController.TimeOfDay;
            destController.TimelapseSpeed = templateController.TimelapseSpeed;
        }
        
        private void CopyPhoto(Event ev, PhotoFullInfo origin)
        {
            if (origin == null) return;
            var photoFileInfo = origin.Files.First();

            var copiedPhoto = new PhotoFullInfo
            {
                ResolutionHeight = origin.ResolutionHeight,
                ResolutionWidth = origin.ResolutionWidth,
                Files = new List<FileInfo> {new FileInfo(origin, photoFileInfo)}
            };
            ev.SetPhoto(copiedPhoto);
        }
        
        private void CopyVideoClip(Event ev, VideoClipFullInfo origin)
        {
            if (origin == null) return;
            var videoFileInfo = origin.Files.First();

            var copiedVideo = new VideoClipFullInfo
            {
                ResolutionHeight = origin.ResolutionHeight,
                ResolutionWidth = origin.ResolutionWidth,
                Duration = origin.Duration,
                Files = new List<FileInfo> {new FileInfo(origin, videoFileInfo)}
            };
            ev.SetVideo(copiedVideo);
        }
    }
}