using System;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Represents a unit in the game.
/// Automatically registers itself to the selection system when enabled,
/// and unregisters when destroyed.
/// </summary>
public class Unit : MonoBehaviour
{
    private float unitHealth;
    public float unitMaxHealth;
    public HealthTracker healthTracker;

    #region Unity Events

    private void Start()
    {
        // Add this unit to the global unit list when the game starts
        UnitSelectionController.Instance.unitsList.Add(gameObject);

        // Health assigments
        unitHealth = unitMaxHealth;
        UpdateHealthUI();
    }

    private void UpdateHealthUI()
    {
        healthTracker.UpdateSliderValue(unitHealth, unitMaxHealth);

        if (unitHealth <= 0)
        {
            // Dying Logic

            // Destruction Logic
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        // Remove this unit from the global unit list when destroyed
        UnitSelectionController.Instance.unitsList.Remove(gameObject);
    }

    internal void TakeDamge(float damageToInflict)
    {
        unitHealth -= damageToInflict;
        UpdateHealthUI();
    }

    #endregion
}

