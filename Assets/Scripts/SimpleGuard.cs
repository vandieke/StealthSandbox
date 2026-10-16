using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SimpleGuard : EnemyBase
{
    [SerializeField, Min(0.5f)] private float searchRadius = 3f;
    private int searchQuarter = -1;
    private List<Node> searchPath;
    private int searchPathIndex;
    private Node previousSearchGoal;

    protected override void Patrol()
    {
        //we move from a to b
    }
    //test
    protected override void Chase()
    {
        //this here ensures the enemy moves straight towards the player if there are no obstacles in the way. 
        if (HasClearLine())
        {
            Vector2 dir = enemyEyes.CurrentLocation - (Vector2)mover.transform.position;
            mover.SetMoveInput(dir.normalized);
            lastGoalNode = null;   
            repathTimer = 0;
            return;
        }
        repathTimer -= Time.deltaTime;
        if (repathTimer <= 0f)
        {
            repathTimer = repathInterval;
            Node goal = pathfinder.Grid.GetNodeFromWorldPosition(enemyEyes.CurrentLocation);
            if (goal != lastGoalNode)
            {
                lastGoalNode = goal;
                Node start = pathfinder.Grid.GetNodeFromWorldPosition(mover.transform.position);
                path = pathfinder.CalculatePath(start, goal);
                pathIndex = 1;
                routeIsPlanned = true;
            }
        }

        if (path == null)
        {
            Vector2 straightDir = enemyEyes.CurrentLocation - (Vector2)transform.position;
            mover.SetMoveInput(straightDir);
            return;
        }
        if (pathIndex < path.Count)
        {
            Vector2 target = path[pathIndex].worldPosition;
            Vector2 dir = target - (Vector2)mover.transform.position;

            if (dir.magnitude < 0.15f)
            {
                pathIndex++;
                mover.Stop();
            }
            else
            {
                mover.SetMoveInput(dir.normalized);
            }
            return;
        }
        mover.Stop();
    }
    protected override void Search()
    {
        if (!routeIsPlanned)
        {
            Node start = pathfinder.Grid.GetNodeFromWorldPosition(mover.transform.position);
            Node goal = pathfinder.Grid.GetNodeFromWorldPosition(enemyEyes.LastKnownLocation);

            path = pathfinder.CalculatePath(start, goal);
            pathIndex = 0;
            routeIsPlanned = true;
        }
        if (!searchHasStarted)
        {
            if (path == null)
            {
                mover.Stop();
                searchHasStarted = true;
                searchQuarter = -1;
                previousSearchGoal = null;
                return;
            }
            if (pathIndex < path.Count)
            {
                Vector2 target = path[pathIndex].worldPosition;
                Vector2 dir = target - (Vector2)mover.transform.position;

                if (dir.magnitude < 0.15f)
                {
                    pathIndex++;
                    mover.Stop();
                }
                else
                {
                    mover.SetMoveInput(dir.normalized);
                }
                return;
            }
            mover.Stop();
            // Once arrived, keep searching even after moving away from this spot.
            searchHasStarted = true;
            searchQuarter = -1;
            previousSearchGoal = null;
        }

        int quarter = Mathf.Clamp(Mathf.FloorToInt(searchTimer / (Mathf.Max(searchTime, 0.01f) / 4f)), 0, 3);
        if (quarter != searchQuarter)
        {
            searchQuarter = quarter;
            PlanRandomSearchPath();
        }
        if (searchPath == null || searchPathIndex >= searchPath.Count)
        {
            mover.Stop();
            return;
        }

        Vector2 searchTarget = searchPath[searchPathIndex].worldPosition;
        Vector2 searchMove = searchTarget - (Vector2)mover.transform.position;
        if (searchMove.magnitude < 0.15f)
        {
            searchPathIndex++;
            mover.Stop();
        }
        else mover.SetMoveInput(searchMove.normalized);
    }

    private void PlanRandomSearchPath()
    {
        searchPath = null;
        searchPathIndex = 0;
        Node start = pathfinder.Grid.GetNodeFromWorldPosition(mover.transform.position);
        if (start == null || !start.Available) return;

        // Choose actual free cells around the last sighting, not arbitrary world points.
        var candidates = new List<Node>();
        foreach (Node node in pathfinder.Grid.Nodes)
        {
            if (node.Available && node != start && node != previousSearchGoal &&
                (node.worldPosition - enemyEyes.LastKnownLocation).sqrMagnitude <= searchRadius * searchRadius)
                candidates.Add(node);
        }

        // A free cell can still be unreachable. Try a few alternatives this quarter.
        for (int attempt = 0; attempt < 12 && candidates.Count > 0; attempt++)
        {
            int index = Random.Range(0, candidates.Count);
            Node goal = candidates[index];
            candidates.RemoveAt(index);
            List<Node> candidatePath = pathfinder.CalculatePath(start, goal);
            if (candidatePath == null || candidatePath.Count == 0) continue;
            searchPath = candidatePath;
            previousSearchGoal = goal;
            return;
        }
    }
}
