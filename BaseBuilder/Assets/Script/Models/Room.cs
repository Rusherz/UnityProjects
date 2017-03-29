﻿using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Schema;

public class Room : IXmlSerializable {

	Dictionary<string, float> atmosGases;

	List<Tile> tiles;

	public Room() {
		tiles = new List<Tile>();
		atmosGases = new Dictionary<string, float> ();
	}

    public int ID {
        get {
            return World.currentWorld.GetRoomID(this);
        }
    }

	public void AssignTile( Tile t ) {
		if(tiles.Contains(t)) {
			// This tile already in this room.
			return;
		}

		if(t.room != null) {
			// Belongs to some other room
			t.room.tiles.Remove(t);
		}

		t.room = this;
		tiles.Add(t);
	}

	public void UnAssignAllTiles() {
		for (int i = 0; i < tiles.Count; i++) {
			tiles[i].room = World.currentWorld.GetOutSideRoom();	// Assign to outside
		}
		tiles = new List<Tile>();
	}

	public bool IsOutsideRoom(){
		return this == World.currentWorld.GetOutSideRoom ();
	}

	public void ChangeGas(string type, float amount){
		if (IsOutsideRoom ()) {
			return;
		}

		if (atmosGases.ContainsKey (type)) {
			atmosGases [type] += amount;
		} else {
			atmosGases[type] = amount;
		}

		if (atmosGases [type] < 0) {
			atmosGases [type] = 0;
		}

	}

	public float GetGasAmount(string type){
		if (atmosGases.ContainsKey (type)) {
			return atmosGases [type];
		} 
		return 0;
	}

	public float GetGasPercentage(string type){
		if (atmosGases.ContainsKey (type)) {
			float t = 0;
			foreach (string n in atmosGases.Keys) {
				t += atmosGases [n];
			}
			return atmosGases [type] / t;
		} else {
			return 0;
		}
	}

	public string[] GetGases(){
		return atmosGases.Keys.ToArray ();
	}

	public static void DoRoomFloodFill(Tile tile, bool onlyIfOutside = false) {
		// sourceFurniture is the piece of furniture that may be
		// splitting two existing rooms, or may be the final 
		// enclosing piece to form a new room.
		// Check the NESW neighbours of the furniture's tile
		// and do flood fill from them

		World world = World.currentWorld;

		Room oldRoom = tile.room;

		if (oldRoom != null) {

			// Try building new rooms for each of our NESW directions
			foreach (Tile t in tile.GetNeighbours()) {
				if (t.room != null && (onlyIfOutside == false || t.room.IsOutsideRoom ())) {
					ActualFloodFill (t, oldRoom);
				}
			}

			tile.room = null;
			oldRoom.tiles.Remove (tile);

			// If this piece of furniture was added to an existing room
			// (which should always be true assuming with consider "outside" to be a big room)
			// delete that room and assign all tiles within to be "outside" for now

			if (!oldRoom.IsOutsideRoom ()) {
				// At this point, oldRoom shouldn't have any more tiles left in it,
				// so in practice this "DeleteRoom" should mostly only need
				// to remove the room from the world's list.

				if (oldRoom.tiles.Count > 0) {
					Debug.LogError ("'oldRoom' still has tiles assigned to it. This is clearly wrong.");
				}

				world.DeleteRoom (oldRoom);
			}
		} else {
			ActualFloodFill (tile, null);
		}

	}

	protected static void ActualFloodFill(Tile tile, Room oldRoom) {
		if(tile == null) {
			// We are trying to flood fill off the map, so just return
			// without doing anything.
			return;
		}

		if(tile.room != oldRoom) {
			// This tile was already assigned to another "new" room, which means
			// that the direction picked isn't isolated. So we can just return
			// without creating a new room.
			return;
		}

		if(tile.furniture != null && tile.furniture.roomEnclosure) {
			// This tile has a wall/door/whatever in it, so clearly
			// we can't do a room here.
			return;
		}

		if(tile.Type == TileType.Empty) {
			// This tile is empty space and must remain part of the outside.
			return;
		}


		// If we get to this point, then we know that we need to create a new room.

		Room newRoom = new Room();
		Queue<Tile> tilesToCheck = new Queue<Tile>();
		tilesToCheck.Enqueue(tile);

		bool IsConnectedToSpace = false;

		while(tilesToCheck.Count > 0) {
			Tile t = tilesToCheck.Dequeue();


			if( t.room != newRoom ) {
				newRoom.AssignTile(t);

				Tile[] ns = t.GetNeighbours( );
				foreach(Tile t2 in ns) {
					if (t2 == null || t2.Type == TileType.Empty) {
						// We have hit open space (either by being the edge of the map or being an empty tile)
						// so this "room" we're building is actually part of the Outside.
						// Therefore, we can immediately end the flood fill (which otherwise would take ages)
						// and more importantly, we need to delete this "newRoom" and re-assign
						// all the tiles to Outside.
						IsConnectedToSpace = true;
						/*if (oldRoom != null) {
							newRoom.UnAssignAllTiles ();
							return;
						}*/
					} else {

						// We know t2 is not null nor is it an empty tile, so just make sure it
						// hasn't already been processed and isn't a "wall" type tile.
						if (t2.room != newRoom && (t2.furniture == null || t2.furniture.roomEnclosure == false)) {
							tilesToCheck.Enqueue (t2);
						}
					}
				}

			}
		}

		if (IsConnectedToSpace) {
			newRoom.UnAssignAllTiles ();
			return;
		}

		if (oldRoom != null) {
			newRoom.CopyGas (oldRoom);
		} else {

		}

		// Tell the world that a new room has been formed.
		World.currentWorld.AddRoom(newRoom);
	}

	void CopyGas(Room other){
		foreach(string n in other.atmosGases.Keys){
			this.atmosGases [n] = other.atmosGases [n];
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

    public XmlSchema GetSchema() {
        return null;
    }

    public void WriteXml(XmlWriter writer) {


        foreach (string s in atmosGases.Keys) {
            writer.WriteStartElement("Param");
            writer.WriteAttributeString("Name", s);
            writer.WriteAttributeString("value", atmosGases[s].ToString());
            writer.WriteEndElement();
        }
    }

    public void ReadXml(XmlReader reader) {
        //movementCost = int.Parse(reader.GetAttribute ("MovementCost"));


        if (reader.ReadToDescendant("Param")) {
            do {
                string s = reader.GetAttribute("Name");
                float v = float.Parse(reader.GetAttribute("value"));
                atmosGases[s] = v;
            } while (reader.ReadToNextSibling("Param"));
        }
    }

}
