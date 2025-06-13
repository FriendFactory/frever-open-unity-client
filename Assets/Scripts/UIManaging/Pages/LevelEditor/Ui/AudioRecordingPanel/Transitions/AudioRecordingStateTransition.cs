using System;
using Modules.LevelManaging.Editing;

namespace UIManaging.Pages.LevelEditor.Ui
{
    [Serializable]
    public class AudioRecordingStateTransition
    {
        public AudioRecordingState source;
        public AudioRecordingState destination;
        public string tweenId;
        public bool forward = true;
        public bool instant;
        public float duration = 0.5f;
    }
}