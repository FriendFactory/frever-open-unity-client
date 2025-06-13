using I2.Loc;
using UnityEngine;

namespace UIManaging.Pages.AppSettingsPage.UI
{
    internal sealed class AdvancedSettingsPageLoc : MonoBehaviour
    {
        [SerializeField] private LocalizedString _pageHeader;
        [Space]
        [SerializeField] private LocalizedString _settingsHeader;
        [Space]
        [SerializeField] private LocalizedString _hiResRenderTitle;
        [SerializeField] private LocalizedString _hiResRenderDesc;
        [SerializeField] private LocalizedString _hiResRenderAlert1;
        [SerializeField] private LocalizedString _hiResRenderAlert2;
        [Space]
        [SerializeField] private LocalizedString _hiResExportTitle;
        [SerializeField] private LocalizedString _hiResExportDesc;
        [SerializeField] private LocalizedString _hiResExportAlert1;
        [SerializeField] private LocalizedString _hiResExportAlert2;
        [Space]
        [SerializeField] private LocalizedString _optiMemTitle;
        [SerializeField] private LocalizedString _optiMemDesc;
        [SerializeField] private LocalizedString _optiMemAlert;
        [Space]
        [SerializeField] private LocalizedString _commonAlertTitle;
        [SerializeField] private LocalizedString _commonAlertButton;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public string PageHeader => _pageHeader;

        public string SettingsHeader => _settingsHeader;

        public string HiResRenderTitle => _hiResRenderTitle;
        public string HiResRenderDesc => _hiResRenderDesc;
        public string HiResRenderAlert1 => _hiResRenderAlert1;
        public string HiResRenderAlert2 => _hiResRenderAlert2;

        public string HiResExportTitle => _hiResExportTitle;
        public string HiResExportDesc => _hiResExportDesc;
        public string HiResExportAlert1 => _hiResExportAlert1;
        public string HiResExportAlert2 => _hiResExportAlert2;

        public string OptiMemTitle => _optiMemTitle;
        public string OptiMemDesc => _optiMemDesc;
        public string OptiMemAlert => _optiMemAlert;

        public string CommonAlertTitle => _commonAlertTitle;
        public string CommonAlertButton => _commonAlertButton;
    }
}