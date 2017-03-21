using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstalledObject {

	// Location of object
	public Tile tile{ get; protected set; }

	// Used for selecting image of object
	public string objectType{ get; protected set;	}

	// Movement if different for object
	float MovementCost = 1f;

	// Incase object is bigger then one tile
	int width = 1;
	int height = 1;

	public bool linksToNeighbour { get; protected set; }

	Action<InstalledObject> OnInstalledObjectChanged;

	protected InstalledObject(){
	}

	static public InstalledObject CreateProto(string ObjectType, float MovementCost=1, int width=1, int height=1, bool LinksToNeighbour=false){
		InstalledObject obj = new InstalledObject ();
		obj.objectType = ObjectType;
		obj.MovementCost = MovementCost;
		obj.width = width;
		obj.height = height;
		obj.linksToNeighbour = LinksToNeighbour;
		return obj;
	}

	static public InstalledObject PlaceObject( InstalledObject proto, Tile tile){
		InstalledObject obj = new InstalledObject ();

		obj.objectType = proto.objectType;
		obj.MovementCost = proto.MovementCost;
		obj.width = proto.width;
		obj.height = proto.height;
		obj.tile = tile;
		obj.linksToNeighbour = proto.linksToNeighbour;
		if (!tile.InstallTileObject (obj)) {
			return null;
		}

		return obj;
	}

	public void RegisterOnInstalledObjectChanged(Action<InstalledObject> callbackfunc){
		OnInstalledObjectChanged += callbackfunc;
	}

	public void UnregisterOnInstalledObjectChanged(Action<InstalledObject> callbackfunc){
		OnInstalledObjectChanged -= callbackfunc;
	}

}