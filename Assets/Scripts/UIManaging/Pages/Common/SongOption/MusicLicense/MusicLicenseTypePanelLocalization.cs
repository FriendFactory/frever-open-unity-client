
using I2.Loc;
using UnityEngine;

namespace UIManaging.Pages.Common.SongOption.MusicLicense
{
    public class MusicLicenseTypePanelLocalization : MonoBehaviour
    {
        [SerializeField] private LocalizedString _publicSounds;
        [SerializeField] private LocalizedString _mySounds;

        public string PublicSounds => _publicSounds;
        public string MySounds => _mySounds;
    }
}
