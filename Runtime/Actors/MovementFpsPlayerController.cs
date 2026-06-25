using Deucarian.Combat;
using Deucarian.TemplateGameMovementFps.Movement;
using UnityEngine;

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
        private MovementFpsTemplateController _session;
        private MovementFpsPlayerDefinition _playerDefinition;
        private MovementFpsGunDefinition _gun;
        private WallrunnerMotor _motor;
        private HealthState _health;
        private Vector3 _cameraStandLocalPosition;
        private float _pitch;
        private float _cameraRoll;
        private float _fireCooldown;
        private bool _initialized;

        public WallrunnerMotor Motor => _motor;
        public Camera ViewCamera => viewCamera;
        public double CurrentHealth => _health == null ? 0d : _health.CurrentHealth;
        public double MaximumHealth => _health == null ? 0d : _health.MaximumHealth;
        public bool IsAlive => _health != null && _health.IsAlive;
        public float PickupRadius => _session == null ? 4.2f : _session.PlayerPickupRadius;
        public double GunDamage => _gun.Damage + (_session == null ? 0d : _session.Progression.GunDamageBonus);

        public void Initialize(MovementFpsTemplateController session, MovementFpsPlayerDefinition playerDefinition, MovementFpsGunDefinition gunDefinition)
        {
            _session = session;
            _playerDefinition = playerDefinition;
            _gun = gunDefinition;
            _motor = GetComponent<WallrunnerMotor>();
            if (viewCamera == null)
            {
                viewCamera = GetComponentInChildren<Camera>();
            }

            if (muzzle == null && viewCamera != null)
            {
                GameObject muzzleObject = new GameObject("Muzzle");
                muzzleObject.transform.SetParent(viewCamera.transform, false);
                muzzleObject.transform.localPosition = new Vector3(0.22f, -0.16f, 0.46f);
                muzzle = muzzleObject.transform;
            }

            _cameraStandLocalPosition = viewCamera == null ? Vector3.zero : viewCamera.transform.localPosition;
            if (viewCamera != null)
            {
                viewCamera.fieldOfView = baseFieldOfView;
            }

            _initialized = true;
            ResetPlayer(Vector3.up * 1.2f, Quaternion.identity);
        }

        public void ResetPlayer(Vector3 position, Quaternion rotation)
        {
            transform.SetPositionAndRotation(position, rotation);
            _pitch = 0f;
            _cameraRoll = 0f;
            _fireCooldown = 0f;
            _health = new HealthState(new CombatantId("combatant.player"), _playerDefinition.MaximumHealth, _playerDefinition.MaximumHealth);
            _motor.ResetMotor(position);
            if (viewCamera != null)
            {
                viewCamera.transform.localPosition = _cameraStandLocalPosition == Vector3.zero ? new Vector3(0f, 0.62f, 0f) : _cameraStandLocalPosition;
                viewCamera.transform.localRotation = Quaternion.identity;
                viewCamera.fieldOfView = baseFieldOfView;
            }
        }

        private void OnEnable()
        {
            _input.Enable();
        }

        private void OnDisable()
        {
            _input.Disable();
        }

        private void Update()
        {
            if (!_initialized || _session == null || _session.IsGameplayPaused || !IsAlive)
            {
                return;
            }

            _input.Read();
            TickLook(_input.Look);
            TickMovement(_input.Move, _input.SprintHeld, _input.SlidePressed, _input.SlideHeld, _input.JumpPressed, _input.JumpHeld, Time.deltaTime);
            TickGun(_input.FireHeld, Time.deltaTime);
            TickCameraPosture(Time.deltaTime);
        }

        public void TickMovement(Vector2 move, bool sprintHeld, bool slidePressed, bool slideHeld, bool jumpPressed, bool jumpHeld, float deltaTime)
        {
            _motor.Tick(move, sprintHeld, slidePressed, slideHeld, jumpPressed, jumpHeld, deltaTime);
            if (viewCamera != null)
            {
                _motor.SetLookDirection(viewCamera.transform.forward);
            }
        }

        public Deucarian.Combat.DamageResult FireAt(MovementFpsEnemyActor enemy)
        {
            if (enemy == null)
            {
                return null;
            }

            return enemy.ApplyDamage(GunDamage, new CombatantId("combatant.player"));
        }

        public void ApplyDamage(double amount)
        {
            if (_health == null || !_health.IsAlive)
            {
                return;
            }

            DamageRequest request = new DamageRequest(
                _health.Id,
                new[] { new DamageComponent(BasicMovementFpsGame.KineticDamageType, amount) },
                sourceId: new CombatantId("combatant.enemy"));
            DamageResolver.Apply(_session.CombatCatalog, _health, null, request);
            if (!_health.IsAlive)
            {
                _session.HandlePlayerDefeated();
            }
        }

        private void TickLook(Vector2 lookDelta)
        {
            Vector2 look = lookDelta * mouseSensitivity;
            transform.Rotate(Vector3.up, look.x, Space.World);
            _pitch = Mathf.Clamp(_pitch - look.y, -84f, 84f);
            ApplyCameraLocalRotation();
        }

        private void TickGun(bool fireHeld, float deltaTime)
        {
            _fireCooldown = Mathf.Max(0f, _fireCooldown - deltaTime);
            if (!fireHeld || _fireCooldown > 0f)
            {
                return;
            }

            float cadenceMultiplier = 1f + (float)(_session == null ? 0d : _session.Progression.GunCadenceMultiplier);
            _fireCooldown = _gun.FireIntervalSeconds / Mathf.Max(0.1f, cadenceMultiplier);
            FireHitscan();
            _pitch = Mathf.Clamp(_pitch - _gun.RecoilPitchDegrees, -84f, 84f);
        }

        private void FireHitscan()
        {
            if (viewCamera == null)
            {
                return;
            }

            Vector3 origin = muzzle == null ? viewCamera.transform.position : muzzle.position;
            Vector3 direction = viewCamera.transform.forward;
            if (_gun.SpreadDegrees > 0f)
            {
                Vector2 spread = Random.insideUnitCircle * _gun.SpreadDegrees;
                direction = Quaternion.Euler(-spread.y, spread.x, 0f) * direction;
            }

            RaycastHit[] hits = Physics.RaycastAll(origin, direction, _gun.Range, ~0, QueryTriggerInteraction.Ignore);
            float nearestDistance = float.MaxValue;
            MovementFpsEnemyActor target = null;
            for (int index = 0; index < hits.Length; index++)
            {
                RaycastHit hit = hits[index];
                if (hit.collider == null || hit.collider.transform.IsChildOf(transform))
                {
                    continue;
                }

                MovementFpsEnemyActor enemy = hit.collider.GetComponentInParent<MovementFpsEnemyActor>();
                if (enemy != null && hit.distance < nearestDistance)
                {
                    nearestDistance = hit.distance;
                    target = enemy;
                }
            }

            if (target != null)
            {
                FireAt(target);
            }
        }

        private void TickCameraPosture(float deltaTime)
        {
            if (viewCamera == null || _motor == null)
            {
                return;
            }

            float targetDrop = _motor.State == WallrunnerMovementState.Sliding ? slideCameraDrop : 0f;
            Vector3 target = _cameraStandLocalPosition + Vector3.down * targetDrop;
            float slideT = 1f - Mathf.Exp(-slideCameraFollowSpeed * deltaTime);
            viewCamera.transform.localPosition = Vector3.Lerp(viewCamera.transform.localPosition, target, slideT);

            float followT = 1f - Mathf.Exp(-cameraFeelFollowSpeed * deltaTime);
            if (!movementCameraFeelEnabled)
            {
                viewCamera.fieldOfView = Mathf.Lerp(viewCamera.fieldOfView, baseFieldOfView, followT);
                _cameraRoll = Mathf.Lerp(_cameraRoll, 0f, followT);
                ApplyCameraLocalRotation();
                return;
            }

            float lateralSpeed = Vector3.ProjectOnPlane(_motor.Velocity, Vector3.up).magnitude;
            float speedKick = Mathf.Clamp01(lateralSpeed / Mathf.Max(0.1f, speedFieldOfViewReference)) * speedFieldOfViewKick;
            float stateKick = ResolveMovementStateFieldOfViewKick();
            viewCamera.fieldOfView = Mathf.Lerp(viewCamera.fieldOfView, baseFieldOfView + speedKick + stateKick, followT);
            _cameraRoll = Mathf.Lerp(_cameraRoll, ResolveMovementCameraRoll(), followT);
            ApplyCameraLocalRotation();
        }

        private float ResolveMovementStateFieldOfViewKick()
        {
            switch (_motor.State)
            {
                case WallrunnerMovementState.Sliding:
                    return slideFieldOfViewKick;
                case WallrunnerMovementState.Wallrunning:
                    return wallrunFieldOfViewKick;
                case WallrunnerMovementState.Vaulting:
                    return vaultFieldOfViewKick;
                default:
                    return 0f;
            }
        }

        private float ResolveMovementCameraRoll()
        {
            Vector3 lateralVelocity = Vector3.ProjectOnPlane(_motor.Velocity, Vector3.up);
            if (lateralVelocity.sqrMagnitude <= 0.01f)
            {
                return 0f;
            }

            float side = Mathf.Clamp(Vector3.Dot(transform.right, lateralVelocity.normalized), -1f, 1f);
            if (_motor.State == WallrunnerMovementState.Wallrunning)
            {
                if (_motor.ActiveWallrunStyle == WallrunTraversalStyle.Vertical)
                {
                    return 0f;
                }

                Vector3 wallNormal = Vector3.ProjectOnPlane(_motor.ActiveWallrunNormal, Vector3.up);
                if (wallNormal.sqrMagnitude > 0.0001f)
                {
                    side = Mathf.Clamp(Vector3.Dot(transform.right, wallNormal.normalized), -1f, 1f);
                }

                return -side * wallrunCameraRollDegrees;
            }

            return _motor.State == WallrunnerMovementState.Sliding ? -side * slideCameraRollDegrees : 0f;
        }

        private void ApplyCameraLocalRotation()
        {
            if (viewCamera != null)
            {
                viewCamera.transform.localRotation = Quaternion.Euler(_pitch, 0f, _cameraRoll);
            }
        }
    }
}
