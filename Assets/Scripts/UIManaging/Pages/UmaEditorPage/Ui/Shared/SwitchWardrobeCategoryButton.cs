using Modules.WardrobeManaging;
using UIManaging.Common.Buttons;
using UIManaging.Pages.UmaEditorPage.Ui.WardrobePanel;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.UmaEditorPage.Ui.Shared
{
    [RequireComponent(typeof(Button))]
    public sealed class SwitchWardrobeCategoryButton: BaseButton 
    {
        [SerializeField] private WardrobeCategoryData _categoryData;
        [SerializeField] private WardrobePanelUIBase _wardrobePanel;

        protected override void OnClickHandler()
        {
            _wardrobePanel.SwitchCategory(_categoryData.CategoryId, _categoryData.SubCategoryId);
        }


        protected override void OnEnable()
        {
            base.OnEnable();
            
            _wardrobePanel.CategoryChanged += RefreshState;
            var subcategoryId = _wardrobePanel.CurrentSubCategory?.Id ?? 0;
            RefreshState(_wardrobePanel.CurrentCategory.Id, subcategoryId);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            
            _wardrobePanel.CategoryChanged -= RefreshState;
        }
        
        private void RefreshState(long categoryId, long subCategoryId)
        {
            var isSelected = _categoryData.CategoryId == categoryId && _categoryData.SubCategoryId == subCategoryId;
            Button.interactable = !isSelected;
        }
    }
}