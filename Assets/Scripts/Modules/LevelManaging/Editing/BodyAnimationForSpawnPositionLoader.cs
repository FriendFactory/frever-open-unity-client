using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Bridge.Models.ClientServer.Assets;
using Extensions;
using JetBrains.Annotations;
using Modules.AssetsManaging;
using Modules.AssetsManaging.LoadArgs;

namespace Modules.LevelManaging.Editing
{
    [UsedImplicitly]
    internal sealed class BodyAnimationForSpawnPositionLoader
    {
        private readonly IAssetManager _assetManager;

        private int _waitForBodyAnimationsCount;
        private CancellationTokenSource _cancellationTokenSource;

        public bool IsLoading => _waitForBodyAnimationsCount > 0;
        public IReadOnlyCollection<BodyAnimationInfo> LoadedAnimationModels { get; private set; }
        public bool HasLoadedAnimations => LoadedAnimationModels != null && LoadedAnimationModels.Any();
        
        public BodyAnimationForSpawnPositionLoader(IAssetManager assetManager)
        {
            _assetManager = assetManager;
        }

        public void LoadAnimations(BodyAnimationInfo[] bodyAnimations, CancellationToken token = default)
        {
            Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
            token.Register(Cancel);
            LoadedAnimationModels = bodyAnimations;
            foreach (var bodyAnimation in bodyAnimations)
            {
                LoadBodyAnimation(bodyAnimation, token);
            }
        }

        public void Cancel()
        {
            _cancellationTokenSource?.CancelAndDispose();
            _cancellationTokenSource = null;
            _waitForBodyAnimationsCount = 0;
        }

        public void Reset()
        {
            Cancel();
            LoadedAnimationModels = null;
        }

        private void LoadBodyAnimation(BodyAnimationInfo bodyAnimation, CancellationToken token)
        {
            _waitForBodyAnimationsCount++;

            var args = new BodyAnimationLoadArgs
            {
                CancellationToken = token,
                DeactivateOnLoad = true
            };
            _assetManager.Load(bodyAnimation, args, x => OnAnimationLoadingFinished(), x => OnAnimationLoadingFinished());
        }

        private void OnAnimationLoadingFinished()
        {
            _waitForBodyAnimationsCount--;
        }
    }
}