using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// RTS-style camera controller with support for:
/// - Keyboard movement
/// - Edge scrolling (including diagonal directions)
/// - Mouse dragging (middle button)
/// - Dynamic speed boost using Left Control
/// </summary>
public class CameraController : MonoBehaviour
{
    #region Singleton

    public static CameraController instance;

    #endregion

    #region General

    [Header("General")]
    [SerializeField] private Transform cameraTransform;

    // Optional target to follow directly
    public Transform followTarget;

    private Vector3 targetPosition;
    private Vector3 dragStartPosition;
    private Vector3 dragCurrentPosition;

    #endregion

    #region Movement Settings

    [Header("Movement Settings")]
    [SerializeField] private float globalSpeedMultiplier = 1f; // Global speed multiplier
    [SerializeField] private float movementSmoothness = 5f; // Camera smoothing factor

    [SerializeField] private float keyboardSpeed = 10f;
    [SerializeField] private float edgeScrollSpeed = 8f;
    [SerializeField] private float mouseDragSpeed = 12f;

    [SerializeField] private float normalSpeedMultiplier = 1f;
    [SerializeField] private float fastSpeedMultiplier = 2f;

    [SerializeField] private float diagonalBalanceFactor = 1.6f; // Reduce horizontal weight for balanced diagonals

    #endregion

    #region Movement Toggles

    [Header("Movement Modes")]
    [SerializeField] private bool enableKeyboardMovement = true;
    [SerializeField] private bool enableEdgeScrolling = true;
    [SerializeField] private bool enableMouseDrag = true;

    #endregion

    #region Edge Scrolling

    [Header("Edge Scrolling")]
    [SerializeField] private float edgeThreshold = 50f; // Distance from edge to activate scrolling

    [SerializeField] private Texture2D cursorArrowUp;
    [SerializeField] private Texture2D cursorArrowDown;
    [SerializeField] private Texture2D cursorArrowLeft;
    [SerializeField] private Texture2D cursorArrowRight;
    [SerializeField] private Texture2D cursorArrowUpLeft;
    [SerializeField] private Texture2D cursorArrowUpRight;
    [SerializeField] private Texture2D cursorArrowDownLeft;
    [SerializeField] private Texture2D cursorArrowDownRight;

    private bool cursorChanged = false;

    private enum CursorDirection
    {
        UP, DOWN, LEFT, RIGHT,
        UP_LEFT, UP_RIGHT, DOWN_LEFT, DOWN_RIGHT,
        DEFAULT
    }

    private CursorDirection currentCursor = CursorDirection.DEFAULT;

    #endregion

    #region Unity Methods

    private void Start()
    {
        instance = this;
        targetPosition = transform.position;
    }

    private void Update()
    {
        if (followTarget != null)
        {
            // Directly follow a target if assigned
            transform.position = followTarget.position;
        }
        else
        {
            HandleCameraMovement();
        }

        // Stop following target with Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            followTarget = null;
        }
    }

    #endregion

    #region Movement Logic

    // Returns true if the Left Control key is currently held
    private bool IsFastSpeedActive => Input.GetKey(KeyCode.LeftControl);

    // Handles all active movement types
    private void HandleCameraMovement()
    {
        if (enableMouseDrag) HandleMouseDrag();
        if (enableKeyboardMovement) HandleKeyboardMovement();
        if (enableEdgeScrolling) HandleEdgeScrolling();

        // Smoothly move toward the target position
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.unscaledDeltaTime * movementSmoothness);

        // Keep cursor inside window
        Cursor.lockState = CursorLockMode.Confined;
    }

    // Keyboard movement (WASD / arrows)
    private void HandleKeyboardMovement()
    {
        float speedMultiplier = IsFastSpeedActive ? fastSpeedMultiplier : normalSpeedMultiplier;
        float speed = keyboardSpeed * speedMultiplier * globalSpeedMultiplier * Time.unscaledDeltaTime;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            targetPosition += transform.forward * speed;

        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            targetPosition -= transform.forward * speed;

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            targetPosition += transform.right * speed;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            targetPosition -= transform.right * speed;
    }

    // Handles edge scrolling movement and cursor direction based on mouse position
    private void HandleEdgeScrolling()
    {
        float speedMultiplier = IsFastSpeedActive ? fastSpeedMultiplier : normalSpeedMultiplier;
        float speed = edgeScrollSpeed * speedMultiplier * globalSpeedMultiplier * Time.unscaledDeltaTime;

        Vector2 screenSize = new Vector2(Screen.width, Screen.height);
        Vector2 mousePos = Input.mousePosition;

        // Direction from center of screen to mouse position
        Vector2 mouseFromCenter = mousePos - screenSize / 2f;

        // Compensate for screen width dominance to balance diagonal detection
        Vector2 adjustedDir = new Vector2(mouseFromCenter.x / diagonalBalanceFactor, mouseFromCenter.y).normalized;

        // Check proximity to edges
        bool nearHorizontalEdge = mousePos.x < edgeThreshold || mousePos.x > screenSize.x - edgeThreshold;
        bool nearVerticalEdge = mousePos.y < edgeThreshold || mousePos.y > screenSize.y - edgeThreshold;

        if (nearHorizontalEdge || nearVerticalEdge)
        {
            // Convert to world movement direction
            Vector3 worldDirection =
                (transform.right * adjustedDir.x +
                 transform.forward * adjustedDir.y).normalized;

            targetPosition += worldDirection * speed;

            // Convert angle to directional cursor
            float angle = Mathf.Atan2(adjustedDir.y, adjustedDir.x) * Mathf.Rad2Deg;
            angle = (angle + 360f) % 360f;

            CursorDirection cursorDir = GetCursorDirectionFromAngle(angle);
            SetCursor(cursorDir);
            cursorChanged = true;
        }
        else if (cursorChanged)
        {
            SetCursor(CursorDirection.DEFAULT);
            cursorChanged = false;
        }
    }

    // Mouse drag with middle button to pan camera
    private void HandleMouseDrag()
    {
        if (Input.GetMouseButtonDown(2) && !EventSystem.current.IsPointerOverGameObject())
        {
            if (GetGroundPoint(out Vector3 hit))
                dragStartPosition = hit;
        }

        if (Input.GetMouseButton(2) && !EventSystem.current.IsPointerOverGameObject())
        {
            if (GetGroundPoint(out Vector3 hit))
            {
                dragCurrentPosition = hit;
                Vector3 offset = dragStartPosition - dragCurrentPosition;

                float speedMultiplier = IsFastSpeedActive ? fastSpeedMultiplier : normalSpeedMultiplier;
                targetPosition += offset * mouseDragSpeed * speedMultiplier * globalSpeedMultiplier * Time.unscaledDeltaTime;
            }
        }
    }

    // Raycast to horizontal plane to get ground position under cursor
    private bool GetGroundPoint(out Vector3 point)
    {
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (groundPlane.Raycast(ray, out float distance))
        {
            point = ray.GetPoint(distance);
            return true;
        }

        point = Vector3.zero;
        return false;
    }

    // Determine cursor direction based on angle in degrees
    private CursorDirection GetCursorDirectionFromAngle(float angle)
    {
        if (angle >= 337.5f || angle < 22.5f) return CursorDirection.RIGHT;
        if (angle >= 22.5f && angle < 67.5f) return CursorDirection.UP_RIGHT;
        if (angle >= 67.5f && angle < 112.5f) return CursorDirection.UP;
        if (angle >= 112.5f && angle < 157.5f) return CursorDirection.UP_LEFT;
        if (angle >= 157.5f && angle < 202.5f) return CursorDirection.LEFT;
        if (angle >= 202.5f && angle < 247.5f) return CursorDirection.DOWN_LEFT;
        if (angle >= 247.5f && angle < 292.5f) return CursorDirection.DOWN;
        if (angle >= 292.5f && angle < 337.5f) return CursorDirection.DOWN_RIGHT;

        return CursorDirection.DEFAULT;
    }

    #endregion

    #region Cursor Handling

    // Change the system cursor depending on direction
    private void SetCursor(CursorDirection direction)
    {
        if (currentCursor == direction) return;

        Texture2D cursorTexture = null;
        Vector2 hotspot;

        switch (direction)
        {
            case CursorDirection.UP:
                cursorTexture = cursorArrowUp;
                hotspot = Vector2.zero;
                break;
            case CursorDirection.DOWN:
                cursorTexture = cursorArrowDown;
                hotspot = new Vector2(cursorArrowDown.width / 2f, cursorArrowDown.height);
                break;
            case CursorDirection.LEFT:
                cursorTexture = cursorArrowLeft;
                hotspot = Vector2.zero;
                break;
            case CursorDirection.RIGHT:
                cursorTexture = cursorArrowRight;
                hotspot = new Vector2(cursorArrowRight.width, cursorArrowRight.height / 2f);
                break;
            case CursorDirection.UP_LEFT:
                cursorTexture = cursorArrowUpLeft;
                hotspot = new Vector2(0, cursorArrowUpLeft.height);
                break;
            case CursorDirection.UP_RIGHT:
                cursorTexture = cursorArrowUpRight;
                hotspot = new Vector2(cursorArrowUpRight.width, 0);
                break;
            case CursorDirection.DOWN_LEFT:
                cursorTexture = cursorArrowDownLeft;
                hotspot = new Vector2(0, cursorArrowDownLeft.height);
                break;
            case CursorDirection.DOWN_RIGHT:
                cursorTexture = cursorArrowDownRight;
                hotspot = new Vector2(cursorArrowDownRight.width, cursorArrowDownRight.height);
                break;
            default:
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                currentCursor = CursorDirection.DEFAULT;
                return;
        }

        Cursor.SetCursor(cursorTexture, hotspot, CursorMode.Auto);
        currentCursor = direction;
    }

    #endregion
}