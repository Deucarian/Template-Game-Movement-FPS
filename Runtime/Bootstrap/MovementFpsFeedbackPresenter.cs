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
    internal sealed class MovementFpsFeedbackPresenter : IMovementFpsRunFeedback, IDisposable
    {
        private const string PresentationRootName = "Movement FPS Presentation";
        private const string WeaponPulseName = "Movement FPS Weapon Pulse";
        private const string PowerPulseName = "Movement FPS Power Pulse";
        private const string EnemyPulseName = "Movement FPS Enemy Pulse";
        private const string PickupPulseName = "Movement FPS Pickup Pulse";
        private const string RunPulseName = "Movement FPS Run State Pulse";
        private const string PresentationAudioName = "Movement FPS Feedback Audio";
        private Transform _presentationRoot;
        private ParticleSystem _weaponPulse;
        private ParticleSystem _powerPulse;
        private ParticleSystem _enemyPulse;
        private ParticleSystem _pickupPulse;
        private ParticleSystem _runPulse;
        private AudioSource _feedbackAudio;
        private AudioClip _weaponClip;
        private AudioClip _projectileClip;
        private AudioClip _powerClip;
        private AudioClip _enemyClip;
        private AudioClip _pickupClip;
        private AudioClip _runClip;
        private readonly Transform _runtimeRoot;
        private readonly Func<Vector3> _playerPosition;
        private bool _disposed;

        internal MovementFpsFeedbackPresenter(Transform root, Func<Vector3> playerPosition)
        {
            _runtimeRoot = root;
            _playerPosition = playerPosition;
        }

        public void Defeated() => PlayFeedback(_runPulse, _playerPosition(), 38, _runClip);
        public void ExperienceCollected(bool draftOpened) => PlayFeedback(_pickupPulse, _playerPosition(), draftOpened ? 34 : 12, draftOpened ? _runClip : _pickupClip);
        public void UpgradeChosen() => PlayFeedback(_runPulse, _playerPosition(), 32, _runClip);
        public void Victory() => PlayFeedback(_runPulse, _playerPosition(), 58, _runClip);
        public void EnemyKilled(Vector3 position, bool miniBoss) => PlayFeedback(_enemyPulse, position, miniBoss ? 54 : 22, _enemyClip);
        internal void EnemySpawned(Vector3 position, bool miniBoss) => PlayFeedback(_enemyPulse, position, miniBoss ? 42 : 14, miniBoss ? _runClip : _enemyClip);
        internal void ProjectileSpawned(Vector3 origin, Vector3 direction) => PlayFeedback(_weaponPulse, origin + direction * 2f, 14, _projectileClip);
        internal void PickupSpawned(Vector3 position) => PlayFeedback(_pickupPulse, position, 12, _pickupClip);

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            if (_presentationRoot != null) UnityObjectUtility.DestroySafely(_presentationRoot.gameObject);
            UnityObjectUtility.DestroySafely(_weaponClip);
            UnityObjectUtility.DestroySafely(_projectileClip);
            UnityObjectUtility.DestroySafely(_powerClip);
            UnityObjectUtility.DestroySafely(_enemyClip);
            UnityObjectUtility.DestroySafely(_pickupClip);
            UnityObjectUtility.DestroySafely(_runClip);
        }

        internal void PlayWeaponFeedback(Vector3 origin, Vector3 direction, MovementFpsGunKind kind)
        {
            Vector3 resolvedDirection = direction.sqrMagnitude <= 0.0001f ? Vector3.forward : direction.normalized;
            PlayFeedback(_weaponPulse, origin + resolvedDirection * (kind == MovementFpsGunKind.Projectile ? 2f : 5f), kind == MovementFpsGunKind.Projectile ? 14 : 8, kind == MovementFpsGunKind.Projectile ? _projectileClip : _weaponClip);
        }

        internal void PlayPowerFeedback(Vector3 position, MovementFpsAutoPowerKind kind)
        {
            int count = kind == MovementFpsAutoPowerKind.ChainBolt ? 24 : kind == MovementFpsAutoPowerKind.GroundRift ? 34 : 18;
            PlayFeedback(_powerPulse, position, count, _powerClip);
        }

        internal void Build()
        {
            GameObject root = new GameObject(PresentationRootName);
            root.transform.SetParent(_runtimeRoot, false);
            _presentationRoot = root.transform;
            _weaponPulse = CreatePulse(WeaponPulseName, new Color(0.78f, 0.48f, 1f), 0.16f, 3.2f, 0.32f);
            _powerPulse = CreatePulse(PowerPulseName, new Color(0.2f, 0.86f, 1f), 0.22f, 2.8f, 0.45f);
            _enemyPulse = CreatePulse(EnemyPulseName, new Color(1f, 0.25f, 0.28f), 0.24f, 3.0f, 0.48f);
            _pickupPulse = CreatePulse(PickupPulseName, new Color(0.4f, 1f, 0.55f), 0.14f, 2.2f, 0.35f);
            _runPulse = CreatePulse(RunPulseName, new Color(1f, 0.82f, 0.24f), 0.32f, 3.6f, 0.7f);

            GameObject audioObject = new GameObject(PresentationAudioName);
            audioObject.transform.SetParent(_presentationRoot, false);
            _feedbackAudio = audioObject.AddComponent<AudioSource>();
            _feedbackAudio.playOnAwake = false;
            _feedbackAudio.spatialBlend = 0f;
            _feedbackAudio.volume = 0.28f;
            _weaponClip = CreateTone("movement-fps-carbine", 620f, 0.055f, 0.16f);
            _projectileClip = CreateTone("movement-fps-launcher", 210f, 0.13f, 0.22f);
            _powerClip = CreateTone("movement-fps-power", 860f, 0.12f, 0.18f);
            _enemyClip = CreateTone("movement-fps-enemy", 330f, 0.08f, 0.16f);
            _pickupClip = CreateTone("movement-fps-pickup", 1040f, 0.07f, 0.15f);
            _runClip = CreateTone("movement-fps-run-state", 120f, 0.24f, 0.22f);
        }

        private ParticleSystem CreatePulse(string name, Color color, float startSize, float startSpeed, float lifetime)
        {
            GameObject instance = new GameObject(name);
            instance.transform.SetParent(_presentationRoot, false);
            ParticleSystem particles = instance.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = particles.main;
            main.loop = false;
            main.playOnAwake = false;
            main.startLifetime = lifetime;
            main.startSpeed = startSpeed;
            main.startSize = startSize;
            main.startColor = color;
            main.maxParticles = 120;
            ParticleSystem.EmissionModule emission = particles.emission;
            emission.enabled = false;
            ParticleSystem.ShapeModule shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.35f;
            return particles;
        }

        private void PlayFeedback(ParticleSystem particles, Vector3 position, int count, AudioClip clip)
        {
            if (particles != null)
            {
                particles.transform.position = position;
                particles.Emit(Mathf.Max(1, count));
            }

            if (_feedbackAudio != null && clip != null)
            {
                _feedbackAudio.PlayOneShot(clip);
            }
        }

        private static AudioClip CreateTone(string name, float frequency, float durationSeconds, float volume)
        {
            const int sampleRate = 22050;
            int sampleCount = Mathf.Max(1, Mathf.CeilToInt(sampleRate * durationSeconds));
            float[] samples = new float[sampleCount];
            for (int i = 0; i < sampleCount; i++)
            {
                float t = i / (float)sampleRate;
                float fade = Mathf.Clamp01(1f - i / (float)sampleCount);
                samples[i] = Mathf.Sin(Mathf.PI * 2f * frequency * t) * volume * fade;
            }

            AudioClip clip = AudioClip.Create(name, sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

    }
}
