using UnityEngine;
using System.Collections;

public class ClaimScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
		string[] args = new string[1];
		args[0] = gameObject.name;
		EventSystem.DispatchEvent("CLAIM_ADDED", args);
	}
	
	void ObjectNodeReady () {
		GetComponent<AtavismNode>().RegisterObjectPropertyChangeHandler("scale", ScaleHandler);
		if (GetComponent<AtavismNode>().PropertyExists("scale")) {
			UnityEngine.Debug.Log("Got scale");
			Vector3 scaleObj = (Vector3) GetComponent<AtavismNode>().GetProperty("scale");
			gameObject.transform.localScale = scaleObj;
		}
	}

	void OnDestroy() {
		string[] args = new string[1];
		args[0] = gameObject.name;
		EventSystem.DispatchEvent("CLAIMED_REMOVED", args);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public void ScaleHandler(object sender, PropertyChangeEventArgs args) {
		UnityEngine.Debug.Log("Got scale");
		Vector3 scaleObj = (Vector3) GetComponent<AtavismNode>().GetProperty("scale");
		gameObject.transform.localScale = scaleObj;
	}
}
