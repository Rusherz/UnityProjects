using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour {

	public float movementSpeed { get; set; }
	public float lookSpeed { get; set; }
	public float jump { get; set; }

	float vertVel;
	float vertRotation = 0f;

	Camera cam;
	CharacterController charController;

	// Use this for initialization
	void Start () {
		charController = GetComponent<CharacterController> ();
		cam = GetComponentInChildren<Camera> ();
		movementSpeed = 5;
		lookSpeed = 5;
	}
	
	// Update is called once per frame
	void Update () {

		Movement ();
		Look ();

	}

	/*
	 * 
	 * Methods
	 * 
	 */

	void Movement(){
		float forward = Input.GetAxis ("Vertical") * movementSpeed;
		float sideway = Input.GetAxis ("Horizontal") * movementSpeed;

		Debug.Log (Input.GetAxis ("Vertical"));

		vertVel = Physics.gravity.y * Time.deltaTime;

		if (charController.isGrounded && Input.GetButtonDown ("Jump")) {
			vertVel = jump;
		}

		Vector3 movement = new Vector3 (sideway, vertVel, forward);

		movement = transform.rotation * movement;

		charController.Move (movement * Time.deltaTime);

	}

	void Look(){
		float rotX = Input.GetAxis ("Mouse X") * lookSpeed;
		float rotY = Input.GetAxis ("Mouse Y") * lookSpeed;


		// Horizontal Look
		transform.Rotate (0, rotX, 0);

		// Vertical Look
		vertRotation -= rotY;
		vertRotation = Mathf.Clamp (vertRotation, -60, 60);
		cam.transform.localRotation = Quaternion.Euler (vertRotation, 0, 0);


	}



}
