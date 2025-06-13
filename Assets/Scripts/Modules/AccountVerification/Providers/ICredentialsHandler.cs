using System.Threading.Tasks;
using Bridge.AccountVerification.Models;

namespace Modules.AccountVerification.Providers
{
    public interface ICredentialsHandler
    {
        LinkableCredentialType Type { get; }
        Task<CredentialsRequestResult> RequestCredentialsAsync();
    }
}