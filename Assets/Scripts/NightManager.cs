using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    public Text riskDisplayText;        
    public float fadeDuration = 1.5f;

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

        // Check immediate Game Over condition if risk hits 100%
        if (globalRiskPercentage >= 100f)
        {
            TriggerGameOver();
        }
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

        // 2. Display Risk Percentage on Screen
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

        currentNight++;

        // 3. Check if all 5 nights are finished
        if (currentNight >= nights.Count)
        {
            EvaluateFinalEndings();
            yield break;
        }

        // 4. Fade back in for the next night
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
            timer += Time.deltaTime;
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
        Debug.Log("GAME OVER: Global Risk reached 100%!");
        // TODO: Handle Game Over scene loading or UI display here
    }

    void EvaluateFinalEndings()
    {
        Debug.Log("All 5 nights complete!");

        if (globalRiskPercentage <= 0f)
        {
            Debug.Log("ENDING 1: Perfect Ending (0% Risk achieved via 5 flawless nights)!");
        }
        else if (globalRiskPercentage < 30f)
        {
            Debug.Log("ENDING 2: Good Ending (Under 30% Risk)!");
        }
        else
        {
            Debug.Log("ENDING 3: Standard/Bad Ending (30% or higher Risk)!");
        }
    }

    public ConditionData GenerateCondition()
    {
        if (currentNight >= nights.Count) return null;
        List<ConditionData> pool = nights[currentNight].availableConditions;
        if (pool == null || pool.Count == 0) return null;
        return pool[Random.Range(0, pool.Count)];
    }
}