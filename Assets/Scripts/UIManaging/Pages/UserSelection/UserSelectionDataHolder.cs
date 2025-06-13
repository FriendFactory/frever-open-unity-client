using System.Linq;
using Abstract;
using Bridge.Services.UserProfile;

namespace UIManaging.Pages.UserSelection
{
    public class UserSelectionDataHolder: BaseContextDataView<UserSelectionPanelModel>
    {
        public UserSelectionItemModel GetSelectionItem(Profile profile)
        {
            return ContextData.Items.FirstOrDefault(item => item.ShortProfile.Id == profile.MainGroupId);
        }
        
        protected override void OnInitialized()
        {
            
        }
    }
}