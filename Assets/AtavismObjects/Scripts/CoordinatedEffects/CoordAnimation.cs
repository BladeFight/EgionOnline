using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CoordAnimation : CoordinatedEffect {
	
	public string animationName;
	public float animationLength;
	public AudioClip soundClip;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (activationTime != 0 && Time.time > activationTime) {
			Run();
		}
	}
	
	public override void Execute(Dictionary<string, object> props) {
		base.props = props;
		Debug.Log("Executing CoordAnimationEffect with num props: " + props.Count);
		foreach (string prop in props.Keys) {
			Debug.Log(prop + ":" + props[prop]);
		}
		
		if (activationDelay == 0) {
			Run ();
		} else {
			activationTime = Time.time + activationDelay;
		}
	}
	
	public void Run() {
		ObjectNode node;
		if (target == CoordinatedEffectTarget.Caster) {
        	node = ClientAPI.WorldManager.GetObjectNode((OID)props["sourceOID"]);
        } else {
			node = ClientAPI.WorldManager.GetObjectNode((OID)props["targetOID"]);
        }
		
		// Play attack animation
		node.GameObject.GetComponent<AtavismMobController>().PlayAnimation(animationName, animationLength);
		
		if (soundClip != null) {
			// Play sound clip on caster
			Transform slotTransform = node.GameObject.GetComponent<AtavismMobAppearance>().GetSocketTransform(AttachmentSockets.Root);
			GameObject soundObject = new GameObject();
			soundObject.transform.position = slotTransform.position;
			soundObject.transform.parent = slotTransform;
			AudioSource audioSource = soundObject.AddComponent<AudioSource>();
			audioSource.clip = soundClip;
			audioSource.Play();
			Destroy(soundObject, duration);
		}
		
		// Now destroy this object
		if (destroyWhenFinished)
			Destroy(gameObject, duration);
	}
}
