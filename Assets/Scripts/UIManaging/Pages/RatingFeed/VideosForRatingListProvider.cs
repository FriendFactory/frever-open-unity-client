using System.Collections.Generic;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.VideoServer;
using JetBrains.Annotations;
using Models;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.RatingFeed
{
    [UsedImplicitly]
    internal class VideosForRatingListProvider
    {
        private const string ERROR_VIDEO_ALREADY_RATED = "Video participated in rating";
        private readonly IBridge _bridge;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        public List<Video> Videos { get; }
        public StatusCode Status { get; private set; } = StatusCode.Unknown;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        [Inject]
        public VideosForRatingListProvider(IBridge bridge)
        {
            _bridge = bridge;
            Videos = new List<Video>();
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public async Task InitializeAsync(Level levelData)
        {
            var result = await _bridge.GetVideoToRateListAsync(levelData.Id);

            if (!result.IsSuccess)
            {
                HandleRequestError(levelData, result.ErrorMessage);
                return;
            }

            Status = StatusCode.Success;
            Videos.AddRange(result.Models);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void HandleRequestError(Level levelData, string errorMessage)
        {
            if (errorMessage.Contains(ERROR_VIDEO_ALREADY_RATED))
            {
                Status = StatusCode.AlreadyRated;
                Debug.LogWarning($"[{GetType().Name}] Video for this level is already rated: {levelData.Id}");
            }
            else
            {
                Status = StatusCode.Error;
                Debug.LogError($"[{GetType().Name}] Failed to get videos for rating: {errorMessage}");
            }
        }

        //---------------------------------------------------------------------
        // Nested
        //---------------------------------------------------------------------

        public enum StatusCode
        {
            Unknown,
            Error,
            AlreadyRated,
            Success
        }
    }
}