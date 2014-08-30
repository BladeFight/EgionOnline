using UnityEngine;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Collections;
using System.Collections.Generic;

// Structure of a Atavism Instance
public class Instance: DataStructure
{
	public int id = 0;				// Database index
	// General Parameters
	public string name = "name";					// Instance name
	public bool createOnStartup = false;			// True, if wants to create at startup
	public int islandType;							// Type of island
	public int administrator;						// The account that has administration privileges of the island
	string spawn = null;							// Prefab Object to spawn 
	public Vector3 spawnLoc= new Vector3 (0, 0, 0); // Spawn world location
		
	public Instance() {
	// Database fields
	fields = new Dictionary<string, string> () {
		{"island_name", "string"},
		{"createOnStartup", "bool"},
		{"administrator", "int"},
	};
	}

	public string Spawn {
		get {
			return spawn;
		}
		set {
			spawn = value;
			if (spawn != null) {
				// Try to get as Scene Object
				GameObject tempObject = GameObject.Find(spawn);
				// If the object is at the Scene
				if (tempObject != null)
					// Get object transform
					spawnLoc = tempObject.transform.position;
			}
		}
	}
	
	public Instance Clone()
	{
		return (Instance) this.MemberwiseClone();
	}
		
	public override string GetValue (string fieldKey)
	{
		switch (fieldKey) {
		case "id":
			return id.ToString();
			break;
		case "island_name":
			return name;
			break;
		case "administrator":
			return administrator.ToString();
			break;
		}	
		return "";
	}
}
