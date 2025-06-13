using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bridge.Models.ClientServer.Assets;
using Extensions;
using JetBrains.Annotations;
using Modules.AssetsManaging;
using Modules.LevelManaging.Editing.AssetChangers.SpawnFormations;
using UnityEngine;
using CharacterController = Models.CharacterController;
using Event = Models.Event;

namespace Modules.LevelManaging.Editing.AssetChangers
{
    [UsedImplicitly]
    internal sealed class BodyAnimationChanger : BaseChanger
    {
        private readonly BodyAnimationChangingAlgorithmBase[] _algorithms;
        
        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public BodyAnimationChanger(IAssetManager assetManager, SpawnFormationProvider spawnFormationProvider, CharacterSpawnFormationChanger characterSpawnFormationChanger, IAnimationGroupProvider animationGroupProvider)
        {
            _algorithms = new BodyAnimationChangingAlgorithmBase[]
            {
                new SingleCharacterBodyAnimationChangingAlgorithm(assetManager, spawnFormationProvider, characterSpawnFormationChanger),
                new GroupedBodyAnimationChangingAlgorithm(assetManager, spawnFormationProvider, characterSpawnFormationChanger, animationGroupProvider)
            };
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public async Task Run(Event ev, ICollection<BodyAnimLoadArgs> bodyAnimLoadArgs)
        {
            CancelLoadings();
            
            var algorithm = GetAlgorithm(bodyAnimLoadArgs);
            
            foreach (var animLoadArg in bodyAnimLoadArgs)
            {
                InvokeAssetStartedUpdating(DbModelType.BodyAnimation, animLoadArg.BodyAnimation.Id);
            }
            
            var context = new BodyAnimationChangingContext
            {
                RequestedChanges = bodyAnimLoadArgs,
                TargetEvent = ev
            };
            
            var result = await algorithm.Run(context);
            ProcessFailedLoadings(result);
            
            InvokeAssetUpdated(DbModelType.BodyAnimation);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void CancelLoadings()
        {
            foreach (var algorithm in _algorithms)
            {
                algorithm.Cancel();
            }
        }
        
        private BodyAnimationChangingAlgorithmBase GetAlgorithm(ICollection<BodyAnimLoadArgs> bodyAnimLoadArgs)
        {
            var algorithmType = GetAlgorithmType(bodyAnimLoadArgs);
            return _algorithms.First(x=>x.AlgorithmType == algorithmType);
        }

        private AlgorithmType GetAlgorithmType(ICollection<BodyAnimLoadArgs> bodyAnimLoadArgs)
        {
            if (bodyAnimLoadArgs.Count == 1 && bodyAnimLoadArgs.First().BodyAnimation.BodyAnimationGroupId.HasValue)
            {
                return AlgorithmType.MultipleCharacterAnimation;
            }
            return AlgorithmType.SingleCharacterAnimation;
        }
        
        private static void ProcessFailedLoadings(LoadingResult result)
        {
            foreach (var failedLoading in result.FailedLoadings)
            {
                Debug.LogError($"Failed to load {failedLoading.Key.Name} body animation. Reason: {failedLoading.Value}");
            }
        }
    }

    public class BodyAnimLoadArgs
    {
        public BodyAnimationInfo BodyAnimation;
        public ICollection<CharacterController> CharacterController;
    }
}