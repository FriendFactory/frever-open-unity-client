using System;
using System.Linq;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.ClientServer.Assets;
using JetBrains.Annotations;
using Modules.FreverUMA;
using UMA;
using UMA.CharacterSystem;
using Zenject;

namespace UIManaging.Pages.AvatarPreview.Ui
{
    [UsedImplicitly]
    public sealed class AvatarDisplayCharacterModel: IAvatarDisplayModel
    {
        public event Action CloseRoomRequested;
        public event Action AvatarReady;
        
        public DynamicCharacterAvatar Avatar { get; private set; }
        public long GenderId { get; }
        
        [Inject] private IBridge _bridge;
        [Inject] private AvatarHelper _avatarHelper;
        
        private readonly CharacterFullInfo _character;
        private readonly OutfitFullInfo _outfit;
        
        public AvatarDisplayCharacterModel(Args args)
        {
            _character = args.Character;
            _outfit = args.Outfit;
            GenderId = args.Character.GenderId;
        }
        
        public void CloseRoom()
        {
            _avatarHelper.UnloadAllUmaBundles();
            CloseRoomRequested?.Invoke();
        }

        public async Task<DynamicCharacterAvatar> GetAvatar()
        {
            Avatar = await _avatarHelper.PrepareAvatar(_character.GenderId);
            
            // Disable texture compression in Character Editor
            UMAGeneratorBase.compressRenderTexture = false;

            await _avatarHelper.LoadCharacterToAvatar(Avatar, _character, _outfit);

            AvatarReady?.Invoke();
            
            return Avatar;
        }

        public Task<BodyAnimationInfo> GetIdleBodyAnimation()
        {
            return GetBodyAnimation(171); // TODO: replace hardcoded id
        }

        public Task<BodyAnimationInfo> GetWaveBodyAnimation()
        {
            return GetBodyAnimation(173); // TODO: replace hardcoded id
        }
        
        private async Task<BodyAnimationInfo> GetBodyAnimation(long id)
        {
            var result = await _bridge.GetBodyAnimationListAsync(id, 0, 0, 0);
            
            if (result.IsSuccess)
            {
                return result.Models.First();
            }
            
            throw new Exception($"Failed to load body animation. Reason: {result.ErrorMessage}");
        }

        public struct Args
        {
            public CharacterFullInfo Character;
            public OutfitFullInfo Outfit;
        }
        
        public class Factory: PlaceholderFactory<Args, AvatarDisplayCharacterModel> { }
    }
}