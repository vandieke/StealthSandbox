using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SimpleGuard : EnemyBase
{
    [SerializeField, Min(0.05f)] private float searchArrivalDistance = 0.8f;
    private int searchQuarter = -1;
    private int searchDirectionIndex = -1;
    private Vector2 searchDirection;

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
                searchDirectionIndex = -1;
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
            searchDirectionIndex = -1;
        }

        int quarter = Mathf.Clamp(Mathf.FloorToInt(searchTimer / (Mathf.Max(searchTime, 0.01f) / 4f)), 0, 3);
        if (quarter != searchQuarter)
        {
            searchQuarter = quarter;
            // Choose once per quarter; exclude the previous direction.
            int choice = searchDirectionIndex < 0 ? Random.Range(0, 4) : Random.Range(0, 3);
            if (searchDirectionIndex >= 0 && choice >= searchDirectionIndex) choice++;
            searchDirectionIndex = choice;
            switch (choice)
            {
                case 0: searchDirection = Vector2.up; break;
                case 1: searchDirection = Vector2.down; break;
                case 2: searchDirection = Vector2.left; break;
                case 3: searchDirection = Vector2.right; break;
            }
        }
        mover.SetMoveInput(searchDirection);
    }
}
