using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UIManaging.Pages.StyleSelection.UI
{
    public class StyleSelectionElement : MonoBehaviour
    {
        [SerializeField] private Image _thumbnail;
        [SerializeField] private Button _button;
        [SerializeField] private Image _background;

        public void Initialize(Sprite background, Sprite thumbnail, UnityAction onClick)
        {
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(onClick);
            
            _background.sprite = background;
            
            if (thumbnail == null) return;
            
            _thumbnail.sprite = thumbnail;
            _thumbnail.preserveAspect = true;
        }
       
        private void OnDestroy()
        {
            if (_thumbnail.sprite)
            {
                Destroy(_thumbnail.sprite);
            }
        }
    }
}