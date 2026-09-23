using UnityEngine;
using UnityEngine.InputSystem;

public class ToggleCanvasController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The Canvas or Panel GameObject to show/hide when pressing E.")]
    public GameObject canvasToToggle;

    [Tooltip("Reference to the CameraController in the scene.")]
    public CameraController cameraController;

    [Header("Settings")]
    public Key toggleKey = Key.E;

    private bool isOpen = false;

    void Start()
    {
        if (cameraController == null)
        {
            cameraController = FindFirstObjectByType<CameraController>();
        }

        if (canvasToToggle != null)
        {
            canvasToToggle.SetActive(false);
        }
    }

    void Update()
    {
        // Check for 'E' key press using the new Input System Keyboard API
        if (Keyboard.current != null && Keyboard.current[toggleKey].wasPressedThisFrame)
        {
            ToggleMenu();
        }
    }

    public void ToggleMenu()
    {
        isOpen = !isOpen;

        if (canvasToToggle != null)
        {
            canvasToToggle.SetActive(isOpen);
        }

        if (cameraController != null)
        {
            if (isOpen)
            {
                // Lock camera rotation while UI is open
                cameraController.LockCamera();
            }
            else
            {
                // Unlocks camera and recalculates target rotation based on 
                // the cursor's new position so camera movement stays smooth.
                cameraController.UnlockCamera();
            }
        }
    }

    public bool IsOpen()
    {
        return isOpen;
    }
}