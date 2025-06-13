#if !IGNORE_WARNINGS
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

internal sealed class CheckWarningsAfterBuild : IPostprocessBuildWithReport
{
    private const string SCRIPT_COMPILATION_STEP_NAME = "Compile scripts";
    private const string FREVER_SCRIPTS_ROOT_FOLDER = "Assets/Scripts";
    
    public int callbackOrder { get; }
    
    public void OnPostprocessBuild(BuildReport report)
    {
        var warnings = GetWarningsFromNon3rdPackagesScripts(report);
        if (warnings.Count > 0)
        {
            var warningsMessage = string.Join($"{Environment.NewLine}\\t", warnings);
            throw new BuildFailedException($"There are new warnings from Frever scripts. Warnings count: {warnings.Count}{Environment.NewLine}{warningsMessage}");
        }
    }

    private List<string> GetWarningsFromNon3rdPackagesScripts(BuildReport report)
    {
        var compilationStep = report.steps.First(x => x.name == SCRIPT_COMPILATION_STEP_NAME);
        var allWarnings = compilationStep.messages.Select(x => x.content);
        return allWarnings.Where(x => x.StartsWith(FREVER_SCRIPTS_ROOT_FOLDER)).ToList();
    }
}
#endif
