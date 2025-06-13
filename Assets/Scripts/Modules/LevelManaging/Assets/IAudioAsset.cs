using Bridge.Models.Common;
using UnityEngine;

namespace Modules.LevelManaging.Assets
{
    public interface IAudioAsset: IAsset
    {
        IPlayableMusic MusicModel { get; }
        float TotalLength { get; }
        AudioSource AudioSource { get; }
        AudioClip Clip { get; set; }
        void PlayFixedRange(float startPoint, float playLength);
        void SetVolume(float volume);
    }
}