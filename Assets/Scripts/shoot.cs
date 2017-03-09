using UnityEngine;
using System.Collections;

public class shoot : MonoBehaviour {

	private static gunMaster gm;
	private Ray ray;
	private float firing = 0;
	private GameObject parent;

	float timeRemaining = 0;

	public GameObject debris;

	// Use this for initialization
	void Start () {
		Screen.lockCursor = true; 
		gm = GetComponent<gunMaster> ();
		parent = GameObject.FindGameObjectWithTag ("ADS");
	}
	
	// Update is called once per frame
	void Update () { 
		firing = Mathf.Clamp(firing, 0, 10);
		if (Input.GetMouseButton (0) && timeRemaining <= 0) {
			if(gunMaster.selected != -1){
				timeRemaining = 60 / gm.guns[gunMaster.selected].getRpm();
				Debug.Log(60 / gm.guns[gunMaster.selected].getRpm());

				if(!Input.GetMouseButton(1) && firing != 0){
					Debug.Log(firing);
					firing += 1;       
					float randomRadius = Random.Range( 0, firing / 10 ); 
					float randomAngle = Random.Range ( 0, 2f * Mathf.PI );
					
					//Calculating the raycast direction
					Vector3 direction = new Vector3(
						randomRadius * Mathf.Cos( randomAngle ),
						randomRadius * Mathf.Sin( randomAngle ),
						10f
						);
					direction = Camera.main.transform.TransformDirection( direction.normalized );
					ray = new Ray( Camera.main.transform.position, direction );
				}else{
					firing += 1;
					Debug.Log(firing);
					ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
				}
				RaycastHit hitInfo;

				if(Physics.Raycast(ray, out hitInfo)){
					Vector3 hitPoint = hitInfo.point;
					Instantiate(debris, hitPoint, Quaternion.identity);
				}
				Fire ();
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
