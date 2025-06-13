using System;
using System.Collections;
using Common;
using Bridge.Models.VideoServer;
using UIManaging.Common.Hashtags;
using UIManaging.Common.InputFields;
using UIManaging.Common.Ui;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.SharingPage.Ui
{
    internal sealed class DescriptionInputDecorator
    {
        private const float VALUE_CHANGED_DELAY = 0.05f;

        private readonly IInputFieldAdapter _inputFieldAdapter;
        private readonly HashtagsPanel _hashtagsPanel;
        private readonly DescriptionMentionsHandler _mentionsHandler;
        private readonly DescriptionHashtagsHandler _hashtagsHandler;
        private readonly DescriptionLimitHandler _limitHandler;
        private readonly WaitForSeconds _waitForValueChangedDelay;

        private string _prevText = string.Empty;
        private Coroutine _valueChangedCoroutine;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public bool IsCharLimitExceeded => _limitHandler.IsCharLimitExceeded;

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        public event Action MentionRequested;
        public event Action<bool> CharacterLimitExceededStatusChanged;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public DescriptionInputDecorator(IInputFieldAdapter inputFieldAdapter, HashtagsPanel hashtagsPanel,
                                         int characterLimit, Text characterLimitText,
                                         Color characterLimitNormal, Color characterLimitExceeded)
        {
            _inputFieldAdapter = inputFieldAdapter;
            _hashtagsPanel = hashtagsPanel;

            _mentionsHandler = new DescriptionMentionsHandler(_inputFieldAdapter);
            _hashtagsHandler = new DescriptionHashtagsHandler(_inputFieldAdapter, _hashtagsPanel);
            _limitHandler = new DescriptionLimitHandler(_inputFieldAdapter, characterLimit, characterLimitText,
                                                        characterLimitNormal, characterLimitExceeded);

            _waitForValueChangedDelay = new WaitForSeconds(VALUE_CHANGED_DELAY);
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void OnEnable()
        {
            _inputFieldAdapter.OnValueChanged += OnValueChanged;
            _limitHandler.CharacterLimitExceededStatusChanged += OnCharacterLimitExceededChanged;
#if !ADVANCEDINPUTFIELD_TEXTMESHPRO 
            _mentionsHandler.MentionRequested += OnMentionRequested;
            _hashtagsPanel.HashtagItemClicked += OnHashtagItemClicked;
#endif
            _limitHandler.UpdateCharacterLimitText();
        }

        public void OnDisable()
        {
            _inputFieldAdapter.OnValueChanged -= OnValueChanged;
            _limitHandler.CharacterLimitExceededStatusChanged -= OnCharacterLimitExceededChanged;
#if !ADVANCEDINPUTFIELD_TEXTMESHPRO 
            _mentionsHandler.MentionRequested -= OnMentionRequested;
            _hashtagsPanel.HashtagItemClicked -= OnHashtagItemClicked;
#endif
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnHashtagItemClicked(HashtagInfo hashtagInfo)
        {
            _hashtagsHandler.ReplaceHashtag(hashtagInfo);
            _hashtagsPanel.Hide();
        }
        
        private void OnValueChanged(string text)
        {
            var coroutineSource = CoroutineSource.Instance;
            if (_valueChangedCoroutine != null) {coroutineSource.StopCoroutine(_valueChangedCoroutine);}
            _valueChangedCoroutine = coroutineSource.StartCoroutine(OnValueChangedCoroutine());
        }

        private void OnBatchValueChanged()
        {
            _limitHandler.StripMarkTags();
#if !ADVANCEDINPUTFIELD_TEXTMESHPRO 
            _mentionsHandler.HandleMentions(_prevText);
            _hashtagsHandler.HandleHashtags();
#endif
            _limitHandler.HandleCharacterLimit();

            _prevText = _inputFieldAdapter.Text;
        }

        private void OnMentionRequested()
        {
            MentionRequested?.Invoke();
        }

        private void OnCharacterLimitExceededChanged(bool isExceeded)
        {
            CharacterLimitExceededStatusChanged?.Invoke(isExceeded);
        }

        //---------------------------------------------------------------------
        // Coroutines
        //---------------------------------------------------------------------

        private IEnumerator OnValueChangedCoroutine()
        {
            yield return _waitForValueChangedDelay;
            OnBatchValueChanged();
        }
    }
}