using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Extensions;
using Modules.AssetsManaging;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Editing.AssetChangers.SpawnFormations;

namespace Modules.LevelManaging.Editing.AssetChangers
{
    internal sealed class GroupedBodyAnimationChangingAlgorithm : BodyAnimationChangingAlgorithmBase
    {
        private readonly IAnimationGroupProvider _animationGroupProvider;
        
        public GroupedBodyAnimationChangingAlgorithm(IAssetManager assetManager, SpawnFormationProvider spawnFormationProvider, CharacterSpawnFormationChanger spawnFormationChanger, IAnimationGroupProvider animationGroupProvider) : base(assetManager, spawnFormationProvider, spawnFormationChanger)
        {
            _animationGroupProvider = animationGroupProvider;
        }

        public override AlgorithmType AlgorithmType => AlgorithmType.MultipleCharacterAnimation;

        protected override Task BeforeStartLoading()
        {
            return Task.CompletedTask;
        }

        protected override async Task<ICollection<BodyAnimLoadArgs>> GetAnimationsToLoad()
        {
            if (Context.TargetEvent.GetCharactersCount() == 1) return Context.RequestedChanges;
            
            var bodyAnim = Context.RequestedChanges.First().BodyAnimation;
            var group = await _animationGroupProvider.GetAnimationGroup(bodyAnim.BodyAnimationGroupId.Value);
            var controllers = Context.TargetEvent.GetOrderedCharacterControllers();
            var output = new List<BodyAnimLoadArgs>();
            for (var i = 0; i < controllers.Length && i < group.Length; i++)
            {
                output.Add(new BodyAnimLoadArgs
                {
                    BodyAnimation = group.ElementAt(i),
                    CharacterController = new [] {controllers.ElementAt(i)}
                });
            }
            
            return output;
        }

        protected override Task AfterAnimationLoadedAndApplied()
        {
            var groupId = AnimationsToLoad.First().BodyAnimation.BodyAnimationGroupId;
            var characterWithLinkedAnimations =
                TargetEvent.CharacterController.Where(x => x.GetBodyAnimation().BodyAnimationGroupId == groupId)
                           .Select(x=>x.CharacterId).ToArray();
            
            //place all characters on the same place
            var spawnPosition = TargetEvent.GetTargetSpawnPosition();
            var characterAssets = AssetManager.GetAllLoadedAssets<ICharacterAsset>()
                                              .Where(x=>characterWithLinkedAnimations.Contains(x.Id)).ToArray();
            var setLocation = AssetManager.GetAllLoadedAssets<ISetLocationAsset>()
                                          .First(x => x.Id == TargetEvent.GetSetLocationId());
            setLocation.Attach(spawnPosition, characterAssets);
            setLocation.ResetPosition(characterAssets);
            var spawnFormation = SpawnFormationProvider.GetDefaultSpawnFormationId(TargetEvent); 
            SpawnFormationChanger.Run(spawnFormation, TargetEvent);
            foreach (var controller in TargetEvent.CharacterController)
            {
                controller.CharacterSpawnPositionId = spawnPosition.Id;
            }
            return Task.CompletedTask;
        }
    }
}