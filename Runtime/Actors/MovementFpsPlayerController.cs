using System;
using System.Collections.Generic;
using Deucarian.Combat;
using Deucarian.TemplateGameMovementFps.Combat;
using Deucarian.TemplateGameMovementFps.Movement;
using Deucarian.TemplateGameMovementFps.Progression;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Deucarian.TemplateGameMovementFps.Actors
{
    [RequireComponent(typeof(WallrunnerMotor))]
    public sealed class MovementFpsPlayerController : MonoBehaviour
    {
        [SerializeField]
        private Camera viewCamera;

        [SerializeField]
        private Transform muzzle;

        [SerializeField, Min(0.01f)]
        private float mouseSensitivity = 0.08f;

        [SerializeField, Min(0f)]
        private float slideCameraDrop = 0.46f;

        [SerializeField, Min(1f)]
        private float slideCameraFollowSpeed = 16f;

        [Header("Movement Camera Feel")]
        [SerializeField]
        private bool movementCameraFeelEnabled = true;

        [SerializeField, Range(55f, 110f)]
        private float baseFieldOfView = 76f;

        [SerializeField, Min(0f)]
        private float speedFieldOfViewKick = 10f;

        [SerializeField, Min(0.1f)]
        private float speedFieldOfViewReference = 28f;

        [SerializeField, Min(0f)]
        private float slideFieldOfViewKick = 3.5f;

        [SerializeField, Min(0f)]
        private float wallrunFieldOfViewKick = 5.5f;

        [SerializeField, Min(0f)]
        private float vaultFieldOfViewKick = 4f;

        [SerializeField, Range(0f, 16f)]
        private float wallrunCameraRollDegrees = 7f;

        [SerializeField, Range(0f, 12f)]
        private float slideCameraRollDegrees = 3f;

        [SerializeField, Min(1f)]
        private float cameraFeelFollowSpeed = 12f;

        private readonly FpsInputReader _input = new FpsInputReader();
        private readonly MovementFpsPlayerLoadout _loadout = new MovementFpsPlayerLoadout();
        private readonly MovementFpsPlayerHealth _health = new MovementFpsPlayerHealth();
        private readonly MovementFpsCameraSettings _cameraSettings = new MovementFpsCameraSettings();
        private MovementFpsTemplateController _session;
        private WallrunnerMotor _motor;
        private MovementFpsCameraPresenter _camera;
        private MovementFpsPlayerCombat _combat;
        private bool _initialized;

        private MovementFpsPlayerCombat Combat => _combat ?? (_combat = new MovementFpsPlayerCombat(
            this, _session, _loadout, _camera, () => viewCamera, () => muzzle));

        public WallrunnerMotor Motor => _motor;
        public Camera ViewCamera => viewCamera;
        public double CurrentHealth => _health.CurrentHealth;
        public double MaximumHealth => _health.MaximumHealth;
        public bool IsAlive => _health.IsAlive;
        public float PickupRadius => _session == null ? 4.2f : _session.PlayerPickupRadius;
        public IReadOnlyList<MovementFpsGunRuntimeState> Guns => _loadout.Guns;
        public IReadOnlyList<MovementFpsAutoPowerRuntimeState> AutoPowers => _loadout.AutoPowers;
        public MovementFpsGunRuntimeState CurrentGun => _loadout.CurrentGun;
        public double GunDamage => Combat.GunDamage;

        public void Initialize(MovementFpsTemplateController session, MovementFpsPlayerDefinition playerDefinition,
            IReadOnlyList<MovementFpsGunDefinition> startingGuns, IReadOnlyList<MovementFpsAutoPowerDefinition> startingPowers)
        {
            _session = session;
            _loadout.Configure(startingGuns, startingPowers);
            _health.Configure(session == null ? null : session.CombatCatalog, playerDefinition.MaximumHealth);
            _motor = GetComponent<WallrunnerMotor>();
            if (viewCamera == null) viewCamera = GetComponentInChildren<Camera>();
            if (muzzle == null && viewCamera != null)
            {
                GameObject muzzleObject = new GameObject("Muzzle");
                muzzleObject.transform.SetParent(viewCamera.transform, false);
                muzzleObject.transform.localPosition = new Vector3(0.22f, -0.16f, 0.46f);
                muzzle = muzzleObject.transform;
            }
            CopyCameraSettings();
            _camera = new MovementFpsCameraPresenter(transform, () => viewCamera, _motor, _cameraSettings);
            _combat = new MovementFpsPlayerCombat(this, session, _loadout, _camera, () => viewCamera, () => muzzle);
            _initialized = true;
            ResetPlayer(Vector3.up * 1.2f, Quaternion.identity);
        }

        public void ResetPlayer(Vector3 position, Quaternion rotation)
        {
            CopyCameraSettings();
            transform.SetPositionAndRotation(position, rotation);
            _camera.ResetAngles();
            _loadout.Reset();
            _health.Reset();
            _motor.ResetMotor(position);
            _camera.ResetView();
        }

        private void OnEnable() => _input.Enable();
        private void OnDisable() => _input.Disable();
        private void OnDestroy() => _input.Dispose();

        private void Update()
        {
            if (!_initialized || _session == null || _session.IsGameplayPaused || !IsAlive) return;
            CopyCameraSettings();
            _input.Read();
            _camera.TickLook(_input.Look);
            TickMovement(_input.Move, _input.SprintHeld, _input.SlidePressed, _input.SlideHeld, _input.JumpPressed, _input.JumpHeld, Time.deltaTime);
            _loadout.TickGun(_input.FireHeld, _input.ReloadPressed, _input.NextGunPressed, Time.deltaTime, _combat.GunCadenceMultiplier, _combat);
            _loadout.TickAutoPowers(Time.deltaTime, _combat);
            _camera.TickCameraPosture(Time.deltaTime);
        }

        public Deucarian.Combat.DamageResult FireAt(MovementFpsEnemyActor enemy) => Combat.FireAt(enemy);
        public MovementFpsProjectileActor FireProjectileAtForTest(MovementFpsEnemyActor enemy) => Combat.FireProjectileAtForTest(enemy);
        public bool AddGun(MovementFpsGunDefinition definition) => _loadout.AddGun(definition);
        public bool AddAutoPower(MovementFpsAutoPowerDefinition definition) => _loadout.AddAutoPower(definition);
        public bool HasGun(string id) => _loadout.HasGun(id);
        public bool HasAutoPower(string id) => _loadout.HasAutoPower(id);
        public void TickAutoPowersForTest(float deltaTime) => _loadout.TickAutoPowers(deltaTime, _combat);

        public void ApplyDamage(double amount)
        {
            if (_health.ApplyDamage(amount)) _session.HandlePlayerDefeated();
        }

        private void CopyCameraSettings()
        {
            _cameraSettings.MouseSensitivity = mouseSensitivity;
            _cameraSettings.SlideCameraDrop = slideCameraDrop;
            _cameraSettings.SlideCameraFollowSpeed = slideCameraFollowSpeed;
            _cameraSettings.MovementCameraFeelEnabled = movementCameraFeelEnabled;
            _cameraSettings.BaseFieldOfView = baseFieldOfView;
            _cameraSettings.SpeedFieldOfViewKick = speedFieldOfViewKick;
            _cameraSettings.SpeedFieldOfViewReference = speedFieldOfViewReference;
            _cameraSettings.SlideFieldOfViewKick = slideFieldOfViewKick;
            _cameraSettings.WallrunFieldOfViewKick = wallrunFieldOfViewKick;
            _cameraSettings.VaultFieldOfViewKick = vaultFieldOfViewKick;
            _cameraSettings.WallrunCameraRollDegrees = wallrunCameraRollDegrees;
            _cameraSettings.SlideCameraRollDegrees = slideCameraRollDegrees;
            _cameraSettings.CameraFeelFollowSpeed = cameraFeelFollowSpeed;
        }

        public void TickMovement(Vector2 move, bool sprintHeld, bool slidePressed, bool slideHeld, bool jumpPressed, bool jumpHeld, float deltaTime)
        {
            _motor.Tick(move, sprintHeld, slidePressed, slideHeld, jumpPressed, jumpHeld, deltaTime);
            if (viewCamera != null)
            {
                _motor.SetLookDirection(viewCamera.transform.forward);
            }
        }
    }
}
