using UnityEngine;
using System.Collections;

public class NetworkManager : MonoBehaviour {

	public int players = 0;

	// Use this for initialization
	void Start () {
		Connect ();
		players++;
	}

	void Connect(){
		PhotonNetwork.ConnectUsingSettings ("v0.0.0");
	}

	void OnGUI(){
		GUILayout.Label (PhotonNetwork.playerList.ToStringFull());
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
		Player.GetComponent<PhotonView> ().RPC ("setPlayerName", PhotonTargets.All, "Player " + PhotonNetwork.countOfPlayers);
		players++;
        Player.transform.FindChild("RecoilHolder").transform.FindChild("PlayerCamera").gameObject.SetActive(true);
        Player.GetComponentInChildren<PlayerController> ().enabled = true;
		Player.GetComponentInChildren<MeshRenderer> ().enabled = false;
	}

}
