using System.Collections.Generic;
using TMPro;
using UIManaging.Common;
using UIManaging.Common.Args.Views.Profile;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.VotingFeed
{
    public class VotingOptionView : MonoBehaviour
    {
        public LocalVideoView Video;
        public Image VideoMask;
        public Image VideoHighlight;
        public RectTransform ContainerTransform;
        public CanvasGroup OverlayCanvas;
        public Image OverlayBackground;
        public UserPortraitView UserPortrait;
        public TextMeshProUGUI UserNickname;
        public StarScoreView RatingScore;
        public Button VideoButton;
        public Button LikeButton;
        public Color OverlayColorSelected;
        public Color OverlayColorUnselected;

        public void SetOverlayColor(bool selected)
        {
            OverlayBackground.color = selected ? OverlayColorSelected : OverlayColorUnselected;
        }
    }
}