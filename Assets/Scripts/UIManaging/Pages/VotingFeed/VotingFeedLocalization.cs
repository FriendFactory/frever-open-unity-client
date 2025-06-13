using I2.Loc;
using UnityEngine;

namespace UIManaging.Pages.VotingFeed
{
    public class VotingFeedLocalization : MonoBehaviour
    {
        [SerializeField] private LocalizedString _ofText;

        public string OfText => _ofText;
    }
}