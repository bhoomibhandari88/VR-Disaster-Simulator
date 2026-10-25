using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace RescueSim.EditorTools
{
    /// <summary>
    /// Runs the EditMode or PlayMode suite from a menu item. The result callback is re-registered after every
    /// domain reload (PlayMode runs reload scripts) and the summary is written to Builds/TestSummary.txt in the project.
    /// </summary>
    [InitializeOnLoad]
    public static class RunTests
    {
        static TestRunnerApi _api;

        static RunTests()
        {
            _api = ScriptableObject.CreateInstance<TestRunnerApi>();
            _api.RegisterCallbacks(new Cb());
        }

        [MenuItem("Rescue Sim/Tests/Run EditMode")] static void Edit() => Run(TestMode.EditMode);
        [MenuItem("Rescue Sim/Tests/Run PlayMode")] static void Play() => Run(TestMode.PlayMode);

        static void Run(TestMode mode)
        {
            File.Delete(SummaryPath());
            _api.Execute(new ExecutionSettings(new Filter { testMode = mode }));
        }

        static string SummaryPath()
        {
            var dir = Path.Combine(Directory.GetCurrentDirectory(), "Builds");
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, "TestSummary.txt");
        }

        class Cb : ICallbacks
        {
            static readonly StringBuilder Fails = new StringBuilder();
            public void RunStarted(ITestAdaptor t) { Fails.Clear(); }
            public void TestStarted(ITestAdaptor t) { }
            public void TestFinished(ITestResultAdaptor r)
            {
                if (!r.HasChildren && r.TestStatus == TestStatus.Failed) Fails.AppendLine($"FAIL {r.FullName}: {r.Message}");
            }
            public void RunFinished(ITestResultAdaptor r)
            {
                var s = $"passed={r.PassCount} failed={r.FailCount} skipped={r.SkipCount} inconclusive={r.InconclusiveCount}\n{Fails}";
                File.WriteAllText(SummaryPath(), s);
                Debug.Log("[RunTests] " + s);
            }
        }
    }
}
