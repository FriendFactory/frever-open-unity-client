using Bridge.Models.Common;

namespace Modules.AssetsManaging
{
    public abstract class LoadAssetArgs<T>: LoadArgsBase  where T: IEntity
    {
        
    }
}