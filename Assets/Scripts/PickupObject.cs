using UnityEngine;

public class PickupObject : MonoBehaviour
{
    [Header("Item Type")]
    public string itemType;
    [Tooltip("Whether this item can be handed to a customer as a cure attempt. Leave false for decorations, tools, or anything that should just be a normal placeable object.")]
    public bool isGivable = false;
    
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
}