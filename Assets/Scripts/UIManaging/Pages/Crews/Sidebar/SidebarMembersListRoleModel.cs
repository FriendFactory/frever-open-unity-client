using Modules.Crew;

namespace UIManaging.Pages.Crews.Sidebar
{
    public sealed class SidebarMembersListRoleModel : SidebarMembersListItemModel
    {
        private const string HEADER_FORMAT = "{0} – {1}";
        
        public readonly string Header;

        public SidebarMembersListRoleModel(long roleId, string roleName, int userCount)
        {
            Header = string.Format(HEADER_FORMAT, roleName, userCount);
        }
    }
}