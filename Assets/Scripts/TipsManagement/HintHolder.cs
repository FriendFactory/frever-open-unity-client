using UnityEngine;
using Zenject;

namespace TipsManagment
{
    public class HintHolder : MonoBehaviour
    {
        [Inject] private TipManager _tipManager;
        private RectTransform _rectTransform;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        private void OnEnable()
        {
            if (_tipManager == null)
            {
                Debug.LogWarning("Tip manager not injected!");
                return;
            }

            _tipManager.SetHintHolder(_rectTransform);
        }
    }
}