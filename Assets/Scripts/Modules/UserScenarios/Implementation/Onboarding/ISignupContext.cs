using Bridge.Authorization.Models;
using Navigation.Args;

namespace Modules.UserScenarios.Implementation.Onboarding
{
    public interface ISignupContext
    {
        ICredentials Credentials { get; set; }
    }
}