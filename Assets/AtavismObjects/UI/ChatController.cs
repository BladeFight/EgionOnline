using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class ChatController : AtavismWindowTemplate {
	
	private Vector2 scrollPosition;
	
	private List<String> messages = new List<String>();
	
	private string userMessage = "";
	
	private bool typingMessage = false;
	private bool sendingMessage = false;
	private bool focusChanged = true;
		
	void Start() {
		SetupRect();
		ToggleOpen();
		
		//chatWindow = new Rect(Screen.width - 310, Screen.height - 160, 300, 160);
		// Register for 
		EventSystem.RegisterEvent("CHAT_MSG_SERVER", this);
		EventSystem.RegisterEvent("CHAT_MSG_SAY", this);
		EventSystem.RegisterEvent("CHAT_MSG_SYSTEM", this);
	}
	
	void OnDestroy () {
		EventSystem.UnregisterEvent("CHAT_MSG_SERVER", this);
		EventSystem.UnregisterEvent("CHAT_MSG_SAY", this);
		EventSystem.UnregisterEvent("CHAT_MSG_SYSTEM", this);
	}

	void OnGUI() {
		if (!open)
			return;
	
		GUI.depth = uiLayer;
		GUI.skin = skin;
		
		if (EnterPressed()) {
			if (!typingMessage) {
				 typingMessage = true;
			}
			else {
				// Send message
	   	    	if (userMessage.Length > 0) {
					AddMyChatMessage(userMessage);
					userMessage = "";
				}
				typingMessage = false;
			}
			Event.current.Use();	 
		}
		ShowChatWindow(1);
		//chatWindow = GUI.Window (1, chatWindow, ShowChatWindow, "");
	}
	
	private bool EnterPressed() {
		return (Event.current.type == EventType.keyDown && Event.current.character == '\n');
	}
	
	void ShowChatWindow(int id) {	
		GUILayout.BeginArea(uiRect, skin.GetStyle("Box"));
		//GUILayout.BeginArea(chatWindow, skin.GetStyle("chatbox"));
		GUI.depth = 20;
		GUI.SetNextControlName("scroll"); 
		scrollPosition = GUILayout.BeginScrollView (scrollPosition);
		foreach(string message in messages) {
			GUILayout.BeginHorizontal();
			GUILayout.Label(message);
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			GUILayout.Space(3);
		}
				
	    GUILayout.EndScrollView();
		if (!typingMessage && !focusChanged) {
			GUI.FocusControl("");
			focusChanged = true;
	   		 //GUI.FocusControl("scroll");
	   	}
	   	
	   	GUI.SetNextControlName("text"); 
	   	userMessage = GUILayout.TextField(userMessage);
		if (typingMessage) {
	   		 GUI.FocusControl("text");
			focusChanged = false;
		}
		
//		GUILayout.Label("PEnter to type/send");
		GUILayout.EndArea();
	}
	
	private void AddMyChatMessage(String message) {
		//AddChatMessage(message);
		SendChatMessage(message);
	}
	
	// This method to be called when remote chat message is received
	void AddChatMessage(String message) {
		messages.Add(message);
		scrollPosition.y = 10000000000; // To scroll down the messages window
	}
		
	// Send the chat message to all other users
	private void SendChatMessage(String message) {
		AtavismCommand.HandleCommand(message);
	}
	
	public void OnEvent(EventData eData) {
		if (eData.eventType == "CHAT_MSG_SERVER") {
			AddChatMessage(eData.eventArgs[0]);
		} else if (eData.eventType == "CHAT_MSG_SAY") {
			Debug.Log("Got chat say event with numargs: " + eData.eventArgs.Length);
			AddChatMessage("[" + eData.eventArgs[1] + "]: " + eData.eventArgs[0]);
		} else if (eData.eventType == "CHAT_MSG_SYSTEM") {
			Debug.Log("Got system event with numargs: " + eData.eventArgs.Length);
			AddChatMessage("[system]: " + eData.eventArgs[0]);
		}
	}
}
