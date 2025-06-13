using System.Collections.Generic;
using Bridge.Models.ClientServer.ThemeCollection;

namespace Modules.ThemeCollection
{
    public interface IThemeCollectionService
    {
        IEnumerable<ThemeCollectionInfo> ThemeCollections { get; }
        
        void ShowCollection(long? id = null);
    }
}