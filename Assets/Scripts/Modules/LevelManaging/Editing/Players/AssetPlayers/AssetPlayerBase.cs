using Extensions;
using Modules.LevelManaging.Assets;
using UnityEngine;

namespace Modules.LevelManaging.Editing.Players.AssetPlayers
{
    public abstract class AssetPlayerBase<TAsset> : IAssetPlayer<TAsset> where TAsset: IAsset
    {
        public DbModelType TargetType => Target.AssetType;
        public long AssetId => Target.Id;
        public IAsset TargetAsset => Target;
        protected TAsset Target { get; private set; }
        public bool IsPlaying { get; private set; }

        public virtual void SetTarget(IAsset target)
        {
            Target = (TAsset) target;
            if (Target == null && target != null)
            {
                Debug.LogError("Tried to use asset player for not appropriate asset type");
            }
        }

        public void Play()
        {
            IsPlaying = true;
            OnPlay();
        }

        public void Pause()
        {
            IsPlaying = false;
            OnPause();
        }

        public void Resume()
        {
            IsPlaying = true;
            OnResume();
        }

        public void Stop()
        {
            IsPlaying = false;
            OnStop();
        }

        public abstract void Simulate(float time);

        public virtual void Cleanup()
        {
        }

        protected abstract void OnPlay();
        protected abstract void OnPause();
        protected abstract void OnResume();
        protected abstract void OnStop();
    }
}