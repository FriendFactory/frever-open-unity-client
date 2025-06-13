using BrunoMikoski.AnimationSequencer;
using DG.Tweening;

namespace Extensions
{
    public static class AnimationSequencerExtensions
    {
        /// <summary>
        /// Initializes the AnimationSequencer by playing it and setting the progress to 0.
        /// Useful if sequence contains steps that are set the initial state of the object and you want to apply them before the animation starts.
        /// </summary>
        /// <param name="animationSequencer"></param>
        public static void Initialize(this AnimationSequencerController animationSequencer)
        {
            animationSequencer.PlayingSequence.InsertCallback(0f, () =>
            {
                animationSequencer.SetProgress(0f, false);
            });
            
            animationSequencer.Play();
        }
    }
}