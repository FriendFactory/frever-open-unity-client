using Abstract;
using TMPro;
using UnityEngine;

namespace UIManaging.Pages.Crews.Sidebar
{
    internal sealed class SidebarMembersListRoleView : BaseContextDataView<SidebarMembersListRoleModel>
    {
        [SerializeField] private TMP_Text _roleName;

        // [Inject] private CharacterThumbnailsDownloader _thumbnailsDownloader;

        protected override void OnInitialized()
        {
            _roleName.text = ContextData.Header;
        }
    }
}