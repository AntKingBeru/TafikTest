using UnityEngine;
using UnityEngine.AI;

public class NavMeshPathTest : MonoBehaviour
{
    [SerializeField] private NavMeshAgent navMeshAgent;
    [SerializeField] private Transform targetDest;
    
    void Start()
    {
        navMeshAgent.SetDestination(targetDest.position);
    }
    
    void Update()
    {
        
    }
}