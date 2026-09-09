using UnityEngine;

namespace Deucarian.TemplateGameMovementFps.Movement
{
    public enum WallrunnerMovementState
    {
        Grounded,
        Sprinting,
        Sliding,
        Airborne,
        Wallrunning,
        Vaulting
    }

    public enum VaultTraversalStyle
    {
        None,
        Flow,
        SafetyMantle
    }

    public enum WallrunTraversalStyle
    {
        Horizontal,
        Vertical
    }

}
