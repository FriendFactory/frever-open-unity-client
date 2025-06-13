using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.Common.TabsManager
{
    public class SwitchColorTabView : TabView
    {
        [SerializeField] private Graphic _targetGraphic;
        [SerializeField] private Color32 _normalColor;
        [SerializeField] private Color32 _selectedColor;

        protected override void OnEnable()
        {
            base.OnEnable();
            RefreshVisuals();
        }

        protected override void OnBeforeOnToggleValueChangedEvent(bool isOn)
        {
            base.OnBeforeOnToggleValueChangedEvent(isOn);
            if (isOn)
            {
                _targetGraphic.CrossFadeColor(_selectedColor, 0, true, false);
            }
            else
            {
                _targetGraphic.CrossFadeColor(_normalColor, 0, true, false);
            }
        }

        public override void RefreshVisuals()
        {
            base.RefreshVisuals();
            OnBeforeOnToggleValueChangedEvent(Toggle.isOn);
        }
    }
}
