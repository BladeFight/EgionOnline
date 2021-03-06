using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class ClientAPI : MonoBehaviour
{
	static ClientAPI instance;
	
	public GameObject defaultObject;
	public string mobTag;
	public LayerMask playerLayer;
	public string playerTag;
	public GameObject scriptObject;
	public AtavismResourceManager resourceManager;
	
	public string masterServer = "";
	public string WorldId = "atavism_demo";
	public bool webPlayer = false;
	
	public static World World;
	public static ObjectNode mouseOverTarget;
	public static bool mouseLook = false;
	
	void Start() {
		if (instance == null) {
			instance = this;
		} else {
			GameObject.DestroyImmediate(gameObject);
			return;
		}
		Client client = gameObject.AddComponent<Client>();
		client.Initalise(scriptObject, defaultObject, WorldId, webPlayer, playerLayer,
			playerTag, mobTag);
		client.DefaultMasterServer = masterServer;
		client.MasterServer = masterServer;
		client.DefaultWorldId = WorldId;
		if (resourceManager != null) {
			client.resourceManager = resourceManager;
		}
		ScriptObject.BroadcastMessage("ClientReady");
	}
	
	public static void Write (string message)
	{
		string[] eventArgs = new string[1];
		eventArgs [0] = message;
		EventSystem.DispatchEvent ("CHAT_MSG_SYSTEM", eventArgs);
	}
	
	public static Player GetPlayerObject ()
	{
		return Client.Instance.WorldManager.Player;
	}
	
	public static long GetPlayerOid ()
	{
		return Client.Instance.WorldManager.PlayerId;
	}
	
	public static MobNode GetTargetObject ()
	{
		return Client.Instance.WorldManager.Target;
	}
	
	public static long GetTargetOid ()
	{
		return Client.Instance.WorldManager.TargetId;
	}
	
	public static void SetTarget(long oid) {
		WorldManager.TargetId = oid;
	}
	
	public static void ClearTarget() {
		WorldManager.TargetId = -1;
	}

	public static ObjectNode GetObjectNode(long oid) {
		if (WorldManager.GetObjectNode(oid) != null)
			return WorldManager.GetObjectNode(oid);
		else
			return null;
	}
	
	public static object GetObjectProperty(long oid, string propName) {
		if (WorldManager.GetObjectNode(oid) != null)
			return WorldManager.GetObjectNode(oid).GetProperty(propName);
		else
			return null;
	}
	
	public static void InputControllerActivated(AtavismInputController inputController) {
		Client.Instance.WorldManager.InputControllerActivated(inputController);
	}
	
	public static AtavismInputController GetInputController() {
		return Client.Instance.ActiveInputController;
		//return (AtavismInputController)Client.Instance.WorldManager.Player.GameObject.GetComponent(Client.Instance.InputController);
	}
	
	public static List<CharacterEntry> GetCharacterEntries() {
		return Client.Instance.CharacterEntries;
	}
	
	public static void RegisterEventHandler (string eventName, System.EventHandler eventHandler)
	{
		/*if (eventName in _newWorldEvents) {
        _deprecated("1.1", "ClientAPI.RegisterEventHandler('" + eventName + "')", "ClientAPI.World.RegisterEventHandler('" + eventName + "')");
        World.RegisterEventHandler(eventName, eventHandler);
		}
    else*/ 
		if (eventName == "WorldConnect")
			AtavismAPI.WorldConnect += eventHandler;
		else if (eventName == "WorldDisconnect")
			AtavismAPI.WorldDisconnect += eventHandler;
		else if (eventName == "FrameStarted")
			AtavismAPI.FrameStarted += new FrameStartedHandler(eventHandler);
		else if (eventName == "FrameEnded")
			AtavismAPI.FrameEnded += new FrameEndedHandler(eventHandler);
		else if (eventName == "CameraWaterEvent")
			AtavismAPI.CameraWaterEvent += new WaterEventHandler(eventHandler);
		else if (eventName == "ZoneEvent") {
			AtavismAPI.ZoneEvent += new ZoneEventHandler(eventHandler);
		} else if (eventName == "PlayerInitialized")
			WorldManager.PlayerInitializedEvent += new PlayerInitializedHandler(eventHandler);
		//else
		//	ClientAPI.LogError ("Invalid event name '%s' passed to ClientAPI.RegisterEventHandler" % str (eventName));
	}
	
	public static void SetBlockIntercept(int x, int y, int z, int type) {
		Dictionary<string, object> props = new Dictionary<string, object> ();
		props.Add("voxelandID", 1);
		props.Add("blockx", x);
		props.Add("blocky", y);
		props.Add("blockz", z);
		props.Add("type", type);
		NetworkAPI.SendExtensionMessage (ClientAPI.GetPlayerOid (), false, "ao.SET_BLOCK", props);
		GetPlayerObject ().MobController.PlayAnimation("mining", 1.5f);
	}
	
	public static WorldManager WorldManager {
		get {
			return Client.Instance.WorldManager;
		}
	}
	
	public static GameObject ScriptObject {
		get {
			return instance.scriptObject;
		}
	}
}
