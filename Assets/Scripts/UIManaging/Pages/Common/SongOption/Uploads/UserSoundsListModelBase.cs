using System;
using System.Collections.Generic;
using Bridge;
using Bridge.Models.Common;
using UIManaging.Common.Loaders;

namespace UIManaging.Pages.Common.SongOption.Uploads
{
    internal abstract class UserSoundsListModelBase<TModel, TItemModel> : GenericPaginationLoader<TModel> where TModel : IEntity
    {
        public string SearchQuery { get; set; } = string.Empty;
        
        protected readonly IBridge Bridge;
        
        protected UserSoundsListModelBase(IBridge bridge)
        {
            Bridge = bridge;
            ItemModels = new List<TItemModel>();
        }

        public IList<TItemModel> ItemModels { get; protected set; }
        
        protected override int DefaultPageSize => 7;
        
        protected override void OnNextPageLoaded(TModel[] page) => AddItems(page);
        protected override void OnFirstPageLoaded(TModel[] page) => AddItems(page);

        protected abstract void AddItems(IEnumerable<TModel> page);
    }
}