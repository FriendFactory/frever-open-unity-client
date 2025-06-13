using Bridge.Authorization.Models;
using Modules.UserScenarios.Implementation.Common;
using Modules.UserScenarios.Implementation.Onboarding;
using Navigation.Args;
using Navigation.Core;

namespace Modules.UserScenarios.Implementation.UserCredentialsChanging
{
    public sealed class EditNameContext: ISignupContext, IExitContext
    {
        public string OriginalName { get; set; }
        public string SelectedName { get; set; }
        public ICredentials Credentials { get; set; }
        public PageId OpenedFromPage { get; set; }
        public bool LoginMethodUpdated { get; set; } = false;
    }
}