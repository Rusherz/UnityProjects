using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class BuildModeController : MonoBehaviour {

	bool buildModeIsObjects = false;
	TileType buildModeTile = TileType.Floor;
	string buildModeObjectType;
	GameObject furniturePreview;
	FurnitureSpriteController fsc;
	MouseController mc;

	// Use this for initialization
	void Start () {
		mc = GameObject.FindObjectOfType<MouseController> ();
		fsc = GameObject.FindObjectOfType<FurnitureSpriteController> ();
		furniturePreview = new GameObject ();
		furniturePreview.transform.SetParent (this.transform);
		furniturePreview.AddComponent<SpriteRenderer> ().sortingLayerName = "InstalledObject";;
		furniturePreview.SetActive(false);
	}

	void Update(){
		if (buildModeIsObjects && buildModeObjectType != null && buildModeObjectType != "") {
			ShowFurnitureSpriteAtCord (buildModeObjectType, mc.GetMouseOverTile ());
		} else {
			furniturePreview.SetActive (false);
		}
	}

	public bool IsObjectDraggable(){
		if (!buildModeIsObjects) {
			return true;
		}

		Furniture proto = WorldController.Instance.world.furniturePrototypes [buildModeObjectType];

		return proto.width == 1 && proto.height == 1;

	}

	void ShowFurnitureSpriteAtCord(string furnitureType, Tile t){
		furniturePreview.SetActive (true);
		SpriteRenderer sr = furniturePreview.GetComponent<SpriteRenderer>();
		sr.sprite = fsc.GetSpriteForFurniture (furnitureType);
		if (WorldController.Instance.world.IsFurniturePLacementValid (furnitureType, t)) {
			sr.color = new Color (0.5f, 1f, 0.5f, 0.25f);
		} else {
			sr.color = new Color (1f, 0.5f, 0.5f, 0.25f);
		}
		Furniture proto = t.world.furniturePrototypes [furnitureType];
		furniturePreview.transform.position = new Vector3 (t.X  + ((proto.height - 1) / 2f), t.Y  + ((proto.height - 1) / 2f), 0);
	}

	public void SetMode_BuildFloor( ) {
		buildModeIsObjects = false;
		buildModeTile = TileType.Floor;
	}
	
	public void SetMode_Bulldoze( ) {
		buildModeIsObjects = false;
		buildModeTile = TileType.Empty;
	}

	public void SetMode_BuildFurniture( string objectType ) {
		buildModeIsObjects = true;
		buildModeObjectType = objectType;
	}

	public void DoBuild(Tile t){
		if(buildModeIsObjects == true) {
			string furnType = buildModeObjectType;
			if (WorldController.Instance.world.IsFurniturePLacementValid (furnType, t) && t.JobPending == null) {
				Job j;
				if (WorldController.Instance.world.furnitureJobPrototypes.ContainsKey (furnType)) {
					j = WorldController.Instance.world.furnitureJobPrototypes [furnType].Clone();
					j.tile = t;
				} else {
					Debug.LogError ("There is no furniture job prototype for " + furnType);
					j = new Job (t, furnType, FurnitureActions.JobComplete_Building, 0.1f, null);
				}

				j.furnitureprototype = WorldController.Instance.world.furniturePrototypes [furnType];

				t.JobPending = j;

				WorldController.Instance.world.jobQueue.Enqueue (j);

				j.RegisterJobCancelCallBack ((theJob) => {
					theJob.tile.JobPending = null;
				});
			}
		}else {
			t.Type = buildModeTile;
		}
	}



}