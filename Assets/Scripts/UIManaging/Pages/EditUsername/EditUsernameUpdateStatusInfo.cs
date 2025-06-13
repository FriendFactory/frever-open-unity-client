using Common.Abstract;
using TMPro;
using UIManaging.Common.Args.Views.Profile;
using UnityEngine;

namespace UIManaging.Pages.EditUsername
{
    public class EditUsernameUpdateStatusInfo: BaseContextPanel<UsernameUpdateStatus>
    {
        [SerializeField] private TMP_Text _info;
        [SerializeField] private EditUsernameLocalization _localization;

        protected override bool IsReinitializable => true;

        protected override void OnInitialized()
        {
            if (ContextData.CanUpdate) return;

            _info.text = _localization.GetNextUsernameUpdateInfo(ContextData.DaysUntilNextUpdate);
        }
    }
}