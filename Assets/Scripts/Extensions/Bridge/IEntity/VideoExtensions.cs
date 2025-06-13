using Bridge.Models.VideoServer;

namespace Extensions
{
    public static class VideoExtensions
    {
        public static bool HasMusic(this Video video) => video.Songs?.Length > 0 || video.UserSounds?.Length > 0;
    }
}