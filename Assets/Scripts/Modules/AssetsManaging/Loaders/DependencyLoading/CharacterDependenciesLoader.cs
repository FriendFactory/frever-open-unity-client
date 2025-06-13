using System.Threading.Tasks;
using Bridge.Models.ClientServer.Assets;
using Modules.AssetsManaging.LoadArgs;
using Modules.FreverUMA.ViewManagement;
using Modules.LevelManaging.Assets;

namespace Modules.AssetsManaging.Loaders.DependencyLoading
{
    internal sealed class CharacterDependenciesLoader: DependencyLoader<CharacterFullInfo, CharacterLoadArgs>
    {
        private readonly CharacterViewContainer _characterViewContainer;

        public CharacterDependenciesLoader(CharacterViewContainer characterViewContainer)
        {
            _characterViewContainer = characterViewContainer;
        }

        public override bool HasDependenciesToLoad(IAsset asset, CharacterLoadArgs args)
        {
            var characterAsset = asset as ICharacterAsset;
            if (!characterAsset.HasView)
            {
                return true;
            }
            var appliedOutfit = characterAsset.OutfitId;
            return appliedOutfit != args.Outfit?.Id;
        }

        protected override async Task<LoadResult> LoadDependencies(CharacterFullInfo entity, CharacterLoadArgs args, IAsset asset)
        {
            var view = await _characterViewContainer.GetView(entity, args.Outfit, args.CancellationToken);
            if(args.CancellationToken.IsCancellationRequested)return new LoadResult {IsCancelled = true};
            
            var characterAsset = asset as ICharacterAsset;
            characterAsset.ChangeView(view);
            return new LoadResult {IsSuccess = true};
        }
    }
}