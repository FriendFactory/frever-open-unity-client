using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.ClientServer.Level.Full;
using Bridge.Models.ClientServer.StartPack.Metadata;
using Bridge.Models.Common;
using Bridge.Services._7Digital.Models.TrackModels;
using Models;
using Newtonsoft.Json;

namespace Extensions
{
    public static class DbModelExtensions
    {
        private static readonly Dictionary<Type, DbModelType> MODEL_TYPE = new Dictionary<Type, DbModelType> 
        {
            { typeof(BodyAnimationCategory), DbModelType.AnimCategory },
            { typeof(BodyAnimationInfo), DbModelType.BodyAnimation },
            { typeof(BodyAnimationFullInfo), DbModelType.BodyAnimation },
            { typeof(CharacterInfo), DbModelType.Character },
            { typeof(CharacterFullInfo), DbModelType.Character },
            { typeof(Gender), DbModelType.Gender },
            { typeof(CameraAnimationFullInfo), DbModelType.CameraAnimation },
            { typeof(CameraAnimationTemplate), DbModelType.CameraAnimationTemplate },
            { typeof(CharacterController), DbModelType.CharacterController },
            { typeof(CharacterSpawnPositionInfo), DbModelType.CharacterSpawnPosition },
            { typeof(FaceAnimationFullInfo), DbModelType.FaceAnimation },
            { typeof(Event), DbModelType.Event },
            { typeof(Level), DbModelType.Level },
            { typeof(SongInfo), DbModelType.Song },
            { typeof(SongFullInfo), DbModelType.Song },
            { typeof(VfxInfo), DbModelType.Vfx },
            { typeof(VfxFullInfo), DbModelType.Vfx },
            { typeof(VfxController), DbModelType.VfxController },
            { typeof(VoiceFilterFullInfo), DbModelType.VoiceFilter },
            { typeof(VoiceTrackFullInfo), DbModelType.VoiceTrack },
            { typeof(SetLocationFullInfo), DbModelType.SetLocation },
            { typeof(UserSoundFullInfo), DbModelType.UserSound },
            { typeof(FavouriteMusicInfo), DbModelType.UserSound },
            { typeof(CharacterSpawnPositionFormation), DbModelType.SpawnFormation },
            { typeof(OutfitShortInfo), DbModelType.Outfit },
            { typeof(OutfitFullInfo), DbModelType.Outfit },
            { typeof(CameraFilterInfo), DbModelType.CameraFilter},
            { typeof(CameraFilterVariantInfo), DbModelType.CameraFilterVariant },
            { typeof(VideoClipFullInfo), DbModelType.VideoClip },
            { typeof(PhotoFullInfo), DbModelType.UserPhoto },
            { typeof(CaptionFullInfo), DbModelType.Caption },
            { typeof(UmaBundleFullInfo), DbModelType.UmaBundle},
            { typeof(SetLocationBundleInfo), DbModelType.SetLocationBundle},
            { typeof(CharacterControllerBodyAnimation), DbModelType.CharacterControllerBodyAnimation},
            { typeof(MusicController), DbModelType.MusicController},
            { typeof(CharacterControllerFaceVoice), DbModelType.CharacterControllerFaceVoice},
            { typeof(SetLocationController), DbModelType.SetLocationController},
            { typeof(CameraController), DbModelType.CameraController},
            { typeof(CameraFilterController), DbModelType.CameraFilterController},
            { typeof(ExternalTrackInfo), DbModelType.ExternalTrack},
            { typeof(SetLocationBackground), DbModelType.SetLocationBackground}
        };

        private static readonly DbModelType[] AUDIO_TYPES = {DbModelType.Song, DbModelType.VoiceTrack, DbModelType.UserSound, DbModelType.ExternalTrack};

        public static DbModelType GetModelType<T>(this T model) where T : IEntity
        {
            var type = model != null? model.GetType() : typeof(T);
            return GetModelType(type);
        }
        
        public static DbModelType GetModelType<T>() where T : IEntity
        {
            return MODEL_TYPE.TryGetValue(typeof(T), out var value) ? value : default;
        }

        public static DbModelType GetModelType(Type type)
        {
            return MODEL_TYPE.TryGetValue(type, out var value) 
                ? value 
                : throw new ArgumentException($"There is no registered {nameof(DbModelType)} for {type.Name}");
        }

        public static bool IsAudioType(this DbModelType modelType)
        {
            return AUDIO_TYPES.Contains(modelType);
        }

        public static T Clone<T>(this T modelToClone) where T : IEntity
        {
            var jsonCopy = JsonConvert.SerializeObject(modelToClone);
            return JsonConvert.DeserializeObject<T>(jsonCopy);
        }
        
        public static Task<T> CloneAsync<T>(this T modelToClone) where T : IEntity
        {
            return Task.Run(() => modelToClone.Clone());
        }

        public static bool Compare<T>(this T model, T other) where T : IEntity
        {
            if (model == null || other == null) return false;

            if (model.GetType() != other.GetType()) return false;

            if (model.Id == 0 && other.Id == 0)
            {
                return model.GetHashCode() == other.GetHashCode();
            }
                
            return model.Id == other.Id;
        }

        public static string GetVersion(this IMainFileContainable target)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));

            return target.Files.IsNullOrEmpty() ? null : target.Files.First().Version;
        }
    }
}