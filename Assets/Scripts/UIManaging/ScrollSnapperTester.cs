using UnityEngine;
using UnityEngine.UI;

namespace UIManaging
{
    public class ScrollSnapperTester : MonoBehaviour
    {
        private ScrollRect _scrollRect;
        public Button leftButton;
        public Button rightButton;

        // Start is called before the first frame update
        void Start()
        {
            _scrollRect = GetComponent<ScrollRect>();
            leftButton.onClick.AddListener(SnapRight);
            rightButton.onClick.AddListener(SnapLeft);
        }

        void SnapRight() {
            int numElements = _scrollRect.content.childCount;
            if(numElements < 1)
                return;

            float childWidth = 1f / (numElements);
            float roundedPosition = Mathf.Round(_scrollRect.horizontalNormalizedPosition / childWidth) * childWidth;
            _scrollRect.horizontalNormalizedPosition = roundedPosition;
            _scrollRect.horizontalNormalizedPosition -= childWidth;
        }

        void SnapLeft() {
            int numElements = _scrollRect.content.childCount;
            if(numElements < 1)
                return;

            float childWidth = 1f / (numElements);
            float roundedPosition = Mathf.Round(_scrollRect.horizontalNormalizedPosition / childWidth) * childWidth;
            _scrollRect.horizontalNormalizedPosition = roundedPosition;
            _scrollRect.horizontalNormalizedPosition += childWidth;
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
