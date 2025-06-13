using System.Collections;
using Extensions;
using UI.UIAnimators;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.Common.SongOption.MusicLicense
{
    internal sealed class MusicLicencePanelAnimator: MonoBehaviour
    {
        [SerializeField] private MusicLicensePanelToggle _toggle;
        [SerializeField] private MusicLicenseTypePanel _panel;
        [SerializeField] private ParallelUiAnimationPlayer _animationPlayer;
        [Header("Sorting")] 
        [SerializeField] private Canvas _headerCanvas;
        [SerializeField] private int _overrideSortingOrder = 2;
        [SerializeField] private Image _headerBackground;
        
        private bool IsTransitioning { get; set; }

        private int _defaultSortingOrder;

        private void Awake()
        {
            _defaultSortingOrder = _headerCanvas.sortingOrder; 
            
            _panel.SetActive(false);
            _headerBackground.SetActive(false);
            _animationPlayer.PlayHideAnimationInstant();
        }

        private void OnEnable()
        {
            _toggle.ValueChanged += OnToggleValueChanged;
            
            _panel.ClickedOutside += OnClickedOutside;
        }

        private void OnDisable()
        {
            _toggle.ValueChanged -= OnToggleValueChanged;
            
            _panel.ClickedOutside -= OnClickedOutside;
        }
        
        private void OnToggleValueChanged(bool isOn)
        {
            if (IsTransitioning) return;
            
            IsTransitioning = true;
            
            if (isOn)
            {
                _headerBackground.SetActive(true);
                _panel.SetActive(true);
                
                _headerCanvas.overrideSorting = true;
                _headerCanvas.sortingOrder = _overrideSortingOrder;

                _animationPlayer.PlayShowAnimation(() => IsTransitioning = false);
            }
            else
            {
                StartCoroutine(HideWithDelay());
            }
        }
        
        private void OnClickedOutside() => _toggle.SetState(false);

        private IEnumerator HideWithDelay()
        {
            // need to wait a few frames, because canvas may be rebuilt due to the GO toggling 
            // and playing animation in parallel produces visual glitches
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            _animationPlayer.PlayHideAnimation(() =>
            {
                _headerCanvas.overrideSorting = false;
                _headerCanvas.sortingOrder = _defaultSortingOrder;

                _panel.SetActive(false);
                _headerBackground.SetActive(false);

                IsTransitioning = false;
            });
        }
    }
}