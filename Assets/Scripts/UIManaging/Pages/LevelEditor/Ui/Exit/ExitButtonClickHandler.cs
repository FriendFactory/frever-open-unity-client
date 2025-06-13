using System.Threading.Tasks;
using Navigation.Args;

namespace UIManaging.Pages.LevelEditor.Ui.Exit
{
    internal interface IExitButtonClickHandler
    {
        ExitButtonBehaviour ExitButtonBehaviour { get; }
        Task HandleClickAsync();
    }
    
    internal abstract class ExitButtonClickHandler: IExitButtonClickHandler
    {
        public abstract ExitButtonBehaviour ExitButtonBehaviour { get; }
        
        public abstract Task HandleClickAsync();
    }
}