using Abstract;
using Bridge.Models.ClientServer.Crews;

namespace UIManaging.Pages.Crews
{
    internal abstract class CrewTabContent : BaseContextDataView<CrewModel>
    {
        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Show()
        {
            gameObject.SetActive(true);
        }
        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}