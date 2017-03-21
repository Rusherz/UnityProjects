using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WorldController : MonoBehaviour {

	static WorldController _instance;
	public static WorldController Instance { get; protected set; }

	public Sprite floorSprite;

	public World world { get; protected set; }

	Dictionary<Tile, GameObject> TileGameObjectMap;
	Dictionary<Furniture, GameObject> InstalledGameObjectMap;
	Dictionary<string, Sprite> InstalledObjectSprites;

	// Use this for initialization
	void Start () {

		InstalledObjectSprites = new Dictionary<string, Sprite> ();
		Sprite[] sprites = Resources.LoadAll<Sprite> ("Images/Objects/");
		foreach (Sprite s in sprites)
        {
            Debug.Log(s.name);
            InstalledObjectSprites [s.name] = s;
		}

		Instance = this;

		world = new World ();

		world.RegisterInstalledObjectCreated (OnInstalledObjectCreated);

		TileGameObjectMap = new Dictionary<Tile, GameObject> ();
		InstalledGameObjectMap = new Dictionary<Furniture, GameObject> ();

		// Create a GameObject for each tile for them to show
		for (int x = 0; x < world.Width; x++) {
			for (int y = 0; y < world.Height; y++) {

				GameObject tile_go = new GameObject ();
				Tile tile_data = world.getTileAt(x, y);

				TileGameObjectMap.Add (tile_data, tile_go);

				tile_go.name = "Tile_" + x + "_" + y;
				tile_go.transform.position = new Vector3 (tile_data.X, tile_data.Y, 0);
				tile_go.transform.SetParent (this.transform, true);

				tile_go.AddComponent<SpriteRenderer> ();
				tile_data.RegTileTypeChanged (OnTileTypeChanged);
			}
		}

		//world.RandomTiles ();

	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public Tile GetTileAtWorldCoord(Vector3 coord){
		int x = Mathf.FloorToInt (coord.x);
		int y = Mathf.FloorToInt (coord.y);
		return world.getTileAt (x, y);
	}

	void DestroyAllTileGameObjects(){
		while (TileGameObjectMap.Count > 0) {
			Tile tile_data = TileGameObjectMap.Keys.First();
			GameObject tile_go = TileGameObjectMap [tile_data];
			TileGameObjectMap.Remove (tile_data);
			tile_data.UnregTileTypeChanged (OnTileTypeChanged);
			Destroy (tile_go);
		}
	}

	void OnTileTypeChanged(Tile tile_data){

		if (!TileGameObjectMap.ContainsKey (tile_data)) {
			Debug.LogError ("TileGameObjectMap does not contain the tile_data");
		}
		GameObject tile_go = TileGameObjectMap[tile_data];

		if (tile_go == null) {
			Debug.LogError ("TileGameObjectMap does not contain the tile_go");
		}
		if (tile_data.Type == Tile.TileType.Floor) {
			tile_go.GetComponent<SpriteRenderer> ().sprite = floorSprite;
		} else if (tile_data.Type == Tile.TileType.Empty) {
			tile_go.GetComponent<SpriteRenderer> ().sprite = null;
		} else {
			Debug.LogError ("OnTileTypeChanged - Unknown tile type.");
		}
	}

	public void OnInstalledObjectCreated(Furniture furn){
		GameObject furn_go = new GameObject ();

		InstalledGameObjectMap.Add (furn, furn_go);

		furn_go.name = furn.objectType + "_" + furn.tile.X + "_" + furn.tile.Y;
		furn_go.transform.position = new Vector3 (furn.tile.X , furn.tile.Y, 0);
		furn_go.transform.SetParent (this.transform, true);

		furn_go.AddComponent<SpriteRenderer> ().sprite = GetSpriteForInstalledObject(furn);
		furn_go.GetComponent<SpriteRenderer> ().sortingLayerName = "InstalledObject";
		furn.RegisterOnInstalledObjectChanged (OnInstalledObjectChanged);
	}

	Sprite GetSpriteForInstalledObject(Furniture obj){
		if (!obj.linksToNeighbour) {
			return InstalledObjectSprites [obj.objectType];
		}

		int x = obj.tile.X;
		int y = obj.tile.Y;

		string SpriteName = obj.objectType + "_";
		Tile t;
		t = world.getTileAt (x, y + 1);
		if (t != null && t.installedObject != null && t.installedObject.objectType == obj.objectType) {
			SpriteName += "N";
		}
		t = world.getTileAt (x + 1, y);
		if (t != null && t.installedObject != null && t.installedObject.objectType == obj.objectType) {
			SpriteName += "E";
		}
		t = world.getTileAt (x, y - 1);
		if (t != null && t.installedObject != null && t.installedObject.objectType == obj.objectType) {
			SpriteName += "S";
		}
		t = world.getTileAt (x - 1, y);
		if (t != null && t.installedObject != null && t.installedObject.objectType == obj.objectType) {
			SpriteName += "W";
		}

		return InstalledObjectSprites [SpriteName];
	}

	void OnInstalledObjectChanged(Furniture furn){

		if (!InstalledGameObjectMap.ContainsKey (furn)) {
			return;
		}

		GameObject furn_go = InstalledGameObjectMap [furn];
		furn_go.GetComponent<SpriteRenderer> ().sprite = GetSpriteForInstalledObject(furn);

	}

}
