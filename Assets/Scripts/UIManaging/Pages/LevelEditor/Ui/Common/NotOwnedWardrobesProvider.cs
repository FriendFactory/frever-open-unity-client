using System.Collections.Generic;
using System.Linq;
using Bridge.Models.ClientServer.Assets;
using Extensions;
using JetBrains.Annotations;
using Modules.LevelManaging.Editing.LevelManagement;
using Modules.WardrobeManaging;
using UIManaging.Pages.Common.UsersManagement;

namespace UIManaging.Pages.LevelEditor.Ui.Common
{
    [UsedImplicitly]
    internal sealed class NotOwnedWardrobesProvider: INotOwnedWardrobesProvider
    {
        private readonly ILevelManager _levelManager;
        private readonly IWardrobeStore _wardrobeStore;
        private readonly LocalUserDataHolder _localUserDataHolder;

        public NotOwnedWardrobesProvider(ILevelManager levelManager, IWardrobeStore wardrobeStore, LocalUserDataHolder userDataHolder)
        {
            _levelManager = levelManager;
            _wardrobeStore = wardrobeStore;
            _localUserDataHolder = userDataHolder;
        }

        // TODO: will be good to add some caching here, because this method is called multiple times in a row from different components
        public IEnumerable<WardrobeShortInfo> GetNotOwnedWardrobes()
        {
            if (_wardrobeStore.UserHasFreeAccessToAllAssets) return Enumerable.Empty<WardrobeShortInfo>();
            
            var outfits = _levelManager.TargetEvent.CharacterController
                                          .Where(x => x.Character.GroupId == _localUserDataHolder.GroupId)
                                          .Where(x=>x.Outfit != null)
                                          .Select(x => new { x.Outfit, x.Character.GenderId })
                                          .ToList();

            return outfits.SelectMany(x=> x.Outfit.GetWardrobesForGender(x.GenderId))
                          .DistinctBy(x=> x.Id)
                          .Where(x => !_wardrobeStore.IsOwned(x))
                          .Select(x=> x.ToShortInfo());
        }
    }
}