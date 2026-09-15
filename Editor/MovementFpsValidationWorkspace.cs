using System;
using Deucarian.Editor;
using Deucarian.GameplayFoundation;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Controls = Deucarian.Editor.DeucarianEditorWorkspaceControls;

namespace Deucarian.TemplateGameMovementFps.Editor
{
    public sealed class MovementFpsValidationWorkspace : EditorWindow
    {
        private DeucarianEditorPageSession navigation;
        public static void Open() => DeucarianEditorWindowPages.ShowStandalone<MovementFpsValidationWorkspace>(
            "Movement FPS", new Vector2(560, 480));
        private void CreateGUI()
        {
            navigation?.Dispose();
            navigation = new DeucarianEditorPageSession(this, "movement-fps-home", _ => { });
            navigation.Navigate("deucarian.template.movement-fps.validation");
        }
        private void OnDisable() { navigation?.Dispose(); navigation = null; }
        public static IDeucarianEditorPage CreatePage()
        {
            var root = new VisualElement();
            var workspace = new DeucarianEditorWorkspace(root, Application.productName);
            workspace.Title.text = "Movement FPS";
            workspace.Subtitle.text = "Check the sample before you play.";
            DeucarianEditorWorkspaceNavigation.Populate(workspace, "deucarian.template.movement-fps.validation");
            var scroll = Controls.Scroll("movement-validation"); workspace.Content.Add(scroll);
            var panel = Controls.Panel("movement-sample"); scroll.Add(panel);
            var state = new DeucarianEditorStatusSummary("movement-state");
            state.Root.AddToClassList("dw-panel-flush"); panel.Add(state.Root);
            state.Set("Sample content", "Ready to validate.", DeucarianEditorStatus.Info, "shield-check");
            state.Root.RemoveFromClassList("dw-focus--info");
            var results = new VisualElement { name = "movement-check-results" }; panel.Add(results);
            var details = new Foldout { text = "Validation details", value = false }; details.AddToClassList("dw-foldout"); panel.Add(details);
            ContentValidationReport report = null;
            Action refresh = () =>
            {
                results.Clear(); details.Clear();
                foreach (string title in new[] { "Sample source", "Content library" })
                {
                    var row = Controls.Region(null, "dw-check-row");
                    bool source = title == "Sample source";
                    bool? okay = report == null ? (bool?)null : source ? MovementFpsEditorContentValidation.HasBundledSampleContent() : report.Succeeded;
                    row.Add(Controls.Icon(okay == null ? "circle" : okay == true ? "circle-check" : "circle-x"));
                    row.Add(Controls.Label(title, "dw-check-title"));
                    row.Add(Controls.Label(okay == null ? "Not checked" : okay == true ? "Passed" : "Action required", "dw-muted"));
                    results.Add(row);
                }
                if (report == null) details.Add(Controls.Label("Run validation to inspect the bundled sample.", "dw-muted"));
                else
                {
                    state.Set("Sample content", report.Succeeded ? "Sample validation passed." : "Sample validation needs attention.",
                        report.ErrorCount > 0 ? DeucarianEditorStatus.Error : report.WarningCount > 0 ? DeucarianEditorStatus.Warning : DeucarianEditorStatus.Success, "shield-check");
                    foreach (var issue in report.Issues)
                        details.Add(Controls.Label(issue.Severity + " · " + issue.Path + "\n" + issue.Message, "dw-note"));
                    if (report.Issues.Count == 0) details.Add(Controls.Label("No issues found.", "dw-muted"));
                }
            };
            var validate = Controls.Button("Validate sample", () =>
            { report = MovementFpsEditorContentValidation.BuildBasicSampleReport(); refresh(); }, true);
            validate.name = "movement-validate"; state.Actions.Add(validate);
            refresh();
            return new DeucarianEditorPage(root, dispose: workspace.Dispose);
        }
    }
}
