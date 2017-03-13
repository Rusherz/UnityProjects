using UnityEngine;
using System.Collections;

public class shoot : MonoBehaviour {

	private static gunMaster gm;
	private Ray ray;
	private float firing = 0;
	private GameObject parent;
	private Camera cam;

	float timeRemaining = 0;

	public GameObject debris;

	// Use this for initialization
	void Start () {
		Screen.lockCursor = true; 
		gm = GetComponent<gunMaster> ();
		cam = GetComponentInChildren<Camera> ();
		parent = GameObject.FindGameObjectWithTag ("ADS");
	}
	
	// Update is called once per frame
	void Update () { 
		firing = Mathf.Clamp(firing, 0, 10);
		if (Input.GetMouseButton (0) && timeRemaining <= 0) {
			if(gunMaster.selected != -1){
				timeRemaining = 60 / gm.guns[gunMaster.selected].getRpm();

				if(!Input.GetMouseButton(1) && firing != 0){
					firing += 1;       
					float randomRadius = Random.Range( 0, firing / 10 ); 
					float randomAngle = Random.Range ( 0, 2f * Mathf.PI );
					
					//Calculating the raycast direction
					Vector3 direction = new Vector3(
						randomRadius * Mathf.Cos( randomAngle ),
						randomRadius * Mathf.Sin( randomAngle ),
						10f
						);
					direction = cam.transform.TransformDirection( direction.normalized );
					ray = new Ray( cam.transform.position, direction );
				}else{
					firing += 1;
					ray = new Ray(cam.transform.position, cam.transform.forward);
				}
				RaycastHit hitInfo;

				Fire ();

				if(Physics.Raycast(ray, out hitInfo)){
					Vector3 hitPoint = hitInfo.point;
					var hit = hitInfo.collider;
					if(hit.GetComponent<PlayerHealth>() != null){
						hit.GetComponent<PlayerHealth>().damage(gm.guns[gunMaster.selected].getDamage());
					}
					Instantiate(debris, hitPoint, Quaternion.identity);
				}

			}
		} else {
			timeRemaining -= 1 * Time.deltaTime;
			firing -= 1 * Time.deltaTime;
		}
	}

	void Fire ()
	{
		Recoil.StartRecoil(0.05f, gm.guns[gunMaster.selected].getDamage() / 3f, 5f);
	}

}
