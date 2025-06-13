using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UIManaging.Pages.UmaEditorPage.Ui
{
    public class SlotView : MonoBehaviour
    {
        [SerializeField]
        [FormerlySerializedAs("text")]
        private TextMeshProUGUI _text;

        [SerializeField]
        [FormerlySerializedAs("button")]
        private Button _button;

        [SerializeField]
        [FormerlySerializedAs("thumb")]
        private Image _thumbnail;

        public void Setup(string slotName, Sprite thumbnail, Action onClick)
        {
            _text.text = slotName;
            this._thumbnail.sprite = thumbnail;
            _button.onClick.AddListener(()=> onClick());
        }

    }
}