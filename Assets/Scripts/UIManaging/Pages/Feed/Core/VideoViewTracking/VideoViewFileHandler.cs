using System.Globalization;
using System.IO;
using System.Text;
using Bridge.VideoServer;
using Newtonsoft.Json;
using UnityEngine;

namespace UIManaging.Pages.Feed.Ui.Feed
{
    public sealed class VideoViewFileHandler
    {
        private readonly string _cacheFolder;
        private readonly string _fileName = "VideoViews.txt";
        private StringBuilder _builder;
        private CultureInfo _cultureInfo;

        public VideoViewFileHandler()
        {
            _cacheFolder = Path.Combine(Application.persistentDataPath, "Save/FeedVideoViews");
        }

        public bool HasDataFromPreviousSession()
        {
            var fileInfo = new FileInfo(GetFilePath());
            return fileInfo.Exists;
        }
        
        public VideoView[] LoadPreviousSessionNotSentData()
        {
            var fileDataJson = File.ReadAllText(GetFilePath());
            return ConvertFromJson(fileDataJson);
        }

        public void DeleteSavedDataFromPreviousSession()
        {
            if (HasDataFromPreviousSession())
            {
                File.Delete(GetFilePath());
            }
        }

        public void Save(VideoView[] videoViews)
        {
            var json = ConvertToJson(videoViews);
            SaveJsonFile(json);
        }

        private string ConvertToJson(VideoView[] videoViews)
        {
            return JsonConvert.SerializeObject(videoViews);
        }

        private VideoView[] ConvertFromJson(string json)
        {
            return JsonConvert.DeserializeObject<VideoView[]>(json);
        }
        
        private void SaveJsonFile(string text)
        {
            var filePath = GetFilePath();
            
            if (string.IsNullOrEmpty(text) || string.IsNullOrWhiteSpace(text) || string.IsNullOrEmpty(filePath)) return;
            
            var directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(filePath, text);
        }
        
        private string GetFilePath()
        {
            return Path.Combine(_cacheFolder, _fileName);
        }
    }
}
