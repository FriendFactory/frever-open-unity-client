using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;

namespace EditorTools
{
    internal class GitRepoInfo : IDisposable
    {
        private bool _disposed;
        private readonly Process _gitProcess;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public string CommitHash => RunCommand("rev-parse HEAD");

        public string BranchName => RunCommand("rev-parse --abbrev-ref HEAD");

        public string TrackedBranchName => RunCommand("rev-parse --abbrev-ref --symbolic-full-name @{u}");

        public bool HasUnpushedCommits => !string.IsNullOrWhiteSpace(RunCommand("log @{u}..HEAD"));

        public bool HasUncommittedChanges => !string.IsNullOrWhiteSpace(RunCommand("status --porcelain"));

        private bool IsGitRepository => !string.IsNullOrWhiteSpace(RunCommand("log -1"));

        //---------------------------------------------------------------------
        // Static
        //---------------------------------------------------------------------

        public static GitRepoInfo GetRepoInfo()
        {
            return GetRepoInfoForPath(Application.dataPath);
        }

        public static GitRepoInfo GetRepoInfoForPath(string path, string gitPath = null)
        {
            var repositoryInformation = new GitRepoInfo(path, gitPath);
            return repositoryInformation.IsGitRepository ? repositoryInformation : null;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public IEnumerable<string> Log
        {
            get
            {
                var skip = 0;
                while (true)
                {
                    var entry = RunCommand($"log --skip={skip++} -n1");
                    if (string.IsNullOrWhiteSpace(entry)) yield break;
                    yield return entry;
                }
            }
        }

        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;
            _gitProcess.Dispose();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private GitRepoInfo(string path, string gitPath)
        {
            var processInfo = new ProcessStartInfo
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                FileName = Directory.Exists(gitPath) ? gitPath : "git",
                CreateNoWindow = true,
                WorkingDirectory = (path != null && Directory.Exists(path)) ? path : Environment.CurrentDirectory
            };

            _gitProcess = new Process {StartInfo = processInfo};
        }

        private string RunCommand(string args)
        {
            _gitProcess.StartInfo.Arguments = args;
            _gitProcess.Start();
            var output = _gitProcess.StandardOutput.ReadToEnd().Trim();
            _gitProcess.WaitForExit();
            return output;
        }
    }
}
