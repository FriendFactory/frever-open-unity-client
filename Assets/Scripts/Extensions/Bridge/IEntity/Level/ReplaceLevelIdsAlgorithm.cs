using System.Linq;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.ClientServer.Level.Full;
using Models;

namespace Extensions
{
    internal sealed class ReplaceLevelIdsAlgorithm : ReplaceIdsAlgorithm<Level>
    {
        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public ReplaceLevelIdsAlgorithm()
        {
            ChildTypesMustBeReplaced = new[]
            {
                typeof(FaceAnimationFullInfo),
                typeof(VoiceTrackFullInfo),
                typeof(CameraAnimationFullInfo),
                typeof(CharacterControllerFaceVoice),
                typeof(CharacterController),
                typeof(CameraController),
                typeof(VfxController),
                typeof(SetLocationController),
                typeof(MusicController),
                typeof(CharacterControllerBodyAnimation),
                typeof(Event),
                typeof(Level),
                typeof(CameraFilterController),
                typeof(VideoClipFullInfo),
                typeof(PhotoFullInfo),
                typeof(CaptionFullInfo)
            };

            FkPropertiesNames = ChildTypesMustBeReplaced.Select(x => x.Name + ID_FIELD_NAME).ToArray();
        }
    }
}