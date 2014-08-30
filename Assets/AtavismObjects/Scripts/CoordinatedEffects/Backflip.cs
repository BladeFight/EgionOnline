using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BackflipEffect : CoordinatedEffect {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public override void Execute(Dictionary<string, object> props) {
		Debug.Log("Executing BackflipEffect with num props: " + props.Count);
		foreach (string prop in props.Keys) {
			Debug.Log(prop + ":" + props[prop]);
		}
		
		ObjectNode target = ClientAPI.WorldManager.GetObjectNode((OID)props["targetOID"]);
        ObjectNode caster = ClientAPI.WorldManager.GetObjectNode((OID)props["sourceOID"]);
	}
}
