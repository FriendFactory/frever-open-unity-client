using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Modules.Sound
{
    [CreateAssetMenu(fileName = "SoundBank.asset", menuName = "Friend Factory/Sound System/Sound Bank", order = 1)]
    public class SoundBank : ScriptableObject
    {
        [SerializeField]
        private List<SoundBankEntry> _entries;

        private Dictionary<SoundType, AudioClip> _clips;

        public bool AudioClipOfType(SoundType soundType, out AudioClip clip)
        {
            clip = _entries.FirstOrDefault(e => e.SoundType == soundType)?.Clip;
            
            return clip != null;
        }
    }
}