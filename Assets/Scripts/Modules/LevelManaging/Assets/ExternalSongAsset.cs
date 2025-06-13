using Bridge.Services._7Digital.Models.TrackModels;
using Extensions;

namespace Modules.LevelManaging.Assets
{
    public interface IExternalTrackAsset: IAudioAsset, IAsset<ExternalTrackInfo>
    {
    }

    public class ExternalTrackAsset : BaseMusicAsset<ExternalTrackInfo>, IExternalTrackAsset
    {
        public override DbModelType AssetType => DbModelType.ExternalTrack;
    }
}