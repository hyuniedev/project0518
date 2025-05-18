using System;
using UnityEngine;
using UnityEngine.AI;

public class Soldier : MonoBehaviour
{
    private NavMeshAgent _agent;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    public void MoveTo(Vector3 newPosition)
    {
        _agent.SetDestination(newPosition);
    }
}