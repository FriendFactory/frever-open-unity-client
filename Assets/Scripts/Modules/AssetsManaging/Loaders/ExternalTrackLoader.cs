using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Bridge.Services._7Digital.Models.TrackModels;
using Modules.AssetsManaging.LoadArgs;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Assets.AssetDependencies;
using Modules.MusicCacheManaging;

namespace Modules.AssetsManaging.Loaders
{
    internal sealed class ExternalTrackLoader : BaseSongAssetLoader<ExternalTrackInfo, ExternalTrackLoadArgs>
    {
        private readonly ILicensedMusicProvider _licensedMusicProvider;
        
        public ExternalTrackLoader(IBridge bridge, AudioSourceManager audioSourceManager, ILicensedMusicProvider licensedMusicProvider) : base(bridge, audioSourceManager)
        {
            _licensedMusicProvider = licensedMusicProvider;
        }
        
        protected override async Task Download(CancellationToken cancellationToken = default)
        {
            var result = await _licensedMusicProvider.GetExternalTrackClip(Model.Id, cancellationToken);
            if (IsCancellationRequested)
            {
                OnCancelled();
                return;
            }
            
            if (result.AudioClip != null)
            {
                OnFileLoaded(result.AudioClip);
            }
            else
            {
                OnFailed(GetErrorResult(result.ErrorMessage));
            }
        }

        protected override void InitAsset()
        {
            var view = new ExternalTrackAsset();
            InitSong(view, Model);
        }
    }
}
