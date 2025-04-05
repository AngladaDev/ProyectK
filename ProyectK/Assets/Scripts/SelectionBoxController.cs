using UnityEngine;

/// <summary>
/// Handles the selection box logic for selecting multiple units in the game.
/// </summary>
public class UnitSelectionBox : MonoBehaviour
{
    private Camera myCam;

    [SerializeField]
    private RectTransform boxVisual;

    private Rect selectionBox;

    private Vector2 startPosition;
    private Vector2 endPosition;

    #region Unity Methods

    // Initialize references and reset selection values.
    private void Start()
    {
        myCam = Camera.main;
        startPosition = Vector2.zero;
        endPosition = Vector2.zero;
        DrawVisual();
    }

    // Update is called once per frame to check input and update the selection box.
    private void Update()
    {
        // Start dragging
        if (Input.GetMouseButtonDown(0))
        {
            startPosition = Input.mousePosition;
            selectionBox = new Rect(); // Reset selection box
        }

        // While dragging
        if (Input.GetMouseButton(0))
        {
            if (boxVisual.rect.width > 0 || boxVisual.rect.height > 0)
            {
                UnitSelectionController.Instance.DeselectAll();
                SelectUnits();
            }

            endPosition = Input.mousePosition;
            DrawVisual();
            DrawSelection();
        }

        // On release
        if (Input.GetMouseButtonUp(0))
        {
            SelectUnits();

            startPosition = Vector2.zero;
            endPosition = Vector2.zero;
            DrawVisual();
        }
    }

    #endregion

    #region Drawing Methods

    // Updates the visual representation of the selection box.
    private void DrawVisual()
    {
        Vector2 boxStart = startPosition;
        Vector2 boxEnd = endPosition;

        Vector2 boxCenter = (boxStart + boxEnd) / 2;
        boxVisual.position = boxCenter;

        Vector2 boxSize = new Vector2(Mathf.Abs(boxStart.x - boxEnd.x), Mathf.Abs(boxStart.y - boxEnd.y));
        boxVisual.sizeDelta = boxSize;
    }

    // Calculates the rectangular area for the selection box.
    private void DrawSelection()
    {
        if (Input.mousePosition.x < startPosition.x)
        {
            selectionBox.xMin = Input.mousePosition.x;
            selectionBox.xMax = startPosition.x;
        }
        else
        {
            selectionBox.xMin = startPosition.x;
            selectionBox.xMax = Input.mousePosition.x;
        }

        if (Input.mousePosition.y < startPosition.y)
        {
            selectionBox.yMin = Input.mousePosition.y;
            selectionBox.yMax = startPosition.y;
        }
        else
        {
            selectionBox.yMin = startPosition.y;
            selectionBox.yMax = Input.mousePosition.y;
        }
    }

    #endregion

    #region Selection Logic

    // Selects units within the current selection box.
    private void SelectUnits()
    {
        foreach (var unit in UnitSelectionController.Instance.unitsList)
        {
            if (selectionBox.Contains(myCam.WorldToScreenPoint(unit.transform.position)))
            {
                UnitSelectionController.Instance.DragSelect(unit);
            }
        }
    }

    #endregion
}