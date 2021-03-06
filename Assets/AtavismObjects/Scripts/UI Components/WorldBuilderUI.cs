﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WorldBuilderUI : AtavismWindowTemplate {
	
	private Claim activeClaim = null;
	private Claim newClaim = null;
	
	GameObject currentReticle;
	private Vector3 hitPoint;
	private Vector3 hitNormal;
	private RaycastHit hit;
	
	AtavismInventoryItem itemBeingPlaced;
	GameObject objectBeingEdited;
	
	public KeyCode toggleButton;

	// Use this for initialization
	void Start () {
		SetupRect();
	
		// Register for 
		EventSystem.RegisterEvent("PLACE_CLAIM_OBJECT", this);
		EventSystem.RegisterEvent("CLAIM_OBJECT_CLICKED", this);
		
	}
	
	// Update is called once per frame
	void Update () {
		// Get the active claim on each frame
		activeClaim = ClientAPI.ScriptObject.GetComponent<WorldBuilder>().ActiveClaim;
		
		if (GetBuildingState() == WorldBuildingState.SelectItem && Input.GetMouseButtonDown(0)) {
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			RaycastHit hit;
			// Casts the ray and get the first game object hit
			if (Physics.Raycast (ray, out hit, Mathf.Infinity)) {
				ClaimObject cObject = hit.transform.gameObject.GetComponent<ClaimObject>();
				if (cObject != null) {
					int objectID = cObject.ID;
					if (activeClaim.claimObjects.ContainsKey(objectID)) {
						SetBuildingState(WorldBuildingState.EditItem);
						objectBeingEdited = activeClaim.claimObjects[objectID];
					}
				}
			}
		}
		
		if ((GetBuildingState() == WorldBuildingState.PlaceItem || GetBuildingState() == WorldBuildingState.MoveItem) && currentReticle != null) {
			GetHitLocation();
			float delta = Input.GetAxis ("Mouse ScrollWheel");
			if (delta > 0)
				currentReticle.transform.Rotate (new Vector3 (0, -90, 0));
			else if (delta < 0)
				currentReticle.transform.Rotate (new Vector3 (0, 90, 0));
			
			if ((currentReticle != null) && (activeClaim != null) && 
				(ClientAPI.ScriptObject.GetComponent<WorldBuilder>().InsideClaimArea (activeClaim, hitPoint)))
				currentReticle.transform.position = hitPoint;
				
			if (Input.GetMouseButtonDown (0) && !UiSystem.IsMouseOverFrame()) {
				if (GetBuildingState() == WorldBuildingState.PlaceItem) {
					List<int> effectPositions = itemBeingPlaced.GetEffectPositionsOfTypes("ClaimObject");
					Dictionary<string, object> props = new Dictionary<string, object>();
					props.Add("claim", activeClaim.id);
					props.Add("gameObject", itemBeingPlaced.itemEffectValues[effectPositions[0]]);
					props.Add("loc", currentReticle.transform.position);
					props.Add("orient", currentReticle.transform.rotation);
					props.Add("itemID", itemBeingPlaced.templateId);
					props.Add("itemOID", itemBeingPlaced.ItemId);
					NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "voxel.PLACE_CLAIM_OBJECT", props);
					ClientAPI.Write("Send place claim object message");
					ClearCurrentReticle(true);
					itemBeingPlaced = null;
					SetBuildingState(WorldBuildingState.Standard);
					gameObject.GetComponent<Cursor>().ChangeWorldBuilderState(false);
				} else if (GetBuildingState() == WorldBuildingState.MoveItem) {
					SetBuildingState(WorldBuildingState.EditItem);
					ClearCurrentReticle(false);
				}
				
			}
		}
	}
	
	void OnGUI() {
		if (!open)
			return; 
		
		GUI.depth = uiLayer;
		GUI.skin = skin;
		
		if (GetBuildingState() == WorldBuildingState.Standard || GetBuildingState() == WorldBuildingState.PlaceItem || 
		    GetBuildingState() == WorldBuildingState.SelectItem) {
			DrawClaimControlWindow();
		} else if (GetBuildingState() == WorldBuildingState.CreateClaim) {
			DrawCreateClaimWindow();
		} else if (GetBuildingState() == WorldBuildingState.EditItem || GetBuildingState() == WorldBuildingState.MoveItem) {
			DrawEditItemWindow();
		}
	}
	
	void DrawClaimControlWindow() {
		GUI.Box(uiRect, "");
		GUILayout.BeginArea (uiRect, skin.GetStyle ("Window"));
		GUILayout.BeginHorizontal ();
		GUILayout.Label ("World Builder");
		if (GUILayout.Button("x", GUILayout.Width(20))) {
			ToggleOpen();
		}
		GUILayout.EndHorizontal();
		
		// If no active claim, show nothing
		if (activeClaim == null) {
			GUILayout.Label("Active Claim: None");
		} else {
			GUILayout.Label("Active Claim: " + activeClaim.name);
			if (activeClaim.playerOwned) {
				// If the player owns the claim, show a sell button
				/*if (GUILayout.Button("Sell", GUILayout.Width(80))) {
					// Do stuff
				}*/
				// Place Item Button
				if (GUILayout.Button("Place Item", GUILayout.Width(120))) {
					SetBuildingState(WorldBuildingState.PlaceItem);
					// Open the main bag
					gameObject.GetComponent<BagBar>().OpenBag(0);
					// Set WorldBuilder State
					gameObject.GetComponent<Cursor>().ChangeWorldBuilderState(true);
				}
				// Select object
				if (GUILayout.Button("Select Object", GUILayout.Width(120))) {
					SetBuildingState(WorldBuildingState.SelectItem);
					gameObject.GetComponent<Cursor>().ChangeWorldBuilderState(false);
				}
				
			} else if (activeClaim.forSale) {
				// if not player owned, show purchase button if for sale
				GUILayout.Label("Claim is for sale!");
				string currencyName = ClientAPI.ScriptObject.GetComponent<Inventory>().GetCurrencyName(activeClaim.currency);
				GUILayout.Label("Cost: " + activeClaim.cost + " " + currencyName);
				if (GUILayout.Button("Buy")) {
					Dictionary<string, object> props = new Dictionary<string, object> ();
					props.Add("claimID", activeClaim.id);
					NetworkAPI.SendExtensionMessage (ClientAPI.GetPlayerOid (), false, "voxel.PURCHASE_CLAIM", props);
					ClientAPI.Write("Sent buy claim message");
				}
			} else {
				GUILayout.Label("Claim is not for sale.");
			}
		}
		GUILayout.FlexibleSpace();
		// Check if user is an admin
		int adminLevel = (int) ClientAPI.GetObjectProperty(ClientAPI.GetPlayerOid(), "adminLevel");
		if (adminLevel == 5) {
			GUILayout.Label("Claim Admin:");
			if (activeClaim == null) {
				if (GUILayout.Button("Create claim", GUILayout.Width(120))) {
					SetBuildingState(WorldBuildingState.CreateClaim);
					newClaim = new Claim();
				}
			} else {
				if (GUILayout.Button("Delete claim", GUILayout.Width(120))) {
					Dictionary<string, object> props = new Dictionary<string, object> ();
					props.Add("claimID", activeClaim.id);
					NetworkAPI.SendExtensionMessage (ClientAPI.GetPlayerOid (), false, "voxel.DELETE_CLAIM", props);
					gameObject.GetComponent<Cursor>().ChangeWorldBuilderState(false);
				}
			}
		}
		
		GUILayout.EndArea();
	}
	
	void DrawEditItemWindow() {
		GUI.Box(uiRect, "");
		GUILayout.BeginArea (uiRect, skin.GetStyle ("Window"));
		GUILayout.BeginHorizontal ();
		GUILayout.Label ("Edit Claim Object");
		if (GUILayout.Button("x", GUILayout.Width(20))) {
			SetBuildingState(WorldBuildingState.Standard);
		}
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Label("Name: " + objectBeingEdited.name);
		GUILayout.EndHorizontal();
		if (GUILayout.Button("Move")) {
			SetCurrentReticle(objectBeingEdited);
			SetBuildingState(WorldBuildingState.MoveItem);
		}
		if (GUILayout.Button("Convert to Item")) {
			Dictionary<string, object> props = new Dictionary<string, object>();
			props.Add("action", "convert");
			props.Add("claimID", activeClaim.id);
			props.Add("objectID", objectBeingEdited.GetComponent<ClaimObject>().ID);
			NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "voxel.EDIT_CLAIM_OBJECT", props);
			SetBuildingState(WorldBuildingState.Standard);
			ClearCurrentReticle(false);
		}
		if (GUILayout.Button("Save")) {
			Dictionary<string, object> props = new Dictionary<string, object>();
			props.Add("action", "save");
			props.Add("claimID", activeClaim.id);
			props.Add("objectID", objectBeingEdited.GetComponent<ClaimObject>().ID);
			props.Add("loc", objectBeingEdited.transform.position);
			props.Add("orient", objectBeingEdited.transform.rotation);
			NetworkAPI.SendExtensionMessage(ClientAPI.GetPlayerOid(), false, "voxel.EDIT_CLAIM_OBJECT", props);
			SetBuildingState(WorldBuildingState.Standard);
			ClearCurrentReticle(false);
		}
		
		GUILayout.EndArea();
	}
	
	void DrawCreateClaimWindow() {
		GUI.Box(uiRect, "");
		GUILayout.BeginArea (uiRect, skin.GetStyle ("Window"));
		GUILayout.BeginHorizontal ();
		GUILayout.Label ("Create Claim");
		if (GUILayout.Button("x", GUILayout.Width(20))) {
			ToggleOpen();
		}
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Label("Name:");
		newClaim.name = GUILayout.TextField(newClaim.name, GUILayout.Width(100));
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Label("Size:");
		newClaim.area = int.Parse(GUILayout.TextField("" + newClaim.area));
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		newClaim.playerOwned = GUILayout.Toggle(newClaim.playerOwned, "Owned");
		newClaim.forSale = GUILayout.Toggle(newClaim.forSale, "For Sale");
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Label("Cost:");
		newClaim.cost = int.Parse(GUILayout.TextField("" + newClaim.cost));
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Label("Currency:");
		newClaim.currency = int.Parse(GUILayout.TextField("" + newClaim.currency));
		GUILayout.EndHorizontal();
		if (GUILayout.Button("Place Claim Here")) {
			Dictionary<string, object> props = new Dictionary<string, object> ();
			props.Add("loc", ClientAPI.GetPlayerObject().Position);
			props.Add("name", newClaim.name);
			props.Add("size", newClaim.area);
			props.Add("owned", newClaim.playerOwned);
			props.Add("forSale", newClaim.forSale);
			props.Add("cost", newClaim.cost);
			props.Add("currency", newClaim.currency);
			NetworkAPI.SendExtensionMessage (ClientAPI.GetPlayerOid (), false, "voxel.CREATE_CLAIM", props);
			ClientAPI.Write("Sent create claim message");
			SetBuildingState(WorldBuildingState.Standard);
		}
		GUILayout.EndArea();
	}
	
	bool GetHitLocation ()
	{	
		//Debug.Log("GetHitLocation");
		Ray ray = Camera.main.ScreenPointToRay (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, 0));
		bool hitSomething = Physics.Raycast (ray, out hit, 200f);
		if (activeClaim != null && ((activeClaim.area == 0) ||
		 	ClientAPI.ScriptObject.GetComponent<WorldBuilder>().InsideClaimArea (activeClaim, hit.point))) {
			hitNormal = hit.normal;
			hitPoint = hit.point;
			return hitSomething;
		} else {
			return false;
		}
	}
	
	public void OnEvent(EventData eData) {
		if (eData.eventType == "PLACE_CLAIM_OBJECT") {
			if (GetBuildingState() != WorldBuildingState.PlaceItem)
				return;
			if (!open)
				ToggleOpen();
			OID itemOID = OID.fromString(eData.eventArgs[0]);
			itemBeingPlaced = ClientAPI.ScriptObject.GetComponent<Inventory>().GetInventoryItem(itemOID);
			// Does the item have a ClaimObject effect?
			List<int> effectPositions = itemBeingPlaced.GetEffectPositionsOfTypes("ClaimObject");
			if (effectPositions.Count == 0) {
				itemBeingPlaced = null;
				return;
			} else {
				string prefabName = itemBeingPlaced.itemEffectValues[effectPositions[0]];
				prefabName = prefabName.Remove(0, 17);
				prefabName = prefabName.Remove(prefabName.Length - 7);
				// Create an instance of the game Object
				GameObject prefab = (GameObject)Resources.Load(prefabName);
				GetHitLocation();
				SetCurrentReticle((GameObject)UnityEngine.Object.Instantiate(prefab, hitPoint, Quaternion.identity));
			}
		} else if (eData.eventType == "CLAIM_OBJECT_CLICKED") {
			int objectID = int.Parse(eData.eventArgs[0]);
			if (GetBuildingState() == WorldBuildingState.SelectItem && activeClaim.claimObjects.ContainsKey(objectID)) {
				SetCurrentReticle(activeClaim.claimObjects[objectID]);
			} 
		}
	}
	
	void SetCurrentReticle(GameObject obj) {
		currentReticle = obj;
		Collider[] colliders = currentReticle.GetComponents<Collider> ();
		foreach (Collider col in colliders)
			col.enabled = false;
		colliders = currentReticle.GetComponentsInChildren<Collider> ();
		foreach (Collider col in colliders)
			col.enabled = false;
		// Disable mouse wheel scroll
		ClientAPI.GetInputController().MouseWheelDisabled = true;
	}
	
	void ClearCurrentReticle(bool destroyObj) {
		if (destroyObj) {
			DestroyImmediate(currentReticle);
		}
		currentReticle = null;
		ClientAPI.GetInputController().MouseWheelDisabled = false;
	}
	
	/// <summary>
	/// Gets the BuildingState from the World Builder
	/// </summary>
	/// <returns>The building state.</returns>
	WorldBuildingState GetBuildingState() {
		return ClientAPI.ScriptObject.GetComponent<WorldBuilder>().BuildingState;
	}
	
	/// <summary>
	/// Tells the WorldBuilder to update the BuildingState
	/// </summary>
	/// <param name="newState">New state.</param>
	void SetBuildingState(WorldBuildingState newState) {
		ClientAPI.ScriptObject.GetComponent<WorldBuilder>().BuildingState = newState;
	}
	
	public void ToggleOpen() {
		open = !open;
		if (open) {
			UiSystem.AddFrame(frameName, uiRect);
			SetBuildingState(WorldBuildingState.Standard);
			ClientAPI.ScriptObject.GetComponent<WorldBuilder>().ShowClaims = true;
			
		} else {
			UiSystem.RemoveFrame(frameName, new Rect(0, 0, 0, 0));
			SetBuildingState(WorldBuildingState.None);
			ClientAPI.ScriptObject.GetComponent<WorldBuilder>().ShowClaims = false;
			ClearCurrentReticle(true);
			gameObject.GetComponent<Cursor>().ChangeWorldBuilderState(false);
		}
	}
}
