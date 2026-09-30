using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance;

    [Header("UI References")]
    [Tooltip("The parent panel for the pause menu.")]
    public GameObject pauseMenuPanel;

    [Tooltip("Button to resume the game.")]
    public Button resumeButton;

    [Tooltip("Button to quit back to the main menu scene.")]
    public Button quitButton;

    [Tooltip("Button to quit the application completely to the desktop.")]
    public Button quitToDesktopButton;

    [Header("Scene Settings")]
    [Tooltip("The exact name of your main menu scene.")]
    public string mainMenuSceneName = "MainMenu";

    private bool isPaused = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // Hook up button listeners
        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitToTitle);

        if (quitToDesktopButton != null)
            quitToDesktopButton.onClick.AddListener(QuitToDesktop);

        // Ensure pause menu is hidden at start
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
    }

    void Update()
    {
        // Check for Escape key press using standard input or new Input System
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f; // Freezes game time

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);

        // Unlock cursor so the player can click the buttons
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f; // Resumes normal game time

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
    }

    public void QuitToTitle()
    {
        // Always restore time scale before changing scenes, otherwise the next scene will stay frozen!
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void QuitToDesktop()
    {
        Debug.Log("Quitting to desktop...");
        Application.Quit();
    }
}