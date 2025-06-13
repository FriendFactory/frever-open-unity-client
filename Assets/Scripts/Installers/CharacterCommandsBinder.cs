using System.Collections.Generic;
using Modules.FreverUMA;
using Modules.WardrobeManaging;
using UMA.CharacterSystem;
using Zenject;

namespace Installers
{
    internal static class CharacterEditCommandsBinder
    {
        public static void BindCharacterEditCommands(this DiContainer container)
        {
            container.BindFactory<DynamicCharacterAvatar, string, AccessoryItem, AccessoryItem, ICollection<AccessoryItem>, ColorData, CharacterSlotEditCommand, CharacterSlotEditCommand.Factory>();
        }
    }
}