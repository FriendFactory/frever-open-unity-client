using UnityEngine;

namespace Modules.LevelManaging.Assets.AssetDependencies
{
    public sealed class AudioSourceManager : MonoBehaviour
    {
        [SerializeField]
        private AudioSource _characterAudioSource;
        [SerializeField]
        private AudioSource _songAudioSource;
        [SerializeField]
        private AudioSource songPreviewAudioSource;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public AudioSource CharacterAudioSource => _characterAudioSource;
        public AudioSource SongAudioSource => _songAudioSource;
        public AudioSource SongPreviewAudioSource => songPreviewAudioSource;
    }
}
