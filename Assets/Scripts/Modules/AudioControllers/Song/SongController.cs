using System;
using System.Collections.Generic;
using UnityEngine;

namespace Modules.AudioControllers.Song
{
    public class SongController : MonoBehaviour
    {
        private readonly List<SongCondition> _conditions = new List<SongCondition>();
        private AudioSource _audioSource;

        public event EventHandler SongStarted;
        public event EventHandler SongPaused;

        public void Setup(AudioSource source)
        {
            _audioSource = source;

            _conditions.Add(new SongSelectedCondition());

            foreach (var songCondition in _conditions)
            {
                songCondition.Subscribe();
            }

            //Subscribe SongEditorViewModel.SongStarted to OnSongStarted();
        }

        private void OnSongStarted()
        {
            SongStarted?.Invoke(this, EventArgs.Empty);

            foreach (var songCondition in _conditions)
            {
                if (songCondition.CheckCondition() == false)
                {
                    _audioSource.Stop();
                    SongPaused?.Invoke(this, EventArgs.Empty);
                }
            }
        }
    }
}
