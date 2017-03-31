using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

public class TileSpriteController : MonoBehaviour {

	SpriteController spriteController;
	Dictionary<Tile, GameObject> tileGameObjectMap;

	World world{
		get { return WorldController.Instance.world; }
	}

	// Use this for initialization
	void Start () {

		spriteController = GameObject.FindObjectOfType<SpriteController> ();

		tileGameObjectMap = new Dictionary<Tile, GameObject>();

		world.RegisterTileChanged( OnTileChanged );

		for (int x = 0; x < world.Width; x++) {
			for (int y = 0; y < world.Height; y++) {
				Tile tile_data = world.GetTileAt(x, y);

				GameObject tile_go = new GameObject();

				tileGameObjectMap.Add( tile_data, tile_go );

				tile_go.name = "Tile_" + x + "_" + y;
				tile_go.transform.position = new Vector3( tile_data.X, tile_data.Y, 0);
				tile_go.transform.SetParent(this.transform, true);

				tile_go.AddComponent<SpriteRenderer>().sprite = spriteController.Sprites[tile_data.Type.ToString()];
				OnTileChanged (tile_data);

			}
		}
		World.currentWorld.PlaceObjectsAroundMap ();
	}

	void DestroyAllTileGameObjects() {

		while(tileGameObjectMap.Count > 0) {
			Tile tile_data = tileGameObjectMap.Keys.First();
			GameObject tile_go = tileGameObjectMap[tile_data];

			tileGameObjectMap.Remove(tile_data);

			tile_data.UnregisterTileTypeChangedCallback( OnTileChanged );

			Destroy( tile_go );
		}

	}

	void OnTileChanged( Tile tile_data ) {

		if(tileGameObjectMap.ContainsKey(tile_data) == false) {
			Debug.LogError("tileGameObjectMap doesn't contain the tile_data -- did you forget to add the tile to the dictionary? Or maybe forget to unregister a callback?");
			return;
		}

		GameObject tile_go = tileGameObjectMap[tile_data];

		if(tile_go == null) {
			Debug.LogError("tileGameObjectMap's returned GameObject is null -- did you forget to add the tile to the dictionary? Or maybe forget to unregister a callback?");
			return;
		}

		tile_go.GetComponent<SpriteRenderer>().sprite = spriteController.Sprites[tile_data.Type.ToString()];

	}

}