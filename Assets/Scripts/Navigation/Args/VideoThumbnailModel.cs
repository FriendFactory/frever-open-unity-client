using System;
using System.Collections.Generic;
using System.Text;
using RenderHeads.Media.AVProVideo;

namespace Navigation.Args
{
    public class VideoThumbnailModel
    {
        private static readonly StringBuilder STRING_BUILDER = new();

        public event Action<object> OnPlayerVisible;
        public event Action<object> OnPlayerDisabled;
        public event Action<object> OnPlayerCleared;
        public event Action<object> OnPlayerError;

        public readonly string VideoUrl;

        //---------------------------------------------------------------------
        // HTTP
        //---------------------------------------------------------------------

        private readonly Dictionary<string, string> _signedCookies;
        private HttpHeader? _header;

        public HttpHeader HttpHeader
        {
            get
            {
                _header ??= SetupHeader();
                return _header.Value;
            }
        }

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------
        
        public VideoThumbnailModel(string videoUrl, Dictionary<string, string> signedCookies = null)
        {
            VideoUrl = videoUrl;
            _signedCookies = signedCookies;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void PlayerVisible(object senderView) => OnPlayerVisible?.Invoke(senderView);
        
        public void PlayerCancelled(object senderView) => OnPlayerCleared?.Invoke(senderView);
        
        public void PlayerError(object senderView) => OnPlayerError?.Invoke(senderView);
        
        public void PlayerDisabled(object senderView) => OnPlayerDisabled?.Invoke(senderView);

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private HttpHeader SetupHeader()
        {
            if (_signedCookies is null || _signedCookies.Count == 0)
            {
                return new HttpHeader();
            }

            STRING_BUILDER.Clear();

            foreach (var cook in _signedCookies)
            {
                STRING_BUILDER.Append($"{cook.Key}={cook.Value}; ");
            }

            return new HttpHeader("Cookie", STRING_BUILDER.ToString());
        }
    }
}