using System;
using Modules.SignUp;
using UIManaging.Common.Args.Views.Profile;

namespace UIManaging.Pages.EditUsername
{
    public class EditUsernameModel
    {
        private readonly ISignUpService _signUpService;
        
        public string SelectedUsername { get; set; }
        public string OriginalUsername { get; set; }
        
        public UsernameUpdateStatus UsernameUpdateStatus { get; }

        public EditUsernameModel(string selectedName, string originalName, DateTime? usernameUpdateAvailableOn)
        {
            SelectedUsername = selectedName;
            OriginalUsername = originalName;
            UsernameUpdateStatus = new UsernameUpdateStatus(usernameUpdateAvailableOn);
        }
        
        public EditUsernameModel(string nickname, DateTime? usernameUpdateAvailableOn)
        {
            OriginalUsername = nickname;
            SelectedUsername = nickname;
            UsernameUpdateStatus = new UsernameUpdateStatus(usernameUpdateAvailableOn);
        }
    }
}