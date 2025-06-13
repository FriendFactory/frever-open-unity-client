namespace UIManaging.Pages.Crews
{
    internal class RoleControlledVisibilitySelectable : RoleControlledSelectable
    {
        protected override void Refresh()
        {
            _selectable.gameObject.SetActive(_crewService.LocalUserMemberData.RoleId <= (long)_unlocksAt);
        }
    }
}