using UnityEngine;

namespace StealthSandbox
{
    // Shared movement only. Your AI decides which direction the guard should move.
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class ActorMotor2D : MonoBehaviour
    {
        [SerializeField, Min(0)] private float speed = 4f;
        [SerializeField] private Transform facingMarker;
        private Rigidbody2D body;
        private Vector2 moveInput;
        public Vector2 FacingDirection { get; private set; } = Vector2.up;
        public float Speed { get => speed; set => speed = Mathf.Max(0, value); }

        private void Awake() => body = GetComponent<Rigidbody2D>();
        public void SetMoveInput(Vector2 direction) => moveInput = Vector2.ClampMagnitude(direction, 1f);
        public void Stop() => SetMoveInput(Vector2.zero);
        public void Face(Vector2 direction)
        {
            if (direction.sqrMagnitude < 0.001f) return;
            FacingDirection = direction.normalized;
            if (facingMarker != null)
                facingMarker.localPosition = (Vector3)(FacingDirection * 0.28f);
        }
        public void SetFacingMarker(Transform marker) => facingMarker = marker;
        private void FixedUpdate()
        {
            body.linearVelocity = moveInput * speed;
            if (moveInput.sqrMagnitude > 0.001f) Face(moveInput);
        }
        private void OnDisable()
        {
            moveInput = Vector2.zero;
            if (body != null) body.linearVelocity = Vector2.zero;
        }
    }
}
