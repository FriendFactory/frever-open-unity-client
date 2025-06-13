using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.UmaEditorPage.Ui
{
    public class ShoppingBagCounterUI : MonoBehaviour
    {
        [SerializeField] private GameObject _saveText;
        [SerializeField] private GameObject _numberContainer;
        [SerializeField] private GameObject _cartIcon;
        [SerializeField] private TextMeshProUGUI _numberText;
        [SerializeField] private RectTransform _layoutRoot;
        [SerializeField] private bool _showCartIcon = true;
        [SerializeField] private bool _showSaveText = true;

        public void SetBagNumber(int number)
        {
            if (_saveText != null) _saveText.SetActive(_showSaveText || number <= 0);
            if (_numberContainer != null) _numberContainer.SetActive(number > 0);
            if (_cartIcon != null) _cartIcon.SetActive(number > 0 && _showCartIcon);
            _numberText.text = number.ToString();
            LayoutRebuilder.ForceRebuildLayoutImmediate(_layoutRoot);
        }
    }
}
