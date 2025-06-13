using Modules.FaceAndVoice.Face.Core;
using Modules.FaceAndVoice.Face.Recording.Core;
using Zenject;

namespace Installers
{
    internal static class FaceAnimationServicesBinder
    {
        public static void BindFaceAnimationServices(this DiContainer container)
        {
            container.Bind<FaceAnimationConverter>().AsSingle();
            container.Bind<FaceBlendShapeMap>().AsSingle();
            container.BindInterfacesAndSelfTo<FaceAnimRecorder>().FromNewComponentOnNewGameObject().AsSingle();
        }
    }
}