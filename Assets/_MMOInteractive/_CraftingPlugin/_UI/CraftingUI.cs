using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CraftingUI : MonoBehaviour
{
    public static CraftingUI Instance;
    private bool display = false;

    public Recipe activeRecipe = null;
    public List<Recipe> recipes = new List<Recipe>();
    Rect craftingRect;
    public string CraftingName;
    private bool craftingProgress = false;
    public float craftProgress;

    Vector2 scroll = new Vector2(0, 0);

    List<CraftingStation> stationScripts = new List<CraftingStation>();

    void Awake()
    {
        Instance = this;
    }

    // Use this for initialization
    void Start()
    {
        craftingRect = new Rect(20, 100, 200, 300);

        recipes.Add(new Recipe("Long Sword"));
        recipes.Add(new Recipe("Iron Ingot"));
        recipes.Add(new Recipe("Bronze"));
    }

    // Update is called once per frame
    void Update()
    {
		Vector3 playerPos = ClientAPI.GetPlayerObject().Position;
        if (Input.GetKeyDown(KeyCode.C))
        {
            stationScripts.Clear();
            GameObject[] stations = GameObject.FindGameObjectsWithTag("Crafting");

            foreach (GameObject obj in stations)
            {
				if (Vector3.Distance(playerPos, obj.transform.position) < 10 && !stationScripts.Contains(obj.GetComponent<CraftingStation>()))
                {
                    stationScripts.Add(obj.GetComponent<CraftingStation>());
                }
            }

            ToggleUI();
        }

        if (craftingProgress)
        {
            craftProgress += Time.deltaTime / 2;
        }

        if (display)
        {
            stationScripts.Clear();
            GameObject[] stations = GameObject.FindGameObjectsWithTag("Crafting");

            foreach (GameObject obj in stations)
            {
				if (Vector3.Distance(playerPos, obj.transform.position) < 10 && !stationScripts.Contains(obj.GetComponent<CraftingStation>()))
                {
                    stationScripts.Add(obj.GetComponent<CraftingStation>());
                }
            }
        }
    }

    void OnGUI()
    {
        if (display)
        {
            GUI.Window(0, craftingRect, CraftingWindow, CraftingName);
        }
    }

    void CraftingWindow(int index)
    {
        GUILayout.BeginArea(new Rect(5, 20, 190, 260));

        if (stationScripts.Count > 0)
        {
            scroll = GUILayout.BeginScrollView(scroll);
            foreach (CraftingStation station in stationScripts)
            {
                station.DisplaySection();
            }

            if (!craftingProgress)
            {
                if (GUILayout.Button("Craft Item"))
                {
                    if (activeRecipe != null)
                    {
                        LinkedList<object> componentIds = new LinkedList<object>();
                        componentIds.AddLast(13);

                        LinkedList<object> componentCount = new LinkedList<object>();
                        componentCount.AddLast(1);
                        CraftingPlugin.Instance.CraftItem(activeRecipe.name, 1, componentIds, componentCount);
                    }
                }
            }
            else
            {
                GUILayout.Label("Crafting Item...");
                float progress = craftProgress * 170;
                GUILayout.Box(progressForground, GUILayout.Width(progress));
                if (progress >= 170)
                {
                    craftingProgress = false;
                    craftProgress = 0;
                    Debug.Log("Crafting is Finished");
                }
            }
            GUILayout.EndScrollView();
        }
        else
        {
            GUILayout.Label("There are no Crafting Stations around!");
        }

        GUILayout.EndArea();
    }

    public void StartProgressBar()
    {
        craftingProgress = true;
    }

    public void ToggleUI()
    {
        display = !display;

        if (display)
            UiSystem.AddFrame("Crafting Window", craftingRect);
        else
            UiSystem.RemoveFrame("Crafting Window", craftingRect);
    }

    public Texture progressBackground;
    public Texture progressForground;

    void DrawProgress(Vector2 location, Vector2 size, float progress){
        GUI.DrawTexture(new Rect(location.x, location.y, size.x, size.y), progressBackground);
        GUI.DrawTexture(new Rect(location.x, location.y, size.x * progress, size.y), progressForground);
    }
}
