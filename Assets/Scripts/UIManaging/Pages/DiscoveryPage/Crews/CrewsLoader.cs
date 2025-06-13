using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.ClientServer.Crews;
using Modules.Amplitude;
using UIManaging.Common.Loaders;
using UnityEngine;

namespace UIManaging.Pages.DiscoveryPage
{
    public class CrewsLoader : GenericPaginationLoader<CrewShortInfo>
    {
        private readonly IBridge _bridge;
        private readonly AmplitudeManager _amplitudeManager;
        
        public CrewsLoader(IBridge bridge, AmplitudeManager amplitudeManager)
        {
            _bridge = bridge;
            _amplitudeManager = amplitudeManager;
        }
        
        public string Filter { get; set; }
        
        protected override int DefaultPageSize => 10;

        protected override async Task<CrewShortInfo[]> DownloadModelsInternal(object targetId, int takeNext, int takePrevious = 0, CancellationToken token = default)
        {
            var skip = Mathf.Max(0, Models.Count - 1);
            var result = await _bridge.GetCrewsList(Filter, takeNext, skip, _amplitudeManager.MlExperimentVariantsHeader, token);

            if (result.IsSuccess) return result.Models;
            if (result.IsError) Debug.LogError("Error loading crews: " + result.ErrorMessage);

            return null;
        }

        protected override void OnNextPageLoaded(CrewShortInfo[] page)
        {
        }

        protected override void OnFirstPageLoaded(CrewShortInfo[] page)
        {
        }
    }
}