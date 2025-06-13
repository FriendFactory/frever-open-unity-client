using System;
using UnityEngine;

namespace Modules.Sound
{
    [Serializable]
    public class SoundBankEntry
    {
        [SerializeField]
        private SoundType _soundType = SoundType.Button1;
        [SerializeField] 
        private AudioClip _clip;

        public SoundType SoundType => _soundType;
        public AudioClip Clip => _clip;

    }
}