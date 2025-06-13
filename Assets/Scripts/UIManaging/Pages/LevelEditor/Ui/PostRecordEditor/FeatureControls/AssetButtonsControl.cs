using System;
using BrunoMikoski.AnimationSequencer;
using Extensions;
using UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.FeatureControls.ViewControls;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.FeatureControls
{
    public sealed class AssetButtonsControl : MonoBehaviour
    {
        [SerializeField] private Toggle _toggle;
        [SerializeField] private AnimationSequencerController _animationSequencer;
        [SerializeField] private float _switchDuration = 0.75f;
        [SerializeField] private AssetButtonSettings[] _buttonSettings;
        [Header("Masking")] 
        [SerializeField] private RectMask2D _rectMask2D;
        [SerializeField] private float _initialHeight = 500f;
        
        private bool _expanded;

        private void Awake()
        {
            if (_rectMask2D != null)
            {
                _rectMask2D.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _initialHeight);
            }
        }

        private void OnEnable()
        {
            _toggle.onValueChanged.AddListener(Switch);
        }

        private void OnDisable()
        {
            _toggle.onValueChanged.RemoveListener(Switch);
        }

        private void OnDestroy()
        {
            _animationSequencer.Kill();
        }
        
        private void Switch(bool isOn)
        {
            if (isOn)
            {
                _animationSequencer.PlayForward();
            }
            else
            {
                _animationSequencer.PlayBackwards();
            }

            _expanded = isOn;
            
            Switch();
        }

        private void Switch()
        {
            foreach (var settings in _buttonSettings)
            {
                if (_expanded)
                {
                    settings.TargetButton.Show(_switchDuration);
                    settings.TargetButton.ShowText(_switchDuration);
                    continue;
                }

                if (settings.HideOnShrink)
                {
                    settings.TargetButton.Hide(_switchDuration);
                }

                if (settings.HideTextOnShrink)
                {
                    settings.TargetButton.HideText(_switchDuration);
                }
            }
        }
        
        [Serializable]
        private struct AssetButtonSettings
        {
            public AssetsButtonView TargetButton;
            public bool HideOnShrink;
            public bool HideTextOnShrink;
        }
    }
}
