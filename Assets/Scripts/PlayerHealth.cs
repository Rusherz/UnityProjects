using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerHealth : MonoBehaviour {

	public float health = 100f;
	private bool dead = false;
	private float deathTime = 5f;

	// Use this for initialization
	void Start () {

	}

	[PunRPC]
	public void damage(float damageDelt){
		Debug.Log (this.name + " took " + damageDelt + "");
		health -= damageDelt;
        GetComponent<PlayerController>().tookDamage = true;
		if(health <= 0){
            GetComponent<PhotonView>().RPC("Die", PhotonTargets.AllViaServer);
		}
	}
    
    [PunRPC]
	void Die(){
        dead = true;
        if (GetComponent<PhotonView>().instantiationId == 0)
        {
            Destroy(gameObject);
        }
        else {
            if (GetComponent<PhotonView>().isMine)
            {
                if(gameObject.tag == "Player")
                {
                    GameObject.FindObjectOfType<NetworkManager>().RespawnCamera.SetActive(true);
                    Debug.Log("is this being called?");
                    GameObject.FindObjectOfType<NetworkManager>().RespawnTimer = 5f;
                }
                PhotonNetwork.Destroy(gameObject);
            }
        }
        PhotonNetwork.Instantiate("TestBullet", new Vector3(0, 1, 0), Quaternion.identity, 0);
        Debug.Log("bullet should be there");
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