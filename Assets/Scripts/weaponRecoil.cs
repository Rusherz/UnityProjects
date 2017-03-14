using UnityEngine;
using System.Collections;

public class weaponRecoil : MonoBehaviour {

	private Vector3 startPoint;
	public static GameObject obj;
	public float recoil;

	// Use this for initialization
	public void setStartPoint () {
		//obj = gunMaster.loadedModel;
		startPoint = obj.transform.localPosition;
	}
	
	void update(){
		
	}

	// Update is called once per frame
	public void Recoil () {
		if (obj != null) {
			if (recoil > 0f) {
				Vector3 maxRecoil = new Vector3 (0f, 0f, 5f);
				// Dampen towards the target rotation
				obj.transform.localPosition = Vector3.Lerp (new Vector3 (startPoint.x, startPoint.y, Mathf.Clamp (obj.transform.localPosition.z, -0.2f, 0f)), -maxRecoil, Time.deltaTime * 2f);
			} else {
				recoil = 0f;
				// Dampen towards the target rotation
				obj.transform.localPosition = Vector3.Lerp (startPoint, new Vector3 (startPoint.x, startPoint.y, obj.transform.localPosition.z), Time.deltaTime * 2f);
			}
		}
	}
}
