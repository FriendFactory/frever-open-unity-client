using System.Linq;
using System.Threading.Tasks;
using Extensions;
using JetBrains.Annotations;
using Models;
using Modules.AssetsManaging.UncompressedBundles;
using Modules.FreverUMA;

namespace Modules.LevelManaging.Editing.LevelManagement
{
    public interface ILevelAssetsUnCompressingService
    {
        Task UnCompressHeavyBundles(Level targetLevel);
        Task UnCompressHeavyBundles(Event targetEvent);
    }

    [UsedImplicitly]
    internal sealed class LevelAssetsUnCompressingService: ILevelAssetsUnCompressingService
    {
        private readonly UncompressedBundlesManager _uncompressedBundlesManager;
        private readonly AvatarHelper _avatarHelper;

        public LevelAssetsUnCompressingService(UncompressedBundlesManager uncompressedBundlesManager, AvatarHelper avatarHelper)
        {
            _uncompressedBundlesManager = uncompressedBundlesManager;
            _avatarHelper = avatarHelper;
        }

        public async Task UnCompressHeavyBundles(Level targetLevel)
        {
            var copyEvents = targetLevel.Event.ToArray();//prevent exc in case of collection modification during foreach loop
            foreach (var ev in copyEvents)
            {
                await UnCompressHeavyBundles(ev);
            }
        }

        public async Task UnCompressHeavyBundles(Event targetEvent)
        {
            var setLocationBundle = targetEvent.GetSetLocationBundle();
            await _uncompressedBundlesManager.DecompressBundle(setLocationBundle);

            var characterAndOutfit = targetEvent.CharacterController.GetCharacterAndOutfitDataUnique();
            foreach (var characterData in characterAndOutfit)
            {
                await _avatarHelper.UnCompressBundlesForFasterAccess(characterData);
            }
        }
    }
}