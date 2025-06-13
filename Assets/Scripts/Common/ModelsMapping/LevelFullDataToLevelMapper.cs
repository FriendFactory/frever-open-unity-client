using System.Collections.Generic;
using System.Linq;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.ClientServer.Level.Full;
using Bridge.Services._7Digital.Models.TrackModels;
using Extensions;
using JetBrains.Annotations;
using Models;

namespace Common.ModelsMapping
{
    [UsedImplicitly]
    internal sealed class LevelFullDataToLevelMapper
    {
        public Level Map(LevelFullData model)
        {
            var output = new Level();
            output.Id = model.Level.Id;
            output.Event = MapEvents(model);
            output.Description = model.Level.Description;
            output.SchoolTaskId = model.Level.SchoolTaskId;
            output.RemixedFromLevelId = model.Level.RemixedFromLevelId;
            output.LevelTypeId = model.Level.LevelTypeId;
            return output;
        }
        
        private List<Event> MapEvents(LevelFullData levelFullData)
        {
            var events = new List<Event>();
            foreach (var eventDto in levelFullData.Level.Event)
            {
                var ev = Map(levelFullData, eventDto);
                events.Add(ev);
            }
            return events;
        }

        private Event Map(LevelFullData levelFullData, EventFullInfo eventDto)
        {
            var ev = new Event();
            ev.Id = eventDto.Id;
            ev.CharacterController = Map(eventDto.CharacterController, levelFullData);
            ev.CameraController.Add(Map(eventDto.CameraController));
            ev.SetLocationController.Add(Map(eventDto.SetLocationController, levelFullData.SetLocations, levelFullData.SetLocationBackgrounds));
            ev.CharacterSpawnPositionFormationId = eventDto.CharacterSpawnPositionFormationId;
            ev.CharacterSpawnPositionId = eventDto.CharacterSpawnPositionId;
            ev.Length = eventDto.Length;
            ev.TargetCharacterSequenceNumber = eventDto.TargetCharacterSequenceNumber;
            ev.LevelSequence = eventDto.LevelSequence;
            ev.HasActualThumbnail = eventDto.HasActualThumbnail;
            ev.Files = eventDto.Files;
            ev.TemplateId = eventDto.TemplateId;
            ev.SetCaptions(eventDto.Caption);

            if (eventDto.VfxController != null)
            {
                ev.VfxController.Add(Map(eventDto.VfxController, levelFullData.Vfx));
            }

            if (eventDto.MusicController != null)
            {
                ev.MusicController.Add(Map(eventDto.MusicController, levelFullData.Songs, levelFullData.ExternalSongs));
            }

            if (eventDto.CameraFilterController != null)
            {
                ev.CameraFilterController.Add(Map(eventDto.CameraFilterController, levelFullData.CameraFilters));
            }
            
            return ev;
        }

        private List<CharacterController> Map(List<CharacterControllerFullInfo> dtos, LevelFullData levelFullData)
        {
            var output = new List<CharacterController>();
            foreach (var dto in dtos)
            {
                var c = new CharacterController();
                c.Id = dto.Id;
                c.ControllerSequenceNumber = dto.ControllerSequenceNumber;
                c.CharacterSpawnPositionId = dto.CharacterSpawnPositionId.Value;//todo: FREV-13793 will change dto CharacterSpawnPositionId to not nullable as well
                c.CharacterId = dto.CharacterId;

                if (levelFullData.Characters != null)
                {
                    c.Character = levelFullData.Characters.FirstOrDefault(x => x.Id == dto.CharacterId);
                }
                
                var bodyAnimControl = Map(dto.BodyAnimation, levelFullData.BodyAnimations);
                c.CharacterControllerBodyAnimation.Add(bodyAnimControl);

                var faceVoiceControl = Map(dto.FaceVoice, levelFullData.VoiceFilters);
                c.CharacterControllerFaceVoice.Add(faceVoiceControl);
                LinkOutfits(levelFullData, dto, c);
                
                output.Add(c);
            }
            return output;
        }

        private static void LinkOutfits(LevelFullData levelFullData, CharacterControllerFullInfo dto, CharacterController controller)
        {
            if (!dto.OutfitId.HasValue) return;
           
            controller.OutfitId = dto.OutfitId;
            if (levelFullData.Outfits != null)
            {
                controller.Outfit = levelFullData.Outfits.First(x => x.Id == controller.OutfitId.Value);
            }
        }

        private CharacterControllerBodyAnimation Map(CharacterControllerBodyAnimationFullInfo dto, ICollection<BodyAnimationFullInfo> bodyAnimationFullInfos)
        {
            var controller = new CharacterControllerBodyAnimation
            {
                Id = dto.Id,
                ActivationCue = dto.ActivationCue,
                EndCue = dto.EndCue
            };
            var bodyAnimDto = bodyAnimationFullInfos.First(x => x.Id == dto.BodyAnimationId);
            controller.SetBodyAnimation(Map(bodyAnimDto));
            return controller;
        }

        private BodyAnimationInfo Map(BodyAnimationFullInfo dto)
        {
            return new BodyAnimationInfo
            {
                Id = dto.Id,
                Files = dto.Files,
                Name = dto.Name,
                Locomotion = dto.Locomotion,
                Looping = dto.Looping,
                BodyAnimationCategoryId = dto.BodyAnimationCategoryId,
                HasFaceAnimation = dto.HasFaceAnimation,
                AssetOffer = dto.AssetOffer,
                MovementTypeId = dto.MovementTypeId,
                BodyAnimationAndVfx = dto.BodyAnimationAndVfx,
            };
        }

        private CharacterControllerFaceVoice Map(CharacterControllerFaceVoiceFullInfo dto, ICollection<VoiceFilterFullInfo> voiceFilters)
        {
            var output = new CharacterControllerFaceVoice
            {
                Id = dto.Id,
                VoiceSoundVolume = dto.VoiceSoundVolume,
                VoiceFilterId = dto.VoiceFilterId
            };
            if (dto.VoiceFilterId.HasValue)
            {
                var voiceFilterFullInfo = voiceFilters.First(x => x.Id == dto.VoiceFilterId.Value);
                output.SetVoiceFilter(voiceFilterFullInfo);
            }

            if (dto.FaceAnimation != null)
            {
                output.SetFaceAnimation(dto.FaceAnimation);
            }

            if (dto.VoiceTrack != null)
            {
                output.SetVoiceTrack(dto.VoiceTrack);
            }
            return output;
        }

        private CameraController Map(CameraControllerFullInfo dto)
        {
            var controller = new CameraController
            {
                Id = dto.Id,
                CameraAnimationTemplateId = dto.CameraAnimationTemplateId,
                TemplateSpeed = dto.TemplateSpeed,
                StartFocusDistance = dto.StartFocusDistance,
                EndFocusDistance = dto.EndFocusDistance,
                CameraNoiseSettingsIndex = dto.CameraNoiseSettingsIndex,
                LookAtIndex = dto.LookAtIndex,
                FollowAll = dto.FollowAll
            };
            controller.SetAnimation(dto.CameraAnimation);
            return controller;
        }

        private VfxController Map(VfxControllerFullInfo dto, ICollection<VfxFullInfo> vfxes)
        {
            var controller = new VfxController();
            controller.Id = dto.Id;
            controller.ActivationCue = dto.ActivationCue;
            controller.EndCue = dto.EndCue;
            controller.VfxId = dto.VfxId;
            controller.Vfx = Map(vfxes.First(x => x.Id == dto.VfxId));
            return controller;
        }

        private VfxInfo Map(VfxFullInfo model)
        {
            return new VfxInfo
            {
                Id = model.Id,
                VfxCategoryId = model.VfxCategoryId,
                Looping = model.Looping,
                Name = model.Name,
                Files = model.Files,
                AssetOffer = model.AssetOffer,
                AnchorPoint = model.AnchorPoint,
                FollowRotation = model.FollowRotation,
                Adjustments = model.Adjustments,
                BodyAnimationAndVfx = model.BodyAnimationAndVfx,
            };
        }
        
        private MusicController Map(MusicControllerFullInfo dto, ICollection<SongFullInfo> songs, ICollection<ExternalSongInfo> externalSongs)
        {
            var controller = new MusicController
            {
                Id = dto.Id,
                ActivationCue = dto.ActivationCue,
                EndCue = dto.EndCue,
                LevelSoundVolume = dto.LevelSoundVolume
            };

            if (dto.SongId.HasValue)
            {
                var song = Map(songs.First(x => x.Id == dto.SongId.Value));
                controller.SetSong(song);
            }
            
            if (dto.UserSound != null)
            {
                controller.SetUserSound(dto.UserSound);
            }

            if (dto.ExternalTrackId.HasValue)
            {
                var externalSong = Map(externalSongs.First(x => x.ExternalTrackId == dto.ExternalTrackId));
                controller.SetExternalTrack(externalSong);
            }

            return controller;
        }

        private ExternalTrackInfo Map(ExternalSongInfo externalSong)
        {
            return new ExternalTrackInfo 
            {
                Id = externalSong.ExternalTrackId,
                Duration = externalSong.Duration,
                ArtistName = externalSong.ArtistName,
                Title = externalSong.SongName
            };
        }

        private SongInfo Map(SongFullInfo dto)
        {
            return new SongInfo
            {
                Id = dto.Id,
                Name = dto.Name,
                Files = dto.Files,
                GenreId = dto.GenreId,
                Duration = dto.Duration,
                Artist = new ArtistInfo
                {
                    Id = dto.ArtistId,
                    Name = dto.ArtistName
                }
            };
        }
        
        private SetLocationController Map(SetLocationControllerFullInfo dto, ICollection<SetLocationFullInfo> setLocations, ICollection<SetLocationBackground> backgrounds)
        {
            var controller = new SetLocationController();
            controller.Id = dto.Id;
            controller.ActivationCue = dto.ActivationCue;
            controller.EndCue = dto.EndCue;
            controller.TimeOfDay = dto.TimeOfDay;
            controller.TimelapseSpeed = dto.TimelapseSpeed;
            controller.SetLocationId = dto.SetLocationId;
            controller.SetLocation = setLocations.First(x => x.Id == dto.SetLocationId);
            controller.PictureInPictureSettings = dto.PictureInPictureSettings;
            
            if (dto.VideoClip != null)
            {
                controller.VideoActivationCue = dto.VideoActivationCue;
                controller.VideoEndCue = dto.VideoEndCue;
                controller.VideoSoundVolume = dto.VideoSoundVolume;
                controller.VideoClipId = dto.VideoClip.Id;
                controller.VideoClip = dto.VideoClip;
            }

            if (dto.Photo != null)
            {
                controller.PhotoId = dto.Photo.Id;
                controller.Photo = dto.Photo;
            }

            if (dto.SetLocationBackgroundId != null)
            {
                controller.SetLocationBackgroundId = dto.SetLocationBackgroundId;
                controller.SetLocationBackground = backgrounds.First(x => x.Id == dto.SetLocationBackgroundId);
            }
            
            return controller;
        }

        private CameraFilterController Map(CameraFilterControllerFullInfo dto, ICollection<CameraFilterInfo> filters)
        {
            var controller = new CameraFilterController
            {
                Id = dto.Id,
                CameraFilterValue = dto.CameraFilterValue
            };

            var filterAssetDto = filters.First(x => x.Id == dto.CameraFilterId);
            controller.SetCameraFilterVariant(filterAssetDto, dto.CameraFilterVariantId);
            return controller;
        }
    }
}