using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Modules.SocialActions
{
    public interface ISocialActionsManager
    {
        Task<List<SocialActionCardModel>> GetAvailableActions(CancellationToken token);
        void DeleteAction(Guid recommendationId, long actionId);
    }
}