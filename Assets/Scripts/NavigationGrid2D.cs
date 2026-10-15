using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class NavigationGrid2D : MonoBehaviour
{
    [SerializeField] private Vector2 origin = new Vector2(-12, -9);
    [SerializeField] private Vector2Int gridSize = new Vector2Int(48, 36);
    [SerializeField, Min(0.1f)] private float cellSize = 0.5f;
    [Tooltip("Guard collider radius plus a small clearance margin.")]
    [SerializeField, Min(0.01f)] private float agentRadius = 0.4f;
    [SerializeField] private LayerMask wallMask = 1 << 8;
    [SerializeField] private bool drawGrid = true;

    public Node[,] Nodes { get; private set; }
    public Vector2Int GridSize => gridSize;
    public float CellSize => cellSize;
    public float AgentRadius => agentRadius;
    private static readonly Vector2Int[] offsets =
    {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
    };

    private void Awake() => GenerateGrid();

    [ContextMenu("Generate / Rescan Grid")]
    public void GenerateGrid()
    {
        gridSize = new Vector2Int(Mathf.Max(1, gridSize.x), Mathf.Max(1, gridSize.y));
        cellSize = Mathf.Max(0.1f, cellSize);
        agentRadius = Mathf.Max(0.01f, agentRadius);
        Physics2D.SyncTransforms();
        Nodes = new Node[gridSize.x, gridSize.y];
        for (int x = 0; x < gridSize.x; x++)
            for (int y = 0; y < gridSize.y; y++)
            {
                Vector2 position = origin + new Vector2(x + 0.5f, y + 0.5f) * cellSize;
                bool available = Physics2D.OverlapCircle(position, agentRadius, wallMask) == null;
                Nodes[x, y] = new Node(new Vector2Int(x, y), position, available);
            }
    }

    public Node GetNode(Vector2Int coordinate)
    {
        if (Nodes == null || coordinate.x < 0 || coordinate.y < 0 ||
            coordinate.x >= gridSize.x || coordinate.y >= gridSize.y) return null;
        return Nodes[coordinate.x, coordinate.y];
    }

    // Outside the grid returns null; blocked cells return their unavailable node.
    public Node GetNodeFromWorldPosition(Vector2 position)
    {
        Vector2 relative = (position - origin) / cellSize;
        return GetNode(new Vector2Int(Mathf.FloorToInt(relative.x), Mathf.FloorToInt(relative.y)));
    }

    public IEnumerable<Node> GetNeighbours(Node node)
    {
        if (node == null || !node.Available) yield break;
        foreach (Vector2Int offset in offsets)
        {
            Node neighbour = GetNode(node.gridCoordinate + offset);
            if (neighbour == null || !neighbour.Available) continue;
            Vector2 direction = neighbour.worldPosition - node.worldPosition;
            // Also check the space BETWEEN cells, so thin walls cannot be crossed.
            if (Physics2D.CircleCast(node.worldPosition, agentRadius, direction,
                direction.magnitude, wallMask).collider == null)
                yield return neighbour;
        }
    }

    public void ResetSearchData()
    {
        if (Nodes == null) return;
        foreach (Node node in Nodes) node.ResetSearchData();
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawGrid) return;
        if (Nodes == null)
        {
            Gizmos.color = Color.yellow;
            Vector2 size = new Vector2(gridSize.x, gridSize.y) * cellSize;
            Gizmos.DrawWireCube(origin + size * 0.5f, size);
            return;
        }
        foreach (Node node in Nodes)
        {
            Gizmos.color = node.Available ? new Color(0.2f, 0.9f, 0.4f, 0.45f) : new Color(1f, 0.2f, 0.2f, 0.65f);
            Gizmos.DrawWireCube(node.worldPosition, Vector3.one * cellSize * 0.9f);
        }
    }
}
