using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ConditionData
{
    [Header("Condition")]
    public Condition condition;

    [Header("Symptoms")]
    public List<Symptoms> symptoms =
        new List<Symptoms>();

    [Header("Cure")]
    [Tooltip("itemType(s) of the PickupObject(s) that count as this condition's cure. Must exactly match what CraftingManager outputs.")]
    public List<string> acceptedCureItemTypes =
        new List<string>();

    [Header("Intro Dialog")]
    [Tooltip("Lines shown one at a time (click Next to advance) when a customer with this condition walks in. Leave empty for no dialog.")]
    public List<string> dialogLines =
        new List<string>();
}