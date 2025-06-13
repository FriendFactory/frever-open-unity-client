using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Common
{
    public class StarScoreView : MonoBehaviour
    {
        public TextMeshProUGUI Score;
        public List<Image> Stars;
        public Color ColorFull;
        public Color ColorEmpty;

        public void SetRating(float value)
        {
            Score.text = value.ToString("0.00");
            
            for (var i = 0; i < Stars.Count; i++)
            {
                Stars[i].color = i + 1 <= value ? ColorFull : ColorEmpty;
            }
        }
    }
}