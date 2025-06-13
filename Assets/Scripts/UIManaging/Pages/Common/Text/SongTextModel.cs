using System.Collections.Generic;
using System.Text;
using Bridge.Models.VideoServer;

namespace UIManaging.Pages.Common.Text
{
    public class SongTextModel : ScrollableTextModel
    {
        public SongTextModel(string songName)
        {
            Text = songName;
        }

        public SongTextModel(Video video)
        {
            Text = GetArtistAndSongsFormatted(video.Songs);
        }

        private string GetArtistAndSongsFormatted(IReadOnlyList<SongInfo> songs)
        {
            var count = songs?.Count ?? 0;

            if (count == 0) return string.Empty;
            
            var builder = new StringBuilder();
            for (var i = 0; i < count; i++)
            {
                var song = songs[i];
                var artist = song.Artist;
                var title = song.Title;
                
                builder.Append($"{artist} - {title}");

                if (i < count - 1)
                {
                    builder.Append(", ");
                }
            }

            return builder.ToString();
        }
    }
}