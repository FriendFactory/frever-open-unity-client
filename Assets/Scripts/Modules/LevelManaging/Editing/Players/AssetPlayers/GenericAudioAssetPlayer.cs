using Bridge.Models.Common;
using Modules.LevelManaging.Assets;

namespace Modules.LevelManaging.Editing.Players.AssetPlayers
{
    public abstract class GenericAudioAssetPlayer<TEntity, TAsset> : GenericTimeDependAssetPlayer<TAsset>, IAudioAssetPlayer 
        where TEntity: IPlayableMusic where TAsset: IAudioAsset
    {
        protected float Volume { get; private set; }

        public void SetVolume(float volume)
        {
            Volume = volume;
            if (IsPlaying)
            {
                ApplyVolumeToAudioSource();
            }
        }
        
        public override void Simulate(float time)
        {
        }

        protected override void OnPlay()
        {
            if (Target == null) return;
            if (StartTime > Target.Clip.length) return;

            Target.AudioSource.clip = Target.Clip;
            Target.AudioSource.time = StartTime;
            ApplyVolumeToAudioSource();
            Target.AudioSource.Play();
        }

        protected override void OnResume()
        {
            Target?.AudioSource.Play();
        }

        protected override void OnPause()
        {
            Target?.AudioSource.Pause();
        }

        protected override void OnStop()
        {
            Target?.AudioSource.Stop();
        }

        private void ApplyVolumeToAudioSource()
        {
            Target.AudioSource.volume = Volume;
        }
    }
}
