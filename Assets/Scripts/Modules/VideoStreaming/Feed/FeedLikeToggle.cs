using Modules.VideoStreaming.Feed.Metrics;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.VideoStreaming.Feed
{
    public class FeedLikeToggle : MonoBehaviour
    {
        [SerializeField] private Toggle toggle;
        [SerializeField] private ToggleAnimator toggleAnimator;
        [SerializeField] private GameObject likedGameObject;
        [SerializeField] private GameObject notLikedGameObject;

        public Toggle Toggle => toggle;

        public void EnableAnimation(bool value)
        {
            toggleAnimator.enabled = value;
        }

        public void RefreshUI(bool isLiked)
        {
            notLikedGameObject.SetActive(!isLiked);
            likedGameObject.SetActive(isLiked);
        }
    }
}