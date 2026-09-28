using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerManager : MonoBehaviour
{
    public static CustomerManager Instance;

    public enum ClinicState
    {
        WaitingForCustomer,
        CustomerEntering,
        Inspection,
        CustomerLeaving,
        Transition
    }

    [Header("Customers")]
    public CustomerController[] customerPrefabs;

    [Header("Positions")]
    public Transform spawnPoint;
    public Transform standPoint;

    [Header("Timing")]
    public float nextCustomerDelay = 2f;

    [Header("Current State")]
    public ClinicState currentState =
        ClinicState.WaitingForCustomer;

    private CustomerController currentCustomer;

    private bool customerInside;
    private bool busy;

    public CustomerData GetCurrentCustomerData()
    {
        return currentCustomer != null
            ? currentCustomer.GetComponent<CustomerData>()
            : null;
    }

    void Awake()
    {
        Instance = this;
    }

    // =========================================================
    // SPAWN CUSTOMER
    // =========================================================

    public void SpawnCustomer()
    {
        if (!NightManager.Instance.HasCustomersRemaining())
            return;

        currentState =
            ClinicState.CustomerEntering;

        CustomerController prefab =
            customerPrefabs[
                Random.Range(
                    0,
                    customerPrefabs.Length
                )
            ];

        // IMPORTANT:
        // Do NOT use spawnPoint.rotation.
        // Customers always start at X=0, Y=180, Z=0.
        currentCustomer =
            Instantiate(
                prefab,
                spawnPoint.position,
                Quaternion.Euler(0f, 180f, 0f)
            );

        AssignCondition(currentCustomer);

        StartCoroutine(
            CustomerEnterRoutine()
        );
    }

    // =========================================================
    // CUSTOMER ENTERING
    // =========================================================

    IEnumerator CustomerEnterRoutine()
    {
        busy = true;

        currentCustomer.MoveTo(
            standPoint.position
        );

        while (currentCustomer.IsMoving())
        {
            yield return null;
        }

        customerInside = true;

        CustomerData data =
            currentCustomer.GetComponent<CustomerData>();

        if (DialogUI.Instance != null &&
            data != null &&
            data.dialogLines != null &&
            data.dialogLines.Count > 0)
        {
            yield return StartCoroutine(
                DialogUI.Instance.ShowDialogRoutine(
                    data.dialogLines
                )
            );
        }

        currentState =
            ClinicState.Inspection;

        Debug.Log(
            "INSPECTION STARTED"
        );

        busy = false;
    }

    // =========================================================
    // GIVE CURE
    // =========================================================

    public bool TryGiveCure(PickupObject item)
    {
        if (currentState != ClinicState.Inspection)
            return false;

        if (busy ||
            currentCustomer == null ||
            item == null)
            return false;

        CustomerData data =
            currentCustomer.GetComponent<CustomerData>();

        if (data == null ||
            data.acceptedCureItemTypes == null ||
            data.acceptedCureItemTypes.Count == 0)
            return false;

        bool wasCorrectCure =
            data.acceptedCureItemTypes.Contains(
                item.itemType
            );

        Debug.Log(
            wasCorrectCure
                ? "CORRECT CURE GIVEN"
                : "WRONG CURE GIVEN"
        );

        StartCoroutine(
            CustomerExitRoutine(wasCorrectCure)
        );

        return true;
    }

    // =========================================================
    // DIAGNOSIS
    // =========================================================

    void EvaluateDiagnosis()
    {
        CustomerData data =
            currentCustomer.GetComponent<CustomerData>();

        if (data == null)
            return;

        List<Symptoms> selectedSymptoms =
            SymptomUI.Instance.GetSelectedSymptoms();

        bool correct =
            selectedSymptoms.Count ==
            data.symptoms.Count;

        if (correct)
        {
            foreach (Symptoms symptom in data.symptoms)
            {
                if (!selectedSymptoms.Contains(symptom))
                {
                    correct = false;
                    break;
                }
            }
        }

        if (correct)
        {
            Debug.Log("CORRECT DIAGNOSIS");

            Debug.Log(
                "Condition: " +
                data.condition
            );

            NightManager.Instance.currentScore++;
        }
        else
        {
            Debug.Log("WRONG DIAGNOSIS");

            Debug.Log(
                "Correct Condition: " +
                data.condition
            );
        }

        SymptomUI.Instance.ResetToggles();
    }

    // =========================================================
    // CUSTOMER LEAVING
    // =========================================================

    IEnumerator CustomerExitRoutine(
        bool cureWasCorrect
    )
    {
        busy = true;

        currentState =
            ClinicState.CustomerLeaving;

        Debug.Log(
            "INSPECTION ENDED"
        );

        NightManager.Instance.RecordCureResult(
            cureWasCorrect
        );

        EvaluateDiagnosis();

        // Turn the customer around BEFORE moving.
        // Their X and Z rotation remain 0.
        currentCustomer.TurnAround();

        // Walk back to the spawn point.
        currentCustomer.MoveTo(
            spawnPoint.position
        );

        while (currentCustomer.IsMoving())
        {
            yield return null;
        }

        Destroy(
            currentCustomer.gameObject
        );

        customerInside = false;

        NightManager.Instance.ConsumeCustomer();

        currentState =
            ClinicState.Transition;

        yield return new WaitForSeconds(
            nextCustomerDelay
        );

        if (NightManager.Instance.HasCustomersRemaining())
        {
            SpawnCustomer();
        }
        else
        {
            currentState =
                ClinicState.WaitingForCustomer;
        }

        busy = false;
    }

    // =========================================================
    // ASSIGN CONDITION
    // =========================================================

    void AssignCondition(
        CustomerController customer
    )
    {
        CustomerData data =
            customer.GetComponent<CustomerData>();

        if (data == null)
        {
            Debug.LogWarning(
                "Customer has no CustomerData component."
            );

            return;
        }

        ConditionData selectedCondition =
            NightManager.Instance.GenerateCondition();

        if (selectedCondition == null)
        {
            Debug.LogWarning(
                "No condition could be assigned to customer."
            );

            return;
        }

        data.condition =
            selectedCondition.condition;

        data.symptoms =
            new List<Symptoms>(
                selectedCondition.symptoms
            );

        data.acceptedCureItemTypes =
            new List<string>(
                selectedCondition.acceptedCureItemTypes
            );

        data.dialogLines =
            new List<string>(
                selectedCondition.dialogLines
            );

        Debug.Log(
            "===== CUSTOMER ASSIGNED ====="
        );

        Debug.Log(
            "Condition: " +
            data.condition
        );

        Debug.Log(
            "Accepted cure item types: " +
            string.Join(
                ", ",
                data.acceptedCureItemTypes
            )
        );

        Debug.Log("Symptoms:");

        foreach (Symptoms symptom in data.symptoms)
        {
            Debug.Log(
                "- " + symptom
            );
        }

        Debug.Log(
            "============================="
        );
    }
}