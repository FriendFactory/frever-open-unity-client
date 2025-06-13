using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.ClientServer.Recommendations;
using Modules.Amplitude;
using UnityEngine;
using Zenject;

namespace Modules.FollowRecommendations
{
    internal class FollowRecommendationsListModel : IFollowRecommendationsListModel
    {
        protected readonly IRecommendationsBridge Bridge;
        protected readonly AmplitudeManager AmplitudeManager;
        
        private List<FollowRecommendation> _models;
        
        public IReadOnlyList<FollowRecommendation> Models => _models;

        public FollowRecommendationsListModel(IRecommendationsBridge bridge, AmplitudeManager amplitudeManager)
        {
            Bridge = bridge;
            AmplitudeManager = amplitudeManager;
            _models =  new List<FollowRecommendation>();
        }

        public async Task InitializeAsync(CancellationToken token)
        {
            _models = await GetFollowRecommendationsAsync(token);
        }

        protected virtual async Task<List<FollowRecommendation>> GetFollowRecommendationsAsync(CancellationToken token)
        {
            var models = new List<FollowRecommendation>();
            var result = await Bridge.GetFollowRecommendations(AmplitudeManager.MlExperimentVariantsHeader, token);
            
            if (result.IsRequestCanceled) return models;
            
            if (result.IsError)
            {
                Debug.LogError($"[{GetType().Name}] Failed to get recommendations # {result.ErrorMessage}");
                return models;
            }
            
            models.AddRange(result.Models);

            return models;
        }
    }
}