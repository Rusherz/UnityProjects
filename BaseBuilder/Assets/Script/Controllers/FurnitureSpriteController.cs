using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

public class FurnitureSpriteController : MonoBehaviour {

	Dictionary<Furniture, GameObject> furnitureGameObjectMap;
	Dictionary<string, Sprite> furnitureSprites;

	World world{
		get { return WorldController.Instance.world; }
	}

	// Use this for initialization
	void Start () {

		LoadSprites();
		furnitureGameObjectMap = new Dictionary<Furniture, GameObject>();

		world.RegisterFurnitureCreated(OnFurnitureCreated);
		foreach (Furniture furn in world.furniture) {
			OnFurnitureCreated (furn);
		}
	}

	void LoadSprites() {
		furnitureSprites = new Dictionary<string, Sprite>();
		Sprite[] sprites = Resources.LoadAll<Sprite>("Images/Objects/");

		//Debug.Log("LOADED RESOURCE:");
		foreach(Sprite s in sprites) {
			//Debug.Log(s);
			furnitureSprites[s.name] = s;
		}
	}

	public void OnFurnitureCreated( Furniture furn ) {
		GameObject furn_go = new GameObject();

		furnitureGameObjectMap.Add( furn, furn_go );

		furn_go.name = furn.objectType + "_" + furn.tile.X + "_" + furn.tile.Y;
		furn_go.transform.position = new Vector3( furn.tile.X + ((furn.width - 1) / 2f), furn.tile.Y + ((furn.height - 1) / 2f), 0);
		furn_go.transform.SetParent(this.transform, true);

		if (furn.objectType == "Door") {
			Tile northTile = world.GetTileAt (furn.tile.X, furn.tile.Y + 1 );
			Tile southTile = world.GetTileAt (furn.tile.X, furn.tile.Y - 1 );

			if (northTile != null && southTile != null && northTile.furniture != null && southTile.furniture != null
				&& northTile.furniture.objectType == "Wall" && southTile.furniture.objectType == "Wall") {
				furn_go.transform.rotation = Quaternion.Euler (0, 0, 90);
			}
		}

		SpriteRenderer sr = furn_go.AddComponent<SpriteRenderer> ();
		sr.sprite = GetSpriteForFurniture(furn);
		sr.sortingLayerName = "InstalledObject";
		sr.color = furn.tint;
		furn.RegisterOnChangedCallback( OnFurnitureChanged );
		furn.RegisterOnRemovedCallback( OnFurnitureRemoved );

	}

	void OnFurnitureRemoved(Furniture furn){
		if(furnitureGameObjectMap.ContainsKey(furn) == false) {
			Debug.LogError("OnFurnitureRemoved -- trying to remove visuals for furniture not in our map.");
			return;
		}

		GameObject furn_go = furnitureGameObjectMap[furn];
		Destroy (furn_go);
		furnitureGameObjectMap.Remove (furn);
	}

	void OnFurnitureChanged( Furniture furn ) {

		if(furnitureGameObjectMap.ContainsKey(furn) == false) {
			Debug.LogError("OnFurnitureChanged -- trying to change visuals for furniture not in our map.");
			return;
		}

		GameObject furn_go = furnitureGameObjectMap[furn];

		furn_go.GetComponent<SpriteRenderer>().sprite = GetSpriteForFurniture(furn);
		furn_go.GetComponent<SpriteRenderer> ().color = furn.tint;

	}




	public Sprite GetSpriteForFurniture(Furniture furn) {
		string spriteName = furn.objectType;
		if(furn.linksToNeighbour == false) {
			if (furn.objectType == "Door") {
				if (furn.GetParam("openess") < 0.1f) {
					spriteName = "Door";
				} else if (furn.GetParam("openess") < 0.5f){
					spriteName = "Door_1";
				} else if (furn.GetParam("openess") < 0.9f){
					spriteName = "Door_2";
				} else{
					spriteName = "Door_3";
				}
			}
			return furnitureSprites[spriteName];
		}


		spriteName = furn.objectType + "_";
		int x = furn.tile.X;
		int y = furn.tile.Y;

		Tile t;

		t = world.GetTileAt(x, y+1);
		if(t != null && t.furniture != null && t.furniture.objectType == furn.objectType) {
			spriteName += "N";
		}
		t = world.GetTileAt(x+1, y);
		if(t != null && t.furniture != null && t.furniture.objectType == furn.objectType) {
			spriteName += "E";
		}
		t = world.GetTileAt(x, y-1);
		if(t != null && t.furniture != null && t.furniture.objectType == furn.objectType) {
			spriteName += "S";
		}
		t = world.GetTileAt(x-1, y);
		if(t != null && t.furniture != null && t.furniture.objectType == furn.objectType) {
			spriteName += "W";
		}

		if(furnitureSprites.ContainsKey(spriteName) == false) {
			Debug.LogError("GetSpriteForInstalledObject -- No sprites with name: " + spriteName);
			return null;
		}

		return furnitureSprites[spriteName];

	}

	public Sprite GetSpriteForFurniture(string objType){
		if (furnitureSprites.ContainsKey (objType)) {
			return furnitureSprites[objType];
		}

		if (furnitureSprites.ContainsKey (objType + "_")) {
			return furnitureSprites[objType + "_"];
		}

		Debug.LogError ("No type for object " + objType);
		return null;
	}

}