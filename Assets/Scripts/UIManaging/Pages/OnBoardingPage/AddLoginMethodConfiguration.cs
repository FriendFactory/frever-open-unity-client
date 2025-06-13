using System;
using Bridge.Authorization.Models;
using UIManaging.PopupSystem.Configurations;

namespace UIManaging.Pages.OnBoardingPage
{
    public sealed class AddLoginMethodConfiguration : PopupConfiguration
    {
        public Action<AddLoginMethodResult> OnComplete { get; set; }
        public Action MoveNextFailed;
        public Action<ICredentials> ValidCredentialsProvided;
        public Action<ICredentials> MoveToSignInRequested;
        public string Username { get; }
        
        public AddLoginMethodConfiguration(string username = null) : base(PopupType.AddLoginMethod, null)
        {
            Username = username;
        }
    }
}