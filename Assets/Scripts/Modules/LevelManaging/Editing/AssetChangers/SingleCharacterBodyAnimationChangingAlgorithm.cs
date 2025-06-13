using System.Collections.Generic;
using System.Threading.Tasks;
using Modules.AssetsManaging;
using Modules.LevelManaging.Editing.AssetChangers.SpawnFormations;

namespace Modules.LevelManaging.Editing.AssetChangers
{
    /// <summary>
    /// Responsible for loading the self-depended body animation
    /// (the animations which doesn't have linked animations to look fine together)
    /// </summary>
    internal sealed class SingleCharacterBodyAnimationChangingAlgorithm : BodyAnimationChangingAlgorithmBase
    {
        public SingleCharacterBodyAnimationChangingAlgorithm(IAssetManager assetManager, SpawnFormationProvider spawnFormationProvider, CharacterSpawnFormationChanger spawnFormationChanger) : base(assetManager, spawnFormationProvider, spawnFormationChanger)
        {
        }

        public override AlgorithmType AlgorithmType => AlgorithmType.SingleCharacterAnimation;

        protected override Task BeforeStartLoading()
        {
            return Task.CompletedTask;
        }

        protected override Task<ICollection<BodyAnimLoadArgs>> GetAnimationsToLoad()
        {
            return Task.FromResult(Context.RequestedChanges);
        }

        protected override Task AfterAnimationLoadedAndApplied()
        {
            return Task.CompletedTask;
        }
    }
}