using TMPro;
using UnityEngine;

namespace TipsManagment
{
    public class PseudoMessageItem : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _textUI;

        private string _messageText;

        public void Init(string text)
        {
            _messageText = text;
            _textUI.text = text;
        }
    }
}