using System.Collections.Generic;
using UnityEngine;

public class CustomerData : MonoBehaviour
{
    [System.Serializable]
    public class CustomerSymptomImageOverride
    {
        public Symptoms symptom;

        [Tooltip("Shown when this specific customer HAS this symptom.")]
        public Sprite presentImage;

        [Tooltip("Shown when this specific customer does NOT have this symptom.")]
        public Sprite absentImage;
    }

    [Header("Condition")]
    public Condition condition;

    [Header("Generated Symptoms")]
    public List<Symptoms> symptoms =
        new List<Symptoms>();

    [Header("Cure")]
    public List<string> acceptedCureItemTypes = new List<string>();

    [Header("Intro Dialog")]
    public List<string> dialogLines =
        new List<string>();

    [Header("Custom Symptom Images (Optional)")]
    [Tooltip("Define unique present/absent images for this customer. If left empty, it falls back to the global SymptomImageDatabase.")]
    public List<CustomerSymptomImageOverride> customSymptomImages =
        new List<CustomerSymptomImageOverride>();

    /// <label>Gets custom image if defined, otherwise returns null for fallback.</label>
    public Sprite GetSymptomImage(Symptoms symptom, bool isPresent)
    {
        foreach (var entry in customSymptomImages)
        {
            if (entry.symptom == symptom)
            {
                return isPresent ? entry.presentImage : entry.absentImage;
            }
        }
        return null;
    }
}

