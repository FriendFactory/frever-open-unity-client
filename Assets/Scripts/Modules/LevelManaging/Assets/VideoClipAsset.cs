using Bridge.Models.ClientServer.Assets;
using Extensions;
using UnityEngine.Video;

namespace Modules.LevelManaging.Assets
{
    public interface IVideoClipAsset: IAsset<VideoClipFullInfo>
    {
        string FilePath { get; }
    }
    
    internal sealed class VideoClipAsset : RepresentationAsset<VideoClipFullInfo>, IVideoClipAsset
    {
        public override DbModelType AssetType => DbModelType.VideoClip;
        public string FilePath { get; private set; }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Init(VideoClipFullInfo representation, string filePath)
        {
            BasicInit(representation);
            FilePath = filePath;
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void SetModelActive(bool value)
        {
        }
    }
}
