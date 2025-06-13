using Common;
using JetBrains.Annotations;
using Modules.TempSaves.Manager;

namespace UIManaging.Pages.Feed.Events.VideoViews
{
    [UsedImplicitly]
    internal class VideoViewsAmplitudeEventFileHandler
    {
        private const string FILE_NAME = "UnsentVideoViewsAmplitudeEvent.txt";
        
        private static readonly string FILE_PATH = $"{Constants.FileDefaultPaths.SAVE_FOLDER}/{FILE_NAME}";
        
        private readonly TempFileManager _tempFileManager;

        public VideoViewsAmplitudeEventFileHandler(TempFileManager tempFileManager)
        {
            _tempFileManager = tempFileManager;
        }

        public void Save(VideoViewsAmplitudeEvent unsentEvent)
        {
            _tempFileManager.SaveDataLocally(unsentEvent, FILE_PATH);
        }

        public bool TryLoad(out VideoViewsAmplitudeEvent unsentEvent)
        {
            unsentEvent = _tempFileManager.GetData<VideoViewsAmplitudeEvent>(FILE_PATH);

            if (_tempFileManager.FileExists(FILE_PATH))
            {
                _tempFileManager.RemoveTempFile(FILE_PATH);
            }
            
            return unsentEvent != null;
        }
    }
}