using Navigation.Core;

namespace Modules.UserScenarios.Implementation.Common
{
    public interface IExitContext
    {
        PageId OpenedFromPage { get; set; }
    }
}