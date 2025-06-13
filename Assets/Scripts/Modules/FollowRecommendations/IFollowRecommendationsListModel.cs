using System.Collections.Generic;
using Bridge.Models.ClientServer.Recommendations;

namespace Modules.FollowRecommendations
{
    public interface IFollowRecommendationsListModel
    {
        IReadOnlyList<FollowRecommendation> Models { get; }
    }
}