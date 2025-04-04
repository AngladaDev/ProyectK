using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;

/// <summary>
/// Controls unit selection, multi-selection and movement marker visualization.
/// Implements singleton pattern for global access.
/// </summary>
public class UnitSelectionController : MonoBehaviour
{
    #region Singleton

    /// <summary>
    /// Global singleton instance of UnitSelectionController.
    /// </summary>
    public static UnitSelectionController Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    #endregion

    #region Public Fields

    /// <summary>
    /// List of all units in the game.
    /// </summary>
    public List<GameObject> unitsList = new List<GameObject>();

    /// <summary>
    /// List of currently selected units.
    /// </summary>
    public List<GameObject> selectedUnits = new List<GameObject>();

    /// <summary>
    /// Layer mask used to identify clickable/selectable objects.
    /// </summary>
    public LayerMask clickable;

    /// <summary>
    /// Layer mask used to identify valid ground for move commands.
    /// </summary>
    public LayerMask ground;

    /// <summary>
    /// GameObject used to visually mark the destination of a move command.
    /// </summary>
    public GameObject groundMarker;

    #endregion

    #region Private Fields

    private Camera cam;

    #endregion

    #region Unity Events

    private void Start()
    {
        cam = Camera.main;   
    }

    private void Update()
    {
        // Handle left-click for selection
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, clickable))
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    MultiSelect(hit.collider.gameObject);
                }
                else
                {
                    SelectByClick(hit.collider.gameObject);
                }
            }
            else
            {
                if (!Input.GetKey(KeyCode.LeftShift))
                {
                    DeselectAll();
                }
            }
        }

        // Handle right-click to place movement marker
        if (Input.GetMouseButtonDown(1) && selectedUnits.Count > 0)
        {
            RaycastHit hit;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, ground))
            {
                groundMarker.transform.position = hit.point;

                // Disable and re-enable for visual animation reset
                groundMarker.SetActive(false);
                groundMarker.SetActive(true);
            }
        }
    }

    #endregion

    #region Selection Logic

    // Selects a single unit and deselects others
    private void SelectByClick(GameObject unit)
    {
        DeselectAll();

        selectedUnits.Add(unit);

        ShowSelectionIndicator(unit, true);
        EnableMovement(unit, true);
    }

    // Adds or removes a unit from the multi-selection list
    private void MultiSelect(GameObject unit)
    {
        if (selectedUnits.Contains(unit) == false)
        {
            selectedUnits.Add(unit);
            ShowSelectionIndicator(unit, true);
            EnableMovement(unit, true);
        }
        else
        {
            EnableMovement(unit, false);
            ShowSelectionIndicator(unit, false);
            selectedUnits.Remove(unit);
        }
    }

    // Deselects all currently selected units
    private void DeselectAll()
    {
        foreach (var unit in selectedUnits)
        {
            EnableMovement(unit, false);
            ShowSelectionIndicator(unit, false);
        }

        groundMarker.SetActive(false);
        
        selectedUnits.Clear();
    }

    #endregion

    #region Utility

    // Enables or disables the movement script on a unit
    private void EnableMovement(GameObject unit, bool shouldMove)
    {
        unit.GetComponent<UnitMovementController>().enabled = shouldMove;
    }

    // Shows or hides the selection indicator of a unit
    private void ShowSelectionIndicator(GameObject unit, bool isVisible)
    {
        unit.transform.GetChild(0).gameObject.SetActive(isVisible);
    }

    #endregion
}

