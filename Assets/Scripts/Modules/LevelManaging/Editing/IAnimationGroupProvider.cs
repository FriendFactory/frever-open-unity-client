using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.ClientServer.Assets;
using JetBrains.Annotations;
using UnityEngine;

namespace Modules.LevelManaging.Editing
{
    internal interface IAnimationGroupProvider
    {
        Task<BodyAnimationInfo[]> GetAnimationGroup(long bodyAnimGroupId, CancellationToken token = default);
    }

    [UsedImplicitly]
    internal sealed class BodyAnimationGroupProvider : IAnimationGroupProvider
    {
        private readonly IBridge _bridge;

        private readonly Dictionary<long, BodyAnimationInfo[]> _animationGroupsCache =
            new Dictionary<long, BodyAnimationInfo[]>();

        public BodyAnimationGroupProvider(IBridge bridge)
        {
            _bridge = bridge;
        }

        public async Task<BodyAnimationInfo[]> GetAnimationGroup(long bodyAnimGroupId, CancellationToken token)
        {
            if (_animationGroupsCache.TryGetValue(bodyAnimGroupId, out var cachedResponse))
            {
                return cachedResponse;
            }

            var response = await _bridge.GetBodyAnimationGroupAsync(bodyAnimGroupId, token);
            if (response.IsSuccess)
            {
                var ordered = response.Models.OrderBy(x => x.OrderIndexInGroup).ToArray();
                _animationGroupsCache[bodyAnimGroupId] = ordered;
                return ordered;
            }

            if (response.IsError)
            {
                Debug.LogError($"Failed ");
            }

            return null;
        }
    }
}
