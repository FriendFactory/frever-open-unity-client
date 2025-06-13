using TMPro;
using UnityEngine;

namespace UIManaging.Pages.PublishPage.Buttons.SendDestinationSelection
{
    internal sealed class MoreView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _countText;

        public void Display(int count)
        {
            _countText.text = $"+{count.ToString()}";
        }
    }
}
