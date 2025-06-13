using Bridge;
using JetBrains.Annotations;

namespace Modules.Encryption
{
    [UsedImplicitly]
    public sealed class EncryptionServiceProvider
    {
        private static IEncryptionBridge _encryptionService;
        
        public static void Initialize(IEncryptionBridge encryptionService)
        {
            _encryptionService = encryptionService;
        }

        public static IEncryptionBridge GetEncryptionService() => _encryptionService;
    }
}