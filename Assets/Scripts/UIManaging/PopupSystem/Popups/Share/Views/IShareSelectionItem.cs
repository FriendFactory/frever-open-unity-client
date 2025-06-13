using Common.Abstract;

namespace UIManaging.PopupSystem.Popups.Share
{
    public interface IShareSelectionItem<TModel> : IContextInitializable<TModel>
        where TModel : IShareSelectionItemModel { }
}