using Abstract;
using Bridge.Models.ClientServer.Crews;
using Extensions;
using UIManaging.Pages.Common.UsersManagement;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.Crews.TrophyHunt
{
    public class TrophyHuntMemberListView : BaseContextDataView<TrophyHuntMemberListModel[]>
    {
        [SerializeField] private TrophyHuntMemberView[] _memberViews;

        protected override void OnInitialized()
        {
            _memberViews.ForEach(obj => obj.SetActive(false));
            
            var count = Mathf.Min(_memberViews.Length, ContextData.Length);            
            
            for (var i = 0; i < count; i++)
            {
                var view = _memberViews[i];
                view.Initialize(ContextData[i]);
                view.SetActive(true);
            }
        }
    }
}