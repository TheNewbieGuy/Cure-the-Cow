using System.Collections.Generic;
using UnityEngine;

public class NightManager : MonoBehaviour
{
    [Header("References")]
    public CustomerManager customerManager;

    public static NightManager Instance;

    [Header("Score")]
    public int currentScore;

    [Header("Cure Results")]
    public int correctCures;
    public int incorrectCures;

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
    public int customersAddedPerNight = 2;

    [Header("Current Night")]
    public int currentNight = 0;

    private int customersRemaining;

    // =========================================================
    // UNITY
    // =========================================================

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        StartNight();
    }

    // =========================================================
    // START NIGHT
    // =========================================================

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

    // =========================================================
    // CUSTOMER COUNT
    // =========================================================

    public bool HasCustomersRemaining()
    {
        return customersRemaining > 0;
    }

    // =========================================================
    // CURE RESULTS
    // =========================================================

    public void RecordCureResult(bool wasCorrect)
    {
        if (wasCorrect)
        {
            correctCures++;
        }
        else
        {
            incorrectCures++;
        }

        Debug.Log(
            wasCorrect
                ? "Cure recorded: CORRECT"
                : "Cure recorded: INCORRECT"
        );
    }

    public void ConsumeCustomer()
    {
        customersRemaining--;

        Debug.Log(
            $"Customers Remaining: {customersRemaining}"
        );

        if (customersRemaining <= 0)
        {
            EndNight();
        }
    }

    // =========================================================
    // END NIGHT
    // =========================================================

    void EndNight()
    {
        Debug.Log(
            $"Night {currentNight + 1} Complete!"
        );

        Debug.Log(
            $"Night Score: {currentScore}"
        );

        currentNight++;

        if (currentNight >= nights.Count)
        {
            Debug.Log("All nights complete!");
            return;
        }

        StartNight();
    }

    // =========================================================
    // GENERATE CONDITION
    // =========================================================

    public ConditionData GenerateCondition()
    {
        if (currentNight >= nights.Count)
        {
            Debug.LogWarning(
                "Current night is outside the available nights list."
            );

            return null;
        }

        List<ConditionData> pool =
            nights[currentNight].availableConditions;

        if (pool == null || pool.Count == 0)
        {
            Debug.LogWarning(
                $"No conditions available for Night {currentNight + 1}."
            );

            return null;
        }

        int index =
            Random.Range(0, pool.Count);

        ConditionData selectedCondition =
            pool[index];

        if (selectedCondition == null)
        {
            Debug.LogWarning(
                "Selected ConditionData is null."
            );

            return null;
        }

        Debug.Log(
            "===== CUSTOMER CONDITION ====="
        );

        Debug.Log(
            "Condition: " +
            selectedCondition.condition
        );

        Debug.Log(
            "===== CONDITION SYMPTOMS ====="
        );

        foreach (Symptoms symptom in selectedCondition.symptoms)
        {
            Debug.Log("- " + symptom);
        }

        Debug.Log(
            "=============================="
        );

        return selectedCondition;
    }
}