using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class BuildModeController : MonoBehaviour {

	bool buildModeIsObjects = false;
	TileType buildModeTile = TileType.Floor;
	string buildModeObjectType;

	// Use this for initialization
	void Start () {
		
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