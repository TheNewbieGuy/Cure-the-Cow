using System.Collections.Generic;
using UnityEngine;

public class CustomerData : MonoBehaviour
{
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
}