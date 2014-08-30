using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum CraftingType
{
    Smelting = 0,
    Woodworking,
}

public class CraftingStation : MonoBehaviour {
    public CraftingType type;
    List<Recipe> recipes = new List<Recipe>();
    public string craftSection;
    public string[] recipeNames;

    bool display = false;

    string title;

	// Use this for initialization
	void Start () {
        foreach (string s in recipeNames)
        {
            recipes.Add(new Recipe(s));
        }
	}
	
	// Update is called once per frame
	void Update () {
        if (display)
        {
            title = " - " + craftSection;
        }
        else
        {
            title = " + " + craftSection;
        }
	
	}

    public void DisplayRecipes()
    {
        GUILayout.Label(craftSection);

        if (recipes.Count > 0)
        {
            foreach (Recipe recipe in recipes)
            {
                if (GUILayout.Button(recipe.name))
                {
                    Debug.Log("Setting Active Recipe!");
                    CraftingUI.Instance.activeRecipe = recipe;
                }
            }
        }
        else
        {
            GUILayout.Label("Sorry, There are no recipes to display!");
        }
    }

    public void DisplaySection()
    {
        if (GUILayout.Button(title))
        {
            display = !display;
        }

        if (display)
            DisplayRecipes();
        GUILayout.FlexibleSpace();
    }

    // Press "C" to open the Crafting Window. Crafting options update automatically
    // As you get with in range of a Station.

    void OnMouseDown()
    {
        //CraftingUI.Instance.CraftingName = "Forge Crafting";
        //CraftingUI.Instance.recipes = recipes;
        CraftingUI.Instance.ToggleUI();
    }
}
