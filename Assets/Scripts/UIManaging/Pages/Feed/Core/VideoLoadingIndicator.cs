using Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.Feed.Core
{
    internal sealed class VideoLoadingIndicator : MonoBehaviour
    {
        [SerializeField] private Image _progressBarBackground;
        [SerializeField] private Image _progressBarFill;

        private void Awake()
        {
            Switch(false);
        }

        public void Switch(bool isOn)
        {
            _progressBarBackground.SetActive(isOn);
            _progressBarFill.SetActive(isOn);
        }
    }
}
