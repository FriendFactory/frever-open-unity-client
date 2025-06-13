using TMPro;
using UnityEngine;

namespace UIManaging.Common.Buttons.Label
{
    public class ButtonTextLabel: MonoBehaviour
    {
        [SerializeField] private TMP_Text _label;
        
        public string Text
        {
            get => _label.text;
            set => _label.text = value;
        }
    }
}