using UnityEngine;
using System.Collections;

public class NetworkPlayer : Photon.MonoBehaviour {

	Vector3 realPosition = Vector3.zero;
	Quaternion realRotation = Quaternion.identity;

	//float lastUpdate;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		if(photonView.isMine){
			
		}else{
			transform.position = Vector3.Lerp(transform.position, realPosition, 0.05f);
			transform.rotation = Quaternion.Lerp(transform.rotation, realRotation, 0.05f);
		}
	}

	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo message){
		if(stream.isWriting){
			stream.SendNext(transform.position);
			stream.SendNext(transform.rotation);
		}else{
			realPosition = (Vector3)stream.ReceiveNext();
			realRotation = (Quaternion)stream.ReceiveNext();
		}
	}

}
