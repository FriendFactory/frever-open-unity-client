using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace UiManaging.Pages.OnBoardingPage.UI.Pages
{
    public class OnBoardingToggle : MonoBehaviour
    {
        [SerializeField] private Color offColor = new Color(194f/255f,194f/255f,194f/255f, 1f);
        [SerializeField] private Color onColor = new Color(248f/255f,6f/255f,157f/255f, 1f);
        [SerializeField] private Toggle toggle;
        [SerializeField] private Graphic[] colorizeItems;

        [Button(nameof(RefreshColors))]
        private void RefreshColors()
        {
            var color = toggle.isOn ? onColor : offColor;

            for (int i = 0; i < colorizeItems.Length; i++)
            {
                colorizeItems[i].color = color;
            }
        }
        
        private void OnEnable()
        {
            RefreshColors();
            toggle.onValueChanged.AddListener(OnToggleValueChanged);
        }

        private void OnDisable()
        {
            toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
        }
        
        private void OnToggleValueChanged(bool value)
        {
            RefreshColors();
        }
    }
}