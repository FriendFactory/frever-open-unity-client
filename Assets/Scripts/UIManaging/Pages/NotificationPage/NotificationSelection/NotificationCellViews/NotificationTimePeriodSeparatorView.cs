using TMPro;
using UnityEngine;

namespace UIManaging.Pages.NotificationPage.NotificationSelection.NotificationItemModels
{
    public class NotificationTimePeriodSeparatorView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;
        
        public string Text { set => _text.text = value; }
    }
}