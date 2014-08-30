using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum AttachmentSockets {
	Root,
	LeftFoot,
	RightFoot,
	Pelvis,
	LeftHip,
	RightHip,
	MainHand,
	OffHand,
	Chest,
	Back,
	LeftShoulder,
	RightShoulder,
	Head,
	Mouth,
	LeftEye,
	RightEye,
	Overhead
}

public class AtavismMobAppearance : MonoBehaviour {
	
	GameObject legs;
	GameObject chest;
	GameObject hands;
	GameObject feet;
	
	// Sockets for attaching weapons (and particles)
	public Transform mainHand;
	public Transform offHand;
	public Transform mainHandRest;
	public Transform offHandRest;
	public Transform head;
	public Transform leftShoulderSocket;
	public Transform rightShoulderSocket;
	
	// Sockets for particles
	public Transform rootSocket;
	public Transform leftFootSocket;
	public Transform rightFootSocket;
	public Transform pelvisSocket;
	public Transform leftHipSocket;
	public Transform rightHipSocket;
	public Transform chestSocket;
	public Transform backSocket;
	public Transform mouthSocket;
	public Transform leftEyeSocket;
	public Transform rightEyeSocket;
	public Transform overheadSocket;
	
	
	Dictionary<Transform, GameObject> attachedItems = new Dictionary<Transform, GameObject>();

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	}
	
	public Transform GetSocketTransform(AttachmentSockets slot) {
		switch (slot) {
		case AttachmentSockets.MainHand:
			return mainHand;
			break;
		case AttachmentSockets.OffHand:
			return offHand;
			break;
		case AttachmentSockets.Head:
			return head;
			break;
		case AttachmentSockets.LeftShoulder:
			return leftShoulderSocket;
			break;
		case AttachmentSockets.RightShoulder:
			return rightShoulderSocket;
			break;
		case AttachmentSockets.Root:
			return transform;
			break;
		case AttachmentSockets.LeftFoot:
			return leftFootSocket;
			break;
		case AttachmentSockets.RightFoot:
			return rightFootSocket;
			break;
		case AttachmentSockets.Pelvis:
			return pelvisSocket;
			break;
		case AttachmentSockets.LeftHip:
			return leftHipSocket;
			break;
		case AttachmentSockets.RightHip:
			return rightHipSocket;
			break;
		case AttachmentSockets.Chest:
			return chestSocket;
			break;
		case AttachmentSockets.Back:
			return backSocket;
			break;
		case AttachmentSockets.Mouth:
			return mouthSocket;
			break;
		case AttachmentSockets.LeftEye:
			return leftEyeSocket;
			break;
		case AttachmentSockets.RightEye:
			return rightEyeSocket;
			break;
		case AttachmentSockets.Overhead:
			return overheadSocket;
			break;
		}
		return null;
	}
	
	void OnDestroy() {
		if (GetComponent<AtavismNode>()) {
			GetComponent<AtavismNode> ().RemoveObjectPropertyChangeHandler("weaponDisplayID", WeaponDisplayHandler);
			GetComponent<AtavismNode> ().RemoveObjectPropertyChangeHandler("weapon2DisplayID", Weapon2DisplayHandler);
			GetComponent<AtavismNode> ().RemoveObjectPropertyChangeHandler("legDisplayID", LegsDisplayHandler);
		}
	}
	
	void ObjectNodeReady () {
		GetComponent<AtavismNode> ().RegisterObjectPropertyChangeHandler ("legDisplayID", LegsDisplayHandler);
		GetComponent<AtavismNode> ().RegisterObjectPropertyChangeHandler ("weaponDisplayID", WeaponDisplayHandler);
		GetComponent<AtavismNode> ().RegisterObjectPropertyChangeHandler ("weapon2DisplayID", Weapon2DisplayHandler);
		//Debug.LogWarning("Registered display properties for: " + name);
	}
	
	public void WeaponDisplayHandler(object sender, PropertyChangeEventArgs args) {
		UnityEngine.Debug.Log("Got weapon display ID");
		ObjectNode node = (ObjectNode)sender;
		string displayID = (string)GetComponent<AtavismNode> ().GetProperty (args.PropertyName);
		// Remove existing item
		if (attachedItems.ContainsKey(mainHand)) {
			Destroy(attachedItems[mainHand]);
			attachedItems.Remove(mainHand);
		}
		if (displayID != null && displayID != "") {
			EquipmentDisplay display = ClientAPI.ScriptObject.GetComponent<Inventory>().LoadEquipmentDisplay(displayID);
			GameObject weapon = (GameObject) Instantiate(display.model, mainHand.position, mainHand.rotation);
			weapon.transform.parent = mainHand;
			attachedItems.Add(mainHand, weapon);
		}
	}
	
	public void Weapon2DisplayHandler(object sender, PropertyChangeEventArgs args) {
		UnityEngine.Debug.Log("Got weapon 2 display ID");
		ObjectNode node = (ObjectNode)sender;
		string displayID = (string)GetComponent<AtavismNode> ().GetProperty (args.PropertyName);
		// Remove existing item
		if (attachedItems.ContainsKey(offHand)) {
			Destroy(attachedItems[offHand]);
			attachedItems.Remove(offHand);
		}
		if (displayID != null && displayID != "") {
			EquipmentDisplay display = ClientAPI.ScriptObject.GetComponent<Inventory>().LoadEquipmentDisplay(displayID);
			GameObject weapon2 = (GameObject) Instantiate(display.model, offHand.position, offHand.rotation);
			weapon2.transform.parent = offHand;
			attachedItems.Add(offHand, weapon2);
		}
	}
	
	public void ChestDisplayHandler(object sender, PropertyChangeEventArgs args) {
		UnityEngine.Debug.Log("Got chest display ID");
		ObjectNode node = (ObjectNode)sender;
		string chestDisplayID = (string)GetComponent<AtavismNode> ().GetProperty (args.PropertyName);
		EquipmentDisplay display = ClientAPI.ScriptObject.GetComponent<Inventory>().LoadEquipmentDisplay(chestDisplayID);
		//Material mat = chest.GetComponent<SkinnedMeshRenderer>().material;
		//mat.SetTexture("_EquipmentTex", display.texture);
	}
	
	public void HandsDisplayHandler(object sender, PropertyChangeEventArgs args) {
		UnityEngine.Debug.Log("Got hands display ID");
		ObjectNode node = (ObjectNode)sender;
		string handDisplayID = (string)GetComponent<AtavismNode> ().GetProperty (args.PropertyName);
		EquipmentDisplay display = ClientAPI.ScriptObject.GetComponent<Inventory>().LoadEquipmentDisplay(handDisplayID);
		//Material mat = hands.GetComponent<SkinnedMeshRenderer>().material;
		//mat.SetTexture("_EquipmentTex", display.texture);
	}
	
	public void LegsDisplayHandler(object sender, PropertyChangeEventArgs args) {
		UnityEngine.Debug.Log("Got leg display ID");
		ObjectNode node = (ObjectNode)sender;
		string legsDisplayID = (string)GetComponent<AtavismNode> ().GetProperty (args.PropertyName);
		EquipmentDisplay display = ClientAPI.ScriptObject.GetComponent<Inventory>().LoadEquipmentDisplay(legsDisplayID);
		Material mat = legs.GetComponent<SkinnedMeshRenderer>().material;
		//legs.GetComponent<SkinnedMeshRenderer>().materials[1] = display.material;
		//mat.SetTexture("_EquipmentTex", display.texture);
	}
	
	public void FeetDisplayHandler(object sender, PropertyChangeEventArgs args) {
		UnityEngine.Debug.Log("Got feet display ID");
		ObjectNode node = (ObjectNode)sender;
		string feetDisplayID = (string)GetComponent<AtavismNode> ().GetProperty (args.PropertyName);
		EquipmentDisplay display = ClientAPI.ScriptObject.GetComponent<Inventory>().LoadEquipmentDisplay(feetDisplayID);
		//Material mat = feet.GetComponent<SkinnedMeshRenderer>().material;
		//mat.SetTexture("_EquipmentTex", display.texture);
	}
}
