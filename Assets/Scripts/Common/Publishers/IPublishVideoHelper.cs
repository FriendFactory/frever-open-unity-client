using System;
using Bridge.Models.VideoServer;

namespace Common.Publishers
{
    public interface IPublishVideoHelper
    {
        event Action VideoUploaded; 
        event Action<Video> VideoPublished;
        bool IsPublishing { get; }
        long? PublishedLevelId { get; }
        bool IsPublishedFromTask { get; }
    }
}