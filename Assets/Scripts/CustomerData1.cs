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

        [Header("Panel Toggle")]
        [Tooltip("Check this to display this symptom on the alternate panel instead of the main panel.")]
        public bool useAlternatePanel;
    }

    [Header("Condition")]
    public Condition condition;

    [Header("Generated Symptoms")]
    public List<Symptoms> symptoms =
        new List<Symptoms>();

    [Header("Cure")]
    public List<string> acceptedCureItemTypes = new List<string>();

    [Header("Custom Symptom Images (Optional)")]
    [Tooltip("Define unique present/absent images for this customer. If left empty, it falls back to the global SymptomImageDatabase.")]
    public List<CustomerSymptomImageOverride> customSymptomImages =
        new List<CustomerSymptomImageOverride>();

    /// <summary>
    /// Gets custom image if defined, and outputs whether it should use the alternate panel.
    /// </summary>
    public Sprite GetSymptomImage(Symptoms symptom, bool isPresent, out bool useAlternatePanel)
    {
        useAlternatePanel = false;

        foreach (var entry in customSymptomImages)
        {
            if (entry.symptom == symptom)
            {
                useAlternatePanel = entry.useAlternatePanel;
                return isPresent ? entry.presentImage : entry.absentImage;
            }
        }
        return null;
    }
}