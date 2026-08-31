using System.Collections.Generic;
using Deucarian.Editor;
using UnityEditor;

namespace Deucarian.TemplateGameMovementFps.Editor
{
    [InitializeOnLoad]
    internal static class MovementFpsControlCenter
    {
        private const string PackageId =
            "com.deucarian.template.game.movement-fps";
        private const string ToolId = "deucarian.template.movement-fps.validation";

        static MovementFpsControlCenter()
        {
            DeucarianToolRegistry.Register(new DeucarianToolDescriptor(
                ToolId,
                "Movement FPS Validation",
                "Validate the bundled Movement FPS sample content.",
                DeucarianControlCenterArea.Developer,
                MovementFpsEditorContentValidation.ValidateContent,
                PackageId,
                searchTerms: new[] { "movement", "fps", "template", "validate" },
                order: 260));
            DeucarianControlCenterRegistry.RegisterCardProvider(new Provider());
        }

        private sealed class Provider : IDeucarianControlCenterCardProvider
        {
            public string Id => PackageId + ".control-center";

            public IEnumerable<DeucarianControlCenterCard> Capture(
                DeucarianControlCenterContext context)
            {
                bool hasBundledSample =
                    MovementFpsEditorContentValidation.HasBundledSampleContent();
                yield return new DeucarianControlCenterCard(
                    PackageId + ".developer",
                    DeucarianControlCenterArea.Developer,
                    "Movement FPS Content",
                    "Run the template-owned sample content validator.",
                    PackageId,
                    hasBundledSample
                        ? DeucarianControlCenterStatus.Success
                        : DeucarianControlCenterStatus.Info,
                    hasBundledSample
                        ? "Bundled sample source is available"
                        : "Bundled sample source is unavailable",
                    order: 260,
                    details: new[]
                    {
                        "The package-owned sample source is summarized; validation details stay in the report."
                    },
                    actions: new[]
                    {
                        new DeucarianControlCenterAction(
                            "validate-content",
                            "Validate Content",
                            MovementFpsEditorContentValidation.ValidateContent)
                    },
                    searchTerms: new[] { "movement", "fps", "sample", "validation" });
            }
        }
    }
}
