using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour {

	public float health = 100f;
	private bool dead = false;
	private float deathTime = 5f;
	public Transform[] gui;

	// Use this for initialization
	void Start () {

	}

	[PunRPC]
	public void damage(float damageDelt){
		Debug.Log (this.name + " took " + damageDelt + "");
		health -= damageDelt;
		if(health <= 0){
			Die ();
		}
	}

	void Die(){
		if(GetComponent<PhotonView>().instantiationId == 0){
			Destroy(gameObject);
		}else{
			if(PhotonNetwork.isMasterClient){
				PhotonNetwork.Destroy (gameObject);
			}
		}
		PhotonNetwork.Instantiate ("TestBullet", new Vector3(0, 1, 0), Quaternion.identity, 0);
		Debug.Log ("bullet should be there");
	}


	//GET AND SET
	public void setHealth(float amount){
		health = amount;
	}

	public float getHealth(){
		return health;
	}

	public void setDead(bool state){
		dead = state;
	}
		
	public bool getDead(){
		return dead;
	}

	public void setDeathTime(float time){
		deathTime = time;
	}

	public float getDeathTime(){
		return deathTime;
	}

}