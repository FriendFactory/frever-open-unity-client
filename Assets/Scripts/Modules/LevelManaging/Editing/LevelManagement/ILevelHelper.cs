using System.Linq;
using System.Threading.Tasks;
using Bridge.Models.Common;
using Bridge.Models.Common.Files;
using Extensions;
using Extensions.ResetEntity;
using JetBrains.Annotations;
using Models;
using Modules.LocalStorage;

namespace Modules.LevelManaging.Editing.LevelManagement
{
    public interface ILevelHelper
    {
        Task PrepareLevelForRemix(Level level);

        Task PrepareLevelForTask(Level level);
        
        Task PrepareLevelForVideoMessage(Level level);

        void InvalidateEventThumbnails(Level level);
    }

    [UsedImplicitly]
    internal sealed class LevelHelper : ILevelHelper
    {
        public async Task PrepareLevelForRemix(Level level)
        {
            level.Description = null;
            PrepareFilesForRemixing(level);
            level.RemixedFromLevelId = level.Id;
            await level.ResetIdsAsync();
            await level.ReplaceEmptyIdsAsync(LocalStorageManager.GetNextLocalId);
        }

        public async Task PrepareLevelForTask(Level level)
        {
            PrepareFilesForTask(level);
            await level.ResetIdsAsync();
            await level.ReplaceEmptyIdsAsync(LocalStorageManager.GetNextLocalId);
            InvalidateEventThumbnails(level);
        }

        public async Task PrepareLevelForVideoMessage(Level level)
        {
            PrepareFilesForVideoMessage(level);
            await level.ResetIdsAsync();
            await level.ReplaceEmptyIdsAsync(LocalStorageManager.GetNextLocalId);
            InvalidateEventThumbnails(level);
        }

        public void InvalidateEventThumbnails(Level level)
        {
            foreach (var ev in level.Event)
            {
                ev.HasActualThumbnail = false;
            }
        }

        private static void PrepareFilesForRemixing(Level level)
        {
            PrepareAssetFilesForCopying(level);
            PrepareEventThumbnailsForCopying(level);
        }

        private static void PrepareFilesForTask(Level level)
        {
            PrepareAssetFilesForCopying(level);
            PrepareEventThumbnailsForCopying(level);
        }

        private static void PrepareFilesForVideoMessage(Level level)
        {
            PrepareCameraAnimationForCopying(level);
            PrepareCharacterFaceVoiceForCopying(level);
            PrepareSetLocationPhotoAndVideoForCopying(level);
        }
        
        private static void PrepareAssetFilesForCopying(Level level)
        {
            PrepareCameraAnimationForCopying(level);
            PrepareCharacterFaceVoiceForCopying(level);
            PrepareSetLocationPhotoAndVideoForCopying(level);
        }

        private static void PrepareSetLocationPhotoAndVideoForCopying(Level level)
        {
            var setLocationControllers = level.Event.SelectMany(x => x.SetLocationController);
            var videoClips = setLocationControllers.Where(x => x.VideoClip != null).Select(x => x.VideoClip)
                                                   .DistinctBy(x => x.Id).ToArray();
            PrepareMainFileForCopying(videoClips);

            var photos = setLocationControllers.Where(x => x.Photo != null).Select(x => x.Photo).DistinctBy(x => x.Id)
                                               .ToArray();
            PrepareMainFileForCopying(photos);
        }

        private static void PrepareCharacterFaceVoiceForCopying(Level level)
        {
            var characterFaceVoice = level.Event.SelectMany(x => x.CharacterController)
                                          .SelectMany(x => x.CharacterControllerFaceVoice);
            var faceAnimations = characterFaceVoice.Where(x => x.FaceAnimation != null).Select(x => x.FaceAnimation)
                                                   .DistinctBy(x => x.Id).ToArray();
            PrepareMainFileForCopying(faceAnimations);

            var voiceTracks = characterFaceVoice.Where(x => x.VoiceTrack != null).Select(x => x.VoiceTrack).ToArray();
            PrepareMainFileForCopying(voiceTracks);
        }

        private static void PrepareCameraAnimationForCopying(Level level)
        {
            var cameraAnimations = level.Event.SelectMany(x => x.CameraController)
                                        .Where(x => x.CameraAnimation != null).Select(x => x.CameraAnimation).ToArray();
            PrepareMainFileForCopying(cameraAnimations);
        }

        private static void PrepareEventThumbnailsForCopying(Level level)
        {
            foreach (var ev in level.Event)
            {
                var copyingThumbnails = ev.Files.Select(x => new FileInfo(ev, x)).ToArray();
                ev.Files.Clear();
                ev.Files.AddRange(copyingThumbnails);
            }
        }

        private static void PrepareMainFileForCopying<T>(T[] entities) where T : IFilesAttachedEntity
        {
            foreach (var filesAttachedEntity in entities)
            {
                PrepareMainFileForCopying(filesAttachedEntity);
            }
        }
        
        private static void PrepareMainFileForCopying<T>(T entity) where T : IFilesAttachedEntity
        {
            var fileInfo = entity.Files.First();
            entity.Files.Clear();
            var copyingFileInfo = new FileInfo(entity, fileInfo);
            entity.Files.Add(copyingFileInfo);
        }
    }
}