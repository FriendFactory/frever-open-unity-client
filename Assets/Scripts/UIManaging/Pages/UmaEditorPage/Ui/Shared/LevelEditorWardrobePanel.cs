using System.Linq;
using UIManaging.Pages.UmaEditorPage.Ui.WardrobePanel;

namespace UIManaging.Pages.UmaEditorPage.Ui.Shared
{
    public sealed class LevelEditorWardrobePanel: WardrobePanelUIBase
    {
        public override void Show()
        {
            var categoryType = ClothesCabinet.CategoryTypes.First(x => x.Id == StartCategoryTypeId);
            SwitchCategoryType(categoryType, StartCategoryId);
            base.Show();
        }
    }
}