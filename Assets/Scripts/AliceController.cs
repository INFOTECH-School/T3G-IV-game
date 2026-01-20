using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AliceController : MonoBehaviour
{
    public List<Transform> waypoints;
    public int currentTargetIndex;
    private NavMeshAgent _agent;
    
    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    void MoveToNextPoint()
    {
        currentTargetIndex++;
        if (currentTargetIndex >= waypoints.Count)
        {
            currentTargetIndex = 0;
        }
        _agent.SetDestination(waypoints[currentTargetIndex].position);
    }

    void Update()
    {
        if (_agent.remainingDistance <= _agent.stoppingDistance)
        {
            MoveToNextPoint();
        }
    }
}
