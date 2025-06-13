using EnhancedUI.EnhancedScroller;
using UnityEngine;

namespace UIManaging.EnhancedScrollerComponents
{
    public class EnhancedScrollerScaler : MonoBehaviour
    {
        [SerializeField] private EnhancedScroller _enhancedScroller;
        [SerializeField] private RectTransform _scrollPaddingReference;
        [SerializeField] private Vector2 _scrollPaddingAnchors;
        [SerializeField] private Vector2 _scrollPaddingOffset;
        
        private void Awake()
        {
            if (_enhancedScroller.scrollDirection == EnhancedScroller.ScrollDirectionEnum.Horizontal)
            {
                _enhancedScroller.padding.left = Mathf.FloorToInt(_scrollPaddingReference.rect.width * _scrollPaddingAnchors.x 
                                                                + _scrollPaddingOffset.x);
                _enhancedScroller.padding.right = Mathf.FloorToInt(_scrollPaddingReference.rect.width * (1 - _scrollPaddingAnchors.y) 
                                                                 + _scrollPaddingOffset.y);
            }
            else
            {
                _enhancedScroller.padding.bottom = Mathf.FloorToInt(_scrollPaddingReference.rect.height * _scrollPaddingAnchors.x 
                                                                  + _scrollPaddingOffset.x);
                _enhancedScroller.padding.top = Mathf.FloorToInt(_scrollPaddingReference.rect.height * (1 - _scrollPaddingAnchors.y) 
                                                               + _scrollPaddingOffset.y);
            }
        }
    }
}