using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Common.Toggles
{
    public class ToggleColorSwap: ToggleSwapBase
    {
        [SerializeField] private Image _targetImage;
        [SerializeField] private Color _isOnColor = Color.black;
        [SerializeField] private Color _isOffColor = Color.gray;
        
        protected override void Swap(bool isOn)
        {
            if (!_targetImage) return;

            _targetImage.color = isOn ? _isOnColor : _isOffColor;
        }
    }
}