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
        GameManager.Instance.RegisterAliceController(this);
        _agent = GetComponent<NavMeshAgent>();
        _agent.SetDestination(waypoints[currentTargetIndex].position); //Initiate the first target
    }

    void MoveToNextPoint()
    {
        currentTargetIndex++;
        if (currentTargetIndex >= waypoints.Count)
        {
            if (!isLooped) return;
            currentTargetIndex = 0;
        }
        _agent.SetDestination(waypoints[currentTargetIndex].position);
    }

    void Update()
    {
        if (_agent.remainingDistance <= _agent.stoppingDistance || _agent.destination == transform.position)
        {
            MoveToNextPoint();
        }
    }
    
    private void OnDestroy()
    {
        GameManager.Instance.UnregisterAliceController();
    }
}
