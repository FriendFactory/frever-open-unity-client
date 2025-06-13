namespace UIManaging.Pages.Crews.Sidebar
{
    internal sealed class SidebarMotdPanelModel
    {
        public long UserRoleId;
        public long CrewId;
        public string Message;

        public SidebarMotdPanelModel(long userRole, long crewId, string message)
        {
            UserRoleId = userRole;
            CrewId = crewId;
            Message = message;
        }
    }
}