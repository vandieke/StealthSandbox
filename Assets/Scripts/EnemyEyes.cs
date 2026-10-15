using StealthSandbox;
using UnityEngine;

[RequireComponent(typeof(PolygonCollider2D))]
public class EnemyEyes : MonoBehaviour
{
    [SerializeField][Range(0.0f, 1.0f)] private float visionWidth = 0f;
    [SerializeField] private LayerMask wallMask = 1 << 8;
    [SerializeField] private ActorMotor2D motor;
    [Tooltip("Use 0 if the polygon points up in local space; 90 if it points right.")]
    [SerializeField] private float visionRotationOffset = 0f;
    public bool EyesOnPlayer = false;
    private PolygonCollider2D visionCollider;

    [Header("Memory")]
    public Vector2 LastKnownLocation = Vector2.zero;
    public Vector2 CurrentLocation = Vector2.zero;
    public bool HasLastKnownLocation = false;
    private LineRenderer memoryMarker;
    private Material markerMaterial;
    private Vector2 markerSize = Vector2.one * 0.75f;
    void Start()
    {
        visionCollider = GetComponent<PolygonCollider2D>();
        if (motor == null) motor = GetComponentInParent<ActorMotor2D>();
        ChangeVisionWidth();
        RotateVisionCone();
    }
    private void ChangeVisionWidth()
    {
        Vector2[] points = visionCollider.points;
        if (points.Length < 5)
        {
            Debug.LogWarning("Vision width expects at least five polygon points (corners 1 and 4).", this);
            return;
        }

        // Apply the configured width once, then write the points back to Unity.
        points[1].x -= visionWidth * 2;
        points[4].x += visionWidth * 2;
        visionCollider.points = points;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        CheckPlayerVisibility(collision);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        CheckPlayerVisibility(collision);
    }

    private void CheckPlayerVisibility(Collider2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;

        Vector2 from = transform.position;
        Vector2 to = collision.transform.position;
        Vector2 direction = to - from;
        RaycastHit2D hit = Physics2D.Raycast(from, direction, direction.magnitude, wallMask);
        bool visible = hit.collider == null;

        if (visible)
        {
            // Memory only contains positions that the guard actually observed.
            CurrentLocation = to;
            LastKnownLocation = to;
            HasLastKnownLocation = true;
            markerSize = collision.bounds.size;
        }
        EyesOnPlayer = visible;
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            EyesOnPlayer = false;
        }
    }

    private void LateUpdate()
    {
        RotateVisionCone();
        bool showMarker = HasLastKnownLocation && !EyesOnPlayer;
        if (!showMarker)
        {
            if (memoryMarker != null) memoryMarker.enabled = false;
            return;
        }

        if (memoryMarker == null)
        {
            var marker = new GameObject("Last Known Location");
            memoryMarker = marker.AddComponent<LineRenderer>();
            markerMaterial = new Material(Shader.Find("Sprites/Default"));
            memoryMarker.sharedMaterial = markerMaterial;
            memoryMarker.useWorldSpace = true;
            memoryMarker.loop = true;
            memoryMarker.positionCount = 4;
            memoryMarker.widthMultiplier = 0.04f;
            memoryMarker.startColor = memoryMarker.endColor = new Color(1f, 0.8f, 0.25f);
            memoryMarker.sortingOrder = 20;
        }

        Vector2 halfSize = markerSize * 0.5f;
        Vector2 center = LastKnownLocation;
        memoryMarker.SetPosition(0, center + new Vector2(-halfSize.x, -halfSize.y));
        memoryMarker.SetPosition(1, center + new Vector2(-halfSize.x, halfSize.y));
        memoryMarker.SetPosition(2, center + new Vector2(halfSize.x, halfSize.y));
        memoryMarker.SetPosition(3, center + new Vector2(halfSize.x, -halfSize.y));
        memoryMarker.enabled = true;
    }

    private void RotateVisionCone()
    {
        if (motor == null) return;
        Vector2 facing = motor.FacingDirection;
        float angle = Mathf.Atan2(facing.y, facing.x) * Mathf.Rad2Deg - 90f;
        // FacingDirection is world-space, so set a world-space rotation too.
        transform.rotation = Quaternion.Euler(0f, 0f, angle + visionRotationOffset);
    }

    private void OnDisable()
    {
        EyesOnPlayer = false;
        if (memoryMarker != null) memoryMarker.enabled = false;
    }

    private void OnDestroy()
    {
        if (memoryMarker != null) Destroy(memoryMarker.gameObject);
        if (markerMaterial != null) Destroy(markerMaterial);
    }
}
