using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldController : MonoBehaviour {

	static WorldController _instance;
	public static WorldController Instance { get; protected set; }

	public Sprite floorSprite;

	public World world { get; protected set; }

	// Use this for initialization
	void Start () {

		if (Instance != null) {
			Debug.Log ("There should never be two world controllers");
		}
		Instance = this;

		world = new World ();

		// Create a GameObject for each tile for them to show
		for (int x = 0; x < world.Width; x++) {
			for (int y = 0; y < world.Height; y++) {

				GameObject tile_go = new GameObject ();
				Tile tile_data = world.getTileAt(x, y);
				tile_go.name = "Tile_" + x + "_" + y;
				tile_go.transform.position = new Vector3 (tile_data.X, tile_data.Y, 0);
				tile_go.transform.SetParent (this.transform, true);

				tile_go.AddComponent<SpriteRenderer> ();
				tile_data.RegTileTypeChanged ((tile) => { OnTileTypeChanged(tile, tile_go); });
			}
		}

		world.RandomTiles ();

	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public Tile GetTileAtWorldCoord(Vector3 coord){
		int x = Mathf.FloorToInt (coord.x);
		int y = Mathf.FloorToInt (coord.y);
		return world.getTileAt (x, y);
	}

	void OnTileTypeChanged(Tile tile_data, GameObject tile_go){
		if (tile_data.Type == Tile.TileType.Floor) {
			tile_go.GetComponent<SpriteRenderer> ().sprite = floorSprite;
		} else if (tile_data.Type == Tile.TileType.Empty) {
			tile_go.GetComponent<SpriteRenderer> ().sprite = null;
		} else {
			Debug.LogError ("OnTileTypeChanged - Unknown tile type.");
		}
	}

}
