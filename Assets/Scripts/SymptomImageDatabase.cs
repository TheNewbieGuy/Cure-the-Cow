using System.Collections.Generic;
using UnityEngine;

// Centralized present/absent image lookup for symptoms. Set this up ONCE
// with one entry per Symptoms value, and every ZoomTarget just says which
// symptom it's checking - no per-zoom-point image assignment needed.
public class SymptomImageDatabase : MonoBehaviour
{
    public static SymptomImageDatabase Instance;

    [System.Serializable]
    public class SymptomImageEntry
    {
        public Symptoms symptom;

        [Tooltip("Shown when the customer HAS this symptom.")]
        public Sprite presentImage;

        [Tooltip("Shown when the customer does NOT have this symptom.")]
        public Sprite absentImage;
    }

    [Header("Symptom Images")]
    [Tooltip("Add one entry per Symptoms value.")]
    public List<SymptomImageEntry> symptomImages =
        new List<SymptomImageEntry>();

    private Dictionary<Symptoms, SymptomImageEntry> lookup;

    void Awake()
    {
        Instance = this;

        lookup = new Dictionary<Symptoms, SymptomImageEntry>();

        foreach (SymptomImageEntry entry in symptomImages)
        {
            if (!lookup.ContainsKey(entry.symptom))
            {
                lookup.Add(entry.symptom, entry);
            }
        }
    }

    /// <summary>
    /// Returns the sprite to show for the given symptom, based on whether
    /// it's present on the current customer. Returns null (and logs a
    /// warning) if this symptom has no entry set up.
    /// </summary>
    public Sprite GetImage(Symptoms symptom, bool isPresent)
    {
        if (lookup == null ||
            !lookup.TryGetValue(symptom, out SymptomImageEntry entry))
        {
            Debug.LogWarning(
                $"SymptomImageDatabase has no entry for {symptom}."
            );

            return null;
        }

        return isPresent ? entry.presentImage : entry.absentImage;
    }
}
