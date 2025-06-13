using Extensions;
using Models;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.EventStateChecker
{
    public interface IAssetStateComparer
    {
        bool AssetStateChanged { get; }
        DbModelType Type { get;}
        void SaveState(Event targetEvent);
        void Check(Event targetEvent);
    }
    
    public abstract class BaseAssetStateComparer : IAssetStateComparer
    {
        public bool AssetStateChanged { get; private set; }
        public abstract DbModelType Type { get;}

        public abstract void SaveState(Event targetEvent);

        public void Check(Event targetEvent)
        {
            AssetStateChanged = CheckInternal(targetEvent);
        }
        protected abstract bool CheckInternal(Event targetEvent);
    }
}
