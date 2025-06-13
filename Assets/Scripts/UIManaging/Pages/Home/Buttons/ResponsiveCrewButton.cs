using Navigation.Args;
using UIManaging.Pages.Common.UsersManagement;
using Zenject;

namespace UIManaging.Pages.Home
{
    public class ResponsiveCrewButton : ResponsiveHomePageButtonBase
    {
        [Inject] private LocalUserDataHolder _dataHolder;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            Interactable = true;
        }

        protected override void OnClick()
        {
            if (_dataHolder.UserProfile.CrewProfile == null)
            {
                Manager.MoveNext(new CrewSearchPageArgs());
            }
            else
            {
                Manager.MoveNext(new CrewPageArgs());
            }
            
            Interactable = false;
        }
    }
}