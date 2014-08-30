using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CraftingComponent {
	public AtavismInventoryItem item = null;
    public int count = 1;
}

public class Crafting : MonoBehaviour {

	public int gridSize = 4;
	
	List<CraftingComponent> gridItems = new List<CraftingComponent>();
	AtavismInventoryItem dye = null;
	AtavismInventoryItem essence = null;
	int recipeID = -1;
	string recipeName = "";
	int recipeItemID = -1;
	int resultItemID = -1;
	AtavismInventoryItem recipeItem = null;
	AtavismInventoryItem resultItem = null;
	
	void Start() {
		int gridCount = gridSize * gridSize;
		for (int i = 0; i < gridCount; i++) {
			gridItems.Add(new CraftingComponent());
		}
	
		// Listen for messages from the server
		NetworkAPI.RegisterExtensionMessageHandler("CraftingGridMsg", HandleCraftingGridResponse);
		NetworkAPI.RegisterExtensionMessageHandler("CraftingMsg", HandleCraftingMessage);
	}
	
	public void SetGridItem(int gridPos, AtavismInventoryItem item) {
		if (item == null) {
			gridItems[gridPos].item.AlterUseCount(-gridItems[gridPos].count);
			gridItems[gridPos].item = null;
			gridItems[gridPos].count = 1;
		} else if (gridItems[gridPos].item == item) {
			gridItems[gridPos].count++;
		} else {
			gridItems[gridPos].item = item;
			gridItems[gridPos].count = 1;
		}
		
		item.AlterUseCount(1);
		
		// Send message to server to work out if we have a valid recipe
		Dictionary<string, object> props = new Dictionary<string, object> ();
		LinkedList<object> itemIds = new LinkedList<object>();
		LinkedList<object> itemCounts = new LinkedList<object>();
		for (int i = 0; i < gridItems.Count; i++) {
			if (gridItems[i].item != null) {
				itemIds.AddLast(gridItems[i].item.templateId);
			} else {
				itemIds.AddLast(-1);
			}
			itemCounts.AddLast(gridItems[i].count);
		}
		props.Add ("componentIDs", itemIds);
		props.Add ("componentCounts", itemCounts);
		NetworkAPI.SendExtensionMessage (ClientAPI.GetPlayerOid(), false, "crafting.GRID_UPDATED", props);
	}
	
	public void SetRecipeItem(AtavismInventoryItem item) {
		recipeItem = item;
	}
	
	public void SetDye(AtavismInventoryItem item) {
		dye = item;
	}
	
	public void SetEssence(AtavismInventoryItem item) {
		essence = item;
	}
	
	public void CraftItem() {
		Dictionary<string, object> props = new Dictionary<string, object>();
		//properties["CraftType"] = craftType;
		LinkedList<object> items = new LinkedList<object>();
		LinkedList<object> itemCounts = new LinkedList<object>();
		for (int i = 0; i < gridItems.Count; i++) {
			if (gridItems[i].item != null) {
				items.AddLast(gridItems[i].item.ItemId.ToLong());
				itemCounts.AddLast(gridItems[i].count);
			}
		}
		props.Add ("components", items);
		props.Add ("componentCounts", itemCounts);
		props.Add ("RecipeId", recipeID);
		NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "crafting.CRAFT_ITEM", props);
	}
	
	public void HandleCraftingGridResponse(Dictionary<string, object> props) {
		recipeID = (int)props["recipeID"];
		recipeName = (string)props["recipeName"];
		recipeItemID = (int)props["recipeItem"];
		resultItemID = (int)props["resultItem"];
		
		if (resultItemID != -1) {
			resultItem = GetComponent<Inventory>().GetItemByTemplateID(resultItemID);
		} else {
			resultItem = null;
		}
		if (recipeItemID != -1) {
			recipeItem = GetComponent<Inventory>().GetItemByTemplateID(recipeItemID);
		} else {
			recipeItem = null;
		}
		string[] args = new string[1];
    	EventSystem.DispatchEvent("CRAFTING_GRID_UPDATE", args);
	}
	
	void HandleCraftingMessage(Dictionary<string, object> target)
	{
		string msgType = (string)target["PluginMessageType"];
		
		switch (msgType)
		{
		case "CraftingStarted":
		{
			GameObject ui = GameObject.Find("UI");
			ClearGrid();
			ui.GetComponent<CraftingUI>().StartProgressBar();
			break;
		}
		case "CraftingFailed":
		{
			Dictionary<string, object> errors = new Dictionary<string,object>();
			errors.Add("ErrorText", (string)target["ErrorMsg"]);
			GameObject ui = GameObject.Find("UI");
			ui.GetComponent<ErrorMessage>().HandleErrorMessage(errors);
			break;
		}
		}
		
		Debug.Log("Got A Crafting Message!");
	}
	
	public void ClearGrid() {
		int gridCount = gridSize * gridSize;
		for (int i = 0; i < gridCount; i++) {
			if (gridItems[i].item != null) {
				gridItems[i].item.ResetUseCount();
			}
		}
		gridItems.Clear();
		for (int i = 0; i < gridCount; i++) {
			gridItems.Add(new CraftingComponent());
		}
		
		// Also clear special slots
		resultItem = null;
		resultItemID = -1;
		dye = null;
		essence = null;
		recipeItem = null;
		recipeItemID = -1;
		
		string[] args = new string[1];
		EventSystem.DispatchEvent("CRAFTING_GRID_UPDATE", args);
	}
	
	public List<CraftingComponent> GridItems {
		get {
			return gridItems;
		}
	}
	
	public AtavismInventoryItem ResultItem {
		get {
			return resultItem;
		}
	}
	
	public AtavismInventoryItem RecipeItem {
		get {
			return recipeItem;
		}
	}
}
