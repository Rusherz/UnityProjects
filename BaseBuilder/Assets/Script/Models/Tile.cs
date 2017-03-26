using UnityEngine;
using System.Collections;
using System;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Schema;

public enum TileType { Empty, Floor };
public enum Enterability { Yes, Never, Soon };

public class Tile : IXmlSerializable{
	private TileType _type = TileType.Empty;
	public TileType Type {
		get { return _type; }
		set {
			TileType oldType = _type;
			_type = value;
			if(cbTileChanged != null && oldType != _type)
				cbTileChanged(this);
		}
	}

	public Room room;

	public Furniture furniture {
		get; protected set;
	}

	public float movementCost {
		get{
			if (Type == TileType.Empty) {
				return 0;
			}
			if (furniture == null) {
				return 1;
			}

			return 1 * furniture.movementCost;
		}
	}

	public Job JobPending;

	public Inventory inventory { get; set;}

	public World world { get; protected set; }

	public int X { get; protected set; }
	public int Y { get; protected set; }

	Action<Tile> cbTileChanged;

	public Tile( World world, int x, int y ) {
		this.world = world;
		this.X = x;
		this.Y = y;
	}

	public void RegisterTileTypeChangedCallback(Action<Tile> callback) {
		cbTileChanged += callback;
	}
	
	public void UnregisterTileTypeChangedCallback(Action<Tile> callback) {
		cbTileChanged -= callback;
	}

	public bool UnplaceFurniture(){

		if (furniture == null) {
			return false;
		}

		Furniture f = furniture;

		for (int x_off = X; x_off < (X + f.width); x_off++) {
			for (int y_off = Y; y_off < (Y + f.height); y_off++) {
				Tile t = world.GetTileAt (x_off, y_off);
				t.furniture = null;
			}
		}
		return true;
	}

	public bool PlaceFurniture(Furniture objInstance) {

		if(objInstance == null) {
			return UnplaceFurniture ();
		}

		if (objInstance != null && !objInstance.IsValidPosition (this)) {
			Debug.LogError("Trying to assign a furniture to a tile that isnt valid!");
			return false;
		}

		for (int x_off = X; x_off < (X + objInstance.width); x_off++) {
			for (int y_off = Y; y_off < (Y + objInstance.height); y_off++) {
				Tile t = world.GetTileAt (x_off, y_off);
				t.furniture = objInstance;
			}
		}
		return true;
	}

	public bool PlaceInvemtory(Inventory inv) {
		if(inv == null) {
			inventory = null;
			return true;
		}

		if(inventory != null) {

			if (inventory.objectType != inv.objectType) {
				return false;
			}

			int numToMove = inv.stackSize;

			if (inventory.stackSize + numToMove > inventory.maxStackSize) {
				numToMove = inventory.maxStackSize - inventory.stackSize;
			}
			inventory.stackSize += numToMove;
			inv.stackSize -= numToMove;
			return true;
		}
		inventory = inv.Clone ();
		inventory.tile = this;
		inv.stackSize = 0;
		return true;
	}

	public bool IsNeighbour(Tile t, bool diagOkay = false){
		return 
			Mathf.Abs(this.X - t.X) + Mathf.Abs(this.Y - t.Y) == 1 
				||
			(diagOkay && (Mathf.Abs(this.X - t.X) == 1 && Mathf.Abs(this.Y - t.Y) == 1));
	}

	public Tile[] GetNeighbours(bool diagOaky = false){
		Tile[] ns;

		if (!diagOaky) {
			ns = new Tile[4];// Tile Order: N E S W
		} else {
			ns = new Tile[8];// Tile Order: N E S W NE SE SW NW
		}

		ns[0] = world.GetTileAt (X, Y + 1);
		ns[1] = world.GetTileAt (X + 1, Y);
		ns[2] = world.GetTileAt (X, Y - 1);
		ns[3] = world.GetTileAt (X - 1, Y);

		if (diagOaky) {
			ns[4] = world.GetTileAt (X + 1, Y + 1);
			ns[5] = world.GetTileAt (X + 1, Y - 1);
			ns[6] = world.GetTileAt (X - 1, Y - 1);
			ns[7] = world.GetTileAt (X - 1, Y + 1);
		}

		return ns;

	}

	public Enterability IsEnterable(){
		if (movementCost == 0) {
			return Enterability.Never;
		}
		if (furniture != null && furniture.IsEnterable != null) {
			return furniture.IsEnterable (furniture);
		}

		return Enterability.Yes;
	}

	public Tile North(){
		return world.GetTileAt (X, Y + 1);
	}
	public Tile East(){
		return world.GetTileAt (X + 1, Y);
	}
	public Tile South(){
		return world.GetTileAt (X, Y - 1);
	}
	public Tile West(){
		return world.GetTileAt (X + 1, Y);
	}

	public XmlSchema GetSchema(){
		return null;
	}

	public void WriteXml(XmlWriter writer){
		writer.WriteAttributeString ("X", X.ToString ());
		writer.WriteAttributeString ("Y", Y.ToString ());
		writer.WriteAttributeString ("Type", ((int)Type).ToString());
	}

	public void ReadXml(XmlReader reader){
		Type = (TileType)int.Parse(reader.GetAttribute("Type"));
	}

}