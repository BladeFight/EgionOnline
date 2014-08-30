using UnityEngine;
using UnityEditor;
using System.Collections;

public class AbilityPrefab {

	// Prefab Parameters
	public int id = -1;
	public string name = "";
	public Texture2D icon = null;
	public string tooltip = "";
	public int cost = 0;
	public string costProperty = "";

	// Prefab file information
	private string prefabName;
	private string prefabPath;
	// Common Prefab Prefix and Sufix
	private string itemPrefix = "Ability";
	private string itemSufix = ".prefab";
	// Base path
	private string basePath = "Assets/Resources/Content/Abilities/";
	// Example Item Prefab Information
	private string basePrefab = "Example Ability Prefab.prefab";
	private string basePrefabPath;

	public AbilityPrefab(int id, string itemName) {
		this.id = id;
		name = itemName;
		prefabName = itemPrefix+itemName+itemSufix;
		prefabPath = basePath+prefabName;
		basePrefabPath = basePath+basePrefab;
	}

	public void Save(string iconNew, string tooltipNew, int costNew, string costPropertyNew)
	{
		icon = (Texture2D) AssetDatabase.LoadAssetAtPath(iconNew, typeof(Texture2D));
		tooltip = tooltipNew;
		cost = costNew;
		costProperty = costPropertyNew;
		
		this.Save ();
	}

	public void Save(Texture2D iconNew, string tooltipNew, int costNew, string costPropertyNew)
	{
		if (icon != null)
			icon = iconNew;
		tooltip = tooltipNew;
		cost = costNew;
		costProperty = costPropertyNew;

		this.Save ();
	}

	// Save data from the class to the new prefab, creating one if it doesnt exist
	public void Save() {
		GameObject item = (GameObject) AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject));

		// If this is a new prefab
		if (item == null) {
			AssetDatabase.CopyAsset(basePrefabPath, prefabPath);
			AssetDatabase.Refresh();
			item = (GameObject) AssetDatabase.LoadAssetAtPath(prefabPath,  typeof(GameObject));
		}

		item.GetComponent<Ability>().id = id;
		item.GetComponent<Ability>().name = name;
		if (icon != null)
			item.GetComponent<Ability>().icon = icon;
		item.GetComponent<Ability>().tooltip = tooltip;
		item.GetComponent<Ability>().cost = cost;
		item.GetComponent<Ability>().costProperty = costProperty;
		
		EditorUtility.SetDirty(item);

		AssetDatabase.Refresh();
	}

	public void Delete() {
		GameObject item = (GameObject) AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject));
		
		// If this is a new prefab
		if (item != null) {
			AssetDatabase.DeleteAsset(prefabPath);
			AssetDatabase.Refresh();
		}
	}

	// Load data from the prefab base on its name
	// return true if the prefab exist and false if there is no prefab
	public bool Load() {

		GameObject item = (GameObject) AssetDatabase.LoadAssetAtPath(prefabPath,  typeof(GameObject));
		
		// If this is a new prefab
		if (item == null) 
			return false;

		id = item.GetComponent<Ability>().id;
		name = item.GetComponent<Ability>().name;
		icon = item.GetComponent<Ability>().icon;
		tooltip = item.GetComponent<Ability>().tooltip;
		cost = item.GetComponent<Ability>().cost;
		costProperty = item.GetComponent<Ability>().costProperty;

		return true;
	}

}
