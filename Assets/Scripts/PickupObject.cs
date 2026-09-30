using UnityEngine;
using TMPro; // Uses standard TextMeshPro for 3D world objects

public class PickupObject : MonoBehaviour
{
    [Header("Item Type")]
    public string itemType;
    [Tooltip("Whether this item can be handed to a customer as a cure attempt. Leave false for decorations, tools, or anything that should just be a normal placeable object.")]
    public bool isGivable = false;

    [Header("Thermometer Settings (Optional)")]
    [Tooltip("3D TextMeshPro component attached to this object to display temperature.")]
    public TextMeshPro temperatureDisplayText;
    
    [HideInInspector]
    public PlacementSpot currentSpot;

    [HideInInspector]
    public Vector3 originalPosition;
    [HideInInspector]
    public Quaternion originalRotation;

    void Start()
    {
        // Save the starting position and rotation
        originalPosition = transform.position;
        originalRotation = transform.rotation;

        // Hide temperature text by default when spawned
        if (temperatureDisplayText != null)
        {
            temperatureDisplayText.text = "";
        }
    }

    public void ReturnToOrigin()
    {
        transform.position = originalPosition;
        transform.rotation = originalRotation;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    public void UpdateTemperatureDisplay(bool hasFever)
    {
        if (temperatureDisplayText == null) return;

        int finalTemp;

        if (hasFever)
        {
            float rawTemp = Random.Range(39f, 41f);
            finalTemp = Mathf.RoundToInt(rawTemp);
            
            // Safety check just in case: ensure fever is at least 38
            if (finalTemp < 38) finalTemp = 38;
        }
        else
        {
            float rawTemp = Random.Range(37f, 39f);
            finalTemp = Mathf.RoundToInt(rawTemp);

            // Safety check: ensure normal temperatures never round up into the fever range (38+)
            if (finalTemp >= 38)
            {
                finalTemp = 37; // Forces it to round down to a safe normal temperature
            }
        }

        temperatureDisplayText.text = $"{finalTemp}°";
    }
}