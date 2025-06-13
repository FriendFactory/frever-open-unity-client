using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace UIManaging.Common.SelectionPanel
{
    public interface ISelectionListModel<TSelectionItemModel> where TSelectionItemModel: class, ISelectionItemModel
    {
        int MaxSelected { get; }
        IReadOnlyList<TSelectionItemModel> Items { get; }
        IReadOnlyList<TSelectionItemModel> SelectedItems { get; }

        event Action<TSelectionItemModel> ItemSelectionChanged;

        void AddItems(ICollection<TSelectionItemModel> items);
        void Clear();
    }
}