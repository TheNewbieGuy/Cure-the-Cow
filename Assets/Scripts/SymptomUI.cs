using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SymptomUI : MonoBehaviour
{
    public static SymptomUI Instance;

    [System.Serializable]
    public class SymptomToggle
    {
        public Symptoms symptom;

        public Toggle toggle;
    }

    [Header("Symptom Toggles")]
    public List<SymptomToggle> symptomToggles =
        new List<SymptomToggle>();

    void Awake()
    {
        Instance = this;
    }

    public List<Symptoms> GetSelectedSymptoms()
    {
        List<Symptoms> selected =
            new List<Symptoms>();

        foreach (SymptomToggle entry in symptomToggles)
        {
            if (entry.toggle.isOn)
            {
                selected.Add(entry.symptom);
            }
        }

        return selected;
    }

    public void ResetToggles()
    {
        foreach (SymptomToggle entry in symptomToggles)
        {
            entry.toggle.isOn = false;
        }
    }
}