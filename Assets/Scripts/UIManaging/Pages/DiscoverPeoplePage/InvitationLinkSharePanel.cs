using I2.Loc;
using TMPro;
using UnityEngine;

namespace UIManaging.Pages.DiscoverPeoplePage
{
    internal sealed class InvitationLinkSharePanel : BaseInvitationLinkSharePanel
    {
        [SerializeField] private TMP_Text _description;
        [Header("Localization")]
        [SerializeField] private LocalizedString _descriptionTemplate;

        protected override void OnInitialized()
        {
            base.OnInitialized();

            _description.text = string.Format(_descriptionTemplate, ContextData.InvitationCode?.SoftCurrency ?? 0);
        }
    }
}