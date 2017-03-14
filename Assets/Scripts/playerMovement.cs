using UnityEngine;
using System.Collections;

[RequireComponent (typeof(CharacterController))]
public class playerMovement : MonoBehaviour {

	public float speedMult = 5.0f;
	public float sensMult = 5.0f;
	public float lookRange = 60.0f;
	public float jump = 5.0f;

	private PlayerHealth ph;
	private bool wasDead = false;
	private float timeWait = 0;
	private int selected = -1;
	private gunMaster gunmaster;

	public bool fired = false;
	public Quaternion startPos;
	float startTime;

	float vertRotation = 0f;
	float verticalVel = 0f;

	private CharacterController charControl;
	private Camera cam;

	public Transform[] gui;

	// Use this for initialization
	void Start () {
		Screen.lockCursor = true;
		charControl = GetComponent<CharacterController>();
		ph = GetComponent<PlayerHealth>();
		cam = GetComponentInChildren<Camera> ();
		gunmaster = GetComponent<gunMaster> ();
		/*Transform temp = this.GetComponentsInParent<Transform> ()[1];
		gui = temp.GetComponentsInChildren<Transform> () [6].GetComponentsInChildren<Transform>();
		// gui[0] = GUI holder
		// gui[1] = crosshair
		// gui[2] = DeathMessage
		// gui[3] = DeathTime
		gui [2].guiText.enabled = false;*/

	}
	
	// Update is called once per frame
	void Update () {
		if(!ph.getDead()){

			selectWeapon();

			if (fired) {
				StartCoroutine(SlerpRot(cam.transform.localRotation, startPos, 0.5f));
			}
			if(wasDead){
				isDead(false);
			}
			//ph.damage (100 * Time.deltaTime);

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
		}else{
			isDead(true);
		}
	}

	void selectWeapon(){
		timeWait -= Time.deltaTime;
		if(timeWait <= 0){
			if(Input.GetKeyDown(KeyCode.Alpha1) && selected != 0){
				gunmaster.GetComponent<PhotonView>().RPC("weapon", PhotonTargets.All, 0);
			}else if(Input.GetKeyDown(KeyCode.Alpha2) && selected != 1){
				gunmaster.GetComponent<PhotonView>().RPC("weapon", PhotonTargets.All, 1);
			}else if(Input.GetKeyDown(KeyCode.Alpha3) && selected != 2){
				gunmaster.GetComponent<PhotonView>().RPC("weapon", PhotonTargets.All, 2);
			}else if(Input.GetKeyDown(KeyCode.Alpha4) && selected != 3){
				gunmaster.GetComponent<PhotonView>().RPC("weapon", PhotonTargets.All, 3);
			}
		}
	}

	public void isDead(bool dead){
		if(dead){
			/*gui[2].guiText.enabled = true;
			gui[3].guiText.text = ((int)ph.getDeathTime() + 1).ToString();
			gui[1].guiTexture.enabled = false;
			wasDead = true;*/
		}else{
			/*gui[2].guiText.enabled = false;
			gui[3].guiText.text = "";
			gui[1].guiTexture.enabled = true;
			wasDead = false;*/
		}
	}

	public void verticalRoation(float rotY){
		vertRotation -= rotY;
		vertRotation = Mathf.Clamp (vertRotation, -lookRange, lookRange);
		cam.transform.localRotation = Quaternion.Euler (vertRotation, 0, 0);
		if (Quaternion.Euler (vertRotation, 0, 0) != cam.transform.localRotation) {
			fired = false;
		}
	}

	public void verticalRoation(float rotY, float starttime){
		cam.transform.localRotation = Quaternion.Euler (-(cam.transform.localRotation.x + rotY), 0, 0);
	}

	IEnumerator SlerpRot(Quaternion startRot, Quaternion endRot, float slerpTime) 
	{
		fired = false;
		float elapsed = 0;
		while(elapsed < slerpTime) 
		{
			elapsed += 1 * Time.deltaTime;
			
			cam.transform.localRotation = Quaternion.Slerp(startRot, endRot, elapsed / slerpTime);
			yield return null;
		}
	}
	
}
