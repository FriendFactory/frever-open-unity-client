using Bridge.Models.ClientServer.Crews;

namespace UIManaging.Pages.Crews.Sidebar
{
    internal class CrewTopListModel
    {
        public CrewTopListModel(CrewShortInfo[] crews)
        {
            Crews = crews;
        }

        private CrewShortInfo[] Crews { get; }
    }
}