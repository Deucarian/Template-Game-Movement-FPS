using System;
using System.IO;
using Deucarian.GameContentAuthoring.Editor;
using Deucarian.GameplayFoundation;
using Deucarian.TemplateGameMovementFps.Editor;
using NUnit.Framework;
using UnityEditor.PackageManager;

namespace Deucarian.TemplateGameMovementFps.Tests
{
    public sealed class MovementFpsEditorContentValidationTests
    {
        [Test]
        public void EditorValidationRunnerBuildsReportForSampleContent()
        {
            ContentValidationReport report = MovementFpsEditorContentValidation.BuildBasicSampleReport();
            string markdown = MovementFpsEditorContentValidation.BuildMarkdownReport(report);

            Assert.IsNotNull(report);
            StringAssert.Contains("Movement FPS Template Content Validation", markdown);
        }

        [Test]
        public void EditorValidationRunnerReportsNoSampleContentErrors()
        {
            ContentValidationReport report = MovementFpsEditorContentValidation.BuildBasicSampleReport();

            Assert.AreEqual(0, report.ErrorCount, string.Join(Environment.NewLine, report.GetMessages()));
        }

        [Test]
        public void EditorValidationRunnerReportsInvalidFixtureThroughAuthoringAdapter()
        {
            ContentValidationReport report = MovementFpsEditorContentValidation.ValidateContentLibrary(null);
            GameContentAuthoringValidationResult authoringResult =
                GameContentAuthoringValidationReports.ToAuthoringResult(report);
            string markdown = GameContentAuthoringValidationReports.ToMarkdown(report, "Invalid Movement FPS Content");

            Assert.That(report.ErrorCount, Is.GreaterThan(0));
            Assert.AreEqual(report.ErrorCount, authoringResult.ErrorCount);
            StringAssert.Contains("## Errors", markdown);
            StringAssert.Contains("Movement FPS content library is missing.", markdown);
            StringAssert.Contains("MovementFps.SampleContent", markdown);
        }

        [Test]
        public void RuntimeAssemblyDoesNotReferenceEditorOnlyAuthoringPackages()
        {
            PackageInfo packageInfo = PackageInfo.FindForAssembly(typeof(BasicMovementFpsGame).Assembly);
            Assert.IsNotNull(packageInfo);
            string runtimeAsmdefPath = Path.Combine(packageInfo.resolvedPath, "Runtime", "Deucarian.TemplateGameMovementFps.asmdef");
            string runtimeAsmdef = File.ReadAllText(runtimeAsmdefPath);

            StringAssert.DoesNotContain("GameContentAuthoring", runtimeAsmdef);
            StringAssert.DoesNotContain("Deucarian.TemplateGameMovementFps.Editor", runtimeAsmdef);
        }
    }
}
