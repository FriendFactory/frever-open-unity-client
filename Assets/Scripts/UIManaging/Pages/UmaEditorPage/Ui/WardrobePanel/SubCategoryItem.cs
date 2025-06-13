using Bridge.Models.Common;
using UnityEngine.UI;
using WardrobeSubCategory = Bridge.Models.ClientServer.StartPack.Metadata.WardrobeSubCategory;

namespace UIManaging.Pages.UmaEditorPage.Ui.WardrobePanel
{
    public class SubCategoryItem : TextWardrobeUIItem<WardrobeSubCategory>
    {
        public Toggle Toggle { get; private set; }

        public override void Setup(WardrobeSubCategory entity)
        {
            base.Setup(entity);
            Toggle = GetComponent<Toggle>();
            _rectTransform.sizeDelta = new UnityEngine.Vector2(_textComponent.textBounds.size.x, _rectTransform.sizeDelta.y);
        }

        protected override void UpdateIsNew()
        {
            _newIcon.SetActive(Entity.HasNew);
        }
    }
}
