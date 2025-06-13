using System.Linq;
using Bridge.Models.ClientServer.Assets;
using Models;

namespace Extensions
{
    public static class CameraControllerExtensions
    {
        public static void SetAnimation(this CameraController cameraController, CameraAnimationFullInfo animation)
        {
            cameraController.CameraAnimation = animation;
            cameraController.CameraAnimationId = animation.Id;
        }
    }
    
    public static class CameraAnimationExtensions
    {
        public static string GetAnimationFilePath(this CameraAnimationFullInfo cameraAnimation)
        {
            return cameraAnimation.Files.First().FilePath;
        }
    }
}