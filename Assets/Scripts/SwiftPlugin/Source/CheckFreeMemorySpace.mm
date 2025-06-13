#include <os/proc.h>

extern "C" 
{
    uint64_t GetFreeMemory(void) 
    {
        return os_proc_available_memory();
    }
    // Unity interface function to retrieve free memory
    unsigned long long _getFreeMemory(void) 
    {
        return GetFreeMemory();
    }
}