using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Controls the movement of the unit using Unity's NavMeshAgent.
/// Listens for right-click input to move the unit to a valid position on the ground.
/// </summary>
public class UnitMovementController : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Layer mask that defines valid ground for movement.")]
    public LayerMask groundLayer;

    [Tooltip("True if the unit is currently following a command from the player.")]
    public bool isCommanded;

    private NavMeshAgent agent;
    private Camera mainCamera;

    private void Start()
    {
        // Cache the main camera and NavMeshAgent component for performance.
        mainCamera = Camera.main;
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        HandlePlayerInput();
        CheckArrival();
    }

    /// <summary>
    /// Handles right-click input and commands the unit to move.
    /// </summary>
    private void HandlePlayerInput()
    {
        if (Input.GetMouseButtonDown(1)) // Right-click
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            // Raycast against the ground layer to find a valid move point
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
            {
                isCommanded = true;

                // Important: we clicked ground, so we want full precision
                agent.stoppingDistance = 0f;

                agent.SetDestination(hit.point);
            }
        }
    }

    /// <summary>
    /// Checks if the unit has reached its destination and resets the command flag.
    /// </summary>
    private void CheckArrival()
    {
        // Only reset if the agent is done calculating and has reached the destination
        if (!agent.pathPending && (!agent.hasPath || agent.remainingDistance <= agent.stoppingDistance))
        {
            isCommanded = false;
        }
    }
}
