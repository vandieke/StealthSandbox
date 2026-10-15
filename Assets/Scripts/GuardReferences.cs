using UnityEngine;

namespace StealthSandbox
{
    // References for your experiments; intentionally contains no AI behaviour.
    [RequireComponent(typeof(ActorMotor2D))]
    public sealed class GuardReferences : MonoBehaviour
    {
        public Transform player;
        public Transform[] patrolPoints;
        public LayerMask wallMask;
        public ActorMotor2D Motor => GetComponent<ActorMotor2D>();
        private void OnDrawGizmosSelected()
        {
            if (patrolPoints == null) return;
            Gizmos.color = Color.yellow;
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                if (patrolPoints[i] == null) continue;
                Gizmos.DrawWireSphere(patrolPoints[i].position, 0.25f);
                var next = patrolPoints[(i + 1) % patrolPoints.Length];
                if (next != null) Gizmos.DrawLine(patrolPoints[i].position, next.position);
            }
        }
    }
}
