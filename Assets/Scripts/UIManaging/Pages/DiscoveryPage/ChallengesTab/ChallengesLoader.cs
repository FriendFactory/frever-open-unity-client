using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.ClientServer.Tasks;
using UnityEngine;

namespace UIManaging.Pages.DiscoveryPage
{
    public sealed class ChallengesLoader
    {
        private readonly IBridge _bridge;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        private int DefaultPageSize { get; }

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public ChallengesLoader(IBridge bridge, int itemsPerPage)
        {
            _bridge = bridge;
            DefaultPageSize = itemsPerPage;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public async Task<TaskInfo[]> DownloadChallenges(object targetId, string filter = null, CancellationToken token = default)
        {
            var result = string.IsNullOrEmpty(filter)
                ? await _bridge.GetTrendingTasksAsync((long?)targetId, DefaultPageSize, 0, token)
                : await _bridge.GetTasksAsync((long?)targetId, DefaultPageSize, 0, filter, token: token);

            if (result.IsError)
            {
                Debug.LogError("Error loading challenges videos: " + result.ErrorMessage);
                return null;
            }

            if (result.IsSuccess)
            {
                return result.Models;
            }

            return null;
        }
    }
}