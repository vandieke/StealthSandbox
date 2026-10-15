using UnityEngine;

namespace StealthSandbox
{
    public sealed class CameraFollow2D : MonoBehaviour
    {
        public Transform target;
        [SerializeField, Min(0.01f)] private float smoothTime = 0.15f;
        private Vector3 velocity;
        private void LateUpdate()
        {
            if (target == null) return;
            var destination = new Vector3(target.position.x, target.position.y, -10f);
            transform.position = Vector3.SmoothDamp(transform.position, destination, ref velocity, smoothTime);
        }
    }
}
