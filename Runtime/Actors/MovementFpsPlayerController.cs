using System.Collections.Generic;
using Deucarian.Combat;
using Deucarian.TemplateGameMovementFps.Combat;
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
        private readonly List<MovementFpsGunRuntimeState> _guns = new List<MovementFpsGunRuntimeState>();
        private readonly List<MovementFpsAutoPowerRuntimeState> _autoPowers = new List<MovementFpsAutoPowerRuntimeState>();
        private readonly List<MovementFpsGunDefinition> _startingGuns = new List<MovementFpsGunDefinition>();
        private readonly List<MovementFpsAutoPowerDefinition> _startingPowers = new List<MovementFpsAutoPowerDefinition>();
        private MovementFpsTemplateController _session;
        private MovementFpsPlayerDefinition _playerDefinition;
        private WallrunnerMotor _motor;
        private HealthState _health;
        private Vector3 _cameraStandLocalPosition;
        private float _pitch;
        private float _cameraRoll;
        private int _currentGunIndex;
        private bool _initialized;

        public WallrunnerMotor Motor => _motor;
        public Camera ViewCamera => viewCamera;
        public double CurrentHealth => _health == null ? 0d : _health.CurrentHealth;
        public double MaximumHealth => _health == null ? 0d : _health.MaximumHealth;
        public bool IsAlive => _health != null && _health.IsAlive;
        public float PickupRadius => _session == null ? 4.2f : _session.PlayerPickupRadius;
        public IReadOnlyList<MovementFpsGunRuntimeState> Guns => _guns;
        public IReadOnlyList<MovementFpsAutoPowerRuntimeState> AutoPowers => _autoPowers;
        public MovementFpsGunRuntimeState CurrentGun => _guns.Count == 0 ? null : _guns[Mathf.Clamp(_currentGunIndex, 0, _guns.Count - 1)];
        public double GunDamage => CurrentGun == null ? 0d : ResolveGunDamage(CurrentGun.Definition);

        public void Initialize(
            MovementFpsTemplateController session,
            MovementFpsPlayerDefinition playerDefinition,
            IReadOnlyList<MovementFpsGunDefinition> startingGuns,
            IReadOnlyList<MovementFpsAutoPowerDefinition> startingPowers)
        {
            _session = session;
            _playerDefinition = playerDefinition;
            _startingGuns.Clear();
            _startingPowers.Clear();
            CopyStartingContent(startingGuns, _startingGuns);
            CopyStartingContent(startingPowers, _startingPowers);
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
            _currentGunIndex = 0;
            ResetLoadout();
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
            TickGun(_input.FireHeld, _input.ReloadPressed, _input.NextGunPressed, Time.deltaTime);
            TickAutoPowers(Time.deltaTime);
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

            MovementFpsGunRuntimeState gun = CurrentGun;
            DamageTypeId damageType = gun == null ? BasicMovementFpsGame.KineticDamageType : gun.Definition.DamageType;
            return enemy.ApplyDamage(GunDamage, new CombatantId("combatant.player"), damageType);
        }

        public MovementFpsProjectileActor FireProjectileAtForTest(MovementFpsEnemyActor enemy)
        {
            if (enemy == null)
            {
                return null;
            }

            MovementFpsGunRuntimeState projectileGun = FindGun(MovementFpsGunKind.Projectile);
            if (projectileGun == null)
            {
                return null;
            }

            Vector3 origin = muzzle == null ? transform.position + Vector3.up * 1.45f : muzzle.position;
            Vector3 target = enemy.transform.position + Vector3.up * 0.4f;
            Vector3 direction = target - origin;
            if (direction.sqrMagnitude <= 0.0001f)
            {
                direction = transform.forward;
            }

            return FireProjectile(projectileGun.Definition, origin, direction.normalized);
        }

        public bool AddGun(MovementFpsGunDefinition definition)
        {
            if (string.IsNullOrWhiteSpace(definition.Id) || HasGun(definition.Id))
            {
                return false;
            }

            _guns.Add(new MovementFpsGunRuntimeState(definition));
            return true;
        }

        public bool AddAutoPower(MovementFpsAutoPowerDefinition definition)
        {
            if (string.IsNullOrWhiteSpace(definition.Id) || HasAutoPower(definition.Id))
            {
                return false;
            }

            _autoPowers.Add(new MovementFpsAutoPowerRuntimeState(definition));
            return true;
        }

        public bool HasGun(string id)
        {
            for (int index = 0; index < _guns.Count; index++)
            {
                if (_guns[index].Definition.Id == id)
                {
                    return true;
                }
            }

            return false;
        }

        public bool HasAutoPower(string id)
        {
            for (int index = 0; index < _autoPowers.Count; index++)
            {
                if (_autoPowers[index].Definition.Id == id)
                {
                    return true;
                }
            }

            return false;
        }

        public void TickAutoPowersForTest(float deltaTime)
        {
            TickAutoPowers(deltaTime);
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

        private void ResetLoadout()
        {
            _guns.Clear();
            _autoPowers.Clear();
            for (int index = 0; index < _startingGuns.Count; index++)
            {
                AddGun(_startingGuns[index]);
            }

            for (int index = 0; index < _startingPowers.Count; index++)
            {
                AddAutoPower(_startingPowers[index]);
            }
        }

        private void TickLook(Vector2 lookDelta)
        {
            Vector2 look = lookDelta * mouseSensitivity;
            transform.Rotate(Vector3.up, look.x, Space.World);
            _pitch = Mathf.Clamp(_pitch - look.y, -84f, 84f);
            ApplyCameraLocalRotation();
        }

        private void TickGun(bool fireHeld, bool reloadPressed, bool nextGunPressed, float deltaTime)
        {
            for (int index = 0; index < _guns.Count; index++)
            {
                _guns[index].Tick(deltaTime);
            }

            if (_guns.Count == 0)
            {
                return;
            }

            if (nextGunPressed)
            {
                _currentGunIndex = (_currentGunIndex + 1) % _guns.Count;
            }

            MovementFpsGunRuntimeState gun = CurrentGun;
            if (reloadPressed)
            {
                gun.StartReload(1f);
            }

            if (!fireHeld || !gun.TryFire(ResolveGunCadenceMultiplier()))
            {
                return;
            }

            FireGun(gun.Definition);
            _pitch = Mathf.Clamp(_pitch - gun.Definition.RecoilPitchDegrees, -84f, 84f);
        }

        private void FireGun(MovementFpsGunDefinition definition)
        {
            if (viewCamera == null)
            {
                return;
            }

            Vector3 origin = muzzle == null ? viewCamera.transform.position : muzzle.position;
            Vector3 direction = ResolveShotDirection(definition);
            if (definition.Kind == MovementFpsGunKind.Projectile)
            {
                FireProjectile(definition, origin, direction);
                return;
            }

            FireHitscan(definition, origin, direction);
        }

        private void FireHitscan(MovementFpsGunDefinition definition, Vector3 origin, Vector3 direction)
        {
            RaycastHit[] hits = Physics.RaycastAll(origin, direction, definition.Range, ~0, QueryTriggerInteraction.Ignore);
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
                target.ApplyDamage(ResolveGunDamage(definition), new CombatantId("combatant.player"), definition.DamageType);
            }
        }

        private MovementFpsProjectileActor FireProjectile(MovementFpsGunDefinition definition, Vector3 origin, Vector3 direction)
        {
            return _session.SpawnProjectile(
                this,
                origin,
                direction,
                definition,
                ResolveGunDamage(definition),
                ResolveProjectileSpeed(definition));
        }

        private Vector3 ResolveShotDirection(MovementFpsGunDefinition definition)
        {
            Vector3 direction = viewCamera == null ? transform.forward : viewCamera.transform.forward;
            if (definition.SpreadDegrees > 0f)
            {
                Vector2 spread = Random.insideUnitCircle * definition.SpreadDegrees;
                direction = Quaternion.Euler(-spread.y, spread.x, 0f) * direction;
            }

            return direction.normalized;
        }

        private void TickAutoPowers(float deltaTime)
        {
            for (int index = 0; index < _autoPowers.Count; index++)
            {
                MovementFpsAutoPowerRuntimeState power = _autoPowers[index];
                power.Tick(deltaTime);
                if (!power.Ready)
                {
                    continue;
                }

                CastAutoPower(power.Definition);
                power.ResetCooldown(1f);
            }
        }

        private void CastAutoPower(MovementFpsAutoPowerDefinition definition)
        {
            double damage = definition.Damage * (1d + (_session == null ? 0d : _session.Progression.AutoPowerDamageMultiplier));
            float radius = definition.Radius;
            switch (definition.Kind)
            {
                case MovementFpsAutoPowerKind.OrbitPulse:
                    _session.DamageEnemiesInRadius(transform.position, radius, damage, definition.DamageType);
                    break;
                case MovementFpsAutoPowerKind.ChainBolt:
                    IReadOnlyList<MovementFpsEnemyActor> targets = _session.GetNearestEnemies(transform.position, definition.Range, definition.TargetCount);
                    for (int index = 0; index < targets.Count; index++)
                    {
                        targets[index].ApplyDamage(damage, new CombatantId("combatant.player"), definition.DamageType);
                    }

                    break;
                case MovementFpsAutoPowerKind.GroundRift:
                    IReadOnlyList<MovementFpsEnemyActor> riftTargets = _session.GetNearestEnemies(transform.position, definition.Range, 1);
                    Vector3 center = riftTargets.Count > 0 ? riftTargets[0].transform.position : transform.position + transform.forward * 8f;
                    _session.DamageEnemiesInRadius(center, radius, damage, definition.DamageType);
                    break;
            }
        }

        private double ResolveGunDamage(MovementFpsGunDefinition definition)
        {
            return definition.Damage + (_session == null ? 0d : _session.Progression.GunDamageBonus);
        }

        private float ResolveGunCadenceMultiplier()
        {
            return 1f + (float)(_session == null ? 0d : _session.Progression.GunCadenceMultiplier);
        }

        private float ResolveProjectileSpeed(MovementFpsGunDefinition definition)
        {
            return definition.ProjectileSpeed * (1f + (float)(_session == null ? 0d : _session.Progression.ProjectileSpeedMultiplier));
        }

        private MovementFpsGunRuntimeState FindGun(MovementFpsGunKind kind)
        {
            for (int index = 0; index < _guns.Count; index++)
            {
                if (_guns[index].Definition.Kind == kind)
                {
                    return _guns[index];
                }
            }

            return null;
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

        private static void CopyStartingContent(IReadOnlyList<MovementFpsGunDefinition> source, List<MovementFpsGunDefinition> destination)
        {
            if (source == null)
            {
                return;
            }

            for (int index = 0; index < source.Count; index++)
            {
                if (!string.IsNullOrWhiteSpace(source[index].Id))
                {
                    destination.Add(source[index]);
                }
            }
        }

        private static void CopyStartingContent(IReadOnlyList<MovementFpsAutoPowerDefinition> source, List<MovementFpsAutoPowerDefinition> destination)
        {
            if (source == null)
            {
                return;
            }

            for (int index = 0; index < source.Count; index++)
            {
                if (!string.IsNullOrWhiteSpace(source[index].Id))
                {
                    destination.Add(source[index]);
                }
            }
        }
    }
}
