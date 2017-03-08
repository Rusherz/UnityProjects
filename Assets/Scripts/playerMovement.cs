using UnityEngine;
using System.Collections;

public class playerMovement : MonoBehaviour {

	public float speedMult = 5.0f;
	public float sensMult = 5.0f;
	public float lookRange = 60.0f;

	float vertRotation = 0f;
	float verticalVel = 0f;

	private CharacterController charControl;

	// Use this for initialization
	void Start () {
		Screen.lockCursor = true;
		charControl = GetComponent<CharacterController>;
	}
	
	// Update is called once per frame
	void Update () {

		float rotX = Input.GetAxis ("Mouse X") * sensMult;
		float rotY = -1 * (Input.GetAxis ("Mouse Y") * sensMult);

		transform.Rotate (0, rotX, 0);

		vertRotation -= rotY;
		vertRotation = Mathf.Clamp (vertRotation, -lookRange, lookRange);
		Camera.main.transform.rotation = Quaternion.Euler (vertRotation, 0, 0);

		float forwardSpeed = Input.GetAxis ("Vertical") * speedMult;
		float strafeSpeed = Input.GetAxis ("Horizontal") * speedMult;

		verticalVel += Physics.gravity.y * Time.deltaTime;

		Vector3 speed = new Vector3 (strafeSpeed, verticalVel, forwardSpeed);

		speed = transform.rotation * speed;

		charControl.Move (speed * Time.deltaTime);
	}
}
