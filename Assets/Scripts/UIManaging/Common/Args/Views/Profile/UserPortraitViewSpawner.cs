using System.Collections.Generic;
using System.Linq;
using Common.Abstract;
using Components;
using UnityEngine;

namespace UIManaging.Common.Args.Views.Profile
{
    public sealed class UserPortraitViewSpawner: BaseContextPanel<List<UserPortraitModel>>
    {
        [SerializeField] private ViewSpawner _viewSpawner;
        [SerializeField] private int _maxVisibleItems = -1;
        
        protected override void OnInitialized()
        {
            var itemsCount = _maxVisibleItems == -1 ? ContextData.Count : Mathf.Min(ContextData.Count, _maxVisibleItems);
            
            _viewSpawner.Spawn<UserPortraitModel, UserPortraitView>(ContextData.Take(itemsCount));
        }
        
        protected override void BeforeCleanUp()
        {
            _viewSpawner.CleanUp<UserPortraitModel, UserPortraitView>();
        }
    }
}