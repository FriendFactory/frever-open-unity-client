using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Common
{
    public class VideoRatingBadge : MonoBehaviour
    {
        private const int GOLD_THRESHOLD = 35;
        private const int SILVER_THRESHOLD = 25;

        [SerializeField] private Image _image;
        [SerializeField] private Sprite _bronzeBadge;
        [SerializeField] private Sprite _silverBadge;
        [SerializeField] private Sprite _goldBadge;
        
        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Show(int rating)
        {
            _image.sprite = rating switch
            {
                >= GOLD_THRESHOLD => _goldBadge,
                >= SILVER_THRESHOLD => _silverBadge,
                _ => _bronzeBadge
            };

            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}