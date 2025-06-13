using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Common.Toggles
{
    public class ToggleSpriteSwap: ToggleSwapBase 
    {
        [SerializeField] private Image _targetImage;
        [SerializeField] private Sprite _isOnSprite;
        [SerializeField] private Sprite _isOffSprite;

        protected override void Swap(bool isOn)
        {
            if (!_targetImage) return;

            _targetImage.sprite = isOn ? _isOnSprite : _isOffSprite;
        }
    }
}