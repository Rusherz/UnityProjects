using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildMenu : MonoBehaviour {

	public GameObject buildFurnitureButtonPrefab;
	BuildModeController bmc;

	// Use this for initialization
	void Start () {
		bmc = GameObject.FindObjectOfType<BuildModeController> ();
		foreach (string s in World.currentWorld.furniturePrototypes.Keys) {
			GameObject go = (GameObject)Instantiate (buildFurnitureButtonPrefab);
			go.transform.SetParent (this.transform);
			go.name = "Button - Build " + s;
			go.GetComponentInChildren<Text> ().text = "Build " + s;
			Button b = go.GetComponent<Button> ();
			b.onClick.AddListener (delegate { 
				string objectId = s;
				bmc.SetMode_BuildFurniture(objectId);				
			});
		}
	}

}
