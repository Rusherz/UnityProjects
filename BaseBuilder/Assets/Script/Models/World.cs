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

	Dictionary<string, Furniture> furniturePrototypes;
	public Dictionary<string, Job> furnitureJobPrototypes;

	// The tile width of the world.
	public int Width { get; protected set; }

	// The tile height of the world
	public int Height { get; protected set; }

	Action<Furniture> cbFurnitureCreated;
	Action<Character> cbCharacterCreated;
	Action<Inventory> cbInventoryCreated;
	Action<Tile> cbTileChanged;

	public JobQueue jobQueue;

	public World(int width, int height) {
		SetUpWorld (width, height);

		CreateCharacter(GetTileAt( Width/2, Height/2 ) );
	}

	public Room GetOutSideRoom(){
		return rooms [0];
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
		jobQueue = new JobQueue();

		Width = width;
		Height = height;

		tiles = new Tile[Width,Height];

		rooms = new List<Room> ();
		rooms.Add (new Room ());

		for (int x = 0; x < Width; x++) {
			for (int y = 0; y < Height; y++) {
				tiles[x,y] = new Tile(this, x, y);
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

		furniturePrototypes.Add("Wall", new Furniture("Wall", 0, 1, 1, true, true));
		furnitureJobPrototypes.Add("Wall", new Job(null, "Wall", FurnitureActions.JobComplete_Building,
			1f, new Inventory[]{ new Inventory("Steel Plate", 5, 0)}));
		


		furniturePrototypes.Add("Stock Pile", new Furniture("Stock Pile", 1, 1, 1, false, false));
		furnitureJobPrototypes.Add("Stock Pile", new Job(null, "Stock Pile", FurnitureActions.JobComplete_Building,-1f, null));
		furniturePrototypes ["Stock Pile"].RegisterAction (FurnitureActions.StockPile_UpdateAction);
		furniturePrototypes ["Stock Pile"].tint = new Color32(186, 31, 31, 255);

		furniturePrototypes.Add("Door", new Furniture("Door", 2, 1, 1, false, true));
		furniturePrototypes["Door"].SetParam("openess", 0);
		furniturePrototypes["Door"].SetParam("is_opening", 0);
		furniturePrototypes ["Door"].RegisterAction(FurnitureActions.Door_UpdateAction);
		furniturePrototypes ["Door"].IsEnterable = FurnitureActions.Door_IsEnterable;
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


	public Furniture PlaceFurniture(string objectType, Tile t) {
		if( furniturePrototypes.ContainsKey(objectType) == false ) {
			Debug.LogError("furniturePrototypes doesn't contain a proto for key: " + objectType);
			return null;
		}

		Furniture furn = Furniture.PlaceInstance( furniturePrototypes[objectType], t);

		if(furn == null) {
			// Failed to place object -- most likely there was already something there.
			return null;
		}

		furniture.Add (furn);

		if (furn.roomEnclosure) {
			Room.DoRoomFloodFill (furn);
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

		writer.WriteStartElement ("Tiles");
		for (int x = 0; x < Width; x++) {
			for (int y = 0; y < Height; y++) {
				writer.WriteStartElement ("Tile");
				tiles [x, y].WriteXml (writer);
				writer.WriteEndElement ();
			}
		}

		writer.WriteEndElement ();
		writer.WriteStartElement ("Furnitures");
		foreach (Furniture furn in furniture) {
			writer.WriteStartElement ("Furniture");
			furn.WriteXml (writer);
			writer.WriteEndElement ();
		}
		writer.WriteEndElement ();

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
		inv.stackSize = UnityEngine.Random.Range (1, 5);
		Tile t;
		t = GetTileAt (Width / 2, Height / 2 - 5);
		inventoryManager.PlaceInventory (t, inv);
		if (cbInventoryCreated != null) {
			cbInventoryCreated (t.inventory);
		}
		inv = new Inventory ();
		inv.stackSize = UnityEngine.Random.Range (1, 5);
		t = GetTileAt (Width / 2 + 2, Height / 2);
		inventoryManager.PlaceInventory (t, inv);
		if (cbInventoryCreated != null) {
			cbInventoryCreated (t.inventory);
		}
		inv = new Inventory ();
		inv.stackSize = UnityEngine.Random.Range (1, 5);
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

	void ReadXmlFurnitures(XmlReader reader){
		if (reader.ReadToDescendant ("Furniture")) {
			do {
				int x = int.Parse(reader.GetAttribute("X"));
				int y = int.Parse(reader.GetAttribute("Y"));
				Furniture furn = PlaceFurniture(reader.GetAttribute("ObjectType"), tiles [x, y]);
				furn.ReadXml (reader);
			} while(reader.ReadToNextSibling ("Furniture"));
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