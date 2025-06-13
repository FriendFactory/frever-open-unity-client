using UnityEngine;
using UnityEngine.UI;

namespace Abstract
{
    public abstract class BaseContextDataButton<T> : BaseContextDataView<T>
    {
        [SerializeField] protected Button _button;

        protected virtual void OnEnable()
        {
            if (_button != null)
            {
                _button.onClick.AddListener(OnUIInteracted);
            }
        }

        protected virtual void OnDisable()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(OnUIInteracted);
            }
        }
    }
}