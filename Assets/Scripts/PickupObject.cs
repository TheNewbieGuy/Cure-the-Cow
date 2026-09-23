using UnityEngine;

public class PickupObject : MonoBehaviour
{
    [Header("Item Type")]
    public string itemType;
    [Tooltip("Whether this item can be handed to a customer as a cure attempt. Leave false for decorations, tools, or anything that should just be a normal placeable object.")]
    public bool isGivable = false;
    [HideInInspector]
    public PlacementSpot currentSpot;
}