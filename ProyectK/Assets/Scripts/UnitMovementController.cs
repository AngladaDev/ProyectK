using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Controls movement of the unit using Unity's NavMeshAgent.
/// It listens for right-clicks to move the unit to the clicked location.
/// </summary>
public class UnitMovementController : MonoBehaviour
{
    /// <summary>
    /// Layer mask that defines valid ground for movement.
    /// </summary>
    public LayerMask ground;

    private NavMeshAgent agent;
    private Camera cam;

    #region Unity Events

    private void Start()
    {
        // Cache the main camera and NavMeshAgent component
        cam = Camera.main;
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        // On right-click, raycast to the ground and move the unit to that position
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, ground))
            {
                agent.SetDestination(hit.point);
            }
        }
    }

    #endregion
}