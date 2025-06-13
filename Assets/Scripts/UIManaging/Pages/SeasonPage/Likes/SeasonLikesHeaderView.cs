using Abstract;
using TMPro;
using UnityEngine;

namespace UIManaging.Pages.SeasonPage.Likes
{
    internal sealed class SeasonLikesHeaderView : BaseContextDataView<SeasonLikesHeaderModel>
    {
        [SerializeField] private TextMeshProUGUI _likesAmountText;

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnInitialized()
        {
            _likesAmountText.text = ContextData.LikesAmount.ToString();
        }
    }
}