using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class PlayerController1 : MonoBehaviour
{
    //Other
    public CharacterController charControl;
    private Camera cam;

    // Use this for initialization
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        charControl = GetComponent<CharacterController>();
        cam = transform.FindChild("PlayerCamera").GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        movement();
    }

    /*
	**
	** Movement Functions & Variables
	**
	*/
    public float speedMult = 5.0f;
    public float sensMult = 5.0f;
    public float lookRange = 60.0f;
    public float jump = 5.0f;
    public float vertRotation = 0f;
    public float verticalVel = 0f;

    void movement()
    {
        float rotX = Input.GetAxis("Mouse X") * sensMult;
        float rotY = (Input.GetAxis("Mouse Y") * sensMult);

        transform.Rotate(0, rotX, 0);

        vertRotation -= rotY;
        vertRotation = Mathf.Clamp(vertRotation, -lookRange, lookRange);
        cam.transform.localRotation = Quaternion.Euler(vertRotation, 0, 0);

        float forwardSpeed = Input.GetAxis("Vertical") * speedMult;
        float strafeSpeed = Input.GetAxis("Horizontal") * speedMult;

        verticalVel += Physics.gravity.y * Time.deltaTime;

        if (charControl.isGrounded && Input.GetButtonDown("Jump"))
        {
            verticalVel = jump;
        }

        Vector3 speed = new Vector3(strafeSpeed, verticalVel, forwardSpeed);

        speed = transform.rotation * speed;

        charControl.Move(speed * Time.deltaTime);
    }

}