using System;
using System.Collections.Generic;
using System.Linq;
using Bridge.Models.ClientServer.Assets;
using Models;

namespace Extensions
{
    /// <summary>
    ///     SetLocation related
    /// </summary>
    public static partial class EventExtensions
    {
        public static bool HasSameSetLocation(this Event ev1, Event ev2)
        {
            ThrowExceptionIfEventIsNull(ev1);
            if (ev2 == null) return false;
            return ev1.GetSetLocationController().SetLocation.Id == ev2.GetSetLocationController().SetLocation.Id;
        }

        public static CharacterSpawnPositionInfo CurrentCharacterSpawnPosition(this Event ev)
        {
            var spawnPositions = ev.GetSpawnPositions();
            return spawnPositions.FirstOrDefault(x => x.Id == ev.CharacterSpawnPositionId) 
                ?? spawnPositions.First();
        }
        
        public static ICollection<CharacterSpawnPositionInfo> GetSpawnPositions(this Event ev)
        {
            if (ev == null) throw new InvalidOperationException(nameof(ev));
            return ev.GetSetLocation().GetSpawnPositions();
        }

        public static void SetSetLocationController(this Event ev, SetLocationController controller)
        {
            ThrowExceptionIfEventIsNull(ev);
            if (ev.SetLocationController == null)
            {
                ev.SetLocationController = new List<SetLocationController>();
            }
            else
            {
                ev.SetLocationController.Clear();
            }

            ev.SetLocationController.Add(controller);
        }

        public static SetLocationFullInfo GetSetLocation(this Event ev)
        {
            if (ev == null) throw new ArgumentNullException(nameof(ev));
            return ev.GetSetLocationController().SetLocation;
        }

        public static SetLocationController GetSetLocationController(this Event ev)
        {
            ThrowExceptionIfEventIsNull(ev);
            return ev.SetLocationController?.FirstOrDefault();
        }

        public static void ResetSetLocationCue(this Event ev)
        {
            ev.GetSetLocationController().ActivationCue = 0;
        }

        public static void SetSetLocation(this Event ev, SetLocationFullInfo setLocation)
        {
            ThrowExceptionIfEventIsNull(ev);
            var setLocationController = ev.GetSetLocationController();
            if (setLocationController == null)
            {
                setLocationController = new SetLocationController();
                ev.SetSetLocationController(setLocationController);
            }
            setLocationController.SetLocation = setLocation;
            setLocationController.SetLocationId = setLocation.Id;
        }

        public static void SetPhoto(this Event ev, PhotoFullInfo photo)
        {
            ThrowExceptionIfEventIsNull(ev);
            var setLocationController = ev.GetSetLocationController();
            setLocationController.Photo = photo;
            setLocationController.PhotoId = photo?.Id;
        }

        public static PhotoFullInfo GetPhoto(this Event ev)
        {
            ThrowExceptionIfEventIsNull(ev);
            return ev.GetSetLocationController()?.Photo;
        }
        
        public static SetLocationBackground GetSetLocationBackground(this Event ev)
        {
            ThrowExceptionIfEventIsNull(ev);
            return ev.GetSetLocationController()?.SetLocationBackground;
        }
        
        public static long? GetSetLocationBackgroundId(this Event ev)
        {
            ThrowExceptionIfEventIsNull(ev);
            return ev.GetSetLocationBackground()?.Id;
        }

        public static void SetVideo(this Event ev, VideoClipFullInfo video)
        {
            ThrowExceptionIfEventIsNull(ev);
            var setLocationController = ev.GetSetLocationController();
            setLocationController.VideoClip = video;
            setLocationController.VideoClipId = video?.Id;
        }

        public static VideoClipFullInfo GetVideo(this Event ev)
        {
            ThrowExceptionIfEventIsNull(ev);
            return ev.GetSetLocationController()?.VideoClip;
        }
        
        public static SetLocationBackground GetFreverBackground(this Event ev)
        {
            ThrowExceptionIfEventIsNull(ev);
            return ev.GetSetLocationController()?.SetLocationBackground;
        }
        
        public static long? GetFreverBackgroundId(this Event ev)
        {
            ThrowExceptionIfEventIsNull(ev);
            return ev.GetSetLocationController()?.SetLocationBackgroundId;
        }
        
        public static void SetFreverBackground(this Event ev, SetLocationBackground background)
        {
            ThrowExceptionIfEventIsNull(ev);
            var setLocationController = ev.GetSetLocationController();
            setLocationController.SetLocationBackground = background;
            setLocationController.SetLocationBackgroundId = background?.Id;
        }

        public static void ResetSetLocationAttachedMedia(this Event ev)
        {
            ThrowExceptionIfEventIsNull(ev);
            ev.SetFreverBackground(null);
            ev.SetPhoto(null);
            ev.SetVideo(null);
        }
        
        public static bool HasVideo(this Event ev)
        {
            ThrowExceptionIfEventIsNull(ev);
            return ev.GetVideo() != null;
        }

        public static bool HasSameSetLocationVideo(this Event target, Event compare)
        {
            ThrowExceptionIfEventIsNull(target);
            var targetVideo = target.GetVideo();
            if (targetVideo == null) return false;
            var compareVideo = compare?.GetVideo();
            if (compareVideo == null) return false;
            return targetVideo.Id == compareVideo.Id;
        }
        
        public static long GetSetLocationId(this Event ev)
        {
            if (ev == null) throw new ArgumentNullException(nameof(ev));
            return ev.GetSetLocationController().SetLocationId;
        }
        
        public static SetLocationBundleInfo GetSetLocationBundle(this Event ev)
        {
            if (ev == null) throw new ArgumentNullException(nameof(ev));
            return ev.GetSetLocation().SetLocationBundle;
        }

        public static CharacterSpawnPositionInfo GetTargetSpawnPosition(this Event ev)
        {
            if (ev == null) throw new ArgumentNullException(nameof(ev));
            return ev.GetSetLocation().GetSpawnPosition(ev.CharacterSpawnPositionId);
        }
    }
}