using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Bridge.Results;
using Bridge.Services._7Digital.Models;
using Bridge.Services._7Digital.Models.TrackModels;
using Extensions;
using UIManaging.Pages.Common.SongOption.SongList;

namespace UIManaging.Pages.Common.SongOption.Search
{
    public class TrackSearchListModel: MusicSearchListModel<PlayableTrackModel, TrackInfo>
    {
        public TrackSearchListModel(IMusicBridge bridge, int searchPageSize = 10) : base(bridge, searchPageSize)
        {
        }
        
        protected override Task<ArrayResult<TrackInfo>> LoadPage(string searchQuery = "", int takeNext = 10, int skip = 0, CancellationToken token = default)
        {
            return _bridge.SearchExternalTracks(searchQuery, _defaultPageSize, skip, cancellationToken: token);
        }

        protected override async Task<bool> ProcessPage(ArrayResult<TrackInfo> page, CancellationToken token)
        {
            var trackIds = page.Models?.Select(model => model.ExternalTrackId).ToArray();
            
            if(trackIds.IsNullOrEmpty()) return false;
            
            var tracksDetails = await _bridge.GetBatchTrackDetails(trackIds, cancellationToken: token);
            
            if (token.IsCancellationRequested) return false;
            
            if (tracksDetails.IsError)
            {
                Debug.LogError($"[{GetType().Name}] Failed to get batch tracks details: {tracksDetails.ErrorMessage}");
                InvokeFetchFailed();
                return false;
            }

            var loadedTracks = tracksDetails.Models;
            
            AddTrackModels(loadedTracks);
            
            if (tracksDetails.TrackErrors.Length == 0)
            {
                return true;
            }

            var failedIds = tracksDetails.TrackErrors.Select(error => error.ItemId);
            var redownloadedTracks = new List<ExternalTrackInfo>();

            await RedownloadFailedTracksAsync();
            
            return true;

            async Task RedownloadFailedTracksAsync()
            {
                var failedTrackDetailsTasks = failedIds.Select(TryAddTrackAsync);

                await Task.WhenAll(failedTrackDetailsTasks);

                AddTrackModels(redownloadedTracks);
            }
            
            async Task TryAddTrackAsync(long trackId)
            {
                if (token.IsCancellationRequested) return;
                
                var result = await _bridge.GetTrackDetails(trackId, token);
                if (result.IsError)
                {
                    return;
                }
                
                if (result.IsRequestCanceled) return;
                
                redownloadedTracks.Add(result.Model);
            }
            
            void AddTrackModels(IEnumerable<ExternalTrackInfo> tracks)
            {
                var models = tracks.Select(trackInfo => new PlayableTrackModel(trackInfo, false)).ToArray();

                LastPageLoaded = models.Length == 0 && tracksDetails.TrackErrors.Length == _defaultPageSize;
                
                _models.AddRange(models);
            }
        }
    }
}