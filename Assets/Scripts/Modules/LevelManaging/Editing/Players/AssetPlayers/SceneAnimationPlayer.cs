using UnityEngine;

namespace Modules.LevelManaging.Editing.Players.AssetPlayers
{
    public class SceneAnimationPlayer : MonoBehaviour
    {
        private UnityEngine.Animation _animation;
        
        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        private UnityEngine.Animation Animation => _animation == null ? _animation = GetComponent<UnityEngine.Animation>() : _animation;

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void PlayAtTime(float time)
        {
            foreach (AnimationState state in Animation)
            {
                state.time = time;
            }
            Resume();

            Animation.Play();
        }

        public void Resume()
        {
            foreach (AnimationState state in Animation)
            {
                state.speed = 1;
            }
        }

        public void Pause()
        {
            foreach (AnimationState state in Animation)
            {
                state.speed = 0;
            }
        }

        public void Stop()
        {
            Animation.Stop();
        }
    }
}