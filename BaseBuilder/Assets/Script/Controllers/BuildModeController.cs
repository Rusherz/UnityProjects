using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public enum BuildMode{
	Floor,
	Furniture,
	Deconstruct
}

public class BuildModeController : MonoBehaviour {

	public BuildMode buildMode = BuildMode.Floor;
	TileType buildModeTile = TileType.Floor;
	public string buildModeObjectType;
	GameObject furniturePreview;

	// Use this for initialization
	void Start () {

	}

	public bool IsObjectDraggable(){
		if (buildMode == BuildMode.Floor || buildMode == BuildMode.Deconstruct) {
			return true;
		}

		Furniture proto = WorldController.Instance.world.furniturePrototypes [buildModeObjectType];

		return proto.width == 1 && proto.height == 1;

	}



	public void SetMode_BuildFloor( ) {
		buildMode = BuildMode.Floor;
		buildModeTile = TileType.Floor;
		GameObject.FindObjectOfType<MouseController> ().StartBuildMode ();
	}
	
	public void SetMode_Bulldoze( ) {
		buildMode = BuildMode.Floor;
		buildModeTile = TileType.Empty;
		GameObject.FindObjectOfType<MouseController> ().StartBuildMode ();
	}
	public void SetMode_Deconstruct() {
		buildMode = BuildMode.Deconstruct;
		GameObject.FindObjectOfType<MouseController> ().StartBuildMode ();
	}

	public void SetMode_BuildFurniture( string objectType ) {
		buildMode = BuildMode.Furniture;
		buildModeObjectType = objectType;
		GameObject.FindObjectOfType<MouseController> ().StartBuildMode ();
	}

	public void DoBuild(Tile t){
		if (buildMode == BuildMode.Furniture) {
			string furnType = buildModeObjectType;
			if (WorldController.Instance.world.IsFurniturePLacementValid (furnType, t) && t.JobPending == null) {
				Job j;
				if (WorldController.Instance.world.furnitureJobPrototypes.ContainsKey (furnType)) {
					j = WorldController.Instance.world.furnitureJobPrototypes [furnType].Clone ();
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
		} else if (buildMode == BuildMode.Floor) {
			t.Type = buildModeTile;
		} else if (buildMode == BuildMode.Deconstruct) {
			if (t.furniture != null) {
				t.furniture.Deconstruct ();
			}
		} else {
			Debug.Log ("Unimplemented build mode");
		}
	}



}