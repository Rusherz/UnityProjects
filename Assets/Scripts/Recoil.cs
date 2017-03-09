using UnityEngine;
using System.Collections;

public class Recoil : MonoBehaviour
{
	private bool once = false;

	private static float recoil = 0.0f;
	private static float maxRecoil_x = -10f;
	private static float maxRecoil_y = 4f;
	private static float recoilSpeed = 2f;
	private weaponRecoil wr;

	void start(){
		wr = GetComponent<weaponRecoil>();
	}

	public static void StartRecoil (float recoilParam, float maxRecoil_xParam, float recoilSpeedParam)
	{
		// in seconds
		recoil = recoilParam;
		maxRecoil_x = maxRecoil_xParam;
		recoilSpeed = recoilSpeedParam;
		maxRecoil_y = Random.Range(-3, 3);
	}
	
	void recoiling ()
	{
		if(Input.GetMouseButton(1) && Input.GetMouseButton(0)){
			wr.recoil = recoil;
			if (recoil > 0f) {
				Quaternion maxRecoil = Quaternion.Euler (-(maxRecoil_x + Input.GetAxis("Mouse X")), (maxRecoil_y + Input.GetAxis("Mouse Y")), 0f);
				// Dampen towards the target rotation
				transform.localRotation = Quaternion.Slerp (transform.localRotation, maxRecoil, Time.deltaTime * recoilSpeed);
				wr.Recoil();
				recoil -= Time.deltaTime;
			} else {
				recoil = 0f;
				// Dampen towards the target rotation
				transform.localRotation = Quaternion.Slerp (transform.localRotation, Quaternion.identity, Time.deltaTime * recoilSpeed / 2);
				wr.Recoil();
			}
		}else if(!Input.GetMouseButton(0)){
			recoil = 0f;
			wr.recoil = recoil;
			// Dampen towards the target rotation
			transform.localRotation = Quaternion.Slerp (transform.localRotation, Quaternion.identity, Time.deltaTime * recoilSpeed / 2);
			wr.Recoil();
		}else{
			wr.recoil = recoil;
			if (recoil > 0f) {
				wr.Recoil();
				recoil -= Time.deltaTime;
			} else {
				wr.recoil = 0f;
				wr.Recoil();
			}
		}
	}

	// Update is called once per frame
	void Update ()
	{
		if (gunMaster.selected != -1 && weaponRecoil.obj != null) {
			if (!once) {
				Debug.Log(once);
				start ();
				once = true;
			}
			recoiling ();
		}
	}
}