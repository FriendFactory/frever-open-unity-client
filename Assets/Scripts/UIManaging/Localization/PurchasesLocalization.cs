using I2.Loc;
using UnityEngine;

namespace UIManaging.Localization
{
    [CreateAssetMenu(fileName = "PurchasesLocalization", menuName = "L10N/PurchasesLocalization", order = 0)]
    public class PurchasesLocalization : ScriptableObject
    {
        [SerializeField] private LocalizedString _purchaseCompletedMessage;
        [SerializeField] private LocalizedString _purchaseFailedMessage;

        public string PurchaseCompletedMessage => _purchaseCompletedMessage; 
        public string PurchaseFailedMessage => _purchaseFailedMessage;
    }
}