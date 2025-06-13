using Modules.SignUp;
using TMPro;
using UnityEngine;

namespace UIManaging.Pages.OnBoardingPage.UI
{
    public class RequirementField : MonoBehaviour
    {
        private static readonly Color COLOR_NORMAL = new Color(0.75f, 0.75f, 0.75f);
        private static readonly Color COLOR_INCORRECT = new Color(0.96f, 0.17f, 0.43f);

        [SerializeField] private RequirementType _type;
        [SerializeField] private TextMeshProUGUI _description;
        [SerializeField] private GameObject _iconIdle;
        [SerializeField] private GameObject _iconCorrect;
        [SerializeField] private GameObject _iconIncorrect;
        [SerializeField] private GameObject _iconLoading;

        public RequirementType Type => _type;
        
        public void UpdateStatus(UsernameRequirementStatus status)
        {
            switch (status)
            {
                case UsernameRequirementStatus.Idle:
                case UsernameRequirementStatus.Correct:
                case UsernameRequirementStatus.Loading:
                    _description.color = COLOR_NORMAL;
                    break;
                case UsernameRequirementStatus.Incorrect:
                    _description.color = COLOR_INCORRECT;
                    break;
            }
        
            _iconIdle.SetActive(status == UsernameRequirementStatus.Idle);
            _iconCorrect.SetActive(status == UsernameRequirementStatus.Correct);
            _iconIncorrect.SetActive(status == UsernameRequirementStatus.Incorrect);
            _iconLoading.SetActive(status == UsernameRequirementStatus.Loading);
        }
    }
}