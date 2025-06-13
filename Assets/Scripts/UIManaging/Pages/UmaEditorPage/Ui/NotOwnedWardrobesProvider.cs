using System.Collections.Generic;
using System.Linq;
using Bridge.Models.ClientServer.Assets;
using Extensions;
using JetBrains.Annotations;
using Modules.FreverUMA;
using Modules.WardrobeManaging;

namespace UIManaging.Pages.UmaEditorPage.Ui
{
    [UsedImplicitly]
    internal sealed class NotOwnedWardrobesProvider: INotOwnedWardrobesProvider
    {
        private readonly ICharacterEditor _characterEditor;
        private readonly IWardrobeStore _wardrobeStore;

        public NotOwnedWardrobesProvider(ICharacterEditor characterEditor, IWardrobeStore wardrobeStore)
        {
            _characterEditor = characterEditor;
            _wardrobeStore = wardrobeStore;
        }

        public IEnumerable<WardrobeShortInfo> GetNotOwnedWardrobes()
        {
            if (_wardrobeStore.UserHasFreeAccessToAllAssets) return Enumerable.Empty<WardrobeShortInfo>();
            return _characterEditor.GetCharacterWardrobes()
                                   .Where(x => !_wardrobeStore.IsOwned(x))
                                   .Select(x => x.ToShortInfo());
        }
    }
}