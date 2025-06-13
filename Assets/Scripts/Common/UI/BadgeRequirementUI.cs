using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Common.UI
{
    public class BadgeRequirementUI : MonoBehaviour
    {

        [SerializeField]
        private GameObject _levelRequirementGO;
        [SerializeField]
        private TextMeshProUGUI _levelText;
        [SerializeField]
        private GameObject _badgeRequirementGO;
        [SerializeField]
        private Image _badgeImage;

        public void SetLevelRequirement(long level)
        {
            _levelRequirementGO.SetActive(true);
            _levelText.text = level.ToString();
        }
    }
}