using Modules.Animation.Lottie;
using UnityEngine;

namespace UIManaging.Common
{
    internal sealed class LottieAnimationSimpleLauncher: MonoBehaviour
    {
        [SerializeField] private LottieAnimationPlayer _animationPlayer;

        private void OnEnable()
        {
            _animationPlayer.Play();
        }

        private void OnDisable()
        {
            _animationPlayer.Stop();
        }
    }
}