using UnityEngine;
using System.Collections;

public class PlayerShooting : MonoBehaviour {

	public float fireRate = 0.25f;
	public float coolDown = 0f;
	public float damage = 25;

	// Update is called once per frame
	void Update () {
		coolDown -= Time.deltaTime;
		if(Input.GetButton("Fire1")){
			Fire();
		}
	}

	void Fire(){

		if(coolDown > 0){
			return;
		}

		Debug.Log ("Fired");

		Ray ray = new Ray (Camera.main.transform.position, Camera.main.transform.forward);

		Transform hitTransform;
		Vector3 hitPoint;
		Health h = null;

		hitTransform = FindClosestHitObject (ray, out hitPoint);

		while(hitTransform.parent && h == null){
			hitTransform = hitTransform.parent;
			h = hitTransform.GetComponent<Health>();
		}

		if(hitTransform != null){
			Debug.Log(hitTransform.transform.name);
			h = hitTransform.transform.GetComponent<Health>();
			if(h != null){
				h.GetComponent<PhotonView>().RPC ("TakeDamage", PhotonTargets.All, damage);
				//h.TakeDamage(damage);
			}
		}

		coolDown = fireRate;

	}

	Transform FindClosestHitObject(Ray ray, out Vector3 hitPoint){
		RaycastHit[] hits = Physics.RaycastAll (ray);

		Transform closestHit = null;
		float distance = 0;
		hitPoint = Vector3.zero;

		foreach (RaycastHit hit in hits){
			if(hit.transform != this.transform && (closestHit == null || hit.distance < distance)){
				closestHit = hit.transform;
				distance = hit.distance;
				hitPoint = hit.point;
			}
		}

		return closestHit;
	}

}
