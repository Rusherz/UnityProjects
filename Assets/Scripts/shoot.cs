using UnityEngine;
using System.Collections;

public class shoot : MonoBehaviour {

	public static float rpm = 800f;
	public float rof = 60f / rpm;
	float timeRemaining = 0;
	playerMovement pm;

	// Use this for initialization
	void Start () {
		Screen.lockCursor = true;
		pm = GameObject.FindObjectOfType(typeof(playerMovement)) as playerMovement;
		Debug.Log (pm);
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButton (0) && timeRemaining <= 0) {
			
			pm.startPos = Camera.main.transform.localRotation;

			timeRemaining = rof;

			Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
			RaycastHit hitInfo;

			//pm.fired = true;
			pm.verticalRoation(Random.Range(0f, 10f), Time.time);

			if(Physics.Raycast(ray, out hitInfo)){
				Vector3 hitPoint = hitInfo.point;
			}


		} else {
			timeRemaining -= Time.deltaTime;
		}

	}
}
