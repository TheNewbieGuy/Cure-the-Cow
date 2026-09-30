using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; // Required for Keyboard input

public class CraftingManager : MonoBehaviour
{
    public static CraftingManager Instance;

    [System.Serializable]
    public class Recipe
    {
        [Header("Required Ingredients")]
        public List<string> ingredients =
            new List<string>();

        [Header("Result")]
        public PickupObject outputPrefab;
    }

    [Header("Crafting Grid")]
    public List<PlacementSpot> craftingSpots =
        new List<PlacementSpot>();

    [Header("Recipes")]
    public List<Recipe> recipes =
        new List<Recipe>();

    [Header("Spawn Result")]
    public Transform outputSpawnPoint;
    
    [Header("Output Placement Spot")]
    [Tooltip("Assign the PlacementSpot component attached to the output spawn area so the game knows when the output has been cleared.")]
    public PlacementSpot outputPlacementSpot;

    [Header("Failure / Junk Output")]
    [Tooltip("Prefab spawned when ingredients do not match any valid recipe (wrong mix, too many, or too few items).")]
    public PickupObject failedRecipePrefab;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        // Press 'C' to trigger crafting
        if (Keyboard.current != null && Keyboard.current.cKey.wasPressedThisFrame)
        {
            // Optional: Block crafting if game is paused or during specific UI states if needed
            Craft();
        }
    }

    // Checks if ANY crafted output or failed recipe item still exists anywhere in the scene
    public bool HasOutput()
    {
        PickupObject[] allPickups = FindObjectsOfType<PickupObject>();
        foreach (var pickup in allPickups)
        {
            // Check if this pickup matches any recipe output prefab OR the failed recipe prefab
            if (failedRecipePrefab != null && pickup.itemType == failedRecipePrefab.itemType)
            {
                return true;
            }

            foreach (var recipe in recipes)
            {
                if (recipe.outputPrefab != null && pickup.itemType == recipe.outputPrefab.itemType)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void Craft()
    {
        // Block crafting if an output remedy already exists in the scene
        if (HasOutput())
        {
            Debug.Log("Clear or destroy the existing remedy before crafting again!");
            return;
        }

        List<string> currentIngredients =
            GetCurrentIngredients();

        if (currentIngredients.Count == 0)
        {
            Debug.Log("No ingredients.");
            return;
        }

        Recipe matchedRecipe =
            FindMatchingRecipe(currentIngredients);

        // If no recipe matches, handle it as a failed/invalid craft
        if (matchedRecipe == null)
        {
            Debug.Log("Invalid recipe. Spawning failed remedy.");
            
            if (SFXManager.Instance != null)
            {
                SFXManager.Instance.PlaySFX("Craft");
            }

            if (failedRecipePrefab != null)
            {
                PickupObject failedItem = Instantiate(
                    failedRecipePrefab,
                    outputSpawnPoint.position,
                    outputSpawnPoint.rotation
                );

                if (outputPlacementSpot != null)
                {
                    outputPlacementSpot.currentObject = failedItem;
                    failedItem.currentSpot = outputPlacementSpot;
                }
            }

            ClearCraftingGrid();
            return;
        }

        Debug.Log(
            "Crafted: " +
            matchedRecipe.outputPrefab.name
        );
        
        if (SFXManager.Instance != null)
        {
            SFXManager.Instance.PlaySFX("Craft");
        }

        PickupObject craftedItem = Instantiate(
            matchedRecipe.outputPrefab,
            outputSpawnPoint.position,
            outputSpawnPoint.rotation
        );

        if (outputPlacementSpot != null)
        {
            outputPlacementSpot.currentObject = craftedItem;
            craftedItem.currentSpot = outputPlacementSpot;
        }

        ClearCraftingGrid();
    }

    List<string> GetCurrentIngredients()
    {
        List<string> ingredients =
            new List<string>();

        foreach (PlacementSpot spot in craftingSpots)
        {
            if (spot.currentObject != null)
            {
                ingredients.Add(
                    spot.currentObject.itemType
                );
            }
        }

        ingredients.Sort();

        return ingredients;
    }

    Recipe FindMatchingRecipe(
        List<string> currentIngredients
    )
    {
        foreach (Recipe recipe in recipes)
        {
            List<string> recipeIngredients =
                new List<string>(
                    recipe.ingredients
                );

            recipeIngredients.Sort();

            if (recipeIngredients.Count !=
                currentIngredients.Count)
                continue;

            bool match = true;

            for (int i = 0;
                 i < recipeIngredients.Count;
                 i++)
            {
                if (recipeIngredients[i] !=
                    currentIngredients[i])
                {
                    match = false;
                    break;
                }
            }

            if (match)
                return recipe;
        }

        return null;
    }

    void ClearCraftingGrid()
    {
        foreach (PlacementSpot spot in craftingSpots)
        {
            if (spot.currentObject != null)
            {
                spot.currentObject.ReturnToOrigin();
                spot.currentObject = null;
            }
        }
    }
}