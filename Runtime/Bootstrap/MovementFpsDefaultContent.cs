using System;
using System.Collections.Generic;
using Deucarian.Common;
using Deucarian.Combat;
using Deucarian.RunUpgrades;
using Deucarian.TemplateGameMovementFps.Actors;
using Deucarian.TemplateGameMovementFps.Combat;
using Deucarian.TemplateGameMovementFps.Movement;
using Deucarian.TemplateGameMovementFps.Progression;
using Deucarian.TemplateGameMovementFps.Run;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Deucarian.TemplateGameMovementFps
{
    internal sealed class MovementFpsDefaultContent
    {
        internal CombatCatalog CombatCatalog { get; } = BasicMovementFpsGame.CreateCombatCatalog();
        internal MovementFpsPlayerDefinition Player { get; } = BasicMovementFpsGame.CreatePlayerDefinition();
        internal MovementFpsEnemyDefinition Enemy { get; } = BasicMovementFpsGame.CreateEnemyDefinition();
        internal MovementFpsEnemyDefinition LeapingRunner { get; } = BasicMovementFpsGame.CreateLeapingRunnerDefinition();
        internal MovementFpsEnemyDefinition BoneBulwark { get; } = BasicMovementFpsGame.CreateBoneBulwarkDefinition();
        internal MovementFpsEnemyDefinition ChoirOgre { get; } = BasicMovementFpsGame.CreateChoirOgreDefinition();
        internal MovementFpsWaveDefinition Wave { get; } = BasicMovementFpsGame.CreatePrototypeWaveDefinition();
        internal MovementFpsGunDefinition Carbine { get; } = BasicMovementFpsGame.CreateCarbineDefinition();
        internal MovementFpsGunDefinition RiftLauncher { get; } = BasicMovementFpsGame.CreateRiftLauncherDefinition();
        internal MovementFpsAutoPowerDefinition OrbitPulse { get; } = BasicMovementFpsGame.CreateOrbitPulseDefinition();
        internal MovementFpsAutoPowerDefinition ChainBolt { get; } = BasicMovementFpsGame.CreateChainBoltDefinition();
        internal MovementFpsAutoPowerDefinition GroundRift { get; } = BasicMovementFpsGame.CreateGroundRiftDefinition();

        internal MovementFpsEnemyDefinition FindEnemy(string id)
        {
            if (Enemy.Id == id)
            {
                return Enemy;
            }

            if (LeapingRunner.Id == id)
            {
                return LeapingRunner;
            }

            if (BoneBulwark.Id == id)
            {
                return BoneBulwark;
            }

            if (ChoirOgre.Id == id)
            {
                return ChoirOgre;
            }

            return default;
        }
    }
}
