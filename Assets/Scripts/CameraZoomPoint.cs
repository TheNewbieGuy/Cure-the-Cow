using UnityEngine;
using UnityEngine.InputSystem;

public class ZoomTarget : MonoBehaviour
{
    [Header("Zoom")]
    public Transform zoomPoint;
    public float zoomSpeed = 5f;

    [Header("Required Item")]
    [Tooltip("Leave empty if no item is required.")]
    public string requiredItemType = "";

    [Header("Symptom Check")]
    [Tooltip("Which symptom this zoom point checks for. Looks the image up from the centralized SymptomImageDatabase - no need to assign images per zoom point.")]
    public Symptoms symptomToCheck;

    private Camera mainCamera;
    private CameraController cameraController;
    private Interaction interaction;

    private bool isZoomedIn;
    private bool isZooming;

    private Vector3 originalPosition;
    private Quaternion originalRotation;

    // Shared across ALL ZoomTarget instances - whichever target currently
    // owns the camera (zoomed in, or mid-transition either direction).
    // Null means no zoom point is active and any of them can be clicked.
    // This is what stops a second zoom point from starting its own
    // zoom-in while a different one is still mid-flight - without this,
    // each ZoomTarget only knew about its OWN state, not anyone else's.
    private static ZoomTarget activeTarget;

    void Start()
    {
        mainCamera = Camera.main;

        if (mainCamera == null)
        {
            Debug.LogError(
                "ZoomTarget: No Main Camera found."
            );

            return;
        }

        cameraController =
            mainCamera.GetComponentInParent<CameraController>();

        interaction =
            FindObjectOfType<Interaction>();

        if (zoomPoint == null)
        {
            Debug.LogError(
                "ZoomTarget on " +
                gameObject.name +
                " has no Zoom Point assigned!"
            );
        }
    }


    void Update()
    {
        if (mainCamera == null ||
            zoomPoint == null)
            return;


        // ==========================================
        // CLICK
        // ==========================================

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            TryClick();
        }


        // ==========================================
        // ZOOM MOVEMENT
        // ==========================================

        if (!isZooming)
            return;


        Vector3 targetPosition;
        Quaternion targetRotation;


        if (isZoomedIn)
        {
            targetPosition =
                zoomPoint.position;

            targetRotation =
                zoomPoint.rotation;
        }
        else
        {
            targetPosition =
                originalPosition;

            targetRotation =
                originalRotation;
        }


        // ==========================================
        // MOVE CAMERA
        // ==========================================

        mainCamera.transform.position =
            Vector3.Lerp(
                mainCamera.transform.position,
                targetPosition,
                Time.deltaTime * zoomSpeed
            );


        mainCamera.transform.rotation =
            Quaternion.Lerp(
                mainCamera.transform.rotation,
                targetRotation,
                Time.deltaTime * zoomSpeed
            );


        // ==========================================
        // CHECK ARRIVAL
        // ==========================================

        if (
            Vector3.Distance(
                mainCamera.transform.position,
                targetPosition
            ) < 0.01f
            &&
            Quaternion.Angle(
                mainCamera.transform.rotation,
                targetRotation
            ) < 0.5f
        )
        {
            mainCamera.transform.position =
                targetPosition;

            mainCamera.transform.rotation =
                targetRotation;

            isZooming = false;


            // ======================================
            // ZOOMED IN
            // ======================================

            if (isZoomedIn)
            {
                // Camera remains locked.

                if (cameraController != null)
                {
                    cameraController.LockCamera();
                }

                ShowSymptomImage();
            }


            // ======================================
            // ZOOMED OUT
            // ======================================

            else
            {
                if (cameraController != null)
                {
                    cameraController.UnlockCamera();
                }

                // We've fully returned - release ownership so any
                // zoom point (including a different one) can be clicked.
                if (activeTarget == this)
                {
                    activeTarget = null;
                }
            }
        }
    }


    // =========================================================
    // CLICK HANDLING
    // =========================================================

    void TryClick()
    {
        // Some OTHER zoom point currently owns the camera (zoomed in or
        // mid-transition) - ignore clicks on this one entirely until it
        // releases ownership.
        if (activeTarget != null && activeTarget != this)
            return;

        // Don't allow clicking while THIS target is transitioning
        if (isZooming)
            return;


        // ==========================================
        // ZOOM OUT
        // ==========================================

        if (isZoomedIn)
        {
            ZoomOut();
            return;
        }


        // ==========================================
        // INSPECTION CHECK
        // ==========================================

        CustomerManager customerManager =
            FindObjectOfType<CustomerManager>();

        if (customerManager == null)
            return;

        if (
            customerManager.currentState !=
            CustomerManager.ClinicState.Inspection
        )
        {
            return;
        }


        // ==========================================
        // ITEM CHECK
        // ==========================================

        if (!HasRequiredItem())
            return;


        // ==========================================
        // RAYCAST
        // ==========================================

        Ray ray =
            mainCamera.ScreenPointToRay(
                Mouse.current.position.ReadValue()
            );


        // Ignore held item

        PickupObject held =
            interaction != null
                ? interaction.HeldObject
                : null;

        Collider heldCollider = null;

        if (held != null)
        {
            heldCollider =
                held.GetComponent<Collider>();

            if (heldCollider != null)
                heldCollider.enabled = false;
        }


        bool hitTarget =
            Physics.Raycast(
                ray,
                out RaycastHit hit,
                100f
            );


        if (heldCollider != null)
            heldCollider.enabled = true;


        if (!hitTarget)
            return;


        // ==========================================
        // TARGET CHECK
        // ==========================================

        if (
            hit.transform == transform ||
            hit.transform.IsChildOf(transform)
        )
        {
            ZoomIn();
        }
    }


    // =========================================================
    // REQUIRED ITEM
    // =========================================================

    bool HasRequiredItem()
    {
        // No item required
        if (
            string.IsNullOrEmpty(
                requiredItemType
            )
        )
        {
            return true;
        }


        if (interaction == null)
            return false;


        PickupObject held =
            interaction.HeldObject;


        if (held == null)
            return false;


        return
            held.itemType ==
            requiredItemType;
    }


    // =========================================================
    // SYMPTOM IMAGE
    // =========================================================

    void ShowSymptomImage()
    {
        if (CustomerManager.Instance == null ||
            SymptomImageDatabase.Instance == null ||
            ZoomInspectionDisplay.Instance == null)
            return;

        CustomerData data =
            CustomerManager.Instance.GetCurrentCustomerData();

        if (data == null)
            return;

        bool isPresent =
            data.symptoms.Contains(symptomToCheck);

        Sprite image =
            SymptomImageDatabase.Instance.GetImage(
                symptomToCheck,
                isPresent
            );

        ZoomInspectionDisplay.Instance.Show(image);
    }


    // =========================================================
    // ZOOM IN
    // =========================================================

    void ZoomIn()
    {
        if (zoomPoint == null)
            return;


        // ==========================================
        // SAVE EXACT CURRENT CAMERA STATE
        // ==========================================

        originalPosition =
            mainCamera.transform.position;

        originalRotation =
            mainCamera.transform.rotation;


        // ==========================================
        // CLAIM OWNERSHIP + LOCK CAMERA CONTROLLER
        // ==========================================

        activeTarget = this;

        if (cameraController != null)
        {
            cameraController.LockCamera();
        }


        isZoomedIn = true;
        isZooming = true;
    }


    // =========================================================
    // ZOOM OUT
    // =========================================================

    void ZoomOut()
    {
        // Keep controller locked while
        // camera returns to its original state.

        if (cameraController != null)
        {
            cameraController.LockCamera();
        }

        if (ZoomInspectionDisplay.Instance != null)
        {
            ZoomInspectionDisplay.Instance.Hide();
        }


        isZoomedIn = false;
        isZooming = true;
        // activeTarget stays set to `this` until arrival (see Update()) -
        // it still owns the camera during the return transition.
    }
}