using Extensions;
using Modules.Amplitude;
using JetBrains.Annotations;
using UIManaging.Common.Buttons;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.ShoppingCart
{
    public class ShoppingCartButton : BaseButton
    {
        [SerializeField] private ShoppingCartManager _shoppingCartManager;
        [SerializeField] private Text _assetsNumberText;
        
        private AmplitudeManager _amplitudeManager;
        
        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------
        
        [Inject]
        [UsedImplicitly]
        public void Construct(AmplitudeManager amplitudeManager)
        {
            _amplitudeManager = amplitudeManager;
        }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        protected override void OnEnable()
        {
            if (_amplitudeManager.IsShoppingCartFeatureEnabled())
            {
                base.OnEnable();
                UpdateAssetsCount();
                _shoppingCartManager.AssetsUpdated += UpdateAssetsCount;
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            _shoppingCartManager.AssetsUpdated -= UpdateAssetsCount;
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnClickHandler()
        {
            _shoppingCartManager.ShowCart();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void UpdateAssetsCount()
        {
            var assetsCount = _shoppingCartManager.ConfirmedAssets.Count;
            if (assetsCount > 0)
            {
                _assetsNumberText.transform.parent.SetActive(true);
                _assetsNumberText.text = assetsCount.ToString();
            }
            else
            {
                _assetsNumberText.transform.parent.SetActive(false);
            }
        }
    }
}