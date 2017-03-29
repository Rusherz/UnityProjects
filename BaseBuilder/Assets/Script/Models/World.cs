using UnityEngine;
using System.Collections.Generic;
using System;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Schema;

public class World : IXmlSerializable{

	// A two-dimensional array to hold our tile data.
	public Tile[,] tiles { get; protected set; }

	public List<Character> characters;
	public List<Furniture> furniture;
	public InventoryManager inventoryManager;
	public List<Room> rooms;

	public Path_TileGraph tileGraph;

	public Dictionary<string, Furniture> furniturePrototypes;
	public Dictionary<string, Job> furnitureJobPrototypes;

	public int Width { get; protected set; }
	public int Height { get; protected set; }

	static public World currentWorld { get; protected set; }

	Action<Furniture> cbFurnitureCreated;
	Action<Character> cbCharacterCreated;
	Action<Inventory> cbInventoryCreated;
	Action<Tile> cbTileChanged;

	public JobQueue jobQueue;

	public World(int width, int height) {
		SetUpWorld (width, height);

        CreateCharacter(GetTileAt(Width / 2, Height / 2));
        CreateCharacter(GetTileAt(Width / 2 - 1, Height / 2 - 1));
        CreateCharacter(GetTileAt(Width / 2 + 1, Height / 2 + 1));
    }

	public Room GetOutSideRoom(){
		return rooms [0];
	}

    public int GetRoomID(Room r) {
        return rooms.IndexOf(r);
    }

    public Room GetRoomFromID(int i) {
        if(i < 0 || i > rooms.Count + 1) {
            return null;
        }
        return rooms[i];
    }

	public void AddRoom(Room r){
		rooms.Add (r);
	}

	public void DeleteRoom(Room r){

		if (r == GetOutSideRoom ()) {
			Debug.LogError ("Tried to delete the outside room");
			return;
		}
		rooms.Remove (r);

		r.UnAssignAllTiles ();
	}

	void SetUpWorld(int width, int height){
		currentWorld = this;

		jobQueue = new JobQueue();

		Width = width;
		Height = height;

		tiles = new Tile[Width,Height];

		rooms = new List<Room> ();
		rooms.Add (new Room ());

		for (int x = 0; x < Width; x++) {
			for (int y = 0; y < Height; y++) {
				tiles[x,y] = new Tile(x, y);
				tiles [x, y].RegisterTileTypeChangedCallback (OnTileChanged);
				tiles [x, y].room = GetOutSideRoom ();
			}
		}

		CreateFurniturePrototypes();
		characters = new List<Character> ();
		furniture = new List<Furniture> ();
		inventoryManager = new InventoryManager ();
	}

	public void Update(float deltaTime){
		foreach (Character c in characters) {
			c.Update (deltaTime);
		}

		foreach(Furniture f in furniture) {
			f.Update(deltaTime);
		}
	}

	public Character CreateCharacter(Tile t){
		Character c = new Character (t);
		characters.Add (c);
		if(cbCharacterCreated != null)
			cbCharacterCreated (c);
		return c;
	}

	void CreateFurniturePrototypes() {
        furniturePrototypes = new Dictionary<string, Furniture>();
        furnitureJobPrototypes = new Dictionary<string, Job>();

        FurnitureCreate.CreateFurniturePrototypes(furniturePrototypes, furnitureJobPrototypes);
    }

	public void RandomizeTiles() {
		
		for (int x = 0; x < Width; x++) {
			for (int y = 0; y < Height; y++) {

				if(UnityEngine.Random.Range(0, 2) == 0) {
					tiles[x,y].Type = TileType.Empty;
				}
				else {
					tiles[x,y].Type = TileType.Floor;
				}

			}
		}
	}

	public Tile GetTileAt(int x, int y) {
		if( x >= Width || x < 0 || y >= Height || y < 0) {
			//.LogError("Tile ("+x+","+y+") is out of range.");
			return null;
		}
		return tiles[x, y];
	}


	public Furniture PlaceFurniture(string objectType, Tile t, bool doRoomFloodFill = true) {
		if( furniturePrototypes.ContainsKey(objectType) == false ) {
			Debug.LogError("furniturePrototypes doesn't contain a proto for key: " + objectType);
			return null;
		}

		Furniture furn = Furniture.PlaceInstance( furniturePrototypes[objectType], t);

		if(furn == null) {
			// Failed to place object -- most likely there was already something there.
			return null;
		}

		furn.RegisterOnRemovedCallback (OnFurnitureRemoved);

		furniture.Add (furn);

		if (doRoomFloodFill && furn.roomEnclosure) {
			Room.DoRoomFloodFill (furn.tile);
		}

		if(cbFurnitureCreated != null) {
			cbFurnitureCreated(furn);
			if (furn.movementCost != 1) {
				InvalidateTileGraph ();
			}
		}

		return furn;
	}

	public void RegisterFurnitureCreated(Action<Furniture> callbackfunc) {
		cbFurnitureCreated += callbackfunc;
	}

	public void UnregisterFurnitureCreated(Action<Furniture> callbackfunc) {
		cbFurnitureCreated -= callbackfunc;
	}

	public void RegisterInventoryCreated(Action<Inventory> callbackfunc) {
		cbInventoryCreated += callbackfunc;
	}

	public void UnregisterInventoryCreated(Action<Inventory> callbackfunc) {
		cbInventoryCreated -= callbackfunc;
	}

	public void RegisterCharacterCreated(Action<Character> callbackfunc) {
		cbCharacterCreated += callbackfunc;
	}

	public void UnregisterCharacterCreated(Action<Character> callbackfunc) {
		cbCharacterCreated -= callbackfunc;
	}

	public void RegisterTileChanged(Action<Tile> callbackfunc) {
		cbTileChanged += callbackfunc;
	}

	public void UnegisterTileChanged(Action<Tile> callbackfunc) {
		cbTileChanged -= callbackfunc;
	}

	void OnTileChanged(Tile t){
		if (cbTileChanged == null) {
			return;
		}
		cbTileChanged (t);
		InvalidateTileGraph ();
	}

	public void InvalidateTileGraph(){
		tileGraph = null;
	}

	public bool IsFurniturePLacementValid(string furnType, Tile t){
		return furniturePrototypes [furnType].IsValidPlacement (t);
	}

	public void OnInventoryCreated(Inventory inv){
		if (cbInventoryCreated != null) {
			cbInventoryCreated (inv);
		}
	}

	public void OnFurnitureRemoved(Furniture furn){
		furniture.Remove (furn);
	}

	/*
	 * 
	 * 
	 * SAVE AND LOADING
	 * 
	 * 
	 * 
	 */

	public World(){

	}

	public XmlSchema GetSchema(){
		return null;
	}

	public void WriteXml(XmlWriter writer){
		writer.WriteAttributeString ("Width", Width.ToString());
		writer.WriteAttributeString ("Height", Height.ToString());

        writer.WriteStartElement("Rooms");
        foreach (Room r in rooms) {

            if(GetOutSideRoom() == r) {
                continue;
            }

            writer.WriteStartElement("Room");
            r.WriteXml(writer);
            writer.WriteEndElement();
        }
        writer.WriteEndElement();

        writer.WriteStartElement ("Tiles");
		for (int x = 0; x < Width; x++) {
			for (int y = 0; y < Height; y++) {
				writer.WriteStartElement ("Tile");
				tiles [x, y].WriteXml (writer);
				writer.WriteEndElement ();
			}
		}
        writer.WriteEndElement ();

        writer.WriteStartElement("Furnitures");
        foreach (Furniture furn in furniture) {
            writer.WriteStartElement("Furniture");
            furn.WriteXml(writer);
            writer.WriteEndElement();
        }
        writer.WriteEndElement();

        writer.WriteStartElement ("Characters");
		foreach (Character chr in characters) {
			writer.WriteStartElement ("Character");
			chr.WriteXml (writer);
			writer.WriteEndElement ();
		}
		writer.WriteEndElement ();
	}

	public void ReadXml(XmlReader reader){

		Width = int.Parse(reader.GetAttribute("Width"));
		Height = int.Parse(reader.GetAttribute ("Height"));

		SetUpWorld (Width, Height);

		while (reader.Read()) {
			switch (reader.Name) {
                case "Rooms":
                    ReadXmlRooms(reader);
                    break;
                case "Tiles":
				    ReadXmlTiles (reader);
				    break;
			    case "Furnitures":
				    ReadXmlFurnitures (reader);
				    break;
			    case "Characters":
				    ReadXmlCharacters (reader);
				    break;
			}

		}

		Inventory inv = new Inventory ();
		inv.stackSize = UnityEngine.Random.Range (20, 20);
		Tile t;
		t = GetTileAt (Width / 2, Height / 2 - 5);
		inventoryManager.PlaceInventory (t, inv);
		if (cbInventoryCreated != null) {
			cbInventoryCreated (t.inventory);
		}
		inv = new Inventory ();
		inv.stackSize = UnityEngine.Random.Range (20, 20);
		t = GetTileAt (Width / 2 + 2, Height / 2);
		inventoryManager.PlaceInventory (t, inv);
		if (cbInventoryCreated != null) {
			cbInventoryCreated (t.inventory);
		}
		inv = new Inventory ();
		inv.stackSize = UnityEngine.Random.Range (20, 20);
		t = GetTileAt (Width / 2 + 5, Height / 2 + 4);
		inventoryManager.PlaceInventory (t, inv);
		if (cbInventoryCreated != null) {
			cbInventoryCreated (t.inventory);
		}

	}



	void ReadXmlTiles(XmlReader reader){

		if (reader.ReadToDescendant ("Tile")) {
			do {
				int x = int.Parse(reader.GetAttribute("X"));
				int y = int.Parse(reader.GetAttribute("Y"));
				tiles [x, y].ReadXml (reader);
			} while(reader.ReadToNextSibling ("Tile"));
		}
	}

    void ReadXmlFurnitures(XmlReader reader) {
        if (reader.ReadToDescendant("Furniture")) {
            do {
                int x = int.Parse(reader.GetAttribute("X"));
                int y = int.Parse(reader.GetAttribute("Y"));
                Furniture furn = PlaceFurniture(reader.GetAttribute("ObjectType"), tiles[x, y], false);
                furn.ReadXml(reader);
            } while (reader.ReadToNextSibling("Furniture"));

            /*foreach (Furniture furn in furniture) {
				Room.DoRoomFloodFill (furn.tile, true);
			}*/
        }
    }

    void ReadXmlRooms(XmlReader reader) {
        if (reader.ReadToDescendant("Room")) {
            do {
                Room r = new Room();
                rooms.Add(r);
                r.ReadXml(reader);
            } while (reader.ReadToNextSibling("Room"));

            /*foreach (Furniture furn in furniture) {
				Room.DoRoomFloodFill (furn.tile, true);
			}*/
        }
    }

    void ReadXmlCharacters(XmlReader reader){
		if (reader.ReadToDescendant ("Character")) {
			do {
				int x = int.Parse(reader.GetAttribute("X"));
				int y = int.Parse(reader.GetAttribute("Y"));
				Character c = CreateCharacter (tiles[x, y]);
				c.ReadXml (reader);
			} while(reader.ReadToNextSibling ("Character"));
		}
	}

}