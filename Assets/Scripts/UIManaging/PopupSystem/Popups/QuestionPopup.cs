using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.PopupSystem.Popups
{
    internal sealed class QuestionPopup : InformationPopup<QuestionPopupConfiguration>
    {
        [SerializeField] private RectTransform _buttonsParent;
        [SerializeField] private Button _buttonPrefab;

        private readonly List<Button> _answerButtons = new List<Button>();
        private VerticalLayoutGroup _layout;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            _layout = GetComponentInChildren<VerticalLayoutGroup>();
        }

        //---------------------------------------------------------------------
        // BasePopup
        //---------------------------------------------------------------------

        protected override void OnConfigure(QuestionPopupConfiguration configuration)
        {
            base.OnConfigure(configuration);
            
            foreach (var button in _answerButtons)
            {
                Destroy(button.gameObject);
            }
            _answerButtons.Clear();

            for (var i = 0; i < configuration.Answers.Count; i++)
            {
                var answerButton = Instantiate(_buttonPrefab, _buttonsParent);
                _answerButtons.Add(answerButton);
                var answer = configuration.Answers[i];
                answerButton.GetComponentInChildren<Text>().text = answer.Key;
                var answerIndex = i;
                answerButton.onClick.AddListener(() => OnAnswerClick(answerIndex, answer.Value));
                answerButton.gameObject.SetActive(true);
            }
        }

        public override void Show()
        {
            base.Show();
            StartCoroutine(ResetSize());
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnAnswerClick(int answerIndex, Action answerCallback)
        {
            answerCallback?.Invoke();
            Hide(answerIndex);
        }

        [SuppressMessage("ReSharper", "Unity.InefficientPropertyAccess")]
        private IEnumerator ResetSize()
        {
            // Workaround for content fitter to reset layout size
            // when content changed while object was disabled (Unity bug)
            _layout.spacing += 0.01f;
            yield return new WaitForEndOfFrame();
            _layout.spacing -= 0.01f;
        }
    }
}