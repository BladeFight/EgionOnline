using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InstancePortal : MonoBehaviour {
	public enum Trigger {
		Collide,
		Click
	}

	public Trigger trigger;
	public string worldName;

	float activeTime;

	void Start () { activeTime = Time.time; }

	void OnTriggerEnter (Collider other) {
		if (other.gameObject == ClientAPI.GetPlayerObject().GameObject) {
			EnterInstance();
		}
	}

	void OnClick () { 
		if (trigger == Trigger.Click) {
			EnterInstance();
		}
	}
	
	void EnterInstance() {
		if (Time.time > activeTime) {
			long targetOid = ClientAPI.GetPlayerObject ().Oid;
			NetworkAPI.SendTargetedCommand (targetOid, "/changeInstance " + worldName);
			activeTime = Time.time + 2;
		}
	}
}
