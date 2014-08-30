using UnityEngine;
using System.Collections;

public class ClaimObject : MonoBehaviour {

	int id;
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public int ID {
		set {
			id = value;
		}
		get {
			return id;
		}
	}
}
