using UnityEngine;
using System.Collections;

public class gunMaster : MonoBehaviour {

	private gunClass[] guns;
	private static GameObject loadedModel;
	private static int Selected = -1;
	private GameObject parent;

	//private weaponRecoil wr;

	// Use this for initialization
	void Start () {
		addGuns ();
		parent = transform.FindChild("PlayerCamera").gameObject;
		//wr = GetComponent<weaponRecoil> ();
	}

	[PunRPC]
	void weapon(int selected){
		Debug.Log (selected);
		if(Selected != -1){
			if(PhotonNetwork.isMasterClient){
				PhotonNetwork.Destroy(loadedModel);
			}
		}
		loadedModel = (GameObject) PhotonNetwork.Instantiate (guns[selected].getGun(), parent.transform.position, parent.transform.rotation, 0);
		loadedModel.transform.parent = transform.FindChild("PlayerCamera");
		NetworkCharacter.loadedModel = loadedModel;
		//wr.setStartPoint();
		Selected = selected;
	}

	// Custom methods start here
	private void addGuns(){
		guns = new gunClass[]{
			new gunClass("AK47", 850, 30, 6, 44, 1000, 10),
			new gunClass("G36C", 780, 30, 6, 38, 1000, 5),
			new gunClass("Vector", 1200, 25, 6, 23, 200, 2),
			new gunClass("552", 690, 30, 6, 47, 1000, 5)};
	}

	public int getSelected(){
		return Selected;
	}

	public GameObject getLoadedModel(){
		return loadedModel;
	}

}