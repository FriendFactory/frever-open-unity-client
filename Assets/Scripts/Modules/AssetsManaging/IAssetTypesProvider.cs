using System.Collections.Generic;
using Extensions;

namespace Modules.AssetsManaging
{
    public interface IAssetTypesProvider
    {
        /// <summary>
        ///   All DbModelTypes which represent Assets(body animation, vfx etc)
        /// </summary>
        IReadOnlyCollection<DbModelType> AssetTypes { get; }
    }
}