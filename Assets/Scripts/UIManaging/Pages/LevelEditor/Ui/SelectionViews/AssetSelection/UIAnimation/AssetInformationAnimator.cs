using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using DG.Tweening;
using EnhancedUI.EnhancedScroller;
using TMPro;
using UIManaging.Pages.LevelEditor.Ui.SelectionItems;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Tween = DG.Tweening.Tween;

namespace UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection.UIAnimation
{
    public class AssetInformationAnimator : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _selectedTitleCanvasGroup;
        [SerializeField] private TextMeshProUGUI _selectedItemTitleText;
        [SerializeField] private TextMeshProUGUI _selectedCategoryTitleText;
        [SerializeField] private float _fadeAnimDuration = 0.3f;
        [SerializeField] private float _fadeAnimDelay = 1f;

        private Tween _showInformationSequence;
        
        private void Awake() => SetupShowInformationPanelAnimation();

        private void SetupShowInformationPanelAnimation()
        {
            _showInformationSequence = DOTween.Sequence()
                .Append(_selectedTitleCanvasGroup.DOFade(1f, _fadeAnimDuration).SetEase(Ease.InQuad))
                .AppendInterval(_fadeAnimDelay)
                .Append(_selectedTitleCanvasGroup.DOFade(0f, _fadeAnimDuration).SetEase(Ease.OutQuad))
                .SetAutoKill(false)
                .Pause();
        }

        public void ShowInformationPanel(AssetSelectionItemModel model)
        {
            if (model == null || !model.IsSelected) return;
            _selectedItemTitleText.text = model.DisplayName;
            _selectedCategoryTitleText.text = model.CategoryName;
            _showInformationSequence.Restart();
        }

    }
}