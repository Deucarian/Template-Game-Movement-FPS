namespace Deucarian.TemplateGameMovementFps.Actors
{
    internal interface IMovementFpsLoadoutEffects
    {
        void FireGun(MovementFpsGunDefinition definition);
        void ApplyRecoil(float degrees);
        void CastAutoPower(MovementFpsAutoPowerDefinition definition);
    }
}
