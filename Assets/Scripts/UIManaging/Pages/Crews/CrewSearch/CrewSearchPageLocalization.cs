using I2.Loc;
using UnityEngine;

namespace UIManaging.Pages.Crews.CrewSearch
{
    public class CrewSearchPageLocalization : MonoBehaviour
    {
        [SerializeField] private LocalizedString _pageHeader;

        public string PageHeader => _pageHeader;
    }
}