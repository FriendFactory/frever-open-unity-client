using Bridge.Models.Common;
using UnityEngine;

namespace Modules.LevelManaging.Assets
{
    public abstract class BaseMusicAsset<TEntity> : RepresentationAsset<TEntity>, IAudioAsset where TEntity: IPlayableMusic
    {
        public IPlayableMusic MusicModel { get; private set; }
        public float TotalLength => Clip.length;
        public AudioSource AudioSource { get; private set; }
        public AudioClip Clip { get; set; }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Init(TEntity represent, AudioSource source, AudioClip clip)
        {
            AudioSource = source;
            AudioSource.playOnAwake = false;
            MusicModel = represent;
            Clip = clip;
            BasicInit(represent);
        }

        public override void CleanUp()
        {
        }

        public void PlayFixedRange(float startPoint, float playLength)
        {
            AudioSource.clip = Clip;
            AudioSource.time = startPoint;
            AudioSource.Play();
            AudioSource.SetScheduledEndTime(AudioSettings.dspTime + playLength);
        }

        public void SetVolume(float volume)
        {
            AudioSource.volume = volume;
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void SetModelActive(bool value)
        {
        }
    }
}
