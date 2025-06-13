using Extensions;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.ConfirmAssetChangesHandlers
{
    internal abstract class ConfirmAssetChangesHandler
    {
        public abstract DbModelType Type { get; }
        public abstract void Run();
    }
}
