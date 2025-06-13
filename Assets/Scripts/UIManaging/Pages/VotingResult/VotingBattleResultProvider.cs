using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.ClientServer.Battles;
using JetBrains.Annotations;
using UnityEngine;

namespace UIManaging.Pages.VotingResult
{
    public interface IVotingBattleResultManager
    {
        Task<BattleResult[]> GetVotingBattleResult(long taskId, CancellationToken token = default);
        Task<bool> IsClaimed(long taskId);
    }
    
    [UsedImplicitly]
    internal sealed class VotingBattleResultManager: IVotingBattleResultManager
    {
        private readonly Dictionary<long, BattleResult[]> _cachedResponses = new Dictionary<long, BattleResult[]>();
        private readonly IBridge _bridge;

        public VotingBattleResultManager(IBridge bridge)
        {
            _bridge = bridge;
        }

        public async Task<BattleResult[]> GetVotingBattleResult(long taskId, CancellationToken token = default)
        {
            if (_cachedResponses.ContainsKey(taskId))
            {
                return _cachedResponses[taskId];
            }
            
            var votingResults = await _bridge.GetVotingBattleResult(taskId, token);
            if (votingResults.IsError)
            {
                Debug.LogError($"Failed to load battles result. Reason: {votingResults.ErrorMessage}");
                return null;
            }

            if (!votingResults.IsRequestCanceled)
            {
                _cachedResponses[taskId] = votingResults.Models;
            }
            return votingResults.Models;
        }

        public async Task<bool> IsClaimed(long taskId)
        {
            var result = await _bridge.GetBattleRewardStatus(taskId);

            if (result.IsError)
            {
                Debug.LogError($"Failed to receive battle reward claim status for task {taskId}, reason: {result.ErrorMessage}");
                return true;
            }

            if (result.IsSuccess)
            {
                return result.IsClaimed;
            }

            return true;
        }
    }
}