using System.Collections;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AppStart
{
    public sealed class LoadingProgressView: MonoBehaviour
    {
        [SerializeField] private Slider _loadingBarSlider;
        [SerializeField] private TMP_Text _progressText;

        void Awake()
        {
            SetProgress(0);
            DontDestroyOnLoad(gameObject);
        }
        
        public void PlayFakeProgress(float duration)
        {
            StartCoroutine(SimulateProgress(duration));
        }

        private void SetProgress(float progress)
        {
            _loadingBarSlider.value = progress;
            _progressText.text = Mathf.Floor(100 * progress).ToString(CultureInfo.InvariantCulture);
        }

        private IEnumerator SimulateProgress(float duration)
        {
            float time = 0;
            while (duration > time)
            {
                yield return null;
                var nextProgress = time / duration;
                time += Time.deltaTime;
                nextProgress = Mathf.Clamp(nextProgress, 0, 0.99f);
                SetProgress(nextProgress);
            }
        
        }
    }
}