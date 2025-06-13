using UnityEngine;

namespace Modules.LevelManaging.Assets
{
    public class PlayAudioOnEnable : MonoBehaviour
    {
        public AudioSource AudioSource;
        public bool PlayOnEnableOnce;

        private void OnEnable()
        {
            if (!PlayOnEnableOnce) return;

            PlayOnEnableOnce = false;
            AudioSource.Play();
        }
    }
}