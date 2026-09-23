using UnityEngine;
using UnityEngine.InputSystem;

public class CameraZoom : MonoBehaviour
{
    public static CameraZoom Instance;

    [Header("Zoom")]
    public float zoomSpeed = 5f;

    [Header("Click Settings")]
    public float clickCooldown = 0.2f;

    private Vector3 originalPosition;
    private Quaternion originalRotation;

    private Vector3 targetPosition;
    private Quaternion targetRotation;

    private bool isZoomed;
    private float nextClickTime;

    private CustomerManager customerManager;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;

        targetPosition = originalPosition;
        targetRotation = originalRotation;

        customerManager =
            FindFirstObjectByType<CustomerManager>();
    }

    void Update()
    {
        // Smooth movement
        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            Time.deltaTime * zoomSpeed
        );

        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            targetRotation,
            Time.deltaTime * zoomSpeed
        );

        // No customer manager
        if (customerManager == null)
            return;

        // Only allow zoom functionality during inspection
        if (customerManager.currentState !=
            CustomerManager.ClinicState.Inspection)
        {
            // If inspection ended while zoomed in,
            // return the camera to its normal position.
            if (isZoomed)
            {
                ZoomOut();
            }

            return;
        }

        // Prevent the click that caused the zoom-in
        if (Time.time < nextClickTime)
            return;

        // Click anywhere while zoomed in
        if (isZoomed &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            ZoomOut();
        }
    }

    public void ZoomIn(Transform zoomPoint)
    {
        // ONLY allow zooming during inspection
        if (customerManager == null)
            return;

        if (customerManager.currentState !=
            CustomerManager.ClinicState.Inspection)
        {
            return;
        }

        if (isZoomed)
            return;

        targetPosition = zoomPoint.position;
        targetRotation = zoomPoint.rotation;

        isZoomed = true;

        // Prevent same click from triggering ZoomOut
        nextClickTime =
            Time.time + clickCooldown;
    }

    void ZoomOut()
    {
        targetPosition = originalPosition;
        targetRotation = originalRotation;

        isZoomed = false;

        nextClickTime =
            Time.time + clickCooldown;
    }

    public bool IsZoomed()
    {
        return isZoomed;
    }
}