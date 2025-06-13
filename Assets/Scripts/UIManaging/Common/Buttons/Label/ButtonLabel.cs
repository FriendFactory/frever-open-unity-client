using UnityEngine;

namespace UIManaging.Common.Buttons.Label
{
    public abstract class ButtonLabel : MonoBehaviour
    {
        public virtual void SetActive(bool isOn)
        {
            gameObject.SetActive(isOn);
        }
    }
}