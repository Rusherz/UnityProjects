using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Furniture {

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

	Action<Furniture> OnInstalledObjectChanged;

	protected Furniture(){
	}

	static public Furniture CreateProto(string ObjectType, float MovementCost=1, int width=1, int height=1, bool LinksToNeighbour=false){
		Furniture obj = new Furniture ();
		obj.objectType = ObjectType;
		obj.MovementCost = MovementCost;
		obj.width = width;
		obj.height = height;
		obj.linksToNeighbour = LinksToNeighbour;
		return obj;
	}

	static public Furniture PlaceObject( Furniture proto, Tile tile){
		Furniture obj = new Furniture ();

		obj.objectType = proto.objectType;
		obj.MovementCost = proto.MovementCost;
		obj.width = proto.width;
		obj.height = proto.height;
		obj.tile = tile;
		obj.linksToNeighbour = proto.linksToNeighbour;
		if (!tile.InstallTileObject (obj)) {
			return null;
		}
		Tile t;
		int x = tile.X;
		int y = tile.Y;

		if (obj.linksToNeighbour) {
			t = tile.world.getTileAt (x, y + 1);
			if (t != null && t.installedObject != null && t.installedObject.objectType == obj.objectType) {
				t.installedObject.OnInstalledObjectChanged (t.installedObject);
			}
			t = tile.world.getTileAt (x + 1, y);
			if (t != null && t.installedObject != null && t.installedObject.objectType == obj.objectType) {
				t.installedObject.OnInstalledObjectChanged (t.installedObject);
			}
			t = tile.world.getTileAt (x, y - 1);
			if (t != null && t.installedObject != null && t.installedObject.objectType == obj.objectType) {
				t.installedObject.OnInstalledObjectChanged (t.installedObject);
			}
			t = tile.world.getTileAt (x - 1, y);
			if (t != null && t.installedObject != null && t.installedObject.objectType == obj.objectType) {
				t.installedObject.OnInstalledObjectChanged (t.installedObject);
			}
		}

		return obj;
	}

	public void RegisterOnInstalledObjectChanged(Action<Furniture> callbackfunc){
		OnInstalledObjectChanged += callbackfunc;
	}

	public void UnregisterOnInstalledObjectChanged(Action<Furniture> callbackfunc){
		OnInstalledObjectChanged -= callbackfunc;
	}

}