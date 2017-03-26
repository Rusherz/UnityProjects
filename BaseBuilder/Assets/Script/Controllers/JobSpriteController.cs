using UnityEngine;
using System.Collections.Generic;

public class JobSpriteController : MonoBehaviour {

	FurnitureSpriteController fsc;
	Dictionary<Job, GameObject> jobGameObjectMap;

	void Start(){
	
		jobGameObjectMap = new Dictionary<Job, GameObject> ();
		fsc = GameObject.FindObjectOfType<FurnitureSpriteController> ();
		WorldController.Instance.world.jobQueue.RegisterJobCreationCallBack (OnJobCreated);
		
	}

	void OnJobCreated(Job j){

		if (j.jobObjectType == null) {
			return;
		}

		if (jobGameObjectMap.ContainsKey (j)) {
			return;
		}

		GameObject job_go = new GameObject();

		jobGameObjectMap.Add( j, job_go );

		job_go.name = "JOB_" + j.jobObjectType + "_" + j.tile.X + "_" + j.tile.Y;
		job_go.transform.position = new Vector3( j.tile.X  + ((j.furnitureprototype.height - 1) / 2f), j.tile.Y  + ((j.furnitureprototype.height - 1) / 2f), 0);
		job_go.transform.SetParent(this.transform, true);

		SpriteRenderer sr = job_go.AddComponent<SpriteRenderer>();
		sr.sprite = fsc.GetSpriteForFurniture (j.jobObjectType);
		sr.color = new Color (1f, 1f, 1f, 0.25f);
		job_go.GetComponent<SpriteRenderer> ().sortingLayerName = "InstalledObject";

		if (j.jobObjectType == "Door") {
			Tile northTile = j.tile.world.GetTileAt (j.tile.X, j.tile.Y + 1 );
			Tile southTile = j.tile.world.GetTileAt (j.tile.X, j.tile.Y - 1 );

			if (northTile != null && southTile != null && northTile.furniture != null && southTile.furniture != null
				&& northTile.furniture.objectType == "Wall" && southTile.furniture.objectType == "Wall") {
				job_go.transform.rotation = Quaternion.Euler (0, 0, 90);
			}
		}

		j.RegisterJobCompleteCallBack (OnJobEnded);
		j.RegisterJobCancelCallBack (OnJobEnded);
	}

	void OnJobEnded(Job j){

		GameObject job_go = jobGameObjectMap [j];
		j.UnregisterJobCompleteCallBack (OnJobCreated);
		j.UnregisterJobCancelCallBack (OnJobEnded);
		Destroy (job_go);

	}

}