using System;
using System.Threading.Tasks;
using Common.Abstract;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.EditingFlow
{
    internal abstract class BaseEditingStepView: BaseContextView<EditingStepModel>
    {
        [SerializeField] private Button _nextButton;
        [SerializeField] private EditingFlowHeaderPanel _header;
        [SerializeField] private AnimatedTransition _animatedTransition;
        
        [Inject] EditingFlowLocalization _localization;
        
        public abstract LevelEditorState State { get; }

        public event Action Shown;
        public event Action Hidden;
        public event Action MoveBack;
        public event Action MoveNext;

        public async Task TransitInAsync(bool instant = false)
        {
            if (!_animatedTransition) return;
            
            await _animatedTransition.FadeInAsync(instant);
        }
        
        public async Task TransitOutAsync(bool instant = false)
        {
            if (!_animatedTransition) return;
            
            await _animatedTransition.FadeOutAsync(instant);
        }

        protected override void OnInitialized()
        {
            _header.Initialize(new EditingFlowHeaderModel(_localization.GetHeader(State), ContextData, OnMoveBack));
        }

        protected override void BeforeCleanUp()
        {
            _header.CleanUp();
        }

        protected override void OnActivated()
        {
            if (_nextButton)
            {
                _nextButton.onClick.AddListener(OnMoveNext);
            }
        }

        protected override void OnDeactivated()
        {
            if (_nextButton)
            {
                _nextButton.onClick.RemoveListener(OnMoveNext);
            }
        }

        protected override void OnShow()
        {
            base.OnShow();
            Shown?.Invoke();
        }

        protected override void OnHide()
        {
            base.OnHide();
            Hidden?.Invoke();
        }

        protected virtual void OnMoveBack() => MoveBack?.Invoke();
        protected virtual void OnMoveNext() => MoveNext?.Invoke();
    }
}