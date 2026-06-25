using UnityEngine;

namespace Deucarian.TemplateGameMovementFps.Actors
{
    [RequireComponent(typeof(Collider))]
    public sealed class MovementFpsPickupActor : MonoBehaviour
    {
        [SerializeField]
        private int experienceAmount = 1;

        [SerializeField]
        private Renderer bodyRenderer;

        private MovementFpsTemplateController _session;

        public int ExperienceAmount => experienceAmount;

        public void Initialize(MovementFpsTemplateController session, int amount)
        {
            _session = session;
            experienceAmount = Mathf.Max(1, amount);
            Collider pickupCollider = GetComponent<Collider>();
            pickupCollider.isTrigger = true;

            if (bodyRenderer == null)
            {
                bodyRenderer = GetComponentInChildren<Renderer>();
            }

            if (bodyRenderer != null)
            {
                bodyRenderer.material.color = new Color(0.35f, 0.85f, 1f, 1f);
            }
        }

        private void Update()
        {
            if (_session == null || _session.IsGameplayPaused || _session.Player == null)
            {
                return;
            }

            Vector3 playerPosition = _session.Player.transform.position + Vector3.up;
            float distance = Vector3.Distance(transform.position, playerPosition);
            if (distance <= 1.1f)
            {
                _session.CollectExperience(experienceAmount);
                Destroy(gameObject);
                return;
            }

            if (distance <= _session.Player.PickupRadius)
            {
                transform.position = Vector3.MoveTowards(transform.position, playerPosition, (12f + _session.Player.PickupRadius) * Time.deltaTime);
            }
        }
    }
}
