using Extensions;
using Modules.LevelManaging.Assets;

namespace Modules.LevelManaging.Editing.Players.AssetPlayers
{
    public interface IAssetPlayer
    {
        DbModelType TargetType { get; }
        long AssetId { get; }
        IAsset TargetAsset { get; }

        void SetTarget(IAsset target);
        bool IsPlaying { get; }
        
        
        void Play();
        void Pause();
        void Resume();
        void Stop();
        void Simulate(float time);
        void Cleanup();
    }
    
    public interface IAssetPlayer<TAsset> : IAssetPlayer where TAsset: IAsset
    {
    }
}