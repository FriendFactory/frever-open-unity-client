using System;
using UIManaging.SnackBarSystem.Configurations;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.SnackBarSystem.SnackBars
{
    internal class StyleBattleResultCompletedSnackbar : SnackBar<StyleBattleResultCompletedSnackbarConfiguration>
    {
        [SerializeField] private Button _viewButton;
        
        private Action _onViewButtonClick;

        public override SnackBarType Type => SnackBarType.StyleBattleResultCompleted;

        public override void OnShown()
        {
            _viewButton.onClick.RemoveAllListeners();
            _viewButton.onClick.AddListener(ViewButtonClick);
            
            base.OnShown();
        }
        
        protected override void OnConfigure(StyleBattleResultCompletedSnackbarConfiguration configuration)
        {
            _onViewButtonClick = configuration.OnViewButtonClick;
        }

        private void ViewButtonClick()
        {
            _onViewButtonClick?.Invoke();
        }
    }
}