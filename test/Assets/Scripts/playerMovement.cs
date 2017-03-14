﻿using UnityEngine;
using System.Collections;

[RequireComponent (typeof(CharacterController))]
public class playerMovement : MonoBehaviour {
	
	public float speedMult = 5.0f;
	public float sensMult = 5.0f;
	public float lookRange = 60.0f;
	public float jump = 5.0f;
	
	float vertRotation = 0f;
	float verticalVel = 0f;
	
	private CharacterController charControl;
	private Camera cam;
	
	// Use this for initialization
	void Start () {
		Screen.lockCursor = true;
		charControl = GetComponent<CharacterController>();
		cam = GetComponentInChildren<Camera> ();
	}
	
	// Update is called once per frame
	void Update () {
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
		cam.transform.localRotation = Quaternion.Euler (vertRotation, 0, 0);
	}
	
}
