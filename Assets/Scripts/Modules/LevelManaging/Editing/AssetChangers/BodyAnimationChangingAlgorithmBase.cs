using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bridge.Models.ClientServer.Assets;
using Extensions;
using Models;
using Modules.AssetsManaging;
using Modules.AssetsManaging.LoadArgs;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Editing.AssetChangers.SpawnFormations;
using TaskExtensions = Extensions.TaskExtensions;

namespace Modules.LevelManaging.Editing.AssetChangers
{
    internal abstract class BodyAnimationChangingAlgorithmBase
    {
        protected readonly IAssetManager AssetManager;
        protected readonly SpawnFormationProvider SpawnFormationProvider;
        protected readonly CharacterSpawnFormationChanger SpawnFormationChanger;
        private readonly List<BodyAnimationInfo> _loadingAnimations = new List<BodyAnimationInfo>();
        private CancellationTokenSource _cancellationTokenSource;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        public abstract AlgorithmType AlgorithmType { get; }
        
        protected BodyAnimationChangingContext Context { get; set; }
        protected LoadingResult LoadingResult { get; set; }
        protected Event TargetEvent => Context.TargetEvent;
        protected ICollection<BodyAnimLoadArgs> AnimationsToLoad { get; private set; }

        private bool IsLoading => _loadingAnimations.Any();

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        protected BodyAnimationChangingAlgorithmBase(IAssetManager assetManager, SpawnFormationProvider spawnFormationProvider, CharacterSpawnFormationChanger spawnFormationChanger)
        {
            AssetManager = assetManager;
            SpawnFormationProvider = spawnFormationProvider;
            SpawnFormationChanger = spawnFormationChanger;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public async Task<LoadingResult> Run(BodyAnimationChangingContext context)
        {
            ValidateContext(context);
            Context = context;
            await BeforeStartLoading();
            AnimationsToLoad = await GetAnimationsToLoad();
            LoadingResult = await LoadAllAnimations(AnimationsToLoad);
            if (LoadingResult.Cancelled)
            {
                return LoadingResult;
            }
            await AfterAnimationLoadedAndApplied();
            return LoadingResult;
        }

        public void Cancel()
        {
            _cancellationTokenSource?.CancelAndDispose();
            _cancellationTokenSource = null;
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected virtual void ValidateContext(BodyAnimationChangingContext context)
        {
            if (context.RequestedChanges.IsNullOrEmpty()) throw new ArgumentNullException(nameof(context.RequestedChanges));
            if (context.TargetEvent == null) throw new ArgumentNullException(nameof(context.TargetEvent));
        }

        protected abstract Task BeforeStartLoading();

        protected abstract Task<ICollection<BodyAnimLoadArgs>> GetAnimationsToLoad();

        protected abstract Task AfterAnimationLoadedAndApplied();
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private async Task<LoadingResult> LoadAllAnimations(ICollection<BodyAnimLoadArgs> bodyAnimLoadArgs)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;
            
            var loadedAnimations = new List<IBodyAnimationAsset>();
            var failedToLoadAssets = new Dictionary<BodyAnimationInfo, string>();
            
            _loadingAnimations.AddRange(bodyAnimLoadArgs.Select(x=> x.BodyAnimation).DistinctBy(x=>x.Id));
            foreach (var bodyAnimLoadArg in bodyAnimLoadArgs)
            {
                var animation = bodyAnimLoadArg.BodyAnimation;
                var characterControllers = bodyAnimLoadArg.CharacterController;
                var args = new BodyAnimationLoadArgs { CancellationToken = token};
                AssetManager.Load(animation, args, 
                                  asset =>
                                  {
                                      _loadingAnimations.Remove(animation);
                                      var bodyAnimAsset = (IBodyAnimationAsset)asset;
                                      loadedAnimations.Add(bodyAnimAsset);
                                      ApplyAnimation(characterControllers, bodyAnimAsset);
                                  },
                                  message =>
                                  {
                                      _loadingAnimations.Remove(animation);
                                      failedToLoadAssets[animation] = message;
                                  }, onCancelled: () =>
                                  {
                                      _loadingAnimations.Remove(animation);
                                  });
            }

            while (IsLoading && !token.IsCancellationRequested)
            {
                await TaskExtensions.DelayWithoutThrowingCancellingException(100, token);
            }

            return new LoadingResult
            {
                LoadedAnimations = loadedAnimations,
                FailedLoadings = failedToLoadAssets,
                Cancelled = token.IsCancellationRequested
            };
        }

        private void ApplyAnimation(ICollection<CharacterController> characterControllers, IBodyAnimationAsset bodyAnimationAsset)
        {
            bodyAnimationAsset.SetActive(true);

            SetBodyAnimation(bodyAnimationAsset.RepresentedModel, characterControllers);
            ResetAnimationStartCue(characterControllers);
        }
        
        private static void SetBodyAnimation(BodyAnimationInfo animation, ICollection<CharacterController> targetControllers)
        {
            foreach (var controller in targetControllers)
            {
                var animationController = controller.GetBodyAnimationController();
                animationController.SetBodyAnimation(animation);
            }
        }
        
        private void ResetAnimationStartCue(ICollection<CharacterController> controllers)
        {
            foreach (var controller in controllers)
            {
                controller.GetBodyAnimationController().ActivationCue = 0;
            }
        }
    }
}