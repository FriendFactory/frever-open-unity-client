using System;
using Bridge.Models.ClientServer.Assets;
using Models;
using UnityEngine;

namespace Extensions
{
    public static class CharacterControllerFaceVoiceExtensions
    {
        public static void SetFaceAnimation(this CharacterControllerFaceVoice faceVoiceController, FaceAnimationFullInfo faceAnimation)
        {
            if (faceVoiceController == null)
            {
                Debug.LogException(new Exception($"{nameof(faceVoiceController)} is null!"));
                return;
            }

            faceVoiceController.FaceAnimation = faceAnimation;

            if (faceAnimation == null) faceVoiceController.FaceAnimationId = null;
            else faceVoiceController.FaceAnimationId = faceAnimation.Id;
        }

        public static void SetVoiceFilter(this CharacterControllerFaceVoice faceVoiceController, VoiceFilterFullInfo voiceFilter)
        {
            if (faceVoiceController == null) throw new ArgumentNullException(nameof(faceVoiceController));

            faceVoiceController.VoiceFilter = voiceFilter;
            faceVoiceController.VoiceFilterId = voiceFilter?.Id;
        }

        public static VoiceFilterFullInfo GetVoiceFilter(this CharacterControllerFaceVoice faceVoiceController)
        {
            if (faceVoiceController == null) throw new ArgumentNullException(nameof(faceVoiceController));

            return faceVoiceController.VoiceFilter;
        }

        public static void SetVoiceTrack(this CharacterControllerFaceVoice faceVoiceController, VoiceTrackFullInfo track)
        {
            faceVoiceController.VoiceTrack = track;
            faceVoiceController.VoiceTrackId = track?.Id;
        }
    }
}
