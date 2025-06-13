using Modules.Animation.Lottie;
using UnityEngine;

namespace UIManaging.Pages.LevelEditor.Ui.EditingFlow.DressUp
{
    internal sealed class PointingArrow: MonoBehaviour
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