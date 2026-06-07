using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NPCWander : MonoBehaviour
{
    [Header("Movement Settings")]
    public float wanderRadius = 10f;
    public float walkSpeed = 2f;

    [Header("Idle/Pausing Settings")]
    public float minIdleTime = 3f;
    public float maxIdleTime = 8f;

    [Header("Animation (Optional)")]
    public Animator animator;
    public string walkAnimBoolName = "isWalking";

    private NavMeshAgent agent;
    private float nextMoveTime;
    private bool isWaiting = false;
    private bool isFrozenByDialogue = false; // Flag to halt loop operations

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = walkSpeed;

        MoveToNewRandomLocation();
    }

    void Update()
    {
        // Bypass the entire movement logic loop if we are locked in a conversation frame
        if (isFrozenByDialogue) return;
        if (agent == null || !agent.isOnNavMesh) return;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (!isWaiting)
            {
                isWaiting = true;
                float waitTime = Random.Range(minIdleTime, maxIdleTime);
                nextMoveTime = Time.time + waitTime;

                UpdateAnimation(false);
            }
            else
            {
                if (Time.time >= nextMoveTime)
                {
                    isWaiting = false;
                    MoveToNewRandomLocation();
                }
            }
        }
        else
        {
            UpdateAnimation(true);
        }
    }

    void MoveToNewRandomLocation()
    {
        if (agent == null || !agent.isOnNavMesh) return;
        Vector3 newTarget = GetRandomNavMeshPosition(transform.position, wanderRadius);
        agent.SetDestination(newTarget);
    }

    Vector3 GetRandomNavMeshPosition(Vector3 startPosition, float distance)
    {
        Vector3 randomDirection = Random.insideUnitSphere * distance;
        randomDirection += startPosition;

        NavMeshHit navHit;
        if (NavMesh.SamplePosition(randomDirection, out navHit, distance, NavMesh.AllAreas))
        {
            return navHit.position;
        }
        return startPosition;
    }

    // NEW EXPOSED FUNCTION: Halts agents and forces animations down to static idle
    public void StopWandering()
    {
        isFrozenByDialogue = true;

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true; // Tells NavMesh to preserve the path but stop moving
            agent.velocity = Vector3.zero;
        }

        UpdateAnimation(false);
    }

    // NEW EXPOSED FUNCTION: Unlocks path tracking constraints
    public void ResumeWandering()
    {
        isFrozenByDialogue = false;

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = false; // Resumes physical movement tracking safely
        }

        // Pick up movement right away instead of getting stuck in long leftover waiting periods
        isWaiting = false;
        MoveToNewRandomLocation();
    }

    void UpdateAnimation(bool isWalking)
    {
        if (animator != null && !string.IsNullOrEmpty(walkAnimBoolName))
        {
            animator.SetBool(walkAnimBoolName, isWalking);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, wanderRadius);
    }
}