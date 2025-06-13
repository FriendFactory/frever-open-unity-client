using System;
using System.Threading.Tasks;

public class CreateCrewStep
{
    public string Header;
    public string Description;
    public Action OnStart;
    public Action OnClose;
    public Func<Task<bool>> Validation;
    public Action OnValidationSuccess;
}
