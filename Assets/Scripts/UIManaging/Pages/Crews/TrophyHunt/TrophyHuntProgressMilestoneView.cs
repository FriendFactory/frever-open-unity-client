using TMPro;
using UnityEngine;

namespace UIManaging.Pages.Crews.TrophyHunt
{
    public class TrophyHuntProgressMilestoneView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;
        
        public void Init(int value)
        {
            _text.text = value.ToString();
        }
    }
}