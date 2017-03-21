using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Tile {

	Action<Tile> TileTypeChanged;

	public enum TileType { Empty, Floor};

	TileType type = TileType.Empty;
	public TileType Type {
		get {
			return type;
		}
		set {
			TileType oldType = type;
			type = value;
			if (TileTypeChanged != null && oldType != type) {
				TileTypeChanged (this);
			}
		}
	}

	Inventory inventory;
	public Furniture installedObject { get; protected set; }

	public World world{ get; protected set; }

	int x;

	public int X {
		get {
			return x;
		}
	}

	int y;

	public int Y {
		get {
			return y;
		}
	}

	public Tile(World world, int x, int y){
		this.world = world;
		this.x = x;
		this.y = y;
	}

	public void RegTileTypeChanged (Action<Tile> callback){
		TileTypeChanged += callback;
	}

	public void UnregTileTypeChanged (Action<Tile> callback){
		TileTypeChanged -= callback;
	}

	public bool InstallTileObject(Furniture ObjectInstance){

		if (ObjectInstance == null) {
			installedObject = null;
			return true;
		}

		if (installedObject != null) {
			//Debug.LogError ("Tile already has an object");
			return false;
		} else {
			installedObject = ObjectInstance;
			return true;
		}

	}

}
