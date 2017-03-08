using UnityEngine;
using System.Collections;

public class shoot : MonoBehaviour {

	public static float rpm = 800f;
	public float rof = 60f / rpm;
	float timeRemaining = 0;

	// Use this for initialization
	void Start () {
		Screen.lockCursor = true;
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButton (0) && timeRemaining <= 0) {

			timeRemaining = rof;

			Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
			RaycastHit hitInfo;

			if(Physics.Raycast(ray, out hitInfo)){
				Vector3 hitPoint = hitInfo.point;
			}


		} else {
			timeRemaining -= Time.deltaTime;
		}

	}
}
