using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CraftingGrid : MonoBehaviour {
	
	public GUISkin skin;
	public AnchorPoint anchor;
	public Vector2 anchorOffset;
	public float height;
	public float width;
	public int buttonSize = 32;
	public int rowCount = 4;
	public int columnCount = 4;
	Rect uiRect;
	
	List<CraftingStation> stationScripts = new List<CraftingStation>();
	
	bool open = false;

	// Use this for initialization
	void Start () {
		height = (rowCount+1) * buttonSize + 100;
		width = columnCount * buttonSize + 24;
		
		if (anchor == AnchorPoint.TopLeft) {
			uiRect = new Rect(anchorOffset.x, anchorOffset.y, width, height);
		} else if (anchor == AnchorPoint.TopRight) {
			uiRect = new Rect(Screen.width - width - anchorOffset.x, anchorOffset.y, width, height);
		} else if (anchor == AnchorPoint.BottomLeft) {
			uiRect = new Rect(anchorOffset.x, Screen.height - height - anchorOffset.y, width, height);
		} else if (anchor == AnchorPoint.BottomRight) {
			uiRect = new Rect(Screen.width - width - anchorOffset.x, Screen.height - height - anchorOffset.y, width, height);
		}
		
		EventSystem.RegisterEvent("CRAFTING_GRID_UPDATE", this);
	}
	
	// Update is called once per frame
	void Update () {
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
			
			ToggleOpen();
		}
	}
	
	void OnGUI() {
		if (!open)
			return;
		GUI.skin = skin;
		
		GUI.Box(uiRect, "");
		GUILayout.BeginArea(new Rect(uiRect));
		GUILayout.BeginHorizontal();
		GUILayout.Label("Crafting");
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("X")) {
			ToggleOpen();
		}
		GUILayout.EndHorizontal();
		GUILayout.Label("Recipe:");
		AtavismInventoryItem recipeItem = ClientAPI.ScriptObject.GetComponent<Crafting>().RecipeItem;
		if (recipeItem != null) {
		}
		
		List<CraftingComponent> gridItems = ClientAPI.ScriptObject.GetComponent<Crafting>().GridItems;
		for (int i = 0; i < rowCount; i++) {
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			for (int j = 0; j < columnCount; j++) {
				int itemPos = i * columnCount + j;
				if (gridItems[itemPos].item != null) {
					if (GUILayout.Button(gridItems[itemPos].item.icon, GUILayout.Width(buttonSize), GUILayout.Height(buttonSize))) {
						gameObject.GetComponent<Cursor>().PickupBagItem(0, 0, gridItems[itemPos].item);
						ClientAPI.ScriptObject.GetComponent<Crafting>().SetGridItem(itemPos, null);
					}
					Vector3 mousePosition = Input.mousePosition;
					mousePosition.y = Screen.height - mousePosition.y;
					/*if (buttonRect.Contains(mousePosition)) {
						bagData.items[i].DrawTooltip(mousePosition.x, mousePosition.y);
					}*/
				} else {
					if (GUILayout.Button("", GUILayout.Width(buttonSize), GUILayout.Height(buttonSize))) {
						if (gameObject.GetComponent<Cursor>().CursorHasItem()) {
							AtavismInventoryItem item = gameObject.GetComponent<Cursor>().GetCursorItem();
							ClientAPI.ScriptObject.GetComponent<Crafting>().SetGridItem(itemPos, item);
							gameObject.GetComponent<Cursor>().ResetCursor();
						}
					}
				}
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
		}
		
		GUILayout.Label("Creates:");
		AtavismInventoryItem resultItem = ClientAPI.ScriptObject.GetComponent<Crafting>().ResultItem;
		if (resultItem != null) {
			GUILayout.Button(resultItem.icon, GUILayout.Width(buttonSize), GUILayout.Height(buttonSize));
			if (GUILayout.Button("Craft")) {
				ClientAPI.ScriptObject.GetComponent<Crafting>().CraftItem();
			}
		}
		
		GUILayout.EndArea();
	}
	
	public void OnEvent(EventData eData) {
		if (eData.eventType == "CRAFTING_GRID_UPDATE") {
			// Update 
		}
	}
	
	public void ToggleOpen() {
		open = !open;
		if (open) {
			UiSystem.AddFrame("CraftingGrid", uiRect);
		} else {
			UiSystem.RemoveFrame("CraftingGrid", new Rect(0, 0, 0, 0));
		}
	}
}
