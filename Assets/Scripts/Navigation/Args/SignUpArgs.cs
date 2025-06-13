using System;
using Bridge.Authorization.Models;
using Navigation.Core;

namespace Navigation.Args
{
    public class SignUpArgs : PageArgs
    {
        public override PageId TargetPage => PageId.SignUp;

        public Action<ICredentials> ValidCredentialsProvided;
        public Action MoveBackRequested;
        public Action MoveNextFailed;
        public Action<ICredentials> MoveToSignInRequested;
    }
}