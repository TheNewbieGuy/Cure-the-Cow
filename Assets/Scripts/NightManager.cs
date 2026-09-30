using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro; // Added for TextMeshPro support

public class NightManager : MonoBehaviour
{
    [Header("References")]
    public CustomerManager customerManager;
    public static NightManager Instance;

    [Header("Score & Risk System")]
    public int currentScore;
    public int correctCures;
    public int incorrectCures;

    [Range(0f, 100f)]
    public float globalRiskPercentage = 35f; // Exactly matches total max reduction (35 customers * 1%)

    [Header("Risk Modifiers")]
    public float riskReductionPerCorrect = 1f;  // -1% per correct
    public float riskIncreasePerIncorrect = 12f; // +12% per mistake

    [Header("UI Transition References")]
    public CanvasGroup fadeCanvasGroup; 
    public TextMeshProUGUI riskDisplayText; // Updated to TextMeshProUGUI
    public float fadeDuration = 1.5f;
    public string mainMenuSceneName = "MainMenu";

    [Header("Ending Canvases")]
    [Tooltip("Canvas shown if global risk reaches 100% at the end of a night.")]
    public GameObject instantGameOverCanvas;
    [Tooltip("Canvas shown if final risk is exactly 0% after all nights.")]
    public GameObject ending1PerfectCanvas;
    [Tooltip("Canvas shown if final risk is under 30% after all nights.")]
    public GameObject ending2GoodCanvas;
    [Tooltip("Canvas shown if final risk is 30% or higher after all nights.")]
    public GameObject ending3StandardCanvas;

    // =========================================================
    // NIGHT DATA
    // =========================================================

    [System.Serializable]
    public class NightData
    {
        [Header("Conditions Available This Night")]
        public List<ConditionData> availableConditions =
            new List<ConditionData>();
    }

    [Header("Nights")]
    public List<NightData> nights =
        new List<NightData>();

    // =========================================================
    // SETTINGS
    // =========================================================

    [Header("Settings")]
    public int startingCustomers = 3;
    public int customersAddedPerNight = 2; // Results in 3, 5, 7, 9, 11 customers per night (35 total)

    [Header("Current Night")]
    public int currentNight = 0;

    private int customersRemaining;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Ensure ending canvases are hidden at start
        if (instantGameOverCanvas) instantGameOverCanvas.SetActive(false);
        if (ending1PerfectCanvas) ending1PerfectCanvas.SetActive(false);
        if (ending2GoodCanvas) ending2GoodCanvas.SetActive(false);
        if (ending3StandardCanvas) ending3StandardCanvas.SetActive(false);

        StartNight();
    }

    public void StartNight()
    {
        currentScore = 0;
        customersRemaining =
            startingCustomers +
            (currentNight * customersAddedPerNight);

        Debug.Log(
            $"Night {currentNight + 1} started | Customers: {customersRemaining}"
        );

        customerManager.SpawnCustomer();
    }

    public bool HasCustomersRemaining()
    {
        return customersRemaining > 0;
    }

    public void RecordCureResult(bool wasCorrect)
    {
        if (wasCorrect)
        {
            correctCures++;
            globalRiskPercentage -= riskReductionPerCorrect;
        }
        else
        {
            incorrectCures++;
            globalRiskPercentage += riskIncreasePerIncorrect;
        }

        // Clamp risk strictly between 0 and 100
        globalRiskPercentage = Mathf.Clamp(globalRiskPercentage, 0f, 100f);

        Debug.Log($"Cure recorded: {(wasCorrect ? "CORRECT" : "INCORRECT")} | Global Risk: {globalRiskPercentage}%");
    }

    public void ConsumeCustomer()
    {
        customersRemaining--;

        if (customersRemaining <= 0)
        {
            StartCoroutine(TransitionToNextNightRoutine());
        }
    }

    IEnumerator TransitionToNextNightRoutine()
    {
        Debug.Log($"Night {currentNight + 1} Complete!");

        // 1. Fade to Black
        yield return StartCoroutine(FadeScreen(0f, 1f, fadeDuration));

        // 2. Display Risk Percentage on Screen while it's black
        if (riskDisplayText != null)
        {
            riskDisplayText.text = $"Global Risk: {globalRiskPercentage:F0}%";
            riskDisplayText.gameObject.SetActive(true);
        }

        // Pause on the risk screen to let the player read it
        yield return new WaitForSeconds(3f);

        if (riskDisplayText != null)
        {
            riskDisplayText.gameObject.SetActive(false);
        }

        // 3. Check if global risk reached 100% by the end of this night
        if (globalRiskPercentage >= 100f)
        {
            TriggerGameOver();
            yield break;
        }

        currentNight++;

        // 4. Check if all nights are finished
        if (currentNight >= nights.Count)
        {
            EvaluateFinalEndings();
            yield break;
        }

        // If it's just a regular night transition, fade back in for the next night
        StartNight();
        yield return StartCoroutine(FadeScreen(1f, 0f, fadeDuration));
    }

    IEnumerator FadeScreen(float startAlpha, float endAlpha, float duration)
    {
        if (fadeCanvasGroup == null) yield break;

        float timer = 0f;
        fadeCanvasGroup.alpha = startAlpha;
        fadeCanvasGroup.gameObject.SetActive(true);

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime; // Uses unscaled time so it works even if timeScale is 0
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, timer / duration);
            yield return null;
        }

        fadeCanvasGroup.alpha = endAlpha;
        if (endAlpha == 0f)
        {
            fadeCanvasGroup.gameObject.SetActive(false);
        }
    }

    void TriggerGameOver()
    {
        Debug.Log("GAME OVER: Global Risk reached 100% at the end of the night!");
        StartCoroutine(HandleEndingSequence(instantGameOverCanvas));
    }

    void EvaluateFinalEndings()
    {
        Debug.Log("All nights complete! Evaluating final ending...");

        GameObject selectedEndingCanvas = null;

        if (globalRiskPercentage <= 0f)
        {
            Debug.Log("ENDING 1: Perfect Ending (0% Risk achieved)!");
            selectedEndingCanvas = ending1PerfectCanvas;
        }
        else if (globalRiskPercentage < 30f)
        {
            Debug.Log("ENDING 2: Good Ending (Under 30% Risk)!");
            selectedEndingCanvas = ending2GoodCanvas;
        }
        else
        {
            Debug.Log("ENDING 3: Standard/Bad Ending (30% or higher Risk)!");
            selectedEndingCanvas = ending3StandardCanvas;
        }

        StartCoroutine(HandleEndingSequence(selectedEndingCanvas));
    }

    IEnumerator HandleEndingSequence(GameObject canvasToShow)
    {
        Time.timeScale = 0f; // Freeze game actions underneath
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // 1. Screen is already black from the risk percentage screen. 
        // Spawn/activate the ending canvas *behind* the black screen (while fadeCanvasGroup alpha is still 1).
        if (canvasToShow != null)
        {
            canvasToShow.SetActive(true);
        }

        // 2. Fade the black screen away (from alpha 1 to 0) to reveal the ending underneath
        yield return StartCoroutine(FadeScreen(1f, 0f, fadeDuration));

        // 3. Show ending canvas for 5 seconds (using unscaled time since timeScale is 0)
        yield return new WaitForSecondsRealtime(5f);

        // 4. Fade back to black (alpha 0 to 1) to cover the ending panel
        yield return StartCoroutine(FadeScreen(0f, 1f, fadeDuration));

        // Restore time scale before changing scenes
        Time.timeScale = 1f;

        // Return to main menu scene
        SceneManager.LoadScene(mainMenuSceneName);
    }
    

    public ConditionData GenerateCondition()
    {
        if (currentNight >= nights.Count) return null;
        List<ConditionData> pool = nights[currentNight].availableConditions;
        if (pool == null || pool.Count == 0) return null;
        return pool[Random.Range(0, pool.Count)];
    }
}