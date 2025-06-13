using System.Collections;
using System.Threading.Tasks;
using UIManaging.Core;
using UnityEngine;

namespace UIManaging.Pages.Home
{
    public abstract class ResponsiveHomePageButtonBase : ButtonBase
    {
        [SerializeField] private float _heightThreshold;
        
        [Space]
        [SerializeField] private GameObject _verticalVariant;
        [SerializeField] private GameObject _horizontalVariant;

        private IEnumerator Start()
        {
            var rectTransform = GetComponent<RectTransform>();
            if (!rectTransform) yield break;
            
            // unity need one frame to calculate size of the RectTransforms
            yield return new WaitForEndOfFrame();

            var currentWidth = rectTransform.rect.width;
            var currentHeight = rectTransform.rect.height;
            var useHorizontalVariant = currentHeight < _heightThreshold && currentWidth > currentHeight;

            Debug.Log($"currentHeight: {currentHeight}, currentWidth: {currentWidth}, useHorizontalVariant: {useHorizontalVariant}");
            
            _verticalVariant.SetActive(!useHorizontalVariant);
            _horizontalVariant.SetActive(useHorizontalVariant);
        }
    }
}