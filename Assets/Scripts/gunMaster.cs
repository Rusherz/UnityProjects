using UnityEngine;
using System.Collections;

public class gunMaster : MonoBehaviour {

	public gunClass[] guns;
	public GameObject[] models;
	public static GameObject loadedModel;
	public static int selected = -1;
	private float switchCoolDown = 1f;
	private float timeWait = 0;
	private GameObject parent;

	private weaponRecoil wr;

	// Use this for initialization
	void Start () {
		addGuns ();
		parent = GameObject.Find (transform.parent.name + "/GameObject/Main Camera/weaponHolder");
		wr = GetComponent<weaponRecoil> ();
	}
	
	// Update is called once per frame
	void Update () {
		timeWait -= Time.deltaTime;
		if(timeWait <= 0){
			if(Input.GetKeyDown(KeyCode.Alpha1) && selected != 0){
				if(selected != -1){
					Destroy(loadedModel);
				}
				loadedModel = Instantiate (models[0]) as GameObject;
				loadedModel.transform.parent = parent.transform;
				loadedModel.transform.localPosition = new Vector3(0, 0, 0);
				loadedModel.transform.localRotation = models[0].transform.localRotation;
				selected = 0;
				wr.setStartPoint();
				timeWait = switchCoolDown;
			}else if(Input.GetKeyDown(KeyCode.Alpha2) && selected != 1){
				if(selected != -1){
					Destroy(loadedModel);
				}
				loadedModel = Instantiate (models[1]) as GameObject;
				loadedModel.transform.parent = parent.transform;
				loadedModel.transform.localPosition = new Vector3(0, 0, 0);
				loadedModel.transform.localRotation = models[1].transform.localRotation;
				selected = 1;
				wr.setStartPoint();
				timeWait = switchCoolDown;
			}else if(Input.GetKeyDown(KeyCode.Alpha3) && selected != 2){
				if(selected != -1){
					Destroy(loadedModel);
				}
				loadedModel = Instantiate (models[2]) as GameObject;
				loadedModel.transform.parent = parent.transform;
				loadedModel.transform.localPosition = models[2].transform.position;
				loadedModel.transform.localRotation = models[2].transform.localRotation;
				selected = 2;
				wr.setStartPoint();
				timeWait = switchCoolDown;
			}else if(Input.GetKeyDown(KeyCode.Alpha4) && selected != 3){
				if(selected != -1){
					Destroy(loadedModel);
				}
				loadedModel = Instantiate (models[3]) as GameObject;
				loadedModel.transform.parent = parent.transform;
				loadedModel.transform.localPosition = new Vector3(0, 0, 0);
				loadedModel.transform.localRotation = models[3].transform.localRotation;
				selected = 3;
				wr.setStartPoint();
				timeWait = switchCoolDown;
			}
		}
	}

	// Custom methods start here
	private void addGuns(){
		guns = new gunClass[]{
			new gunClass("AK-47", 850, 30, 6, 44, 1000, 10),
			new gunClass("G36C", 780, 30, 6, 38, 1000, 5),
			new gunClass("Vector .45 APC", 1200, 25, 6, 23, 200, 2),
			new gunClass("552 Commando", 690, 30, 6, 47, 1000, 5)};
	}

}
