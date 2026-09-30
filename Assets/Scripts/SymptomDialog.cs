using System.Collections.Generic;
using UnityEngine;

public class SymptomDialogDatabase : MonoBehaviour
{
    public static SymptomDialogDatabase Instance;

    [System.Serializable]
    public class SymptomDialogEntry
    {
        public Symptoms symptom;

        [Header("Dialogue Variants")]
        [Tooltip("Add multiple sentences/phrases a customer might say when they have this symptom.")]
        public List<string> dialogueVariants = new List<string>();
    }

    [Header("Database")]
    [Tooltip("Define dialogue variants for each symptom.")]
    public List<SymptomDialogEntry> symptomDialogs = new List<SymptomDialogEntry>();

    private Dictionary<Symptoms, List<string>> dialogLookup;

    void Awake()
    {
        Instance = this;
        dialogLookup = new Dictionary<Symptoms, List<string>>();

        foreach (var entry in symptomDialogs)
        {
            if (!dialogLookup.ContainsKey(entry.symptom))
            {
                dialogLookup.Add(entry.symptom, entry.dialogueVariants);
            }
        }
    }

    /// <summary>
    /// Returns a random dialogue line for the given symptom, or null if none exist.
    /// </summary>
    public string GetRandomDialogForSymptom(Symptoms symptom)
    {
        if (dialogLookup != null && dialogLookup.TryGetValue(symptom, out List<string> variants))
        {
            if (variants != null && variants.Count > 0)
            {
                int randomIndex = Random.Range(0, variants.Count);
                return variants[randomIndex];
            }
        }
        return null;
    }
}