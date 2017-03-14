using UnityEngine;
using System.Collections;

public class shoot : MonoBehaviour
{

	//private static gunMaster gm;
	private Ray ray;
	private float firing = 0;
	private Camera cam;
	float timeRemaining = 0;
	public GameObject debris;

	// Use this for initialization
	void Start ()
	{
		Screen.lockCursor = true; 
		//gm = GetComponent<gunMaster> ();
		cam = GetComponentInChildren<Camera> ();
	}
	
	// Update is called once per frame
	void Update ()
	{ 
		firing = Mathf.Clamp (firing, 0, 10);
		if (Input.GetMouseButtonDown (0) && timeRemaining <= 0) {
			timeRemaining = 0; /// gm.guns [gunMaster.selected].getRpm ();

			if (!Input.GetMouseButton (1) && firing != 0) {
				firing += 1;       
				float randomRadius = Random.Range (0, firing / 10); 			
				float randomAngle = Random.Range (0, 2f * Mathf.PI);

				//Calculating the raycast direction
				Vector3 direction = new Vector3 (randomRadius * Mathf.Cos (randomAngle),randomRadius * Mathf.Sin (randomAngle), 10f);
				direction = cam.transform.TransformDirection (direction.normalized);		
				ray = new Ray (cam.transform.position, direction);	
			} else {			
				firing += 1;				
				ray = new Ray (cam.transform.position, cam.transform.forward);		
			}

			Transform hitInfo;	
			Vector3 hitPoint;

			//Fire ();

			hitInfo = FindClosestHitInfo (ray, out hitPoint);
	
			if (hitInfo != null) {		
				Debug.Log ("We hit: " + hitInfo.name);
				PlayerHealth h = hitInfo.GetComponent<PlayerHealth> ();
				while (h == null && hitInfo.parent) {
					hitInfo = hitInfo.parent;
					h = hitInfo.GetComponent<PlayerHealth> ();
				}	
				if (h != null) {				
					//h.damage (5);		
					float damage = 50;
					h.GetComponent<PhotonView> ().RPC("damage", PhotonTargets.All, damage);
				}	
			}
				
		} else {		
			timeRemaining -= 1 * Time.deltaTime;		
			firing -= 1 * Time.deltaTime;
		}
	}
	/*	
	void Fire (){
		Recoil.StartRecoil (0.05f, gm.guns [gunMaster.selected].getDamage () / 3f, 5f);
	}
	*/
	Transform FindClosestHitInfo (Ray ray, out Vector3 hitPoint){
		RaycastHit[] hits = Physics.RaycastAll (ray);
		Transform closest = null;
		float distance = 0;
		hitPoint = Vector3.zero;
		foreach (RaycastHit Hit in hits) {
			if (Hit.transform != this.transform && (closest == null || Hit.distance < distance)) {
				closest = Hit.transform;
				distance = Hit.distance;
				hitPoint = Hit.point;
			}
		}
		return closest;
	}

}