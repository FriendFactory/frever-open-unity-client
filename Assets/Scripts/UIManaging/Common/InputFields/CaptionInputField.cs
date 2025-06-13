using System.Collections.Generic;
using System.Linq;
using AdvancedInputFieldPlugin;
using Common;
using Extensions;
using TMPro;
using UnityEngine;

namespace UIManaging.Common.InputFields
{
    public sealed class CaptionInputField: AdvancedInputField
    {
        [SerializeField] private float _horizontalPadding = 110;
        private TMP_Text _inputText;
        private TMP_Text _placeholderText;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------
        
        private TMP_Text InputText
        {
            get
            {
                if (_inputText == null) _inputText = TextRenderer.GetComponent<TMP_Text>();
                return _inputText;
            }
        }

        private TMP_Text PlaceholderText
        {
            get
            {
                if (_placeholderText == null)  _placeholderText = transform.Find("TextArea/Content/Placeholder").GetComponent<TMP_Text>();
                return _placeholderText;
            }
        }
        
        private TMP_Text TargetTextComponent => string.IsNullOrEmpty(InputText.text) ? PlaceholderText : InputText;
        
        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public void SetAlignment(TextAlignmentOptions alignmentOptions)
        {
            if (InputText.alignment == alignmentOptions) return;
            FitRectTransformToText();
            InputText.alignment = alignmentOptions;
            CoroutineSource.Instance.ExecuteAtEndOfFrame(() =>
            {
                var tmpRenderer = TextRenderer as TMProTextRenderer;
                if(tmpRenderer.Renderer.textInfo == null) return;
                tmpRenderer.RefreshData();
                UpdateCaretPosition();
            });
        }

        public string GetParsedText()
        {
            var newLineTMPIndexes = TMPTextIndexesHelper.GetExpectedNewLineTMPIndexes(_inputText.textInfo);
            //emoji is being represented by 1 TMP character info, but in string they can take 2 char, that's why we need to convert TMP to indexes in string
            var convertedIndexes = TMPTextIndexesHelper.ConvertTMPCharacterInfoToStringIndexes(Text, newLineTMPIndexes);
            return Text.InsertNewLinesIfNotAdded(convertedIndexes);
        }

        public void Refresh()
        {
            FitRectTransformToText();
        }
        
        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override void OnEnable()
        {
            base.OnEnable();
            ShouldBlockDeselect = true;
            OnValueChanged.AddListener(OnTextChanged);
            PlaceholderText.enableWordWrapping = false;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            ShouldBlockDeselect = false;
            OnValueChanged.RemoveListener(OnTextChanged);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void FitRectTransformToText(float margin = 0)
        {
            var textSize = TargetTextComponent.GetPreferredValues(Mathf.Infinity, Mathf.Infinity);
            var maxSize = Screen.width / TargetTextComponent.canvas.scaleFactor - _horizontalPadding * 2f;
            var width = Mathf.Min(textSize.x + margin, maxSize);
            RectTransform.sizeDelta = new Vector2(width, textSize.y);
        }

        private void OnTextChanged(string text)
        {
            const string wideCharacter = "W";
            var characterWidth = TargetTextComponent.GetPreferredValues(wideCharacter).x;
            FitRectTransformToText(characterWidth * 2);//workaround: add extra space to prevent text wrapping before reach the screen width
        }
    }
}