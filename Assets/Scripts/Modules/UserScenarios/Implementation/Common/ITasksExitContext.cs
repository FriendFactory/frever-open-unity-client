using Bridge.Models.ClientServer.Tasks;
using Modules.UserScenarios.Implementation.Common;

namespace Modules.UserScenarios.Implementation.LevelCreation
{
    public interface ITasksExitContext: IExitContext
    {
        TaskFullInfo Task { get; set; }
    }
}