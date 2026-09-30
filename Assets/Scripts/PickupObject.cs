using System.Collections;
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

    private Rigidbody rb;
    private Coroutine settleCoroutine;

    void Start()
    {
        // Save the starting position and rotation
        originalPosition = transform.position;
        originalRotation = transform.rotation;

        rb = GetComponent<Rigidbody>();

        // Hide temperature text by default when spawned
        if (temperatureDisplayText != null)
        {
            temperatureDisplayText.text = "";
        }
    }

    public void TriggerKinematicDelay(float delay)
    {
        if (rb == null) return;

        if (settleCoroutine != null)
        {
            StopCoroutine(settleCoroutine);
        }

        settleCoroutine = StartCoroutine(SettleAfterDelay(delay));
    }

    IEnumerator SettleAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        settleCoroutine = null;
    }

    public void ReturnToOrigin()
    {
        transform.position = originalPosition;
        transform.rotation = originalRotation;

        if (settleCoroutine != null)
        {
            StopCoroutine(settleCoroutine);
            settleCoroutine = null;
        }

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
            
            if (finalTemp < 38) finalTemp = 38;
        }
        else
        {
            float rawTemp = Random.Range(37f, 39f);
            finalTemp = Mathf.RoundToInt(rawTemp);

            if (finalTemp >= 38)
            {
                finalTemp = 37;
            }
        }

        temperatureDisplayText.text = $"{finalTemp}°";
    }
}