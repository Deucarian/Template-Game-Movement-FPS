using System.Linq;
using Deucarian.Editor;
using Deucarian.TemplateGameMovementFps.Editor;
using NUnit.Framework;

namespace Deucarian.TemplateGameMovementFps.Tests
{
    public sealed class MovementFpsControlCenterTests
    {
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