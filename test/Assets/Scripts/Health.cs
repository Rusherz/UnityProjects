using UnityEngine;
using System.Collections;

public class Health : MonoBehaviour {

	public float health = 100f;

	// Use this for initialization
	void Start () {
	
	}

	[RPC]
	// Update is called once per frame
	public void TakeDamage (float amount) {
		health -= amount;

		if(health <= 0){
			Die();
		}
	}

	void Die(){
		if(GetComponent<PhotonView>().instantiationId == 0){
			Destroy (gameObject);
		}else{
			if(PhotonNetwork.isMasterClient){
				PhotonNetwork.Destroy (gameObject);
			}
		}
	}

}
