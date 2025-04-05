using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
    /// Layer mask used to identify attackable objects.
    /// </summary>
    public LayerMask attackable;

    /// <summary>
    /// Layer mask used to identify valid ground for move commands.
    /// </summary>
    public LayerMask ground;

    /// <summary>
    /// GameObject used to visually mark the destination of a move command.
    /// </summary>
    public GameObject groundMarker;

    public bool enableAttackCursor;

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
        HandleSelectionInput();
        HandleMovementMarker();
        HandleSelectionMode();
    }

    #endregion

    #region Input Handlers

    // Handles unit selection via left click
    private void HandleSelectionInput()
    {
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
    }

    // Handles placing the movement marker via right click
    private void HandleMovementMarker()
    {
        if (Input.GetMouseButtonDown(1) && selectedUnits.Count > 0)
        {
            RaycastHit hit;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, ground))
            {
                //Enable the Ground Maker UI
                groundMarker.transform.position = hit.point;
                StartCoroutine(EnableGroundMarker(hit.point, 1f));
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
        HandleSelection(unit, true);
    }

    // Adds or removes a unit from the multi-selection list
    private void MultiSelect(GameObject unit)
    {
        if (!selectedUnits.Contains(unit))
        {
            selectedUnits.Add(unit);
            HandleSelection(unit, true);
        }
        else
        {
            HandleSelection(unit, false);
            selectedUnits.Remove(unit);
        }
    }

    // Adds units selected by drag box (multi-drag)
    internal void DragSelect(GameObject unit)
    {
        if (!selectedUnits.Contains(unit))
        {
            selectedUnits.Add(unit);
            HandleSelection(unit, true);
        }
    }

    // Deselects all currently selected units
    internal void DeselectAll()
    {
        foreach (var unit in selectedUnits)
        {
            HandleSelection(unit, false);
        }

        groundMarker.SetActive(false);

        selectedUnits.Clear();
    }

    #endregion

    #region Attack

    private void HandleSelectionMode()
    {
        if (selectedUnits.Count > 0 && AtleastOffensiveUnit(selectedUnits))
        {
            RaycastHit hit;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, attackable))
            {
                Debug.Log("Enemy Hovered with mouse");
                enableAttackCursor = true;
                if (Input.GetMouseButtonDown(1))
                {
                    Transform targetEnemy = hit.transform;

                    foreach (GameObject unit in selectedUnits)
                    {
                        if (unit.GetComponent<AttackController>())
                        {
                            unit.GetComponent<AttackController>().target = targetEnemy;
                        }
                    }
                }
            }
            else
            {
                enableAttackCursor = false;
            }
        }
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

    // Handles visual and logic for selection/deselection
    private void HandleSelection(GameObject unit, bool isSelected)
    {
        EnableMovement(unit, isSelected);
        ShowSelectionIndicator(unit, isSelected);
    }

    // This check if in the list there are a lest one offensive unit !! IMPORTANT THIS FUCTION MAY CHANGE IN THE FUTURE I DONT KNOW IF I WILL HAVE NON OFFENSIVE UNITS !!
    private bool AtleastOffensiveUnit(List<GameObject> unitList)
    {
        foreach (GameObject unit in unitList)
        {
            if (unit.GetComponent<AttackController>())
            {
                return true;
            }
        }
        return false;
    }

    // Shows the ground marker at the given position for a limited time
    private System.Collections.IEnumerator EnableGroundMarker(Vector3 position, float duration)
    {
        groundMarker.transform.position = position;

        // Reactivate to reset any animation/FX
        groundMarker.SetActive(false);
        groundMarker.SetActive(true);

        yield return new WaitForSeconds(duration);

        groundMarker.SetActive(false);
    }

    #endregion
}