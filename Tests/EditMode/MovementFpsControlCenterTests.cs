using System.Linq;
using Deucarian.Editor;
using Deucarian.TemplateGameMovementFps.Editor;
using NUnit.Framework;
using UnityEngine.UIElements;

namespace Deucarian.TemplateGameMovementFps.Tests
{
    public sealed class MovementFpsControlCenterTests
    {
        [Test]
        public void NativeValidationPageStartsUncheckedAndPreservesTheExplicitAction()
        {
            using (var page = MovementFpsValidationWorkspace.CreatePage())
            {
                Assert.That(page.Root.Query<IMGUIContainer>().ToList(), Is.Empty);
                Assert.That(page.Root.Q<Button>("movement-validate"), Is.Not.Null);
                Assert.That(page.Root.Q("movement-state").ClassListContains("dw-panel-flush"), Is.True);
                Assert.That(page.Root.Q("movement-state").ClassListContains("dw-focus--info"), Is.False,
                    "Before validation the shield is neutral, not a validation result.");
                Assert.That(page.Root.Query<Label>().ToList().Count(value => value.text == "Not checked"), Is.EqualTo(2));
            }
        }

        [Test]
        public void ContributionUsesBundledSampleStateAndStableValidationAction()
        {
            DeucarianControlCenterSnapshot snapshot =
                DeucarianControlCenterSnapshotBuilder.Capture();
            DeucarianToolDescriptor tool = snapshot.Tools.Single(candidate =>
                candidate.Id == "deucarian.template.movement-fps.validation");
            DeucarianControlCenterCard card = snapshot.Cards.Single(candidate =>
                candidate.Id == "com.deucarian.template.game.movement-fps.developer");

            Assert.That(tool.Area, Is.EqualTo(DeucarianControlCenterArea.Developer));
            Assert.That(card.Area, Is.EqualTo(DeucarianControlCenterArea.Developer));
            Assert.That(
                card.Status,
                Is.EqualTo(MovementFpsEditorContentValidation.HasBundledSampleContent()
                    ? DeucarianControlCenterStatus.Success
                    : DeucarianControlCenterStatus.Info));
            CollectionAssert.AreEqual(
                new[] { "validate-content" },
                card.Actions.Select(action => action.Id).ToArray());
        }
    }
}
