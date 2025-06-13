using System;
using System.Collections;
using Common;
using UIManaging.Common.InputFields;
using UIManaging.Common.Ui;
using UnityEngine;

namespace UIManaging.Pages.Comments.Views
{
    public class CommentInputDecorator
    {
        private const float VALUE_CHANGED_DELAY = 0.05f;

        private readonly IInputFieldAdapter _inputFieldAdapter;
        private readonly DescriptionMentionsHandler _mentionsHandler;
        private readonly WaitForSeconds _waitForValueChangedDelay;

        private string _prevText = string.Empty;
        private Coroutine _valueChangedCoroutine;

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        public event Action MentionRequested;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public CommentInputDecorator(IInputFieldAdapter inputFieldAdapter)
        {
            _inputFieldAdapter = inputFieldAdapter;
            _mentionsHandler = new DescriptionMentionsHandler(_inputFieldAdapter);
            _waitForValueChangedDelay = new WaitForSeconds(VALUE_CHANGED_DELAY);
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void OnEnable()
        {
            _inputFieldAdapter.OnValueChanged += OnValueChanged;
#if !ADVANCEDINPUTFIELD_TEXTMESHPRO
            _mentionsHandler.MentionRequested += OnMentionRequested;
#endif
        }

        public void OnDisable()
        {
            _inputFieldAdapter.OnValueChanged -= OnValueChanged;
#if !ADVANCEDINPUTFIELD_TEXTMESHPRO
            _mentionsHandler.MentionRequested -= OnMentionRequested;
#endif
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void OnValueChanged(string text)
        {
            var coroutineSource = CoroutineSource.Instance;
            if (_valueChangedCoroutine != null) {coroutineSource.StopCoroutine(_valueChangedCoroutine);}
            _valueChangedCoroutine = coroutineSource.StartCoroutine(OnValueChangedCoroutine());
        }

        private void OnBatchValueChanged()
        {
            _mentionsHandler.HandleMentions(_prevText);
            _prevText = _inputFieldAdapter.Text;
        }

        private void OnMentionRequested()
        {
            MentionRequested?.Invoke();
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