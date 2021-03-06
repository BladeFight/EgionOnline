﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AtavismParticles : MonoBehaviour {
	
	public GameObject lootParticle;
	Dictionary<long, GameObject> attachedLootParticles = new Dictionary<long, GameObject>();

	// Use this for initialization
	void Start () {
		
	}
	
	void ClientReady() {
		ClientAPI.WorldManager.RegisterObjectPropertyChangeHandler("lootable", LootParticlesHandler);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public void LootParticlesHandler(object sender, ObjectPropertyChangeEventArgs args) {
    	ObjectNode worldObj = ClientAPI.WorldManager.GetObjectNode(args.Oid);
    	if (worldObj == null) {
        	Debug.Log("Loot Particles: found no object");
        	return;
		}
    	if (worldObj.CheckBooleanProperty("lootable")) {
        	GameObject newLootParticle = (GameObject)GameObject.Instantiate(lootParticle, worldObj.GameObject.transform.position,
				Quaternion.identity);
			newLootParticle.transform.parent = worldObj.GameObject.transform;
			attachedLootParticles.Add(args.Oid, newLootParticle);
		} else if (attachedLootParticles.ContainsKey(args.Oid)){
            Destroy(attachedLootParticles[args.Oid]);
			attachedLootParticles.Remove(args.Oid);
		}
	}
}
