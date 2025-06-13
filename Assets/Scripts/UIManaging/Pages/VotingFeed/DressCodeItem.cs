using TMPro;
using UnityEngine;

namespace UIManaging.Pages.VotingFeed
{
    public class DressCodeItem : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _textName;

        public void SetName(string dressCodeName)
        {
            _textName.text = dressCodeName;
        }
    }
}