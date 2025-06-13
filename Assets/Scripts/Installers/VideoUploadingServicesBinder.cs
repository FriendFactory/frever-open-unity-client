using Modules.GalleryVideoManaging;
using UIManaging.Pages.Common.VideoUploading;
using UIManaging.Pages.LevelEditor.Ui;
using UIManaging.Pages.PublishPage;
using Zenject;

namespace Installers
{
    internal static class VideoUploadingServicesBinder
    {
        public static void BindVideoUploadingServices(this DiContainer container)
        {
            container.Bind<IVideoUploader>().To<VideoUploader>().AsSingle();
            container.BindInterfacesAndSelfTo<PublishVideoHelper>().AsSingle();
            container.BindInterfacesAndSelfTo<PublishVideoController>().AsSingle();
            container.BindInterfacesAndSelfTo<UploadGalleryVideoService>().AsSingle();
        }
    }
}
