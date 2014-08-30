using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class World : MonoBehaviour {
	
	static WorldManager worldManager;

	// Use this for initialization
	void Start () {
        worldManager = ClientAPI.WorldManager;

        //worldManager.ObjectAdded += _handleObjectAdded;
        //worldManager.ObjectRemoved += _handleObjectRemoved;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public ObjectNode GetObjectByOID(object oid) {
		long objOid;
        if (oid is string)
            objOid = OID.fromString((string)oid).ToLong();
        else if (oid is OID) {
			OID tempOid = (OID)oid;
            objOid = tempOid.ToLong();
		} else
            objOid = (long)oid;
        ObjectNode objNode = worldManager.GetObjectNode(objOid);
        if (objNode != null)
            return objNode;
        return null;
	}
        
    public ObjectNode GetObjectByName(string name) {
        ObjectNode objNode = worldManager.GetObjectNode(name);
        if (objNode != null)
            return objNode;
        return null;
	}
	
	#region Properties
	public List<string> WorldObjectNames {
		get {
        	return worldManager.GetObjectNodeNames();
		}
	}
        
    public List<long> WorldObjectOIDs {
		get {
        	return worldManager.GetObjectOidList();
		}
	}
	#endregion Properties
}
