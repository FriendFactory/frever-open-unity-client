using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace Modules.MemoryManaging
{
#if UNITY_IOS
    [UsedImplicitly]
    internal sealed class MemoryManagerIOS: IMemoryManager
    {
        public int GetFreeRamSizeMb()
        {
            return (int)(_getFreeMemory() * 0.000001f); //kb to mb
        }
        
        [DllImport("__Internal")]
        private static extern ulong _getFreeMemory();
    }
#endif
}