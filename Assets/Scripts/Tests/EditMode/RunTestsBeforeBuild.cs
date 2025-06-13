using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace Tests.EditMode
{
    /// <summary>
    ///     Execute EditMode test just before build target platform.
    ///     Prevents the build if any tests was failed
    /// </summary>
    internal sealed class RunTestsBeforeBuild : IPreprocessBuildWithReport
    {
        public int callbackOrder { get; }

        public void OnPreprocessBuild(BuildReport report)
        {
            var api = ScriptableObject.CreateInstance<TestRunnerApi>();

            var resultCollector = new ResultCollector();
            api.RegisterCallbacks(resultCollector);
            api.Execute(new ExecutionSettings
            {
                runSynchronously = true,
                filters = new[]
                {
                    new Filter
                    {
                        testMode = TestMode.EditMode
                    }
                }
            });

            if (resultCollector.Result.FailCount > 0)
                throw new BuildFailedException("One or more validation tests did not pass");
        }

        private sealed class ResultCollector : ICallbacks
        {
            public ITestResultAdaptor Result { get; private set; }

            public void RunStarted(ITestAdaptor testsToRun)
            {
            }

            public void RunFinished(ITestResultAdaptor result)
            {
            }

            public void TestStarted(ITestAdaptor test)
            {
            }

            public void TestFinished(ITestResultAdaptor result)
            {
                Result = result;
            }
        }
    }
}