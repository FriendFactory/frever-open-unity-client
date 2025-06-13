using Bridge.Models.ClientServer.Crews;

namespace UIManaging.Pages.Crews.Sidebar
{
    internal class CrewTopListItemModel
    {
        public CrewTopInfo Crew { get; }
        public int Place { get; }
        
        public CrewTopListItemModel(CrewTopInfo crew, int place)
        {
            Crew = crew;
            Place = place;
        }
    }
}