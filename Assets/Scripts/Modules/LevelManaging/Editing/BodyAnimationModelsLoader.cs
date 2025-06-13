using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.ClientServer.Assets;
using JetBrains.Annotations;

namespace Modules.LevelManaging.Editing
{
    [UsedImplicitly]
    internal sealed class BodyAnimationModelsLoader
    {
        private readonly IBridge _bridge;
        private readonly Dictionary<long, BodyAnimationInfo> _cache = new Dictionary<long, BodyAnimationInfo>();

        public BodyAnimationModelsLoader(IBridge bridge)
        {
            _bridge = bridge;
        }

        public async Task<BodyAnimationInfo[]> Load(long[] ids, CancellationToken token = default)
        {
            await LoadAnimationsToCache(ids, token);
            return ids.Select(x => _cache[x]).ToArray();
        }
        
        public async Task<BodyAnimationInfo> Load(long id, CancellationToken token = default)
        {
            await LoadAnimationsToCache(new []{ id }, token);
            return _cache[id];
        }

        private async Task LoadAnimationsToCache(long[] ids, CancellationToken token)
        {
            if (ids.All(HasCached))
            {
                return;
            }

            var missedAnimations = ids.Where(id => !HasCached(id)).ToArray();
            var resp = await _bridge.GetBodyAnimationByIdsAsync(missedAnimations, token);
            if (resp.IsRequestCanceled) return;
            if (resp.IsError)
            {
                throw new Exception("Failed to load animations");
            }

            foreach (var model in resp.Models)
            {
                _cache[model.Id] = model;
            }
        }

        private bool HasCached(long id)
        {
            return _cache.ContainsKey(id);
        }
    }
}