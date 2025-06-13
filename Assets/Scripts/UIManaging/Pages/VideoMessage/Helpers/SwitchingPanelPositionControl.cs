using System;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.VideoMessage.Helpers
{
    internal sealed class SwitchingPanelPositionControl : MonoBehaviour
    {
        [SerializeField] private Button _switchEditorButton;
        [SerializeField] private int _verticalDistance;

        private void OnEnable()
        {
            var canvas = _switchEditorButton.GetComponent<Image>().canvas;
            var pos = transform.position;
            pos.y = _switchEditorButton.transform.position.y - _verticalDistance * canvas.scaleFactor;
            transform.position = pos;
        }
    }
}
