using System;
using System.IO;
using Deucarian.GameContentAuthoring.Editor;
using Deucarian.GameplayFoundation;
using Deucarian.TemplateGameMovementFps.Content;
using UnityEditor;
using UnityEngine;

namespace Deucarian.TemplateGameMovementFps.Editor
{
    public static class MovementFpsEditorContentValidation
    {
        public const string MenuPath = "Tools/Deucarian/Templates/Movement FPS/Validate Content";
        private const string ReportTitle = "Movement FPS Template Content Validation";
        private const string SampleName = "BasicMovementFpsGame";

        [MenuItem(MenuPath, priority = 331)]
        public static void ValidateContent()
        {
            ContentValidationReport report = BuildBasicSampleReport();
            string summary = GameContentAuthoringValidationReports.BuildSummary(report);
            string markdown = BuildMarkdownReport(report);

            if (report.ErrorCount > 0)
            {
                Debug.LogError(summary + Environment.NewLine + markdown);
            }
            else if (report.WarningCount > 0)
            {
                Debug.LogWarning(summary + Environment.NewLine + markdown);
            }
            else
            {
                Debug.Log(summary + Environment.NewLine + markdown);
            }
        }

        public static ContentValidationReport BuildBasicSampleReport()
        {
            var report = new ContentValidationReport();
            string sampleRoot = ResolveSampleRoot(report);
            if (!string.IsNullOrWhiteSpace(sampleRoot))
            {
                RequireSampleFile(sampleRoot, "Content/DefaultLoadout/loadout.json", report);
            }

            Merge(report, ValidateContentLibrary(BasicMovementFpsGame.CreateContentLibrary()));
            return report;
        }

        public static ContentValidationReport ValidateContentLibrary(MovementFpsContentLibrary library)
        {
            MovementFpsContentValidationReport movementFpsReport = MovementFpsContentValidator.Validate(library);
            var report = new ContentValidationReport();
            for (int index = 0; index < movementFpsReport.Errors.Count; index++)
            {
                report.AddError(movementFpsReport.Errors[index], "MovementFps.SampleContent");
            }

            return report;
        }

        public static string BuildMarkdownReport(ContentValidationReport report)
        {
            return GameContentAuthoringValidationReports.ToMarkdown(report, ReportTitle);
        }

        private static string ResolveSampleRoot(ContentValidationReport report)
        {
            UnityEditor.PackageManager.PackageInfo packageInfo =
                UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(BasicMovementFpsGame).Assembly);
            if (packageInfo == null || string.IsNullOrWhiteSpace(packageInfo.resolvedPath))
            {
                report.AddError("Could not resolve the Movement FPS template package path.", SampleName);
                return null;
            }

            string sampleRoot = Path.Combine(packageInfo.resolvedPath, "Samples~", SampleName);
            if (!Directory.Exists(sampleRoot))
            {
                report.AddError("Could not find the BasicMovementFpsGame sample at " + sampleRoot + ".", SampleName);
                return null;
            }

            return sampleRoot;
        }

        private static void RequireSampleFile(string sampleRoot, string relativePath, ContentValidationReport report)
        {
            string fullPath = Path.Combine(sampleRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(fullPath))
            {
                report.AddError("Required sample content file is missing: " + relativePath + ".", relativePath);
            }
        }

        private static void Merge(ContentValidationReport target, ContentValidationReport source)
        {
            if (target == null || source == null)
            {
                return;
            }

            for (int index = 0; index < source.Issues.Count; index++)
            {
                target.AddIssue(source.Issues[index]);
            }
        }
    }
}
