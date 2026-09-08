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
    internal sealed class MovementFpsCameraPresenter
    {
        private readonly Transform _pose;
        private readonly Func<Camera> _view;
        private readonly WallrunnerMotor _motor;
        private readonly MovementFpsCameraSettings _settings;
        private Vector3 _cameraStandLocalPosition;
        private float _pitch;
        private float _cameraRoll;
        private Camera ViewCamera => _view();

        internal MovementFpsCameraPresenter(Transform pose, Func<Camera> view, WallrunnerMotor motor, MovementFpsCameraSettings settings)
        {
            _pose = pose;
            _view = view;
            _motor = motor;
            _settings = settings;
            _cameraStandLocalPosition = ViewCamera == null ? Vector3.zero : ViewCamera.transform.localPosition;
            if (ViewCamera != null) ViewCamera.fieldOfView = _settings.BaseFieldOfView;
        }

        internal void ResetAngles()
        {
            _pitch = 0f;
            _cameraRoll = 0f;
        }

        internal void ResetView()
        {
            if (ViewCamera == null) return;
            ViewCamera.transform.localPosition = _cameraStandLocalPosition == Vector3.zero ? new Vector3(0f, 0.62f, 0f) : _cameraStandLocalPosition;
            ViewCamera.transform.localRotation = Quaternion.identity;
            ViewCamera.fieldOfView = _settings.BaseFieldOfView;
        }

        internal void ApplyRecoil(float degrees) => _pitch = Mathf.Clamp(_pitch - degrees, -84f, 84f);

        internal void TickLook(Vector2 lookDelta)
        {
            Vector2 look = lookDelta * _settings.MouseSensitivity;
            _pose.Rotate(Vector3.up, look.x, Space.World);
            _pitch = Mathf.Clamp(_pitch - look.y, -84f, 84f);
            ApplyCameraLocalRotation();
        }

        internal void TickCameraPosture(float deltaTime)
        {
            if (ViewCamera == null || _motor == null)
            {
                return;
            }

            float targetDrop = _motor.State == WallrunnerMovementState.Sliding ? _settings.SlideCameraDrop : 0f;
            Vector3 target = _cameraStandLocalPosition + Vector3.down * targetDrop;
            float slideT = 1f - Mathf.Exp(-_settings.SlideCameraFollowSpeed * deltaTime);
            ViewCamera.transform.localPosition = Vector3.Lerp(ViewCamera.transform.localPosition, target, slideT);

            float followT = 1f - Mathf.Exp(-_settings.CameraFeelFollowSpeed * deltaTime);
            if (!_settings.MovementCameraFeelEnabled)
            {
                ViewCamera.fieldOfView = Mathf.Lerp(ViewCamera.fieldOfView, _settings.BaseFieldOfView, followT);
                _cameraRoll = Mathf.Lerp(_cameraRoll, 0f, followT);
                ApplyCameraLocalRotation();
                return;
            }

            float lateralSpeed = Vector3.ProjectOnPlane(_motor.Velocity, Vector3.up).magnitude;
            float speedKick = Mathf.Clamp01(lateralSpeed / Mathf.Max(0.1f, _settings.SpeedFieldOfViewReference)) * _settings.SpeedFieldOfViewKick;
            float stateKick = ResolveMovementStateFieldOfViewKick();
            ViewCamera.fieldOfView = Mathf.Lerp(ViewCamera.fieldOfView, _settings.BaseFieldOfView + speedKick + stateKick, followT);
            _cameraRoll = Mathf.Lerp(_cameraRoll, ResolveMovementCameraRoll(), followT);
            ApplyCameraLocalRotation();
        }

        private float ResolveMovementStateFieldOfViewKick()
        {
            switch (_motor.State)
            {
                case WallrunnerMovementState.Sliding:
                    return _settings.SlideFieldOfViewKick;
                case WallrunnerMovementState.Wallrunning:
                    return _settings.WallrunFieldOfViewKick;
                case WallrunnerMovementState.Vaulting:
                    return _settings.VaultFieldOfViewKick;
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

            float side = Mathf.Clamp(Vector3.Dot(_pose.right, lateralVelocity.normalized), -1f, 1f);
            if (_motor.State == WallrunnerMovementState.Wallrunning)
            {
                if (_motor.ActiveWallrunStyle == WallrunTraversalStyle.Vertical)
                {
                    return 0f;
                }

                Vector3 wallNormal = Vector3.ProjectOnPlane(_motor.ActiveWallrunNormal, Vector3.up);
                if (wallNormal.sqrMagnitude > 0.0001f)
                {
                    side = Mathf.Clamp(Vector3.Dot(_pose.right, wallNormal.normalized), -1f, 1f);
                }

                return -side * _settings.WallrunCameraRollDegrees;
            }

            return _motor.State == WallrunnerMovementState.Sliding ? -side * _settings.SlideCameraRollDegrees : 0f;
        }

        private void ApplyCameraLocalRotation()
        {
            if (ViewCamera != null)
            {
                ViewCamera.transform.localRotation = Quaternion.Euler(_pitch, 0f, _cameraRoll);
            }
        }

    }
}
