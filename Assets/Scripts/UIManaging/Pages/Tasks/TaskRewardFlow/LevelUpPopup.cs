using System.Collections;
using Modules.Sound;
using TMPro;
using UIManaging.Animated;
using UIManaging.PopupSystem.Configurations;
using UIManaging.PopupSystem.Popups;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.Tasks
{
    internal sealed class LevelUpPopup : BasePopup<LevelUpPopupConfiguration>
    {
        [Inject] private ISoundManager _soundManager;

        [SerializeField] private AnimatedUserLevel _animatedUserLevel;
        [SerializeField] private Button _closeButton;
        [SerializeField] private TMP_Text _levelText;
        [SerializeField] private TMP_Text _message;
        [SerializeField] private Button _seasonRewardsButton;

        private Coroutine _animateUserLevelRoutine;
        private bool _waitingForAnimation;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------e

        private void OnEnable()
        {
            if (Configs == null) return;

            _animateUserLevelRoutine =
                StartCoroutine(AnimateUserLevelRoutine(Configs.PreviousLevel, Configs.NewLevel));
            _soundManager.Play(SoundType.LevelUpPopup, MixerChannel.SpecialEffects);
            _soundManager.Play(SoundType.Applause, MixerChannel.SpecialEffects);
        }

        private void OnDisable()
        {
            if (_animateUserLevelRoutine != null)
            {
                StopCoroutine(_animateUserLevelRoutine);
            }
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnConfigure(LevelUpPopupConfiguration configuration)
        {
            _closeButton.onClick.AddListener(OnCloseButtonClick);
            _seasonRewardsButton.onClick.AddListener(OnSeasonRewardButtonClick);
            _levelText.text = configuration.PreviousLevel.ToString();
            _message.text = $"Congratulations!\n You’ve reached level {configuration.NewLevel}";
        }

        //---------------------------------------------------------------------
        // Coroutines
        //---------------------------------------------------------------------

        private IEnumerator AnimateUserLevelRoutine(int startingLevel, int finalLevel)
        {
            var previousLevel = startingLevel;

            for (int i = previousLevel + 1; i <= finalLevel; i++)
            {
                var currentLevel = i;

                _waitingForAnimation = true;
                _animatedUserLevel.Animate(previousLevel, currentLevel);
                _animatedUserLevel.AnimationFinished += OnUserLevelAnimationFinished;

                yield return new WaitWhile(() => _waitingForAnimation);

                _animatedUserLevel.AnimationFinished -= OnUserLevelAnimationFinished;
                previousLevel = currentLevel;
            }
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnUserLevelAnimationFinished()
        {
            _waitingForAnimation = false;
        }

        private void CleanUp()
        {
            _closeButton.onClick.RemoveListener(OnCloseButtonClick);
            _seasonRewardsButton.onClick.RemoveListener(OnSeasonRewardButtonClick);
        }

        private void OnCloseButtonClick()
        {
            CleanUp();
            Hide(false);
        }

        private void OnSeasonRewardButtonClick()
        {
            CleanUp();
            Hide(true);
        }
    }
}