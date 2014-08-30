using UnityEngine;
using UnityEditor;
using MySql.Data;
using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Collections;
using System.Collections.Generic;

// Class that implements the Instances configuration
public class ServerInstances : AtavismDatabaseFunction
{
	public Dictionary<int, Instance> dataRegister;
	public Instance editingDisplay;
	public Instance originalDisplay;
	
	public int[] accountIds = new int[] {1};
	public string[] accountList = new string[] {"~ First Account ~"};

	// Database auxiliar table name
	private string portalTableName = "island_portals";

	// Use this for initialization
	public ServerInstances ()
	{	
		functionName = "Instances";
		// Database tables name
		tableName = "islands";
		functionTitle = "Instance Configuration";
		loadButtonLabel = "Load Instances";
		notLoadedText = "No Instance loaded.";
		// Init
		dataRegister = new Dictionary<int, Instance> ();

		editingDisplay = new Instance ();	
		originalDisplay = new Instance ();			
	}

	public override void Activate ()
	{
		linkedTablesLoaded = false;
	}
	
	private void LoadAccountList ()
	{
		string query = "SELECT id, username FROM account";
		
		// If there is a row, clear it.
		if (rows != null)
			rows.Clear ();
		
		// Load data
		rows = DatabasePack.LoadData (DatabasePack.adminDatabasePrefix, query);
		// Read data
		int optionsId = 0;
		if ((rows != null) && (rows.Count > 0)) {
			accountList = new string[rows.Count];
			accountIds = new int[rows.Count];
			foreach (Dictionary<string,string> data in rows) {
				accountList [optionsId] = data ["id"] + ":" + data ["username"]; 
				accountIds [optionsId] = int.Parse (data ["id"]);
				optionsId++;
			}
		} else {
			accountList = new string[1];
			accountList [optionsId] = "~ First Account ~"; 
			accountIds = new int[1];
			accountIds [optionsId] = 1;
		}
	}
	
	// Load Database Data
	public override void Load ()
	{
		if (!dataLoaded) {
			// Clean old data
			dataRegister.Clear ();
			displayKeys.Clear ();

			// Read all entries from the table
			string query = "SELECT id, island_name, createOnStartup, administrator FROM " + tableName;
			
			// If there is a row, clear it.
			if (rows != null)
				rows.Clear ();
		
			// Load data
			rows = DatabasePack.LoadData (DatabasePack.adminDatabasePrefix, query);
		
			// Read all the data
			if ((rows != null) && (rows.Count > 0)) {
				foreach (Dictionary<string,string> data in rows) {
					Instance display = new Instance ();
					display.id = int.Parse (data ["id"]);
					display.name = data ["island_name"];
					display.createOnStartup = bool.Parse (data ["createOnStartup"]);
					display.administrator = int.Parse(data["administrator"]);
					display.isLoaded = false;
					//Debug.Log("Name:" + display.name  + "=[" +  display.id  + "]");
					dataRegister.Add (display.id, display);
					displayKeys.Add (display.id);
				}
				LoadSelectList ();
			}
			dataLoaded = true;
		}
	}
	
	public void LoadSelectList ()
	{
		//string[] selectList = new string[dataRegister.Count];
		displayList = new string[dataRegister.Count];
		int i = 0;
		foreach (int displayID in dataRegister.Keys) {
			//selectList [i] = displayID + ". " + dataRegister [displayID].name;
			displayList [i] = displayID + ". " + dataRegister [displayID].name;
			i++;
		}
		//displayList = new Combobox(selectList);
	}
	
	void CheckInstance (int instanceID)
	{
		if (!dataRegister [instanceID].isLoaded) {
			// Load in spawn data
			rows.Clear ();
			string query = "SELECT island, name, locX, locY, locZ, gameObject FROM " + portalTableName;
			// We consider name as the GameObject field
			query += " where island = " + dataRegister [instanceID].id; // + " and name='spawn'";
			
			rows = DatabasePack.LoadData (DatabasePack.adminDatabasePrefix, query);
			// Read all the data
			if ((rows != null) && (rows.Count > 0)) {
				foreach (Dictionary<string,string> data in rows) {
					float locX = float.Parse (data ["locX"]);
					float locY = float.Parse (data ["locY"]);
					float locZ = float.Parse (data ["locZ"]);
					dataRegister [instanceID].spawnLoc = new Vector3 (locX, locY, locZ);
					dataRegister [instanceID].Spawn = data ["gameObject"];
					dataRegister [instanceID].isLoaded = true;
				}
			}
		}
	}
	
	// Draw the Instance list
	public override  void DrawLoaded (Rect box)
	{	
		// Setup the layout
		Rect pos = box;
		pos.x += ImagePack.innerMargin;
		pos.y += ImagePack.innerMargin;
		pos.width -= ImagePack.innerMargin;
		pos.height = ImagePack.fieldHeight;
								
		if (dataRegister.Count <= 0) {
			pos.y += ImagePack.fieldHeight;
			ImagePack.DrawLabel (pos.x, pos.y, "You must create an Instance before edit it.");		
			return;
		}
		

		// Draw the content database info
		ImagePack.DrawLabel (pos.x, pos.y, "Instance Configuration");
				
		if (newItemCreated) {
			newItemCreated = false;
			LoadSelectList ();
			newSelectedDisplay = displayKeys.Count - 1;
		}
		

		// Draw data Editor
		if (newSelectedDisplay != selectedDisplay) {
			selectedDisplay = newSelectedDisplay;	
			int displayKey = displayKeys [selectedDisplay];
			CheckInstance (dataRegister [displayKey].id);			
			editingDisplay = dataRegister [displayKey];		
			originalDisplay = editingDisplay.Clone ();
		} 

		//if (!displayList.showList) {
		pos.y += ImagePack.fieldHeight;
		pos.x -= ImagePack.innerMargin;
		pos.y -= ImagePack.innerMargin;
		pos.width += ImagePack.innerMargin;
		DrawEditor (pos, false);
		pos.y -= ImagePack.fieldHeight;
		//pos.x += ImagePack.innerMargin;
		pos.y += ImagePack.innerMargin;
		pos.width -= ImagePack.innerMargin;
		//}
		
		if (state != State.Loaded) {
			// Draw combobox
			pos.width /= 2;
			pos.x += pos.width;
			newSelectedDisplay = ImagePack.DrawCombobox (pos, "", selectedDisplay, displayList);
			pos.x -= pos.width;
			pos.width *= 2;
		}

	}
		
	public override void CreateNewData ()
	{
		editingDisplay = new Instance ();
		originalDisplay = new Instance ();
		selectedDisplay = -1;
	}
	// Edit or Create a new instance
	public override void DrawEditor (Rect box, bool newInstance)
	{
		
		// Setup the layout
		Rect pos = box;
		pos.x += ImagePack.innerMargin;
		pos.y += ImagePack.innerMargin;
		pos.width -= ImagePack.innerMargin;
		pos.height = ImagePack.fieldHeight;
		
		if (!linkedTablesLoaded) {	
			LoadAccountList();
			linkedTablesLoaded = true;
		}

		// Draw the content database info
		if (newInstance) {
			ImagePack.DrawLabel (pos.x, pos.y, "Create a new instance");		
			pos.y += ImagePack.fieldHeight;
		}
		editingDisplay.name = ImagePack.DrawField (pos, "Name:", editingDisplay.name, 0.75f);
		
		pos.y += ImagePack.fieldHeight;
		editingDisplay.spawnLoc = ImagePack.Draw3DPosition (pos, "Spawn Location:", editingDisplay.spawnLoc);

		pos.y += 2 * ImagePack.fieldHeight;
		editingDisplay.Spawn = ImagePack.DrawGameObject (pos, "Game Object:", editingDisplay.Spawn, 0.75f);

		pos.y += ImagePack.fieldHeight;
		editingDisplay.createOnStartup = ImagePack.DrawToggleBox (pos, "Create On Startup:", editingDisplay.createOnStartup);
		
		pos.y += ImagePack.fieldHeight;
		int selectedAccount = GetPositionOfAccount (editingDisplay.administrator);
		selectedAccount = ImagePack.DrawSelector (pos, "Admin Account:", selectedAccount, accountList);
		editingDisplay.administrator = accountIds [selectedAccount];
		
		// Save Instance data
		pos.x -= ImagePack.innerMargin;
		pos.y += 1.4f * ImagePack.fieldHeight;
		pos.width /= 3;
		if (ImagePack.DrawButton (pos.x, pos.y, "Save Data")) {
			if (newInstance)
				InsertEntry ();
			else
				UpdateEntry ();
			
			state = State.Loaded;
		}
		
		// Delete Instance data
		if (!newInstance) {
			pos.x += pos.width;
			if (ImagePack.DrawButton (pos.x, pos.y, "Delete Data")) {
				DeleteEntry ();
				newSelectedDisplay = 0;
				state = State.Loaded;
			}
		}
		
		// Cancel editing
		pos.x += pos.width;
		if (ImagePack.DrawButton (pos.x, pos.y, "Cancel")) {
			editingDisplay = originalDisplay.Clone ();
			if (newInstance)
				state = State.New;
			else
				state = State.Loaded;
		}
		
		if (resultTimeout != -1 && resultTimeout > Time.realtimeSinceStartup) {
			pos.y += ImagePack.fieldHeight;
			ImagePack.DrawText (pos, result);
		}

	}
	
	// Insert new entries into the table
	void InsertEntry ()
	{
		NewResult ("Inserting...");
		// Setup the update query
		string query = "INSERT INTO " + tableName;		
		query += " (island_name, template, administrator, category, status, createOnStartup, islandType, public, password, style, recommendedLevel, description, size) ";
		query += "VALUES ";
		query += "(?island_name, ?template, ?administrator, ?category, ?status, ?createOnStartup, ?islandType, ?public, ?password, ?style, ?recommendedLevel, ?description, ?size)";
		
		int instanceID = -1;

		// Setup the register data		
		List<Register> update = new List<Register> ();
		update.Add (new Register ("island_name", "?island_name", MySqlDbType.VarChar, editingDisplay.name.ToString (), Register.TypesOfField.String));       
		update.Add (new Register ("template", "?template", MySqlDbType.VarChar, "", Register.TypesOfField.String));
		update.Add (new Register ("administrator", "?administrator", MySqlDbType.Int32, editingDisplay.administrator.ToString(), Register.TypesOfField.Int));
		update.Add (new Register ("category", "?category", MySqlDbType.Int32, "1", Register.TypesOfField.Int));
		update.Add (new Register ("status", "?status", MySqlDbType.VarChar, "Active", Register.TypesOfField.String));
		update.Add (new Register ("createOnStartup", "?createOnStartup", MySqlDbType.Byte, editingDisplay.createOnStartup.ToString (), Register.TypesOfField.Bool));
		update.Add (new Register ("islandType", "?islandType", MySqlDbType.Int32, "0", Register.TypesOfField.Int));
		update.Add (new Register ("public", "?public", MySqlDbType.Byte, "true", Register.TypesOfField.Bool));
		update.Add (new Register ("password", "?password", MySqlDbType.VarChar, "", Register.TypesOfField.String));
		update.Add (new Register ("style", "?style", MySqlDbType.VarChar, "", Register.TypesOfField.String));
		update.Add (new Register ("recommendedLevel", "?recommendedLevel", MySqlDbType.Int32, "0", Register.TypesOfField.Int));
		update.Add (new Register ("description", "?description", MySqlDbType.VarChar, "", Register.TypesOfField.String));	
		update.Add (new Register ("size", "?size", MySqlDbType.Int32, "-1", Register.TypesOfField.Int));

		// Update the database
		instanceID = DatabasePack.Insert (DatabasePack.adminDatabasePrefix, query, update);

		// If the insert failed, don't insert the spawn marker
		if (instanceID != -1) {
			int islandID = instanceID;

			query = "INSERT INTO " + portalTableName;
			query += " (island, portalType, faction, locX, locY, locZ, orientX, orientY, orientZ, orientW, displayID, name, gameObject)";
			query += "VALUES ";
			query += "(?island, ?portalType, ?faction, ?locX, ?locY, ?locZ, ?orientX, ?orientY, ?orientZ, ?orientW, ?displayID, ?name, ?gameObject)";

			// Setup the register data		
			update.Clear ();
			update.Add (new Register ("island", "?island", MySqlDbType.Int32, islandID.ToString (), Register.TypesOfField.Int));
			update.Add (new Register ("portalType", "?portalType", MySqlDbType.Byte, "true", Register.TypesOfField.Bool));
			update.Add (new Register ("faction", "?faction", MySqlDbType.Int32, "0", Register.TypesOfField.Int));
			// Note: The database store Int values for locX, locY and LocZ. That is why we convert float to int before save
			update.Add (new Register ("locX", "?locX", MySqlDbType.Int32, Mathf.FloorToInt (editingDisplay.spawnLoc.x).ToString (), Register.TypesOfField.Int));
			update.Add (new Register ("locY", "?locY", MySqlDbType.Int32, Mathf.FloorToInt (editingDisplay.spawnLoc.y).ToString (), Register.TypesOfField.Int));
			update.Add (new Register ("locZ", "?locZ", MySqlDbType.Int32, Mathf.FloorToInt (editingDisplay.spawnLoc.z).ToString (), Register.TypesOfField.Int));
			update.Add (new Register ("orientX", "?orientX", MySqlDbType.Int32, "0", Register.TypesOfField.Int));
			update.Add (new Register ("orientY", "?orientY", MySqlDbType.Int32, "0", Register.TypesOfField.Int));
			update.Add (new Register ("orientZ", "?orientZ", MySqlDbType.Int32, "0", Register.TypesOfField.Int));
			update.Add (new Register ("orientW", "?orientW", MySqlDbType.Int32, "1", Register.TypesOfField.Int));
			update.Add (new Register ("displayID", "?displayID", MySqlDbType.Int32, "1", Register.TypesOfField.Int));
			update.Add (new Register ("name", "?name", MySqlDbType.VarChar, "spawn", Register.TypesOfField.String));	
			update.Add (new Register ("gameObject", "?gameObject", MySqlDbType.VarChar, editingDisplay.Spawn, Register.TypesOfField.String));	

			// Update the database
			DatabasePack.Insert (DatabasePack.adminDatabasePrefix, query, update);
          
			// Update online table to avoid access the database again			
			editingDisplay.id = instanceID;
			editingDisplay.isLoaded = true;
			//Debug.Log("ID:" + instanceID + "ID2:" + editingDisplay.id);
			dataRegister.Add (editingDisplay.id, editingDisplay);
			displayKeys.Add (editingDisplay.id);
			newItemCreated = true;
			NewResult ("New entry inserted");
		} else {
			NewResult ("Error occurred, please check the Console");
		}
	}

	// Update existing entries in the table based on the iddemo_table
	void UpdateEntry ()
	{
		NewResult ("Updating...");
		// Setup the update query
		string query = "UPDATE " + tableName;
		query += " SET island_name=?island_name,";
		query += " createOnStartup=?createOnStartup,";
		query += " islandType=?islandType,";
		query += " administrator=?administrator WHERE id=?id";
		// Setup the register data		
		List<Register> update = new List<Register> ();
		update.Add (new Register ("island_name", "?island_name", MySqlDbType.VarChar, editingDisplay.name.ToString (), Register.TypesOfField.String));
		update.Add (new Register ("createOnStartup", "?createOnStartup", MySqlDbType.Byte, editingDisplay.createOnStartup.ToString (), Register.TypesOfField.Bool));
		update.Add (new Register ("islandType", "?islandType", MySqlDbType.Int32, "0", Register.TypesOfField.Int));
		update.Add (new Register ("administrator", "?administrator", MySqlDbType.Int32, editingDisplay.administrator.ToString(), Register.TypesOfField.Int));
		update.Add (new Register ("id", "?id", MySqlDbType.Int32, editingDisplay.id.ToString (), Register.TypesOfField.Int));
	
		// Update the database
		DatabasePack.Update (DatabasePack.adminDatabasePrefix, query, update);
		
		// Setup the update query
		query = "UPDATE " + portalTableName;
		query += " SET locX=?locX, locY=?locY, locZ=?locZ, gameObject=?gameObject"; 
		query += " where island=?island"; // and name='spawn'";
		// Setup the register data		
		update.Clear ();
		// Note: The database store Int values for locX, locY and LocZ. That is why we convert float to int before save
		update.Add (new Register ("locX", "?locX", MySqlDbType.Int32, Mathf.FloorToInt (editingDisplay.spawnLoc.x).ToString (), Register.TypesOfField.Int));
		update.Add (new Register ("locY", "?locY", MySqlDbType.Int32, Mathf.FloorToInt (editingDisplay.spawnLoc.y).ToString (), Register.TypesOfField.Int));
		update.Add (new Register ("locZ", "?locZ", MySqlDbType.Int32, Mathf.FloorToInt (editingDisplay.spawnLoc.z).ToString (), Register.TypesOfField.Int));
		update.Add (new Register ("island", "?island", MySqlDbType.Int32, editingDisplay.id.ToString (), Register.TypesOfField.Int));
		update.Add (new Register ("gameObject", "?gameObject", MySqlDbType.VarChar, editingDisplay.Spawn, Register.TypesOfField.String));	

		// Update the database
		DatabasePack.Update (DatabasePack.adminDatabasePrefix, query, update);
		
		// Update online table to avoid access the database again			
		dataRegister [displayKeys [selectedDisplay]] = editingDisplay;
		NewResult ("Entry updated");				
	}
	
	// Delete entries from the table
	void DeleteEntry ()
	{
		Register delete = new Register ("id", "?id", MySqlDbType.Int32, editingDisplay.id.ToString (), Register.TypesOfField.Int);
		DatabasePack.Delete (DatabasePack.adminDatabasePrefix, tableName, delete);
		delete = new Register ("island", "?island", MySqlDbType.Int32, editingDisplay.id.ToString (), Register.TypesOfField.Int);
		DatabasePack.Delete (DatabasePack.adminDatabasePrefix, portalTableName, delete);
		
		// Update online table to avoid access the database again			
		dataRegister.Remove (displayKeys [selectedDisplay]);
		displayKeys.Remove (selectedDisplay);
		if (dataRegister.Count > 0)		
			LoadSelectList ();
		else {
			displayList = null;
			dataLoaded = false;
		}

	}
	
	private int GetPositionOfAccount (int accountId)
	{
		for (int i = 0; i < accountIds.Length; i++) {
			if (accountIds [i] == accountId)
				return i;
		}
		return 0;
	}

}