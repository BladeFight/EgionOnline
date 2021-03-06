﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Claim {
	public int id;
	public string name = "";
	public Vector3 loc;
	public int area = 30;
	public bool playerOwned;
	public bool forSale;
	public int cost;
	public int currency;
	public Dictionary<int, int> resources = new Dictionary<int, int>();
	public Dictionary<int, GameObject> claimObjects = new Dictionary<int, GameObject>();
}

public enum WorldBuildingState {
	PlaceItem,
	SelectItem,
	EditItem,
	MoveItem,
	SellClaim,
	PurchaseClaim,
	CreateClaim,
	Standard,
	None
}

public class WorldBuilder : MonoBehaviour {

	WorldBuildingState buildingState = WorldBuildingState.None;

	private List<Claim> claims = new List<Claim>();
	private Claim activeClaim = null;
	bool showClaims = false;
	List<GameObject> claimGameObjects = new List<GameObject>();

	// Use this for initialization
	void Start () {
		EventSystem.RegisterEvent("CLAIM_ADDED", this);
		EventSystem.RegisterEvent("CLAIMED_REMOVED", this);
	
		// Register for messages relating to the claim system
		NetworkAPI.RegisterExtensionMessageHandler("claim_object", ClaimObjectMessage);
		NetworkAPI.RegisterExtensionMessageHandler("claim_object_bulk", ClaimObjectBulkMessage);
		NetworkAPI.RegisterExtensionMessageHandler("move_claim_object", MoveClaimObjectMessage);
		NetworkAPI.RegisterExtensionMessageHandler("remove_claim_object", RemoveClaimObjectMessage);
		NetworkAPI.RegisterExtensionMessageHandler("set_claim", ClaimIDMessage);
		NetworkAPI.RegisterExtensionMessageHandler("claim_updated", ClaimUpdatedMessage);
		NetworkAPI.RegisterExtensionMessageHandler("remove_claim", RemoveClaimMessage);
		NetworkAPI.RegisterExtensionMessageHandler("claim_deleted", RemoveClaimMessage);
		NetworkAPI.RegisterExtensionMessageHandler("claim_made", ClaimMadeMessage);
	}
	
	// Update is called once per frame
	void Update () {
		if (activeClaim == null && buildingState != WorldBuildingState.None) {
			// Check against claims to see if a region has been entered
			foreach (Claim claim in claims) {
				if (InsideClaimArea(claim, ClientAPI.GetPlayerObject().Position)) {
					activeClaim = claim;
					break;
				}
			}
		} else if (buildingState != WorldBuildingState.None) {
			// Check if the player has left the claim
			if (!InsideClaimArea(activeClaim, ClientAPI.GetPlayerObject().Position)) {
				activeClaim = null;
			}
		}
	}
	
	public void OnEvent(EventData eData) {
		if (eData.eventType == "CLAIM_ADDED") {
			GameObject claim = GameObject.Find(eData.eventArgs[0]);
			claimGameObjects.Add(claim);
			claim.SetActive(showClaims);
		} else if (eData.eventType == "CLAIM_REMOVED") {
			GameObject claim = GameObject.Find(eData.eventArgs[0]);
			claimGameObjects.Remove(claim);
		}
	}
	
	public bool InsideClaimArea (Claim claim, Vector3 point)
	{
		if (InRange (point.x, claim.loc.x - claim.area / 2, claim.loc.x + claim.area / 2) && 
		    InRange (point.z, claim.loc.z - claim.area / 2, claim.loc.z + claim.area / 2)) {
			return true;
		}
		return false;
	}
	
	bool InRange (float val, float min, float max)
	{
		return ((val >= min) && (val <= max));
	}
	
	public Claim GetClaim(int claimID) {
		foreach (Claim claim in claims) {
			if (claim.id == claimID)
				return claim;
		}
		return null;
	}
	
	/// <summary>
	/// Handles the Claim Action Message from the server. Passes the data onto the voxel editor.
	/// </summary>
	/// <param name="props">Properties.</param>
	public void ClaimObjectMessage(Dictionary<string, object> props) {
		int objectID = (int)props["id"];
		string prefabName = (string)props["gameObject"];
		Vector3 loc = (Vector3)props["loc"];
		Quaternion orient = (Quaternion)props["orient"];
		int claimID = (int)props["claimID"];
		
		Debug.Log("Got claim object: " + gameObject);
		SpawnClaimObject(objectID, claimID, prefabName, loc, orient);
	}
	
	void SpawnClaimObject(int objectID, int claimID, string prefabName, Vector3 loc, Quaternion orient) {
		// Spawn the object in the world
		prefabName = prefabName.Remove(0, 17);
		prefabName = prefabName.Remove(prefabName.Length - 7);
		GameObject prefab = (GameObject)Resources.Load(prefabName);
		GameObject claimObject = (GameObject)UnityEngine.Object.Instantiate(prefab, loc, orient);
		// Add the Claim Object Component
		ClaimObject cObject = claimObject.AddComponent<ClaimObject>();
		// Add the gameObject to the claim
		Claim claim = GetClaim(claimID);
		claim.claimObjects.Add(objectID, claimObject);
		cObject.ID = objectID;
	}
	
	/// <summary>
	/// Handles the Claim Action Bulk Message which is used to transfer large amounts of actions at once.
	/// </summary>
	/// <param name="props">Properties.</param>
	public void ClaimObjectBulkMessage(Dictionary<string, object> props) {
		int numObjects = (int)props["numObjects"];
		UnityEngine.Debug.Log("Got numObjects: " + numObjects);
		for (int i = 0; i < numObjects; i++) {
			string actionString = (string)props["object_" + i];
			string[] actionData = actionString.Split(';');
			string objectID = actionData[0];
			string claimID = actionData[1];
			string gameObject = actionData[2];
			string[] locData = actionData[3].Split(',');
			Vector3 loc = new Vector3(float.Parse(locData[0]), float.Parse(locData[1]), float.Parse(locData[2]));
			string[] normalData = actionData[4].Split(',');
			Quaternion orient = new Quaternion(float.Parse(normalData[0]), float.Parse(normalData[1]), float.Parse(normalData[2]), float.Parse(normalData[2]));
			UnityEngine.Debug.Log("Something");
			SpawnClaimObject(int.Parse(objectID), int.Parse(claimID), gameObject, loc, orient);
		}
	}
	
	public void MoveClaimObjectMessage(Dictionary<string, object> props) {
		int objectID = (int)props["id"];
		Vector3 loc = (Vector3)props["loc"];
		Quaternion orient = (Quaternion)props["orient"];
		int claimID = (int)props["claimID"];
		
		Claim claim = GetClaim(claimID);
		if (claim != null) {
			claim.claimObjects[objectID].transform.position = loc;
			claim.claimObjects[objectID].transform.rotation = orient;
		}
		
		//Debug.Log("Got claim object: " + gameObject);
		//SpawnClaimObject(objectID, claimID, prefabName, loc, orient);
	}
	
	public void RemoveClaimObjectMessage(Dictionary<string, object> props) {
		int objectID = (int)props["id"];
		int claimID = (int)props["claimID"];
		
		Claim claim = GetClaim(claimID);
		if (claim != null) {
			DestroyImmediate(claim.claimObjects[objectID]);
			claim.claimObjects.Remove(objectID);
		}
	}
	
	public void ClaimIDMessage(Dictionary<string, object> props) {
		Claim claim = new Claim();
		claim.id = (int)props["claimID"];
		claim.name = (string)props["claimName"];
		claim.area = (int)props["claimArea"];
		claim.loc = (Vector3)props["claimLoc"];
		claim.forSale = (bool)props["forSale"];
		if (claim.forSale) {
			claim.cost = (int)props["cost"];
			claim.currency = (int)props["currency"];
		}
		claim.playerOwned = (bool)props["myClaim"];
		claims.Add(claim);
		UnityEngine.Debug.Log("Got new claim data");
	}
	
	public void ClaimUpdatedMessage(Dictionary<string, object> props) {
		int claimID = (int)props["claimID"];
		Claim claim = GetClaim(claimID);
		claim.forSale = (bool)props["forSale"];
		if (claim.forSale) {
			claim.cost = (int)props["cost"];
			claim.currency = (int)props["currency"];
		}
		claim.playerOwned = (bool)props["myClaim"];
		claims.Add(claim);
		UnityEngine.Debug.Log("Got claim update data");
	}
	
	/// <summary>
	/// Handles the Remove Claim Message which means a player is no longer in the radius for a claim
	/// so the client no longer needs to check if they are in its edit radius.
	/// </summary>
	/// <param name="props">Properties.</param>
	public void RemoveClaimMessage(Dictionary<string, object> props) {
		int claimID = (int)props["claimID"];
		Claim claimToRemove = null;
		foreach (Claim claim in claims) {
			if (claim.id == claimID) {
				/*int itemID = (int)props["resource"];
				int count = (int)props["resourceCount"];
				claim.resources[itemID] = count;*/
				foreach (GameObject cObject in claim.claimObjects.Values) {
					DestroyImmediate(cObject);
				}
				claimToRemove = claim;
				break;
			}
		}
		if (claimToRemove != null) {
			claims.Remove(claimToRemove);
		}
		
		UnityEngine.Debug.Log("Got remove claim data");
	}
	
	/// <summary>
	/// Temporary hack to remove the claim deed item
	/// </summary>
	/// <param name="props">Properties.</param>
	public void ClaimMadeMessage(Dictionary<string, object> props) {
		// Something to be doing?
		
	}
	
	public void ClaimAppeared(GameObject claim) {
		claimGameObjects.Add(claim);
		claim.SetActive(showClaims);
	}
	
	public void ClaimRemoved(GameObject claim) {
		claimGameObjects.Remove(claim);
	}
	
	public List<Claim> Claims {
		get {
			return claims;
		}
	}
	
	public Claim ActiveClaim {
		get {
			return activeClaim;
		}
	}
	
	public bool ShowClaims {
		get {
			return showClaims;
		}
		set {
			showClaims = value;
			foreach (GameObject claim in claimGameObjects) {
				claim.SetActive(showClaims);
			}
		}
	}
	
	public WorldBuildingState BuildingState {
		get {
			return buildingState;
		}
		set {
			buildingState = value;
		}
	}
}
