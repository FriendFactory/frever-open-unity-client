using System.Collections.Generic;
using System.Linq;
using AdvancedInputFieldPlugin;
using Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UIManaging.Common.InputFields
{
    public class IgnoredDeselectableAreaAdvancedInputField: AdvancedInputField
    {
        public TMP_InputField.SelectionEvent onSelect { get; } = new TMP_InputField.SelectionEvent();
        public TMP_InputField.SelectionEvent onDeselect { get; } = new TMP_InputField.SelectionEvent();

        private readonly HashSet<RectTransform> _rectsToIgnore = new HashSet<RectTransform>();
        
        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        protected override void Awake()
        {
            base.Awake();
            
            OnBeginEdit.AddListener(OnBeginSelect);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            OnBeginEdit.RemoveListener(OnBeginSelect);
        }
        
        //---------------------------------------------------------------------
        // Public 
        //---------------------------------------------------------------------

        public override void OnDeselect(BaseEventData eventData)
        {
            var mousePoint = Input.mousePosition;
            ShouldBlockDeselect = !_rectsToIgnore.IsNullOrEmpty() && _rectsToIgnore.Any(rect => RectTransformUtility.RectangleContainsScreenPoint(rect, mousePoint));
            
            base.OnDeselect(eventData);
            
            onDeselect?.Invoke(Text);
        }
        
        public void AddIgnoreDeselectOnRect(params RectTransform[] ignoreRect)
        {
            ignoreRect.ForEach(rect => _rectsToIgnore.Add(rect));
        }
        
        public void RemoveIgnoreDeselectOnRect(params RectTransform[] ignoreRect)
        {
            ignoreRect.ForEach(rect => _rectsToIgnore.Remove(rect));
        }
        
        //---------------------------------------------------------------------
        // Helpers 
        //---------------------------------------------------------------------

        private void OnBeginSelect(BeginEditReason reason)
        {
            ShouldBlockDeselect = false;
            onSelect.Invoke(Text);
        }
    }
}