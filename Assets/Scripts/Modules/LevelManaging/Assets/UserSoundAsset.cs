using Bridge.Models.ClientServer.Assets;
using Extensions;

namespace Modules.LevelManaging.Assets
{
    public interface IUserSoundAsset: IAudioAsset, IAsset<UserSoundFullInfo>
    {
    }
    
    internal sealed class UserSoundAsset : BaseMusicAsset<UserSoundFullInfo>, IUserSoundAsset
    {
        public override DbModelType AssetType => DbModelType.UserSound;
    }
}
