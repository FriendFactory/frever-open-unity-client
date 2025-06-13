using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.ClientServer.Tasks;
using Bridge.Results;
using JetBrains.Annotations;

namespace Common.Services
{
    public interface IUserTaskProvider
    {
        Task<Result<TaskFullInfo>> GetTaskFullInfoAsync(long taskId, CancellationToken token = default);
    }

    [UsedImplicitly]
    internal sealed class UserTaskProvider: IUserTaskProvider
    {
        private readonly IBridge _bridge;
        private readonly Dictionary<long, Result<TaskFullInfo>> _cache = new Dictionary<long, Result<TaskFullInfo>>();

        public UserTaskProvider(IBridge bridge)
        {
            _bridge = bridge;
        }

        public async Task<Result<TaskFullInfo>> GetTaskFullInfoAsync(long taskId, CancellationToken token)
        {
            if (_cache.ContainsKey(taskId))
            {
                return _cache[taskId];
            }

            var resp = await _bridge.GetTaskFullInfoAsync(taskId, token);
            if (!resp.IsSuccess)
            {
                return resp;
            }

            _cache[taskId] = resp;
            return resp;
        }
    }
}