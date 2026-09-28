using System.Collections.Generic;
using UnityEngine;

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

    void Awake()
    {
        Instance = this;
    }

    public bool HasOutput()
    {
        if (outputPlacementSpot != null)
        {
            return outputPlacementSpot.currentObject != null;
        }
        return false;
    }

    public void Craft()
    {
        // Block crafting if an output is already sitting on the output spot
        if (HasOutput())
        {
            Debug.Log("Clear the output item before crafting again!");
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

        if (matchedRecipe == null)
        {
            Debug.Log("Invalid recipe.");
            return;
        }

        Debug.Log(
            "Crafted: " +
            matchedRecipe.outputPrefab.name
        );
        SFXManager.Instance.PlaySFX("Craft");

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