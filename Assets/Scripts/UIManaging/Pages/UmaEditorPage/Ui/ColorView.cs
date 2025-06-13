using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UIManaging.Pages.UmaEditorPage.Ui
{
    public class ColorView : MonoBehaviour
    {
        [FormerlySerializedAs("button")]
        public Button Button;
        [FormerlySerializedAs("thumb")]
        public Image Thumbnail;

        public void Setup(Action onClick)
        {
            Button.onClick.AddListener(()=> onClick());
        }
    }
}