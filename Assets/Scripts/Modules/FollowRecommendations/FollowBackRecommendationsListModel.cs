using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.ClientServer.Recommendations;
using Modules.Amplitude;
using UnityEngine;

namespace Modules.FollowRecommendations
{
    internal sealed class FollowBackRecommendationsListModel: FollowRecommendationsListModel
    {
        private const int TARGET_MODELS_COUNT = 10;
        
        public FollowBackRecommendationsListModel(IRecommendationsBridge bridge, AmplitudeManager amplitudeManager) :
            base(bridge, amplitudeManager) { }
        
        protected override async Task<List<FollowRecommendation>> GetFollowRecommendationsAsync(CancellationToken token)
        {
            var models = new List<FollowRecommendation>();
            var followBackResult = await Bridge.GetFollowBackRecommendations(AmplitudeManager.MlExperimentVariantsHeader, token);
            
            if (followBackResult.IsRequestCanceled) return models;
            
            if (followBackResult.IsError)
            {
                Debug.LogError($"[{GetType().Name}] Failed to get recommendations # {followBackResult.ErrorMessage}");
                return models;
            }
            
            models.AddRange(followBackResult.Models);

            var followBackRecommendationsCount = models.Count; 
            var followRecommendationsCount = Mathf.Max(0, TARGET_MODELS_COUNT - followBackRecommendationsCount);

            if (followRecommendationsCount == 0) return models;
            
            var followRecommendationsModels = await base.GetFollowRecommendationsAsync(token);
            followRecommendationsCount = Mathf.Min(followRecommendationsCount, followRecommendationsModels.Count);
            
            models.AddRange(followRecommendationsModels.GetRange(0, followRecommendationsCount));

            return models;
        }
    }
}