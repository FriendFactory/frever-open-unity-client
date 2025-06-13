using System.Linq;
using Bridge.Models.ClientServer.Chat;
using UnityEngine;

namespace UIManaging.Pages.PublishPage.Buttons.SendDestinationSelection
{
    internal sealed class GroupChatView : MonoBehaviour
    {
        [SerializeField] private UserView[] _membersView;

        public void Display(ChatShortInfo chat, long currentUserGroupId)
        {
            var membersToShow = chat.Members.Length > 2 
                ? chat.Members.Where(x => x.Id != currentUserGroupId).ToArray()
                : chat.Members;
            for (var i = 0; i < _membersView.Length && i < membersToShow.Length; i++)
            {
                var member = membersToShow[i];
                _membersView[i].Display(member);
            }
        }
    }
}
