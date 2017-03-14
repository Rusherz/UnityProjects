using UnityEngine;
using System.Collections;

public class NetworkManager : MonoBehaviour {

	public GameObject standbyCamera;
	private SpawnSpot[] spawnSpots;

	public bool offlineMode = false;

	// Use this for initialization
	void Start () {
		Debug.Log ("Start");
		spawnSpots = GameObject.FindObjectsOfType<SpawnSpot> ();
		Connect ();

	}

	void Connect(){
		if(offlineMode){
			PhotonNetwork.offlineMode = true;
			OnJoinedLobby();
		}else{
			Debug.Log ("Connect");
			PhotonNetwork.ConnectUsingSettings("v0.0.0");
		}
	}

	void OnGUI(){
		GUILayout.Label (PhotonNetwork.connectionStateDetailed.ToString());
	}

	void OnJoinedLobby(){
		Debug.Log ("OnJoinedLobby");
		PhotonNetwork.JoinRandomRoom ();
	}

	void OnPhotonRandomJoinFailed(){
		Debug.Log ("OnPhotonRandomJoinFailed");
		PhotonNetwork.CreateRoom (null);
	}

	void OnJoinedRoom(){
		Debug.Log ("OnJoinedRoom");
		SpawnPlayer ();
	}

	void SpawnPlayer(){
		Debug.Log ("SpawnPlayer");
		if(spawnSpots == null){
			Debug.LogError("Dun Fucked Up");
		}
		SpawnSpot spawn = spawnSpots [Random.Range (0, spawnSpots.Length)];
		GameObject Player = (GameObject)PhotonNetwork.Instantiate ("Player", spawn.transform.position, Quaternion.identity, 0);
		standbyCamera.SetActive(false);
		Player.GetComponent<playerMovement> ().enabled = true;
		Player.GetComponent<PlayerShooting> ().enabled = true;
		Player.transform.FindChild ("playerCamera").gameObject.SetActive (true);
	}

}
