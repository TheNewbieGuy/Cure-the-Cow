using System.Collections.Generic;
using UnityEngine;

public class PlacementSpot : MonoBehaviour
{
    [Header("Allowed Item Types")]
    public List<string> allowedItemTypes = new List<string>();

    [Header("Spot Type Options")]
    [Tooltip("If true, placing an item here will instantly destroy (delete) it like a trash can.")]
    public bool isTrashCan = false;

    [Tooltip("If true, this spot is part of the crafting grid and cannot accept items if an output is still waiting.")]
    public bool isCraftingGridSpot = false;

    [Header("Drop Settings")]
    [Tooltip("If true, the item will spawn above this spot and fall down using physics.")]
    public bool spawnAboveAndFall = false;
    [Tooltip("How high above the spot the item will spawn when 'Spawn Above And Fall' is enabled.")]
    public float fallSpawnHeight = 1.5f;

    [HideInInspector]
    public PickupObject currentObject;

    public bool CanPlace(PickupObject obj)
    {
        // Trash cans can always accept allowed items (which immediately get destroyed)
        if (isTrashCan)
        {
            return currentObject == null && allowedItemTypes.Contains(obj.itemType);
        }

        // If this is a crafting grid spot, block placement if an output item is still present
        if (isCraftingGridSpot && CraftingManager.Instance != null && CraftingManager.Instance.HasOutput())
        {
            return false;
        }

        return currentObject == null &&
               allowedItemTypes.Contains(obj.itemType);
    }
}