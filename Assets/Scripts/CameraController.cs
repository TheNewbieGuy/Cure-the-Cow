using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("References")]
    public Transform cameraPivot;

    [Header("Rotation Limits")]
    public float maxYaw = 15f;
    public float maxLookDown = 18f;

    [Header("Movement")]
    public float rotationSmoothness = 5f;

    [Header("Horizontal Deadzone")]
    [Range(0f, 0.5f)]
    public float horizontalDeadzone = 0.25f;

    [Header("Vertical Deadzone")]
    [Range(0f, 1f)]
    public float verticalDeadzone = 0.25f;

    [Tooltip("Moves the vertical deadzone upwards.")]
    [Range(0f, 1f)]
    public float verticalDeadzoneOffset = 0.25f;

    private Vector2 currentRotation;
    private Vector2 targetRotation;

    private bool cameraLocked;

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = false;

        // Get the current pivot rotation
        Vector3 startingEuler =
            cameraPivot.localEulerAngles;

        currentRotation.x =
            NormalizeAngle(startingEuler.y);

        currentRotation.y =
            NormalizeAngle(startingEuler.x);

        targetRotation = currentRotation;
    }

    void Update()
    {
        // ==========================================
        // CAMERA IS LOCKED
        // ==========================================
        //
        // Do absolutely NOTHING.
        //
        // Don't read mouse.
        // Don't calculate target rotation.
        // Don't change current rotation.
        // Don't touch the pivot.
        //
        if (cameraLocked)
            return;


        // ==========================================
        // NORMAL CAMERA CONTROL
        // ==========================================

        UpdateTargetRotation();

        currentRotation =
            Vector2.Lerp(
                currentRotation,
                targetRotation,
                Time.deltaTime * rotationSmoothness
            );

        ApplyRotation();
    }


    // =========================================================
    // CALCULATE WHERE CAMERA SHOULD LOOK
    // =========================================================

    void UpdateTargetRotation()
    {
        Vector2 mousePos =
            Mouse.current.position.ReadValue();

        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        float normalizedX =
            (mousePos.x / screenWidth) * 2f - 1f;

        float normalizedY =
            (mousePos.y / screenHeight) * 2f - 1f;


        // -------------------------
        // HORIZONTAL
        // -------------------------

        float horizontalInfluence =
            GetEdgeInfluence(
                normalizedX,
                horizontalDeadzone
            );

        targetRotation.x =
            horizontalInfluence * maxYaw;


        // -------------------------
        // VERTICAL
        // -------------------------

        float shiftedY =
            normalizedY +
            verticalDeadzoneOffset;

        shiftedY =
            Mathf.Clamp(
                shiftedY,
                -1f,
                0f
            );

        float verticalInfluence =
            GetEdgeInfluence(
                shiftedY,
                verticalDeadzone
            );

        targetRotation.y =
            -verticalInfluence * maxLookDown;
    }


    // =========================================================
    // APPLY ROTATION
    // =========================================================

    void ApplyRotation()
    {
        cameraPivot.localRotation =
            Quaternion.Euler(
                currentRotation.y,
                currentRotation.x,
                0f
            );
    }


    // =========================================================
    // EDGE DEADZONE
    // =========================================================

    float GetEdgeInfluence(
        float value,
        float deadzone
    )
    {
        float absValue =
            Mathf.Abs(value);

        if (absValue < deadzone)
            return 0f;

        float sign =
            Mathf.Sign(value);

        float t =
            Mathf.InverseLerp(
                deadzone,
                1f,
                absValue
            );

        t =
            Mathf.SmoothStep(
                0f,
                1f,
                t
            );

        return t * sign;
    }


    // =========================================================
    // LOCK / UNLOCK
    // =========================================================

    public void LockCamera()
    {
        cameraLocked = true;
    }


    public void UnlockCamera()
    {
        cameraLocked = false;

        // Immediately recalculate the target
        // from the mouse when control returns.

        UpdateTargetRotation();
    }


    public bool IsCameraLocked()
    {
        return cameraLocked;
    }


    // =========================================================
    // GET CURRENT ROTATION
    // =========================================================

    public Vector2 GetCurrentRotation()
    {
        return currentRotation;
    }


    public Vector2 GetTargetRotationValues()
    {
        return targetRotation;
    }


    float NormalizeAngle(float angle)
    {
        if (angle > 180f)
            angle -= 360f;

        return angle;
    }
}