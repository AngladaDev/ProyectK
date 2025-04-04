using UnityEngine;

/// <summary>
/// Represents a unit in the game.
/// Automatically registers itself to the selection system when enabled,
/// and unregisters when destroyed.
/// </summary>
public class Unit : MonoBehaviour
{
    #region Unity Events

    private void Start()
    {
        // Add this unit to the global unit list when the game starts
        UnitSelectionController.Instance.unitsList.Add(gameObject);
    }

    private void OnDestroy()
    {
        // Remove this unit from the global unit list when destroyed
        UnitSelectionController.Instance.unitsList.Remove(gameObject);
    }

    #endregion
}

