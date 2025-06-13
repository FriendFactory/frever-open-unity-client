using System.Linq;
using Bridge.Models.ClientServer.Level.Full;
using Extensions;
using JetBrains.Annotations;
using Models;

namespace Common.ModelsMapping
{
    [UsedImplicitly]
    internal sealed class LevelToLevelFullInfoMapper
    {
        public LevelFullInfo Map(Level model)
        {
            var fullInfo = new LevelFullInfo();
            fullInfo.Id = model.Id;
            fullInfo.RemixedFromLevelId = model.RemixedFromLevelId;
            fullInfo.Event = model.Event.Select(Map).ToList();
            fullInfo.Description = model.Description;
            fullInfo.SchoolTaskId = model.SchoolTaskId;
            fullInfo.LevelTypeId = model.LevelTypeId;
            return fullInfo;
        }
        
        private EventFullInfo Map(Event ev)
        {
            var dto = new EventFullInfo();
            dto.Id = ev.Id;
            dto.Length = ev.Length;
            dto.Files = ev.Files;
            dto.TemplateId = ev.TemplateId;
            dto.HasActualThumbnail = ev.HasActualThumbnail;
            dto.CharacterSpawnPositionId = ev.CharacterSpawnPositionId;
            dto.CharacterSpawnPositionFormationId = ev.CharacterSpawnPositionFormationId.Value;
            dto.TargetCharacterSequenceNumber = ev.TargetCharacterSequenceNumber;
            dto.LevelSequence = ev.LevelSequence;
            dto.CameraController = Map(ev.GetCameraController());
            dto.CharacterController = ev.CharacterController.Select(Map).ToList();
            dto.SetLocationController = Map(ev.GetSetLocationController());
            if (ev.HasMusic())
            {
                dto.MusicController = Map(ev.GetMusicController());
            }

            if (ev.HasCaption())
            {
                dto.Caption = ev.Caption.ToList();
            }

            if (ev.HasVfx())
            {
                dto.VfxController = Map(ev.GetVfxController());
            }

            if (ev.HasCameraFilter())
            {
                dto.CameraFilterController = Map(ev.GetCameraFilterController());
            }
            
            return dto;
        }

        private CameraControllerFullInfo Map(CameraController model)
        {
            var dto = new CameraControllerFullInfo();
            dto.Id = model.Id;
            dto.FollowAll = model.FollowAll;
            dto.TemplateSpeed = model.TemplateSpeed;
            dto.EndFocusDistance = model.EndFocusDistance;
            dto.LookAtIndex = model.LookAtIndex;
            dto.StartFocusDistance = model.StartFocusDistance;
            dto.CameraAnimationTemplateId = model.CameraAnimationTemplateId;
            dto.CameraNoiseSettingsIndex = model.CameraNoiseSettingsIndex;
            dto.CameraAnimation = model.CameraAnimation;
            return dto;
        }

        private MusicControllerFullInfo Map(MusicController model)
        {
            var dto = new MusicControllerFullInfo();
            dto.Id = model.Id;
            dto.ActivationCue = model.ActivationCue;
            dto.EndCue = model.EndCue;
            dto.LevelSoundVolume = model.LevelSoundVolume;
            dto.SongId = model.SongId;
            
            if (model.UserSound != null)
            {
                dto.UserSound = model.UserSound;
            }
            
            if (model.ExternalTrackId.HasValue)
            {
                dto.ExternalTrackId = model.ExternalTrackId;
            }
            return dto;
        }

        private CharacterControllerFullInfo Map(CharacterController model)
        {
            var dto = new CharacterControllerFullInfo();
            dto.Id = model.Id;
            dto.CharacterId = model.CharacterId;
            dto.OutfitId = model.OutfitId;
            dto.ControllerSequenceNumber = model.ControllerSequenceNumber;
            dto.CharacterSpawnPositionId = model.CharacterSpawnPositionId;
            dto.BodyAnimation = Map(model.GetBodyAnimationController());
            dto.FaceVoice = Map(model.GetCharacterControllerFaceVoiceController());
            return dto;
        }

        private CharacterControllerBodyAnimationFullInfo Map(CharacterControllerBodyAnimation model)
        {
            var dto = new CharacterControllerBodyAnimationFullInfo();
            dto.Id = model.Id;
            dto.ActivationCue = model.ActivationCue;
            dto.EndCue = model.EndCue;
            dto.BodyAnimationId = model.BodyAnimationId;
            return dto;
        }

        private CharacterControllerFaceVoiceFullInfo Map(CharacterControllerFaceVoice model)
        {
            var dto = new CharacterControllerFaceVoiceFullInfo();
            dto.Id = model.Id;
            dto.VoiceSoundVolume = model.VoiceSoundVolume;
            dto.VoiceFilterId = model.VoiceFilterId;
            dto.FaceAnimation = model.FaceAnimation;
            dto.VoiceTrack = model.VoiceTrack;
            return dto;
        }

        private VfxControllerFullInfo Map(VfxController model)
        {
            var dto = new VfxControllerFullInfo();
            dto.Id = model.Id;
            dto.ActivationCue = model.ActivationCue;
            dto.EndCue = model.EndCue;
            dto.VfxId = model.VfxId;
            return dto;
        }

        private SetLocationControllerFullInfo Map(SetLocationController model)
        {
            var dto = new SetLocationControllerFullInfo();
            dto.Id = model.Id;
            dto.SetLocationId = model.SetLocationId;
            dto.ActivationCue = model.ActivationCue;
            dto.EndCue = model.EndCue;
            dto.VideoActivationCue = model.VideoActivationCue;
            dto.VideoEndCue = model.VideoEndCue;
            dto.VideoSoundVolume = model.VideoSoundVolume;
            dto.TimeOfDay = model.TimeOfDay;
            dto.TimelapseSpeed = model.TimelapseSpeed;
            dto.PictureInPictureSettings = model.PictureInPictureSettings;
            
            if (model.Photo != null)
            {
                dto.Photo = model.Photo;
                dto.Photo.Id = model.Photo.Id;
            }

            if (model.VideoClip != null)
            {
                dto.VideoClip = model.VideoClip;
            }

            if (model.SetLocationBackground != null)
            {
                dto.SetLocationBackgroundId = model.SetLocationBackground.Id;
            }
            
            return dto;
        }
        
        private CameraFilterControllerFullInfo Map(CameraFilterController model)
        {
            var dto = new CameraFilterControllerFullInfo();
            dto.Id = model.Id;
            dto.CameraFilterVariantId = model.CameraFilterVariantId;
            dto.CameraFilterValue = model.CameraFilterValue;
            return dto;
        }
    }
}