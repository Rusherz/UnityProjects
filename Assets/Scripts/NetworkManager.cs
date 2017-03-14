using UnityEngine;
using System.Collections;

public class NetworkManager : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Connect ();
	}

	void Connect(){
		PhotonNetwork.ConnectUsingSettings ("v0.0.0");
	}

	void OnGUI(){
		GUILayout.Label (PhotonNetwork.connectionStateDetailed.ToString ());
	}

	void OnJoinedLobby(){
		PhotonNetwork.JoinRandomRoom ();
	}

	void OnPhotonRandomJoinFailed(){
		PhotonNetwork.CreateRoom (null);
	}

	void OnJoinedRoom(){
		Debug.Log ("Joined room");
		SpawnPlayer ();
	}

	void SpawnPlayer(){
		GameObject Player = (GameObject)PhotonNetwork.Instantiate ("Player", new Vector3(0, 1.05f, 0), Quaternion.identity, 0);
		Player.GetComponentInChildren<playerMovement> ().enabled = true;
		Player.GetComponentInChildren<shoot> ().enabled = true;
		Player.GetComponentInChildren<gunMaster> ().enabled = true;
		Player.GetComponentInChildren<MeshRenderer> ().enabled = false;
		Player.transform.Find ("PlayerCamera").gameObject.SetActive (true);
	}

}
