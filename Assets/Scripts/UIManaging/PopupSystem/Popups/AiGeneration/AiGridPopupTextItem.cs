using Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.PopupSystem.Popups.AiGeneration
{
    internal sealed class AiGridPopupTextItem: MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _emoji;
        [SerializeField] private TMP_Text _label;

        public Button Button => _button;

        public void SetValue(string emoji, string label = null)
        {
            _emoji.text = emoji;

            if (string.IsNullOrEmpty(label))
            {
                _label.SetActive(false);

            }
            else
            {
                _label.SetActive(true);
                _label.text = label;
            }
        }
    }
}