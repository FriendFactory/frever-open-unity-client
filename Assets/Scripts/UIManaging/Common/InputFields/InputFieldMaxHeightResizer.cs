using System;
using AdvancedInputFieldPlugin;
using UnityEngine;

namespace UIManaging.Common.InputFields
{
    public sealed class InputFieldMaxHeightResizer: IDisposable
    {
        private readonly IInputFieldEventProvider _inputFieldEventProvider;
        private readonly IInputFieldAdapter _inputFieldAdapter;
        private readonly ScrollArea _scrollArea;
        private readonly float _maxHeightCollapsed;
        
        private float _lastResizeMaxHeight;

        public InputFieldMaxHeightResizer(IInputFieldEventProvider inputFieldEventProvider, IInputFieldAdapter inputFieldAdapter, ScrollArea scrollArea, float maxHeightCollapsed = 110f)
        {
            _inputFieldEventProvider = inputFieldEventProvider;
            _inputFieldAdapter = inputFieldAdapter;
            _scrollArea = scrollArea;
            _lastResizeMaxHeight = _inputFieldAdapter.ResizeMaxHeight;
            _maxHeightCollapsed = maxHeightCollapsed;
            
            _inputFieldEventProvider.InputFieldActivated += OnInputFieldActivated;
            _inputFieldEventProvider.InputFieldDeactivated += OnInputFieldDeactivated;
            _inputFieldEventProvider.InputFieldSlidedDown  += ScrollToEnd;
        }

        public void Dispose()
        {
            _inputFieldEventProvider.InputFieldActivated -= OnInputFieldActivated;
            _inputFieldEventProvider.InputFieldDeactivated -= OnInputFieldDeactivated;
            _inputFieldEventProvider.InputFieldSlidedDown  -= ScrollToEnd;
        }

        private void OnInputFieldActivated()
        {
            _inputFieldAdapter.ResizeMaxHeight = _lastResizeMaxHeight;
        }
        
        private void OnInputFieldDeactivated()
        {
            _lastResizeMaxHeight = _inputFieldAdapter.ResizeMaxHeight;
            _inputFieldAdapter.ResizeMaxHeight = _maxHeightCollapsed;
            
            ScrollToEnd();
        }

        private void ScrollToEnd()
        {
            var normalizedScrollPosition = _scrollArea.normalizedPosition;
            _scrollArea.normalizedPosition = new Vector2(normalizedScrollPosition.x, 0f);
        }
    }
}