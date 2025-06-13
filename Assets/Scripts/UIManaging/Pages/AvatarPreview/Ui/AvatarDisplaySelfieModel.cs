using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.ClientServer.Assets;
using Bridge.Services.SelfieAvatar;
using Extensions;
using JetBrains.Annotations;
using Modules.FreverUMA;
using UMA;
using UMA.CharacterSystem;
using Zenject;

namespace UIManaging.Pages.AvatarPreview.Ui
{
    [UsedImplicitly]
    public sealed class AvatarDisplaySelfieModel: IAvatarDisplayModel
    {
        public event Action CloseRoomRequested;
        public event Action AvatarReady;
        
        public DynamicCharacterAvatar Avatar { get; private set; }
        public long GenderId => _genderId;
        
        [Inject] private IBridge _bridge;
        [Inject] private AvatarHelper _avatarHelper;
        
        private readonly JSONSelfie _json;
        private readonly long _genderId;
        
        private readonly List<WardrobeFullInfo> _wardrobes;

        public AvatarDisplaySelfieModel(Args args)
        {
            _json = args.Json;
            _genderId = args.GenderId;
            
            _wardrobes = new List<WardrobeFullInfo>();
        }
        
        public void CloseRoom()
        {
            _avatarHelper.UnloadAllUmaBundles();
            CloseRoomRequested?.Invoke();
        }

        public async Task<DynamicCharacterAvatar> GetAvatar()
        {
            Avatar = await _avatarHelper.PrepareAvatar(_genderId);
            
            // Disable texture compression in Character Editor
            UMAGeneratorBase.compressRenderTexture = false;
            
            _wardrobes.Clear();
            
            await _avatarHelper.LoadCharacterFromSelfie(Avatar, _json, _wardrobes, _genderId);
            await AvatarHelper.WaitWhileAvatarCreated(Avatar);

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

        public CharacterFullInfo GetCharacter()
        {
            var currentCharacter = new CharacterFullInfo
            {
                Wardrobes = _wardrobes,
                UmaRecipe = new UmaRecipeFullInfo
                {
                    J = Encoding.ASCII.GetBytes(Avatar.GetCurrentRecipe())
                }
            };

            return currentCharacter;
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
            public JSONSelfie Json;
            public long GenderId;
        }
        
        public class Factory: PlaceholderFactory<Args, AvatarDisplaySelfieModel> { }
    }
}