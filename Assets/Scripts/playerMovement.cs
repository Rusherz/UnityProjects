using UnityEngine;
using System.Collections;

[RequireComponent (typeof(CharacterController))]
public class playerMovement : MonoBehaviour {

	public float speedMult = 5.0f;
	public float sensMult = 5.0f;
	public float lookRange = 60.0f;
	public float jump = 5.0f;

	public bool fired = false;
	public Quaternion startPos;
	float startTime;

	float vertRotation = 0f;
	float verticalVel = 0f;

	private CharacterController charControl;

	// Use this for initialization
	void Start () {
		Screen.lockCursor = true;
		charControl = GetComponent<CharacterController>();
	}
	
	// Update is called once per frame
	void Update () {

		if (fired) {
			StartCoroutine(SlerpRot(Camera.main.transform.rotation, startPos, 1));
		}

		float rotX = Input.GetAxis ("Mouse X") * sensMult;
		float rotY = (Input.GetAxis ("Mouse Y") * sensMult);

		transform.Rotate (0, rotX, 0);

		verticalRoation (rotY);

		float forwardSpeed = Input.GetAxis ("Vertical") * speedMult;
		float strafeSpeed = Input.GetAxis ("Horizontal") * speedMult;

		verticalVel += Physics.gravity.y * Time.deltaTime;

		if (charControl.isGrounded && Input.GetButtonDown("Jump")) {
			verticalVel = jump;
		}

		Vector3 speed = new Vector3 (strafeSpeed, verticalVel, forwardSpeed);

		speed = transform.rotation * speed;

		charControl.Move (speed * Time.deltaTime);
	}

	public void verticalRoation(float rotY){
		vertRotation -= rotY;
		vertRotation = Mathf.Clamp (vertRotation, -lookRange, lookRange);
		Camera.main.transform.localRotation = Quaternion.Euler (vertRotation, 0, 0);
		if (Quaternion.Euler (vertRotation, 0, 0) != Camera.main.transform.localRotation) {
			fired = false;
		}
	}

	public void verticalRoation(float rotY, float starttime){
		Debug.Log(Camera.main.transform.localRotation.eulerAngles.x);
		startTime = starttime;
		vertRotation -= rotY - (Input.GetAxis ("Mouse X") * sensMult);
		vertRotation = Mathf.Clamp (vertRotation, -lookRange, lookRange);
		Camera.main.transform.localRotation = Quaternion.Euler (vertRotation, 0, 0);
		Debug.Log(Camera.main.transform.localRotation.eulerAngles.x);
	}

	IEnumerator SlerpRot(Quaternion startRot, Quaternion endRot, float slerpTime) 
	{
		fired = false;
		float elapsed = 0;
		while(elapsed < slerpTime) 
		{
			elapsed += Time.deltaTime;
			
			Camera.main.transform.localRotation = Quaternion.Slerp(startRot, endRot, elapsed / slerpTime);
			yield return null;
		}
	}
	
}
