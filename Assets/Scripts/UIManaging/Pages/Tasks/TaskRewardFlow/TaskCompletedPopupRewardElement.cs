using TMPro;
using UnityEngine;

namespace UIManaging.Pages.Tasks.RewardPopUp
{
    public sealed class TaskCompletedPopupRewardElement : MonoBehaviour
    {
        [SerializeField] private TMP_Text _amountText;
        
        public void Show(string amount)
        {
            _amountText.text = amount;
        }
    }
}