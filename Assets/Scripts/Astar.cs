using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NavigationGrid2D))]
public class Astar : MonoBehaviour
{
    public NavigationGrid2D Grid { get; private set; }
    private void Awake() => Grid = GetComponent<NavigationGrid2D>();

    List<Node> openList = new List<Node>();
    List<Node> closedList = new List<Node>();

    public List<Node> CalculatePath(Node startNode, Node endNode)
    {
        if (startNode == null || endNode == null) return null;
        if (!startNode.Available ||  !endNode.Available) return null;
        Grid.ResetSearchData();
        List<Node> path = new List<Node>();
        openList.Clear();
        closedList.Clear();
        startNode.gCost = 0;
        startNode.hCost = CalculateHScore(startNode, endNode);
        openList.Add(startNode);
        //Astar explores all possibilities, not just a single road. so while there are still possibilities we will explore them and see if there are better options.
        while (openList.Count > 0)
        {
            //we select which node we're going to look at. the one with the current lowest f score(most promising).
            Node currentNode = openList[0];
            foreach (Node candidate in openList)
            {
                if (candidate.fCost < currentNode.fCost) currentNode = candidate;
            }
            //now we know which node we're going to look at, we can start the process.
            openList.Remove(currentNode);
            closedList.Add(currentNode);
            if (currentNode == endNode)
            {
                //we build back the path
                Node curNode = currentNode;
                path.Add(curNode);
                while (curNode.parent != null)
                {
                    path.Add(curNode.parent);
                    curNode = curNode.parent;
                }
                path.Reverse();
                return path;
            }
            var neighbours = Grid.GetNeighbours(currentNode);
            foreach (var neighbour in neighbours)
            {
                //we ignore the node if it's not available or if it's already been visited(closedList) otherwise we would loop forever.
                if (!neighbour.Available || closedList.Contains(neighbour)) continue;
                //Only if we can improve the cost, (find a better route through this node) will we update it. At the start all are infinite so this will sort of recursively find the best option.
                float tentativeGCost = currentNode.gCost + 1;
                if (tentativeGCost < neighbour.gCost)
                {
                    neighbour.gCost = tentativeGCost;
                    neighbour.hCost = CalculateHScore(neighbour, endNode);
                    neighbour.parent = currentNode;
                    if (!openList.Contains(neighbour)) openList.Add(neighbour);
                }
            }
        }
        //open is empty and we haven't found a single path to the endnode. 
        return null;
    }

    private int CalculateHScore(Node node, Node endNode)
    {
        //manhattan distance
        return Mathf.Abs((node.gridCoordinate.x - endNode.gridCoordinate.x)) + Mathf.Abs((node.gridCoordinate.y - endNode.gridCoordinate.y));
    }
}
