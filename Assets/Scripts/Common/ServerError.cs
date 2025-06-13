using System;
using JetBrains.Annotations;

namespace Common
{
            
    [Serializable]
    public class ServerError
    {
        [UsedImplicitly] public string ErrorCode { get; set; }
    }
}