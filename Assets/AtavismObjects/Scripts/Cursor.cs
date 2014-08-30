using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Cursor state Enum. Keeps track of what cursor to draw
/// </summary>
public enum CursorState
{
	Cast,
	Cast_Error,
	Speak,
	Speak_Error,
	Attack,
	Attack_Error,
	Loot,
	Loot_Error,
	Default
}

public enum InterfaceOverrideState
{
	Merchant,
	WorldBuilding,
	None
}

public class Cursor : MonoBehaviour
{
	public LayerMask layerMask;
	public Texture defaultCursor;
	public Texture2D attackCursor;
	public Texture2D lootCursor;
	public Texture2D speakCursor;
	Texture2D cursorTexture;
	AtavismInventoryItem _cursorItem = null;
	Ability _cursorAbility = null;
	Bag _cursorBag = null;
	CursorState cursorState = CursorState.Default;
	InterfaceOverrideState interfaceOverrideState = InterfaceOverrideState.None;
	string _cursorFrame = "";
	List<string> _cursorOverFrames = new List<string> ();
	object _mouseDownObject1 = null;
	object _mouseDownObject2 = null;

	// Use this for initialization
	void Start ()
	{
		ClientAPI.WorldManager.RegisterObjectPropertyChangeHandler ("arena_flag", ClickableObjectHandler);
		ClientAPI.WorldManager.RegisterObjectPropertyChangeHandler ("Usable", ClickableObjectHandler);
		Screen.showCursor = false;
		//UiSystem.OverrideFrameDetection = true;
	}
	
	// Update is called once per frame
	void Update ()
	{
		GameObject mouseOver = null;
		AtavismNode mouseOverObject = null;
		Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
		RaycastHit hit;
		// Casts the ray and get the first game object hit
		if (Physics.Raycast (ray, out hit, Mathf.Infinity, layerMask))
			mouseOver = hit.transform.gameObject;
		// Get objectnode from object
		if (mouseOver != null) {
			mouseOverObject = mouseOver.GetComponent<AtavismNode> ();
		}
		_UpdateContextCursor (mouseOverObject);
		// Handle mouse clicks
		if (Input.GetMouseButtonDown (0)) {
			OnLeftClick (true, mouseOverObject);
		}
		if (Input.GetMouseButtonUp (0)) {
			OnLeftClick (false, mouseOverObject);
		}
		if (Input.GetMouseButtonDown (1)) {
			OnRightClick (true, mouseOverObject);
		} 
		if (Input.GetMouseButtonUp (1)) {
			OnRightClick (false, mouseOverObject);
		}
		
		if (Input.GetKeyDown (KeyCode.Escape)) {
			ClientAPI.ClearTarget ();
		}
	}
	
	void OnGUI ()
	{
		Screen.showCursor = false;
		if (ClientAPI.mouseLook)
			return;
		GUI.depth = 1;
		//GameObject go = GameObject.FindGameObjectWithTag("ScriptUpdater");
		if (_cursorItem != null) {
			GUI.DrawTexture (new Rect (Input.mousePosition.x, Screen.height - Input.mousePosition.y, 32, 32), _cursorItem.icon);
			return;
		} else if (_cursorAbility != null) {
			GUI.DrawTexture (new Rect (Input.mousePosition.x, Screen.height - Input.mousePosition.y, 32, 32), _cursorAbility.icon);
			return;
		} else if (_cursorBag != null) {
			GUI.DrawTexture (new Rect (Input.mousePosition.x, Screen.height - Input.mousePosition.y, 32, 32), _cursorBag.icon);
			return;
		}
		switch (cursorState) {
		case CursorState.Attack_Error:
			GUI.color = Color.grey;
			GUI.DrawTexture (new Rect (Input.mousePosition.x, Screen.height - Input.mousePosition.y, 32, 32), attackCursor);
			break;
		case CursorState.Attack:
			GUI.DrawTexture (new Rect (Input.mousePosition.x, Screen.height - Input.mousePosition.y, 32, 32), attackCursor);
			break;
		case CursorState.Loot_Error:
			GUI.color = Color.grey;
			GUI.DrawTexture (new Rect (Input.mousePosition.x, Screen.height - Input.mousePosition.y, 32, 32), lootCursor);
			break;
		case CursorState.Loot:
			GUI.DrawTexture (new Rect (Input.mousePosition.x, Screen.height - Input.mousePosition.y, 32, 32), lootCursor);
			break;
		case CursorState.Speak_Error:
			GUI.color = Color.grey;
			GUI.DrawTexture (new Rect (Input.mousePosition.x, Screen.height - Input.mousePosition.y, 32, 32), speakCursor);
			break;
		case CursorState.Speak:
			GUI.DrawTexture (new Rect (Input.mousePosition.x, Screen.height - Input.mousePosition.y, 32, 32), speakCursor);
			break;
		default:
			GUI.DrawTexture (new Rect (Input.mousePosition.x, Screen.height - Input.mousePosition.y, 32, 32), defaultCursor);
			break;
		}
		GUI.color = Color.white;
	}

	#region API Methods

	public bool CursorHasItem ()
	{
		return _cursorItem != null;
	}

	public bool CursorHasAbility ()
	{
		return _cursorAbility != null;
	}
	
	public bool CursorHasBag ()
	{
		return _cursorBag != null;
	}
	
	
	// Pick up (or drop) an item from a given container and slot.
	// This will set the cursor to the item.
	public void PickupBagItem (int bagNum, int slotNum, AtavismInventoryItem item)
	{
		if (CursorHasItem ()) {
			// Place the item in the slot (possibly taking the item that was in the slot)
			Debug.Log ("Drop item to: " + bagNum + "," + slotNum);
			Debug.Log ("Old item: " + item);
			//_SetContainerItem(containerId, slotId, MarsCursor._cursorItem)
			PlaceCursorItem (bagNum, slotNum);
			_cursorItem = item;
			_UpdateCursor ();
		} else if (CursorHasBag()) {
        	// Place the item in the slot (possibly taking the item that was in the slot)
        	Debug.Log("Drop item to: " + bagNum + "," + slotNum);
        	Debug.Log("Old item: " + item);
        	//_SetContainerItem(containerId, slotId, MarsCursor._cursorItem)
        	PlaceCursorBagAsItem(bagNum, slotNum);
        	_cursorItem = item;
        	_UpdateCursor();
		} else if (CursorHasAbility ()) {
			Debug.Log ("Cannot currently use abilities on items");
		} else {
			// Cursor is empty
			if (item != null) {
				_cursorItem = item;
				_cursorAbility = null;
				_UpdateCursor ();
				Debug.Log ("Pickup item: " + item + "," + item.icon);
			}
		}
	}
	
	public void PickupEquippedItem (int slotNum, AtavismInventoryItem item)
	{
		if (CursorHasItem ()) {
			// Place the item in the slot (possibly taking the item that was in the slot)
			Debug.Log ("Drop item to equipped slot: " + slotNum);
			Debug.Log ("Old item: " + item);
			//_SetContainerItem(containerId, slotId, MarsCursor._cursorItem)
			_cursorItem.Activate ();
			ResetCursor ();
			_UpdateCursor ();
		} else if (CursorHasAbility ()) {
			Debug.Log ("Cannot currently use abilities on items");
		} else {
			// Cursor is empty
			if (item != null) {
				_cursorItem = item;
				_cursorAbility = null;
				_UpdateCursor ();
				Debug.Log ("Pickup item: " + item + "," + item.icon);
			}
		}
	}
	
	public void PickupBag(Bag bag) {
		_cursorBag = bag;
	}

	public void DeleteCursorItem ()
	{
		long targetOid = ClientAPI.GetPlayerObject ().Oid;
		NetworkAPI.SendTargetedCommand (targetOid, "/deleteItemStack " + _cursorItem.ItemId);
		ResetCursor ();
	}

	public void PlaceCursorItem (int bagNum, int slotNum)
	{
		Dictionary<string, object> props = new Dictionary<string, object> ();
		props.Add ("bagNum", bagNum);
		props.Add ("slotNum", slotNum);
		props.Add ("itemOid", _cursorItem.ItemId);
		NetworkAPI.SendExtensionMessage (0, false, "inventory.MOVE_ITEM", props);
		ClientAPI.Write ("Sending move item");
		ResetCursor ();
	}
	
	public AtavismInventoryItem GetCursorItem ()
	{
		return _cursorItem;
	}
	
	/// <summary>
	/// Places the cursor item in the specified bag slot. If the item is not a bag, nothing will happen
	/// </summary>
	/// <param name="slot">Slot.</param>
	public void PlaceCursorItemAsBag (int slot)
	{
		// First verify that the item is of Bag type
		if (_cursorItem.itemType != "Bag") {
			ClientAPI.Write("Only Bag items can be placed in Bag slots");
			return;
		}
		long targetOid = ClientAPI.GetPlayerOid ();
		NetworkAPI.SendTargetedCommand (targetOid, "/placeBag " + _cursorItem.ItemId.ToString() + " " + slot);
		ResetCursor ();
	}
		
	/// <summary>
	/// Places the cursor bag in a new slot. Used to move a bag from one slot to another.
	/// </summary>
	/// <param name="slot">Slot.</param>
	public void PlaceCursorBag (int slot)
	{
		long targetOid = ClientAPI.GetPlayerOid ();
		NetworkAPI.SendTargetedCommand (targetOid, "/moveBag " + _cursorBag.slotNum + " " + slot);
		ResetCursor ();
	}
		
	public void PlaceCursorBagAsItem (int containerId, int slotId)
	{
		long targetOid = ClientAPI.GetPlayerOid ();
		NetworkAPI.SendTargetedCommand (targetOid, "/removeBag " + _cursorBag.slotNum + " " + containerId + " " + slotId);
		ResetCursor ();
	}

	// Reset the cursor to its initial state (point)
	public void ResetCursor ()
	{
		UnityEngine.Debug.Log ("Setting cursor to point");
		// reset our state
		_cursorItem = null;
		_cursorAbility = null;
		_cursorBag = null;
		// Reset the UI cursors
		// Clear the inventory cursor
		cursorTexture = null;
	}
    
	/// Sets the cursor (currently sets the context cursor)
	// This method provides a mapping from a symbolic cursor name to the
	/// actual cursor image that will be used.

	public void SetCursor (CursorState cursorState)
	{
		this.cursorState = cursorState;
	}
	#endregion Cursor API Methods
        
	#region Mouse Control Methods
	public bool InterfaceStateOverriden() {
		if (interfaceOverrideState == InterfaceOverrideState.None)
			return false;
		else
			return true;
	}
	
	public void HandleItemUseOverride(AtavismInventoryItem item) {
		if (interfaceOverrideState == InterfaceOverrideState.Merchant) {
			string[] args = new string[1];
			args[0] = item.ItemId.ToString();
			EventSystem.DispatchEvent("SELL_ITEM", args);
		}
		if (interfaceOverrideState == InterfaceOverrideState.WorldBuilding) {
			string[] args = new string[1];
			args[0] = item.ItemId.ToString();
			EventSystem.DispatchEvent("PLACE_CLAIM_OBJECT", args);
		}
	}
	
	public void ChangeMerchantState(bool open) {
		if (open) 
			interfaceOverrideState = InterfaceOverrideState.Merchant;
		else
			interfaceOverrideState = InterfaceOverrideState.None;
	}
	
	public void MerchantItemEntered ()
	{
		SetCursor (CursorState.Loot);
		_cursorFrame = "MerchantItem";
	}
    
	public void MerchantItemLeft ()
	{
		if (_cursorFrame == "MerchantItem") {
			_cursorFrame = "";
		}
		CheckCursorFrameState ();
	}
	
	public void ChangeWorldBuilderState(bool open) {
		if (open) 
			interfaceOverrideState = InterfaceOverrideState.WorldBuilding;
		else
			interfaceOverrideState = InterfaceOverrideState.None;
	}

	public void FrameEntered (string frameName)
	{
		_cursorOverFrames.Add (frameName);
		//ClientAPI.Write("Cursor: added frame: %s to list" % frameName)
		//ClientAPI.Write("Cursor: frame list is now: %s" % _cursorOverFrames)
		CheckCursorFrameState ();
	}

	public void FrameLeft (string frameName)
	{
		//ClientAPI.Write("Cursor: removing frame: %s from list" % frameName)
		//ClientAPI.Write("Cursor: frame list is currently: %s" % _cursorOverFrames)
		_cursorOverFrames.Remove (frameName);
		//ClientAPI.Write("Cursor: frame list is now: %s" % _cursorOverFrames)
		if (_cursorFrame == "OverFrame") {
			_cursorFrame = "";
			CheckCursorFrameState ();
		}
	}

	public void CheckCursorFrameState ()
	{
		if (_cursorFrame == "") {
			if (_cursorOverFrames.Count > 0) {
				_cursorFrame = "OverFrame";
				if (_cursorItem != null) {
					//ClientAPI.Interface.SetCursor (1, None);
				} else if (_cursorAbility != null) {
					//ClientAPI.Interface.SetCursor (1, None);
				} else {
					ResetCursor ();
				}
			}
		}
	}
	
	#endregion

	#region Helper Methods
	private bool _IsMouseOverUIButton ()
	{
		if (_cursorFrame == "MerchantItem" || _cursorFrame == "OverFrame")
			return true;
		return false;
	}

	private void _UpdateCursor ()
	{
		// Update the cursor based on the _cursorItem or _cursorAbility fields.
		if (_cursorItem != null) {
			// Set the item/ability cursor
			UnityEngine.Debug.Log ("Setting cursor to item icon: " + _cursorItem.icon);
			//ClientAPI.Interface.SetCursor (0, _cursorItem.icon);
		} else if (_cursorAbility != null) {
			// Set the item/ability cursor
			//ClientAPI.Interface.SetCursor (0, _cursorAbility.icon);
		} else {
			// Clear the item/ability cursor
			//ClientAPI.Interface.SetCursor (0, None);
		}
	}
	
	/// <summary>
	/// Works out what cursor should be displayed based on the properties of the object it is over
	/// </summary>
	/// <param name="mouseOverObject">Mouse over object.</param>
	private void _UpdateContextCursor (AtavismNode mouseOverObject)
	{
		// Update the context cursor based on what object it is currently over."""
		if (ClientAPI.mouseLook)
			return;
		if (_IsMouseOverUIButton ())
			return;
		if (mouseOverObject == null) {
			SetCursor (CursorState.Default);
			return;
		} else {
			float dist = (mouseOverObject.Position - ClientAPI.GetPlayerObject ().Position).magnitude;
			if (mouseOverObject.CheckBooleanProperty ("lootable")) {
				if (dist < 4.0)
					SetCursor (CursorState.Loot);
				else
					SetCursor (CursorState.Loot_Error);
			} else if (mouseOverObject.CheckBooleanProperty ("questconcludable")) {
				//ClientAPI.Log ("questconcludable");
				if (dist < 6.0)
					SetCursor (CursorState.Speak);
				else
					SetCursor (CursorState.Speak_Error);
			} else if (mouseOverObject.CheckBooleanProperty ("questavailable")) {
				//ClientAPI.Log ("questavailable");
				if (dist < 6.0)
					SetCursor (CursorState.Speak);
				else
					SetCursor (CursorState.Speak_Error);
			} else if (mouseOverObject.CheckBooleanProperty ("questinprogress")) {
				//ClientAPI.Log ("questavailable");
				if (dist < 6.0)
					SetCursor (CursorState.Speak);
				else
					SetCursor (CursorState.Speak_Error);
			} else if (mouseOverObject.CheckBooleanProperty ("itemstosell")) {
				if (dist < 6.0)
					SetCursor (CursorState.Speak);
				else
					SetCursor (CursorState.Speak_Error);
			} else if (mouseOverObject.PropertyExists ("specialUse")) {
				string use = (string)mouseOverObject.GetProperty ("specialUse");
				if (use == "arenaMaster") {
					if (dist < 6.0)
						SetCursor (CursorState.Speak);
					else
						SetCursor (CursorState.Speak_Error);
				}
				if (use == "Intro") {
					if (dist < 6.0)
						SetCursor (CursorState.Speak);
					else
						SetCursor (CursorState.Speak_Error);
				}
			} else if (mouseOverObject.PropertyExists ("arena_portal")) {
				if (dist < 6.0)
					SetCursor (CursorState.Loot);
				else
					SetCursor (CursorState.Loot_Error);
			} else if (mouseOverObject.PropertyExists ("arena_flag")) {
				if (dist < 6.0)
					SetCursor (CursorState.Loot);
				else
					SetCursor (CursorState.Loot_Error);
			} else if (mouseOverObject.PropertyExists ("DomeWarden")) {
				if (dist < 6.0)
					SetCursor (CursorState.Speak);
				else
					SetCursor (CursorState.Speak_Error);
			} else if (mouseOverObject.PropertyExists ("dialogue_available")) {
				int use = (int)mouseOverObject.GetProperty ("dialogue_available");
				if (use > 0) {
					if (dist < 6.0)
						SetCursor (CursorState.Speak);
					else
						SetCursor (CursorState.Speak_Error);
				}
			} else if (mouseOverObject.PropertyExists ("Usable")) {
				if (dist < 6.0)
					SetCursor (CursorState.Speak);
				else
					SetCursor (CursorState.Speak_Error);
			} else if (mouseOverObject.PropertyExists ("itemavailable")) {
				if (dist < 6.0)
					SetCursor (CursorState.Loot);
				else
					SetCursor (CursorState.Loot_Error);
			} else if (mouseOverObject.CheckBooleanProperty ("attackable")) {
				if (!mouseOverObject.IsPlayer ()) {
					if (dist < 4.0)
						SetCursor (CursorState.Attack);
					else
						SetCursor (CursorState.Attack_Error);
				}
			}
		}
	}

	public void OnLeftClick (bool down, AtavismNode mouseOverObject)
	{
		if (down) {
			// ClientAPI.Write("Mouse down object = " + str(worldObj))
			// store the mouse down object
			_mouseDownObject1 = mouseOverObject;
			return;
		}
		if (mouseOverObject == null) {
			if (_cursorItem != null && !down && !UiSystem.IsMouseOverFrame ()) {
				string[] args = new string[2];
				args [0] = _cursorItem.ItemId.ToString ();
				args [1] = _cursorItem.name;
				EventSystem.DispatchEvent ("DELETE_ITEM_REQ", args);
				ResetCursor ();
			}
			return;
		}
		// ClientAPI.Write("Mouse up object = " + str(worldObj))
		if (mouseOverObject != _mouseDownObject1)
			return;
		// mouse up over the same object as the mouse down
		// that means this is a 'click' on the object
		float dist = (mouseOverObject.Position - ClientAPI.GetPlayerObject ().Position).magnitude;
		// First check if this is an arena object
		if (mouseOverObject.PropertyExists ("arena_portal")) {
			if (dist < 6.0) {
				ClientAPI.Write ("Retrieving available arenas...");
				long playerOid = ClientAPI.GetPlayerObject ().Oid;
				Dictionary<string, object> props = new Dictionary<string, object> ();
				props.Add ("playerOid", playerOid);
				props.Add ("type", (int)mouseOverObject.GetProperty ("arena_portal"));
				NetworkAPI.SendExtensionMessage (0, false, "arena.getTypes", props);
			} else {
				ClientAPI.Write ("That object is too far away (" + dist + " meters)");
			}
		} else if (mouseOverObject.PropertyExists ("itemstosell")) {
			/*if (dist < 6.0)
				MarsContainer.GetMerchantTable ((mouseOverObject.GetProperty ("itemstosell")));
			else
				ClientAPI.Write ("That object is too far away (" + dist + " meters)");*/
		} else if (mouseOverObject.PropertyExists ("arena_flag")) {
			if (dist < 6.0) {
				long playerOid = ClientAPI.GetPlayerObject ().Oid;
				Dictionary<string, object> props = new Dictionary<string, object> ();
				props.Add ("playerOid", playerOid);
				props.Add ("team", mouseOverObject.GetProperty ("arena_flag"));
				NetworkAPI.SendExtensionMessage (0, false, "arena.pickupFlag", props);
			} else {
				ClientAPI.Write ("That object is too far away (" + dist + " meters)");
			}
		} else if (mouseOverObject.PropertyExists ("specialUse")) {
			string use = (string)mouseOverObject.GetProperty ("specialUse");
			if (use == "Intro") {
				if (dist < 6.0)
					EventSystem.DispatchEvent ("SHOW_INTRO", null);
			}
		} else if (mouseOverObject.PropertyExists ("DomeWarden")) {
			int dome = (int)mouseOverObject.GetProperty ("DomeWarden");
			if (dist < 6.0) {
				long playerOid = ClientAPI.GetPlayerObject ().Oid;
				Dictionary<string, object> props = new Dictionary<string, object> ();
				props.Add ("domeID", mouseOverObject.GetProperty ("DomeWarden"));
				NetworkAPI.SendExtensionMessage (playerOid, false, "ao.DOME_ENQUIRY", props);
			}
		} else if (mouseOverObject.CheckBooleanProperty ("Usable")) {
			if (dist < 6.0) {
				long playerOid = ClientAPI.GetPlayerObject ().Oid;
				Dictionary<string, object> props = new Dictionary<string, object> ();
				props.Add ("object", mouseOverObject.Oid);
				NetworkAPI.SendExtensionMessage (playerOid, false, "ao.OBJECT_ACTIVATED", props);
			}
		}
		if (mouseOverObject.PropertyExists ("targetable")) {
			if ((bool)mouseOverObject.GetProperty ("targetable") == false || (string)mouseOverObject.GetProperty ("targetable") == "False")
				return;
		}
		ClientAPI.SetTarget (mouseOverObject.Oid);
		//MarsTarget.TargetByOID (mouseOverObject.OID);
	}

	public void OnRightClick (bool down, AtavismNode mouseOverObject)
	{
		// Handle the right click event (perhaps over an object).
		// For now, we can just always reset the cursor on a right click.
		// At some point, perhaps picking up an item or ability and right clicking
		// on an object in the world will do something, but it doesn't now.
		// Make right mouse up reset the cursor
		if (!down && !UiSystem.IsMouseOverFrame ())
			ResetCursor ();
		if (down) {
			// ClientAPI.Write("Mouse down object = " + str(objNode))
			// store the mouse down object
			_mouseDownObject2 = mouseOverObject;
			return;
		}
		if (mouseOverObject == null)
			return;
		if (mouseOverObject != _mouseDownObject2)
			return;
		if (mouseOverObject.PropertyExists ("targetable")) {
			if ((bool)mouseOverObject.GetProperty ("targetable") == true)
				ClientAPI.SetTarget (mouseOverObject.Oid);
		} else {
			ClientAPI.SetTarget (mouseOverObject.Oid);
		}
		float dist = (mouseOverObject.Position - ClientAPI.GetPlayerObject ().Position).magnitude;
		// On a right click, do the context sensitive action
		if (mouseOverObject.PropertyExists ("click_handler")) {
			ClientAPI.Write ("Invoking custom click handler for object");
			//mouseOverObject.GetProperty ("click_handler") (mouseOverObject, None);
		} else if (mouseOverObject.CheckBooleanProperty ("lootable")) {
			if (dist < 4.0)
				NetworkAPI.SendTargetedCommand (mouseOverObject.Oid, "/getLootList");
			else
				ClientAPI.Write ("That object is too far away (" + dist + " meters)");
		} else if (mouseOverObject.CheckBooleanProperty ("questconcludable")) {
			UnityEngine.Debug.Log ("questconcludable");
			//NetworkAPI.SendQuestConcludeRequestMessage (mouseOverObject.Oid);
			if (dist < 6.0)
				ClientAPI.ScriptObject.GetComponent<NpcInteraction> ().GetInteractionOptionsForNpc (mouseOverObject.Oid);
			else
				ClientAPI.Write ("That object is too far away (" + dist + " meters)");
		} else if (mouseOverObject.CheckBooleanProperty ("questavailable")) {
			UnityEngine.Debug.Log ("questavailable");
			// NetworkAPI.SendQuestInfoRequestMessage (mouseOverObject.Oid);
			if (dist < 6.0)
				ClientAPI.ScriptObject.GetComponent<NpcInteraction> ().GetInteractionOptionsForNpc (mouseOverObject.Oid);
			else
				ClientAPI.Write ("That object is too far away (" + dist + " meters)");
		} else if (mouseOverObject.CheckBooleanProperty ("questinprogress")) {
			UnityEngine.Debug.Log ("questinprogress");
			// NetworkAPI.SendQuestInfoRequestMessage (mouseOverObject.Oid);
			if (dist < 6.0)
				ClientAPI.ScriptObject.GetComponent<NpcInteraction> ().GetInteractionOptionsForNpc (mouseOverObject.Oid);
			else
				ClientAPI.Write ("That object is too far away (" + dist + " meters)");
		} else if (mouseOverObject.CheckBooleanProperty ("itemstosell")) {
			if (dist < 6.0)
				ClientAPI.ScriptObject.GetComponent<NpcInteraction> ().GetInteractionOptionsForNpc (mouseOverObject.Oid);
			else
				ClientAPI.Write ("That object is too far away (" + dist + " meters)");
		}
		else if (mouseOverObject.PropertyExists ("dialogue_available")) {
			int dialogueID = (int)mouseOverObject.GetProperty ("dialogue_available");
			if (dialogueID > 0) {
				if (dist < 6.0) {
					ClientAPI.ScriptObject.GetComponent<NpcInteraction> ().GetInteractionOptionsForNpc (mouseOverObject.Oid);
				}
			}
		} else if (mouseOverObject.CheckBooleanProperty ("attackable")) {
			if (!mouseOverObject.IsPlayer ()) {
				if (dist < 8.0) { // should be 4?
					NetworkAPI.SendAttackMessage (mouseOverObject.Oid, "strike", true);
					ClientAPI.Write ("Sent 'strike' attack for " + mouseOverObject.Oid);
				} else {
					ClientAPI.Write ("That object is too far away (" + dist + " meters)");
				}
			}
		} else if (mouseOverObject.PropertyExists ("specialUse")) {
			string use = (string)mouseOverObject.GetProperty ("specialUse");
			if (use == "arenaMaster") {
				if (dist < 6.0)
					EventSystem.DispatchEvent ("ARENA_MASTER_CLICK", null);
				else
					ClientAPI.Write ("That object is too far away (" + dist + " meters)");
			} else if (use == "Intro") {
				if (dist < 6.0)
					EventSystem.DispatchEvent ("SHOW_INTRO", null);
			}
		} else if (mouseOverObject.PropertyExists ("arena_portal")) {
			if (dist < 6.0) {
				ClientAPI.Write ("Retrieving available arenas...");
				long playerOid = ClientAPI.GetPlayerObject ().Oid;
				Dictionary<string, object> props = new Dictionary<string, object> ();
				props.Add ("playerOid", playerOid);
				props.Add ("type", (int)mouseOverObject.GetProperty ("arena_portal"));
				NetworkAPI.SendExtensionMessage (0, false, "arena.getTypes", props);
			} else {
				ClientAPI.Write ("You need to be closer to the portal to activate it");
			}
		} else if (mouseOverObject.PropertyExists ("arena_flag")) {
			if (dist < 6.0) {
				long playerOid = ClientAPI.GetPlayerObject ().Oid;
				Dictionary<string, object> props = new Dictionary<string, object> ();
				props.Add ("playerOid", playerOid);
				props.Add ("team", mouseOverObject.GetProperty ("arena_flag"));
				NetworkAPI.SendExtensionMessage (0, false, "arena.pickupFlag", props);
			} else {
				ClientAPI.Write ("That object is too far away (" + dist + " meters)");
			}
		} else if (mouseOverObject.CheckBooleanProperty ("Usable")) {
			if (dist < 6.0) {
				long playerOid = ClientAPI.GetPlayerObject ().Oid;
				Dictionary<string, object> props = new Dictionary<string, object> ();
				props.Add ("object", mouseOverObject.Oid);
				NetworkAPI.SendExtensionMessage (playerOid, false, "ao.OBJECT_ACTIVATED", props);
			}
		} else if (mouseOverObject.CheckBooleanProperty ("itemavailable")) {
			if (dist < 6.0) {
				NetworkAPI.SendTargetedCommand (mouseOverObject.Oid, "/openMob");
			}
		}
	}

	public void ClickableObjectHandler (object sender, object args)
	{
		//ObjectNode mouseOverObject = ClientAPI.worldManager.GetObjectByOID (args.Oid);
		//mouseOverObject.Targetable = true;
	}
	
	#endregion Helper Methods
}
