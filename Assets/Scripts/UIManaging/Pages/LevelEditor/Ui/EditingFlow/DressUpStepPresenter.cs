using System;
using Common.UserBalance;
using Extensions;
using Modules.LevelManaging.Editing.LevelManagement;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.Pages.LevelEditor.Ui.EditingFlow.ShoppingCart;
using UIManaging.Pages.UmaEditorPage.Ui;
using UnityEngine;

namespace UIManaging.Pages.LevelEditor.Ui.EditingFlow
{
    internal sealed class DressUpStepPresenter : BaseEditingStepPresenter
    {
        private readonly ShoppingCartHelper _shoppingCartHelper;
        private readonly ILevelManager _levelManager;
        private readonly UserBalanceView _userBalanceView;
        private readonly LocalUserDataHolder _userData;
        private readonly UmaLevelEditorPanelModel _umaLevelEditorPanelModel;

        public DressUpStepPresenter(ILevelManager levelManager, ShoppingCartHelper shoppingCartHelper,
            UserBalanceView userBalanceView, LocalUserDataHolder userData, UmaLevelEditorPanelModel umaLevelEditorPanelModel)
        {
            _levelManager = levelManager;
            _shoppingCartHelper = shoppingCartHelper;
            _userBalanceView = userBalanceView;
            _userData = userData;
            _umaLevelEditorPanelModel = umaLevelEditorPanelModel;
        }

        protected override void OnShown()
        {
            // force to 0 in case of TemplateSetup flow [FREV-20419]
            _levelManager.EditingCharacterSequenceNumber = Model.IsFirstInFlow ? Mathf.Max(0, _levelManager.TargetCharacterSequenceNumber) : 0;

            _umaLevelEditorPanelModel.PanelOpened += OnWardrobePanelOpened;
            _umaLevelEditorPanelModel.PanelClosed += OnWardrobePanelClosed;
        }

        protected override async void OnHidden()
        {
            try
            {
                base.OnHidden();

                _umaLevelEditorPanelModel.PanelOpened -= OnWardrobePanelOpened;
                _umaLevelEditorPanelModel.PanelClosed -= OnWardrobePanelClosed;
                
                if (Model.IsExiting) return;

                if (_shoppingCartHelper.HasNotPurchasedItems) return;
                
                await _levelManager.SaveEditedOutfit(false);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        protected override void OnMoveNext()
        {
            if (!_shoppingCartHelper.HasNotPurchasedItems)
            {
                base.OnMoveNext();
                return;
            }

            _userBalanceView.Initialize(new StaticUserBalanceModel(_userData));
            _userBalanceView.SetActive(true);
            _shoppingCartHelper.ShoppingCartClosed += OnShoppingCartClosed;
            _shoppingCartHelper.Show(() =>
            {
                base.OnMoveNext();
            });
        }

        private void OnShoppingCartClosed()
        {
            _shoppingCartHelper.ShoppingCartClosed -= OnShoppingCartClosed;
            _userBalanceView.SetActive(false);
        }

        private void OnWardrobePanelOpened()
        {
            _levelManager.SetupCharactersForEditing();
        }

        private void OnWardrobePanelClosed()
        {
            _levelManager.StopCharacterEditingMode();
        }
    }
}