using UnityEngine;

[System.Serializable]
public class Node
{
    public bool Available = true;
    public Vector2Int gridCoordinate;
    public Vector2 worldPosition;
    // Search data: reset these before starting a new search on this grid.
    public float gCost = float.PositiveInfinity;
    public float hCost;
    public float fCost => gCost + hCost;
    public Node parent;

    public Node(Vector2Int coordinate, Vector2 position, bool available)
    {
        gridCoordinate = coordinate;
        worldPosition = position;
        Available = available;
    }

    public void ResetSearchData()
    {
        gCost = float.PositiveInfinity;
        hCost = 0;
        parent = null;
    }
}
