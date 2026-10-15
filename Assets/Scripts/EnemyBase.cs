using StealthSandbox;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    [SerializeField] protected EnemyState currentState;
    [SerializeField] protected EnemyState prevState;
    [SerializeField] protected EnemyEyes enemyEyes;
    [SerializeField] protected ActorMotor2D mover;
    protected float searchTimer = 0;
    protected bool searchHasStarted = false;
    [SerializeField] protected float searchTime = 8f;
    private bool searchShouldEnd = false;
    private float lastKnownLocationTimer = 0;
    [SerializeField] private float lastKnownLocationTime = 0;
    private Rigidbody2D rb;

    //pathfinding
    [SerializeField] protected Astar pathfinder;
    protected List<Node> path;
    protected int pathIndex;
    protected bool routeIsPlanned = false;
    [SerializeField] protected float repathInterval = 0.3f;
    protected float repathTimer;
    protected Node lastGoalNode;
    [SerializeField] protected LayerMask obstacleMask;   
    [SerializeField] protected float agentRadius = 0.3f;

    protected bool HasClearLine()
    {
        Vector2 from = mover.transform.position;
        Vector2 toPlayer = enemyEyes.CurrentLocation - from;
        return !Physics2D.CircleCast(from, agentRadius, toPlayer.normalized, toPlayer.magnitude, obstacleMask);
    }

    protected enum EnemyState {
        Patrolling,
        Searching,
        Chasing,
    }
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {

        if (shouldSearch()) StateMachine(EnemyState.Searching);
        if (shouldChase()) StateMachine(EnemyState.Chasing);
        switch (currentState)
        {
            case EnemyState.Patrolling: { Patrol(); LastKnownLocationTimer(); }
                break;
            case EnemyState.Searching: { Search(); SearchTimer(); }
                break;
            case EnemyState.Chasing: { Chase(); }
                break;
        }
    }

    private void StateMachine(EnemyState state)
    {
        if (currentState == state) return;
        Debug.Log("transitioning from: " + currentState + " to " + state);
        prevState = currentState;
        currentState = state;
        searchTimer = 0;
        searchHasStarted = false;
        routeIsPlanned = false;
        searchShouldEnd = false;
        lastKnownLocationTimer = 0;
        repathTimer = 0;
        lastGoalNode = null;
        mover.Stop();
    }
    #region Behavior
    protected virtual void Patrol()
    {

    }
    protected virtual void Search()
    {

    }
    protected virtual void Chase()
    {

    }
    #endregion
    #region Decision criteria

    private bool shouldSearch()
    {
        return (enemyEyes != null && enemyEyes.HasLastKnownLocation && !enemyEyes.EyesOnPlayer && currentState == EnemyState.Chasing && !searchShouldEnd);
    }
    private bool shouldChase()
    {
        return (enemyEyes != null && enemyEyes.EyesOnPlayer);
    }
    private bool shouldPatrol()
    {
        return (enemyEyes != null && enemyEyes.HasLastKnownLocation && !enemyEyes.EyesOnPlayer && !shouldSearch());
    }
    #endregion
    #region Decision timers
    private void SearchTimer()
    {
        if (!searchHasStarted) return;
        searchTimer += Time.deltaTime;
        if (searchTimer >= searchTime)
        {
            searchShouldEnd = true;
            StateMachine(EnemyState.Patrolling);
        }
    }
    private void LastKnownLocationTimer()
    {
        if (enemyEyes == null || !enemyEyes.HasLastKnownLocation || lastKnownLocationTime <= 0) return;
        lastKnownLocationTimer += Time.deltaTime;
        if (lastKnownLocationTimer >= lastKnownLocationTime)
        {
            lastKnownLocationTimer = 0;
            enemyEyes.LastKnownLocation = Vector2.zero;
            enemyEyes.HasLastKnownLocation = false;
        }
    }
    #endregion
}
