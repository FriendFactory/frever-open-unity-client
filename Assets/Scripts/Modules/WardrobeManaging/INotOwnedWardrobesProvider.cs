using System.Collections.Generic;
using Bridge.Models.ClientServer.Assets;

namespace Modules.WardrobeManaging
{
    public interface INotOwnedWardrobesProvider
    {
        IEnumerable<WardrobeShortInfo> GetNotOwnedWardrobes();
    }
}