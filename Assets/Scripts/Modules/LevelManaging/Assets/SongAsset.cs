using Bridge.Models.ClientServer.Assets;
using Extensions;

namespace Modules.LevelManaging.Assets
{
    public interface ISongAsset: IAudioAsset, IAsset<SongInfo>
    {
    }
    
    internal sealed class SongAsset : BaseMusicAsset<SongInfo>, ISongAsset
    {
        public override DbModelType AssetType => DbModelType.Song;
    }
}
