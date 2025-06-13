using System;
using Bridge.Models.VideoServer;

namespace UIManaging.Common.Interfaces
{
    public interface ILocalVideoModel
    {
        event Action VideoLoaded;
        event Action VideoStarted;

        Video Video { get; }
        string LocalFilePath { get; }
        bool IsLoaded { get; }
        bool IsStarted { get; }

        void LoadVideo();
        void StartVideo();
        void Error(string message);
        void CleanUp();
    }
}