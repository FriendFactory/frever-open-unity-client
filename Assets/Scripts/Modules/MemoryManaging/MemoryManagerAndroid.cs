using JetBrains.Annotations;
using UnityEngine;

namespace Modules.MemoryManaging
{
    [UsedImplicitly]
    internal sealed class MemoryManagerAndroid : IMemoryManager
    {
        //todo: FREV-14582 replace with real getting RAM usage on Android
        private const int USED_MEMORY_BY_APP_MB = 1000;//hardcoded value - how many memory do we have for other things not related to assets(measured in Publish screen, when all assets unloaded)
        
        public int GetFreeRamSizeMb()
        {
            var maxAllowed = GetMaxAllowedRamUsageInMb();
            return maxAllowed - USED_MEMORY_BY_APP_MB;
        }
        
        private int GetMaxAllowedRamUsageInMb()
        {
            var deviceRam = SystemInfo.systemMemorySize;
            if (deviceRam < 2100)
                return 1400;
            if (deviceRam < 3100)
                return 1800;
            return 2400;
        }
    }
}