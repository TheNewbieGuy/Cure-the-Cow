using System.Collections.Generic;
using UnityEngine;

public class PlacementSpot : MonoBehaviour
{
    [Header("Allowed Item Types")]
    public List<string> allowedItemTypes = new List<string>();

    [HideInInspector]
    public PickupObject currentObject;

    public bool CanPlace(PickupObject obj)
    {
        return currentObject == null &&
               allowedItemTypes.Contains(obj.itemType);
    }
}