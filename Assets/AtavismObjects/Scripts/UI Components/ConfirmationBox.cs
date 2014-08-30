using UnityEngine;
using System.Collections;

public class ConfirmationBox : AtavismWindowTemplate {
	
	string confirmationType;
	string confirmationMessage;
	object confirmationObject;

	// Use this for initialization
	void Start () {
		SetupRect();
	
		// Register for 
		EventSystem.RegisterEvent("DELETE_ITEM_REQ", this);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void OnDestroy () {
		EventSystem.UnregisterEvent("DELETE_ITEM_REQ", this);
	}
	
	void OnGUI() {
		if (confirmationMessage != null && confirmationMessage != "") {
			GUI.depth = uiLayer;
			GUI.skin = skin;
			GUI.Box(uiRect, "");
			GUI.Label(new Rect(uiRect.x + 5, uiRect.y + 5, uiRect.width-10, 40), confirmationMessage);
			if (GUI.Button(new Rect(uiRect.x + 5, uiRect.yMax - 25, 35, 20), "Yes")) {
				long targetOid = ClientAPI.GetPlayerObject ().Oid;
				NetworkAPI.SendTargetedCommand(targetOid, "/deleteItemStack " + confirmationObject);
				confirmationMessage = "";
				confirmationType = "";
				confirmationObject = null;
			} else if (GUI.Button(new Rect(uiRect.xMax - 35, uiRect.yMax - 25, 30, 20), "No")) {
				confirmationMessage = "";
				confirmationType = "";
				confirmationObject = null;
			}
		}
	}
	
	public void OnEvent(EventData eData) {
		if (eData.eventType == "DELETE_ITEM_REQ") {
			confirmationObject = eData.eventArgs[0];
			confirmationMessage = "Delete " + eData.eventArgs[1] + "?";
			confirmationType = "delete_item";
		}
	}
}

