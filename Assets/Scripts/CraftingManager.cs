using System.Collections.Generic;
using UnityEngine;

public class CraftingManager : MonoBehaviour
{
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

    public void Craft()
    {
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

        Instantiate(
            matchedRecipe.outputPrefab,
            outputSpawnPoint.position,
            outputSpawnPoint.rotation
        );

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
                Destroy(
                    spot.currentObject.gameObject
                );

                spot.currentObject = null;
            }
        }
    }
}