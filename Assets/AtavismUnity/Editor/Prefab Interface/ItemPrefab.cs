using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class ItemPrefab {

	// Prefab Parameters
	public int id = -1;
	public string name = "";
	public Texture2D icon = null;
	public string tooltip = "";
	public string itemType = "";
	public string subType = "";
	public string slot = "";
	public int quality = 0;
	public int currencyType = -1;
	public int cost = 0;
	public bool sellable = true;
	public List<ItemEffectEntry> effects;

	// Prefab file information
	private string prefabName;
	private string prefabPath;
	// Common Prefab Prefix and Sufix
	private string itemPrefix = "Item";
	private string itemSufix = ".prefab";
	// Base path
	private string basePath = "Assets/Resources/Content/Items/";
	// Example Item Prefab Information
	private string basePrefab = "Example Item Prefab.prefab";
	private string basePrefabPath;

	public ItemPrefab(int id, string itemName) {
		this.id = id;
		name = itemName;
		prefabName = itemPrefix+itemName+itemSufix;
		prefabPath = basePath+prefabName;
		basePrefabPath = basePath+basePrefab;
	}

	public void Save(string iconNew, string tooltipNew, string itemTypeNew, string subTypeNew, string slotNew, int qualityNew,
	                 int currencyTypeNew, int costNew, bool sellableNew, List<ItemEffectEntry> effectsNew)
	{
		icon = (Texture2D) AssetDatabase.LoadAssetAtPath(iconNew, typeof(Texture2D));
		tooltip = tooltipNew;
		itemType = itemTypeNew;
		subType = subTypeNew;
		slot = slotNew;
		quality = qualityNew;
		currencyType = currencyTypeNew;
		cost = costNew;
		sellable = sellableNew;
		effects = effectsNew;
		this.Save ();
	}

	public void Save(Texture2D iconNew, string tooltipNew, string itemTypeNew, string subTypeNew, string slotNew, int qualityNew,
	                 int currencyTypeNew, int costNew, bool sellableNew, List<ItemEffectEntry> effectsNew)
	{
		if (icon != null)
			icon = iconNew;
		tooltip = tooltipNew;
		itemType = itemTypeNew;
		subType = subTypeNew;
		slot = slotNew;
		quality = qualityNew;
		currencyType = currencyTypeNew;
		cost = costNew;
		sellable = sellableNew;
		effects = effectsNew;

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

		item.GetComponent<AtavismInventoryItem>().templateId = id;
		item.GetComponent<AtavismInventoryItem>().name = name;
		if (icon != null)
			item.GetComponent<AtavismInventoryItem>().icon = icon;
		item.GetComponent<AtavismInventoryItem>().tooltip = tooltip;
		item.GetComponent<AtavismInventoryItem>().itemType = itemType;
		item.GetComponent<AtavismInventoryItem>().subType = subType;
		item.GetComponent<AtavismInventoryItem>().slot = slot;
		item.GetComponent<AtavismInventoryItem>().quality = quality;
		item.GetComponent<AtavismInventoryItem>().currencyType = currencyType;
		item.GetComponent<AtavismInventoryItem>().cost = cost;
		item.GetComponent<AtavismInventoryItem>().sellable = sellable;
		item.GetComponent<AtavismInventoryItem>().ClearEffects();
		foreach (ItemEffectEntry effect in effects) {
			item.GetComponent<AtavismInventoryItem>().itemEffectTypes.Add(effect.itemEffectType);
			item.GetComponent<AtavismInventoryItem>().itemEffectNames.Add(effect.itemEffectName);
			item.GetComponent<AtavismInventoryItem>().itemEffectValues.Add(effect.itemEffectValue);
		}
		
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

		id = item.GetComponent<AtavismInventoryItem>().templateId;
		name = item.GetComponent<AtavismInventoryItem>().name;
		icon = item.GetComponent<AtavismInventoryItem>().icon;
		tooltip = item.GetComponent<AtavismInventoryItem>().tooltip;
		itemType = item.GetComponent<AtavismInventoryItem>().itemType;
		subType = item.GetComponent<AtavismInventoryItem>().subType;
		slot = item.GetComponent<AtavismInventoryItem>().slot;
		quality = item.GetComponent<AtavismInventoryItem>().quality;
		currencyType = item.GetComponent<AtavismInventoryItem>().currencyType;
		cost = item.GetComponent<AtavismInventoryItem>().cost;
		sellable = item.GetComponent<AtavismInventoryItem>().sellable;

		return true;
	}

}
