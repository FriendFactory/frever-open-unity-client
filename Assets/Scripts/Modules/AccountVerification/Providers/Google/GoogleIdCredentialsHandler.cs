using System.Threading.Tasks;
using Bridge.AccountVerification.Models;
using Bridge.Authorization.Models;
using JetBrains.Annotations;
using Modules.SignUp;
using SA.Foundation.Templates;

namespace Modules.AccountVerification.Providers
{
    [UsedImplicitly]
    public sealed class GoogleIdCredentialsHandler : ICredentialsHandler
    {
        private GooglePlayCredentialsProvider _credentialsProvider;

        public LinkableCredentialType Type => LinkableCredentialType.Google;

        public async Task<CredentialsRequestResult> RequestCredentialsAsync()
        {
            _credentialsProvider ??= new GooglePlayCredentialsProvider();
            
            var tcs = new TaskCompletionSource<CredentialsRequestResult>();

            _credentialsProvider.CredentialsRequestCompleted += OnRequestCompleted;
            _credentialsProvider.CredentialsRequestFailed += OnRequestFailed;

            _credentialsProvider.RequestCredentials();

            return await tcs.Task;

            void OnRequestCompleted(GoogleAuthCredentials credentials)
            {
                _credentialsProvider.CredentialsRequestCompleted -= OnRequestCompleted;
                _credentialsProvider.CredentialsRequestFailed -= OnRequestFailed;

                tcs.SetResult(new CredentialsRequestResult(credentials.GoogleId, credentials.GoogleIdentityToken));
            }

            void OnRequestFailed(SA_Error error)
            {
                _credentialsProvider.CredentialsRequestCompleted -= OnRequestCompleted;
                _credentialsProvider.CredentialsRequestFailed -= OnRequestFailed;

                if (error.Code == 12501)
                {
                    tcs.SetResult(new CredentialsRequestResult());
                    return;
                }

                tcs.SetResult(new CredentialsRequestResult(error.FullMessage));
            }
        }
    }
}