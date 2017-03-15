using UnityEngine;
using System.Collections;

public class NetworkCharacter : Photon.MonoBehaviour {

	Vector3 realPosition = Vector3.zero;
	Quaternion realRotation = Quaternion.identity;
	Vector3 gmRealPosition = Vector3.zero;
	Quaternion gmRealRotation = Quaternion.identity;

	private static gunMaster gm;
	public static GameObject loadedModel;

	// Use this for initialization
	void Start () {
		gm = transform.GetComponent<gunMaster> ();
	}
	
	// Update is called once per frame
	void Update () {
		if(photonView.isMine){

		}else{
			transform.position = Vector3.Lerp(transform.position, realPosition, 0.01f);
			transform.rotation = Quaternion.Lerp(transform.rotation, realRotation, 0.01f);
			if(loadedModel != null){
				Debug.Log("Position before: " + loadedModel.transform.position);
				loadedModel.transform.position = Vector3.Lerp(loadedModel.transform.position, gmRealPosition, 0.01f);
				Debug.Log("Position after: " + loadedModel.transform.position);
				loadedModel.transform.rotation = Quaternion.Lerp(loadedModel.transform.rotation, gmRealRotation, 0.01f);
			}
		}
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info){
		if(stream.isWriting){
			stream.SendNext(transform.position);
			stream.SendNext(transform.rotation);
			if(loadedModel != null){
				stream.SendNext(loadedModel.transform.position);
				stream.SendNext(loadedModel.transform.rotation);
			}
		}else{
			realPosition = (Vector3)stream.ReceiveNext();
			realRotation = (Quaternion)stream.ReceiveNext();
			if(loadedModel != null){
				gmRealPosition = (Vector3)stream.ReceiveNext();
				gmRealRotation = (Quaternion)stream.ReceiveNext();
			}
		}
	}

}
