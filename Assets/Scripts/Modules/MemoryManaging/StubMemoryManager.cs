using JetBrains.Annotations;

namespace Modules.MemoryManaging
{
    [UsedImplicitly]
    internal sealed class StubMemoryManager: IMemoryManager
    {
        private readonly int _stubFreeRamMb;

        public StubMemoryManager(int stubFreeRamMb)
        {
            _stubFreeRamMb = stubFreeRamMb;
        }
        
        public int GetFreeRamSizeMb()
        {
            return _stubFreeRamMb;
        }
    }
}