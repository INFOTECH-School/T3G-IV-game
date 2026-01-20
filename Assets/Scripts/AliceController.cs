using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AliceController : MonoBehaviour
{
    public List<Transform> waypoints;
    public int currentTargetIndex;
    private NavMeshAgent _agent;
    public bool isLooped = false;
    
    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    void MoveToNextPoint()
    {
        if (currentTargetIndex >= waypoints.Count && isLooped)
        {
            currentTargetIndex = 0;
        } else if (currentTargetIndex >= waypoints.Count)
        {
            return;
        }
        _agent.SetDestination(waypoints[currentTargetIndex].position);
        currentTargetIndex++;
    }

    void Update()
    {
        Debug.Log(_agent.destination);
        if (_agent.remainingDistance <= _agent.stoppingDistance || _agent.destination == transform.position)
        {
            MoveToNextPoint();
        }
    }
}
